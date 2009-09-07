//    Copyright (C) 2006-2007  Regis COSNIER
//
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation; either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program; if not, write to the Free Software
//    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.Utils;

namespace CodeTV
{
	// File: Crossbar.cpp
	//
	// Desc: A class for controlling video crossbars. 
	//
	//       This class creates a single object which encapsulates all connected
	//       crossbars, enumerates all unique inputs which can be reached from
	//       a given starting pin, and automatically routes audio when a video
	//       source is selected.
	//
	//       The class supports an arbitrarily complex graph of crossbars, 
	//       which can be cascaded and disjoint, that is not all inputs need 
	//       to traverse the same set of crossbars.
	//
	//       Given a starting input pin (typically the analog video input to
	//       the capture filter), the class recursively traces upstream 
	//       searching for all viable inputs.  An input is considered viable if
	//       it is a video pin and is either:
	//
	//           - unconnected 
	//           - connects to a filter which does not support IAMCrossbar 
	//
	//       Methods:
	//
	//       CCrossbar (IPin *pPin);             
	//       ~CCrossbar();
	//
	//       int GetInputCount (int *pCount);
	//       int GetInputType  (int Index, int * PhysicalType);
	//       int GetInputName  (int Index, TCHAR * pName, int NameSize);
	//       int SetInputIndex (int Index);
	//       int GetInputIndex (int *Index);
	public class CrossbarHelper
	{
		// This class contains routing information for the capture data
		public class Routing
		{
			public Routing() { }
			public Routing(Routing routing)
			{
				leftRouting = routing.leftRouting;
				rightRouting = routing.rightRouting;
				inputIndex = routing.inputIndex;
				outputIndex = routing.outputIndex;
				inputIndexRelated = routing.inputIndexRelated;
				outputIndexRelated = routing.outputIndexRelated;
				crossbar = routing.crossbar;
				inputPhysicalType = routing.inputPhysicalType;
				outputPhysicalType = routing.outputPhysicalType;
				depth = routing.depth;
				inputName = routing.inputName;
			}

			public Routing leftRouting;
			public Routing rightRouting;
			public int inputIndex;
			public int outputIndex;
			public int inputIndexRelated;
			public int outputIndexRelated;
			public IAMCrossbar crossbar;
			public PhysicalConnectorType inputPhysicalType;
			public PhysicalConnectorType outputPhysicalType;
			public int depth;
			public string inputName;
		};

		public static TraceSwitch trace = new TraceSwitch("WDMCrossbar", "WDMCrossbar traces", "Info");

		List<Routing> videoRoutingList = new List<Routing>();
		List<Routing> audioRoutingList = new List<Routing>();
		int currentAudioRoutingIndex = -1;
		int currentVideoRoutingIndex = -1;
		Hashtable pinNameByPhysicalConnectorType = new Hashtable();

		public List<Routing> AudioRoutingList { get { return audioRoutingList; } }
		public List<Routing> VideoRoutingList { get { return videoRoutingList; } }
		public Hashtable PinNameByPhysicalConnectorType { get { return pinNameByPhysicalConnectorType; } }

		public CrossbarHelper(IPin startingVideoInputPin) 
		{
			foreach (PhysicalConnectorType type in Enum.GetValues(typeof(PhysicalConnectorType)))
				this.pinNameByPhysicalConnectorType[type] = StringFromPinType(type);

			int hr = BuildRoutingList(startingVideoInputPin, new Routing(), 0 /* Depth */);
		}

		// Desc: Destructor for the CCrossbar class
		public void Dispose()
		{
			DestroyRoutingList();
		}

		// This function is called recursively, every time a new crossbar is
		// entered as we search upstream.
		//
		// Return values:
		//
		//   0 - Returned on final exit after recursive search if at least
		//       one routing is possible
		//   1 - Normal return indicating we've reached the end of a 
		//       recursive search, so save the current path
		//  -1 - Unable to route anything
		private int BuildRoutingList(IPin startingInputPin, Routing routing, int depth)
		{
			if (startingInputPin == null || routing == null)
				return -1; // E_POINTER;

			// If the pin isn't connected, then it's a terminal pin
			IPin startingOutputPin = null;
			int hr = startingInputPin.ConnectedTo(out startingOutputPin);
			if (hr != 0)
				return ((depth == 0) ? -1 : 1);

			// It is connected, so now find out if the filter supports IAMCrossbar
			PinInfo pinInfo;
			if (startingOutputPin.QueryPinInfo(out pinInfo) == 0)
			{
				//ASSERT (pinInfo.dir == PINDIR_OUTPUT);

				IAMCrossbar crossbar = pinInfo.filter as IAMCrossbar;
				if (crossbar != null)
				{
					int inputs, outputs, inputIndex, outputIndex;
					int inputIndexRelated, outputIndexRelated;
					PhysicalConnectorType inputPhysicalType, outputPhysicalType;

					hr = crossbar.get_PinCounts(out outputs, out inputs);

					// for all output pins
					for (outputIndex = 0; outputIndex < outputs; outputIndex++)
					{
						hr = crossbar.get_CrossbarPinInfo(false, outputIndex, out outputIndexRelated, out outputPhysicalType);

						// for all input pins
						for (inputIndex = 0; inputIndex < inputs; inputIndex++)
						{
							hr = crossbar.get_CrossbarPinInfo(true, inputIndex, out inputIndexRelated, out inputPhysicalType);

							// Can we route it?
							if (crossbar.CanRoute(outputIndex, inputIndex) == 0)
							{
								IPin pPin = null;
								hr = GetCrossbarPinAtIndex(crossbar, inputIndex, true, out pPin);

								// We've found a route through this crossbar
								// so save our state before recusively searching
								// again.
								Routing routingNext = new Routing();
								// doubly linked list
								routingNext.rightRouting = routing;
								routing.leftRouting = routingNext;

								routing.crossbar = crossbar;
								routing.inputIndex = inputIndex;
								routing.outputIndex = outputIndex;
								routing.inputIndexRelated = inputIndexRelated;
								routing.outputIndexRelated = outputIndexRelated;
								routing.inputPhysicalType = inputPhysicalType;
								routing.outputPhysicalType = outputPhysicalType;
								routing.depth = depth;
								routing.inputName = this.pinNameByPhysicalConnectorType[inputPhysicalType] as string;

								hr = BuildRoutingList(pPin, routingNext, depth + 1);
								if (hr == 1)
								{
									routing.leftRouting = null;
									SaveRouting(routing, inputPhysicalType >= PhysicalConnectorType.Audio_Tuner);
								}
							} // if we can route
						} // for all input pins
					}
					//pXbar.Release();
				}
				else
				{
					// The filter doesn't support IAMCrossbar, so this
					// is a terminal pin
					DsUtils.FreePinInfo(pinInfo);
					Marshal.ReleaseComObject(startingOutputPin);

					return (depth == 0) ? -1 : 1;
				}

				DsUtils.FreePinInfo(pinInfo);
			}
			Marshal.ReleaseComObject(startingOutputPin);

			return 0;
		}

		// Make a copy of the current routing, and AddRef the IAMCrossbar interfaces.
		private int SaveRouting(Routing routingNew, bool audioList)
		{
			int depth = routingNew.depth + 1;
			Routing routingCurrent = routingNew;

			if (routingNew == null)
				return -1;

			Trace.WriteLineIf(trace.TraceInfo, string.Format("SaveRouting, Depth={0}, NumberOfRoutings={1}",
				depth, (audioList ? this.audioRoutingList : this.videoRoutingList).Count + 1));

			Routing[] routings = new Routing[depth];

			for (int j = 0; j < depth; j++) 
			{
				routings[j] = new Routing(routingCurrent);
				//ASSERT (pCurrent.pXbar != null);

				// We're holding onto this interface, so AddRef
				//pCurrent.pXbar.AddRef();
				routingCurrent = routingCurrent.rightRouting;

				// Pointers were stack based during recursive search, so update them
				// in the allocated array
				if (j == 0)                   // first element
					routings[j].leftRouting = null;
				else
					routings[j].leftRouting = routings[j - 1];

				if (j == (depth - 1))         // last element
					routings[j].rightRouting = null;
				else
					routings[j].rightRouting = routings[j + 1];
			}
			(audioList ? this.audioRoutingList : this.videoRoutingList).AddRange(routings);

			return 0;
		}


		private void DestroyRoutingList()
		{
			this.audioRoutingList.Clear();
			this.videoRoutingList.Clear();

			Trace.WriteLineIf(trace.TraceInfo, "DestroyRoutingList");

			//int k, Depth;
			//CRouting pCurrent = null, pFirst = null;

			//while (m_RoutingList.Count) 
			//{
			//    pCurrent = pFirst = m_RoutingList[0];
			//    m_RoutingList.RemoveAt(0);

			//    if (pCurrent)
			//    {
			//        Depth = pCurrent.Depth + 1;

			//        for (k = 0; k < Depth; k++) 
			//        {
			//            //ASSERT (pCurrent.pXbar != null);

			//            // Release the crossbar interface
			//            //pCurrent.pXbar.Release();

			//            // Move to the next node in the list
			//            pCurrent = pCurrent.pRightRouting;
			//        }
			//    }
		        
			//    delete [] pFirst;
			//}

			//return 0;
		}


		// Does not AddRef the returned *Pin 
		private int GetCrossbarPinAtIndex(IAMCrossbar crossbar, int pinIndex, bool isInputPin, out IPin pin)
		{
			pin = null;

			if (crossbar == null)
				return -1;

			int cntInPins, cntOutPins;
			if (crossbar.get_PinCounts(out cntOutPins, out cntInPins) != 0)
				return unchecked((int)0x80004005); //E_FAIL;

			int trueIndex = isInputPin ? pinIndex : pinIndex + cntInPins;

			IBaseFilter filter = crossbar as IBaseFilter;
			if (filter != null) 
			{
				IEnumPins enumPins = null;
				if (filter.EnumPins(out enumPins) == 0) 
				{
					int i = 0;
					IPin [] pins = new IPin[1];
					//22 int n;
					//22 while(enumPins.Next(1, pins, out n) == 0) 
					while (enumPins.Next(1, pins, IntPtr.Zero) == 0)
					{
						//pP.Release();
						if (i == trueIndex) 
						{
							pin = pins[0];
							break;
						}
						Marshal.ReleaseComObject(pins[0]);
						i++;
					}
					Marshal.ReleaseComObject(enumPins);
				}
				//pFilter.Release();
			}
		    
			return pin != null ? 0 : unchecked((int)0x80004005); //E_FAIL; 
		}

		// Converts a PinType into a String
		public string StringFromPinType(PhysicalConnectorType type)
		{
			switch (type)
			{
				case PhysicalConnectorType.Video_Tuner: return "Video Tuner";
				case PhysicalConnectorType.Video_Composite: return "Video Composite";
				case PhysicalConnectorType.Video_SVideo: return "Video SVideo";
				case PhysicalConnectorType.Video_RGB: return "Video RGB";
				case PhysicalConnectorType.Video_YRYBY: return "Video YRYBY";
				case PhysicalConnectorType.Video_SerialDigital: return "Video SerialDigital";
				case PhysicalConnectorType.Video_ParallelDigital: return "Video ParallelDigital";
				case PhysicalConnectorType.Video_SCSI: return "Video SCSI";
				case PhysicalConnectorType.Video_AUX: return "Video AUX";
				case PhysicalConnectorType.Video_1394: return "Video 1394";
				case PhysicalConnectorType.Video_USB: return "Video USB";
				case PhysicalConnectorType.Video_VideoDecoder: return "Video Decoder";
				case PhysicalConnectorType.Video_VideoEncoder: return "Video Encoder";

				case PhysicalConnectorType.Audio_Tuner: return "Audio Tuner";
				case PhysicalConnectorType.Audio_Line: return "Audio Line";
				case PhysicalConnectorType.Audio_Mic: return "Audio Mic";
				case PhysicalConnectorType.Audio_AESDigital: return "Audio AESDigital";
				case PhysicalConnectorType.Audio_SPDIFDigital: return "Audio SPDIFDigital";
				case PhysicalConnectorType.Audio_SCSI: return "Audio SCSI";
				case PhysicalConnectorType.Audio_AUX: return "Audio AUX";
				case PhysicalConnectorType.Audio_1394: return "Audio 1394";
				case PhysicalConnectorType.Audio_USB: return "Audio USB";
				case PhysicalConnectorType.Audio_AudioDecoder: return "Audio Decoder";

				default:
					return "Unknown";
			}
		}

		// How many unique video inputs can be selected?
		public int GetVideoInputCount() { return this.videoRoutingList.Count; }

		// What is the physical type of a given input?
		public PhysicalConnectorType GetVideoInputType(int index) { return this.videoRoutingList[index].inputPhysicalType; }

		// Get a text version of an input
		public string GetVideoInputName(int index) { return StringFromPinType(this.videoRoutingList[index].inputPhysicalType); }

		// Select an input 
		public void SetVideoInputIndex(int index, bool setRelated)
		{
			Routing current = this.videoRoutingList[index];

			int depth = current.depth + 1;

			for (int j = 0; j < depth; j++) 
			{
				int hr = current.crossbar.Route(current.outputIndex, current.inputIndex);
				//ASSERT (0 == hr);

				if (setRelated && current.outputIndexRelated != -1 && current.inputIndexRelated != -1)
					hr = current.crossbar.Route(current.outputIndexRelated, current.inputIndexRelated);

				Trace.WriteLineIf(trace.TraceInfo, string.Format("Routing, VideoOutIndex={0} VideoInIndex={1}", 
				        current.outputIndex, current.inputIndex));

				if (index + 1 < this.videoRoutingList.Count)
					current = this.videoRoutingList[index + 1]; //pCurrent++;
			}

			this.currentVideoRoutingIndex = index;
		}

		// What input is currently selected?
		public int GetVideoInputIndex() { return this.currentVideoRoutingIndex; }

		// What input is currently selected?
		public string GetVideoInput()
		{
			if (this.currentVideoRoutingIndex != -1)
				return this.videoRoutingList[this.currentVideoRoutingIndex].inputName;
			else
				return "";
		}

		// Select an input 
		public void SetVideoInput(string inputName, bool setRelated)
		{
			int numberOfInput = GetVideoInputCount();
			for (int i = 0; i < numberOfInput; i++)
				if (this.videoRoutingList[i].inputName == inputName)
				{
					SetVideoInputIndex(i, setRelated);
					break;
				}
		}


		// How many unique video inputs can be selected?
		public int GetAudioInputCount() { return this.audioRoutingList.Count; }

		// What is the physical type of a given input?
		public PhysicalConnectorType GetAudioInputType(int index) { return this.audioRoutingList[index].inputPhysicalType; }

		// Get a text version of an input
		public string GetAudioInputName(int index) { return StringFromPinType(this.audioRoutingList[index].inputPhysicalType); }

		// Select an input 
		public void SetAudioInputIndex(int index, bool setRelated)
		{
			Routing current = this.audioRoutingList[index];

			int depth = current.depth + 1;

			for (int j = 0; j < depth; j++)
			{
				int hr = current.crossbar.Route(current.outputIndex, current.inputIndex);
				//ASSERT (0 == hr);

				if (setRelated && current.outputIndexRelated != -1 && current.inputIndexRelated != -1)
					hr = current.crossbar.Route(current.outputIndexRelated, current.inputIndexRelated);

				Trace.WriteLineIf(trace.TraceInfo, string.Format("Routing, AudioOutIndex={0} AudioInIndex={1}",
						current.outputIndex, current.inputIndex));

				if (index + 1 < this.audioRoutingList.Count)
					current = this.audioRoutingList[index + 1]; //pCurrent++;
			}

			this.currentAudioRoutingIndex = index;
		}

		// What input is currently selected?
		public int GetAudioInputIndex() { return this.currentAudioRoutingIndex; }

		// What input is currently selected?
		public string GetAudioInput()
		{
			if (this.currentAudioRoutingIndex != -1)
				return this.audioRoutingList[this.currentAudioRoutingIndex].inputName;
			else
				return "";
		}

		// Select an input 
		public void SetAudioInput(string inputName, bool setRelated)
		{
			int numberOfInput = GetAudioInputCount();
			for (int i = 0; i < numberOfInput; i++)
				if (this.audioRoutingList[i].inputName == inputName)
				{
					SetAudioInputIndex(i, setRelated);
					break;
				}
		}
	}
}
