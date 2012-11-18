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
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

using DirectShowLib;
using DirectShowLib.BDA;
using System.Drawing;

namespace CodeTV
{
	public class Utils
	{
		[DllImport("user32.dll")]
		public static extern IntPtr GetCapture();

        public static Image ResizeImage(Image image, int newWidth, int newHeight, bool highQuality)
        {
            System.Drawing.Image result = new Bitmap(newWidth, newHeight);
            System.Drawing.Graphics graphic = Graphics.FromImage(result);

            // Set the quality options
            if (highQuality)
            {
                graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphic.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            }

            // Constraint the sizes
            double originalRatio = (double)image.Width / image.Height;
            double standardRatio = (double)newWidth / newHeight;
            int offsetX = 0, offsetY = 0;
            if (originalRatio > standardRatio)
            {
                newHeight = (int)(image.Height * newWidth / image.Width);
                offsetY = (newWidth - newHeight) >> 1;
            }
            else
            {
                newWidth = (int)(image.Width * newHeight / image.Height);
                offsetX = (newHeight - newWidth) >> 1;
            }

            // Draw the image with the resized dimensions
            graphic.DrawImage(image, offsetX, offsetY, newWidth, newHeight);

            return result;
        }
	}

    public class CLSID
    {
        public const string CaptureGraphBuilder2 = "{BF87B6E1-8C27-11D0-B3F0-00AA003761C5}";

        public const string ATSCNetworkProvider = "{0DAD2FDD-5FD7-11D3-8F50-00C04F7971E2}";
        public const string DVBCNetworkProvider = "{DC0C0FE7-0485-4266-B93F-68FBF80ED834}";
        public const string DVBSNetworkProvider = "{FA4B375A-45B4-4D45-8440-263957B11623}";
        public const string DVBTNetworkProvider = "{216C62DF-6D7F-4E9A-8571-05F14EDB766A}";
        public const string ElecardMPEG2VideoDecoder = "{F50B3F13-19C4-11CF-AA9A-02608C9BABA2}";
        public const string SystemDeviceEnum = "{62BE5D10-60EB-11D0-BD3B-00A0C911CE86}";
    }

	public class DsDeviceEx
	{
		private DsDevice dsDevice;

		public DsDeviceEx(DsDevice dsDevice) { this.dsDevice = dsDevice; }
		public DsDevice Device { get { return this.dsDevice; } }
		public override string ToString() { return this.dsDevice.Name; }
	}

	public class TunerTypeEx
	{
		private TunerType channelType;

		public TunerTypeEx(TunerType channelType) { this.channelType = channelType; }
		public TunerType Type { get { return this.channelType; } }

		public Channel GetNewChannel()
		{
			switch (this.channelType)
			{
				case TunerType.DVBT:
					return new ChannelDVBT();
				case TunerType.DVBC:
					return new ChannelDVBC();
				case TunerType.DVBS:
					return new ChannelDVBS();
				case TunerType.Analogic:
					return new ChannelAnalogic();
			}
			return null;
		}

		public override string ToString()
		{
			switch (this.channelType)
			{
				case TunerType.DVBT: return "DVB Terrestrial";
				case TunerType.DVBC: return "DVB Cable";
				case TunerType.DVBS: return "DVB Satellite";
				case TunerType.Analogic: return "Analogic";
			}
			return "Unknown";
		}
	}

	public class FrequencyEx
	{
		private long frequency;

		public FrequencyEx(long frequency) { this.frequency = frequency; }
		public long Frequency { get { return this.frequency; } }
		public override string ToString() { return this.frequency.ToString(); }
	}

	public class FileUtils
	{
		private static string workingDirectory = Directory.GetCurrentDirectory();
		public static string WorkingDirectory { get { return workingDirectory; } }

		public static string GenerateRelativePath(string absoluteFilePath)
		{
			return GenerateRelativePath(WorkingDirectory, absoluteFilePath);
		}

		public static string GenerateRelativePath(string mainDirPath, string absoluteFilePath)
		{
			string[] firstPathParts = mainDirPath.Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
			string[] secondPathParts = absoluteFilePath.Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);

			int sameCounter = 0;
			for (int i = 0; i < Math.Min(firstPathParts.Length, secondPathParts.Length); i++)
			{
				if (!firstPathParts[i].ToLower().Equals(secondPathParts[i].ToLower()))
				{
					break;
				}
				sameCounter++;
			}

			if (sameCounter == 0)
			{
				return absoluteFilePath;
			}

			string newPath = String.Empty;
			for (int i = sameCounter; i < firstPathParts.Length; i++)
			{
				if (i > sameCounter)
				{
					newPath += Path.DirectorySeparatorChar;
				}
				newPath += "..";
			}
			if (newPath.Length == 0)
			{
				newPath = ".";
			}
			for (int i = sameCounter; i < secondPathParts.Length; i++)
			{
				newPath += Path.DirectorySeparatorChar;
				newPath += secondPathParts[i];
			}
			return newPath;
		}

		public static string GetAbsolutePath(string filePath)
		{
			if (filePath.Length > 1 && (filePath[0] == '\\' || filePath[1] == ':'))
				return filePath;
			else
				return WorkingDirectory + "\\" + filePath;
		}
	}

	public sealed class NativeMethodes
	{
		// Graphics.GetHdc() have several "bugs" detailed here : 
		// http://support.microsoft.com/default.aspx?scid=kb;en-us;311221
		// (see case 2) So we have to play with old school GDI...
		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		public static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		[DllImport("gdi32.dll")]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

	}

	public class DeviceEnumerator
	{
		public static DsDevice[] GetDevicesWithThisInPin(Guid mainType, Guid subType)
		{
			DsDevice[] devret;
			ArrayList devs = new ArrayList();

			IFilterMapper2 pMapper = (IFilterMapper2)new FilterMapper2();

			Guid[] arrayInTypes = new Guid[2] { mainType, subType };
			IEnumMoniker pEnum = null;
			int hr = pMapper.EnumMatchingFilters(out pEnum,
					0,                  // Reserved.
					true,               // Use exact match?
					(Merit)((int)Merit.DoNotUse + 1), // Minimum merit.
					true,               // At least one input pin?
					1,                  // Number of major type/subtype pairs for input.
					arrayInTypes,       // Array of major type/subtype pairs for input.
					null,               // Input medium.
					null,               // Input pin category.
					false,              // Must be a renderer?
					true,               // At least one output pin?
					0,                  // Number of major type/subtype pairs for output.
					null,               // Array of major type/subtype pairs for output.
					null,               // Output medium.
					null);              // Output pin category.

			DsError.ThrowExceptionForHR(hr);

			if (hr >= 0 && pEnum != null)
			{
				try
				{
					try
					{
						// Enumerate the monikers.
						IMoniker[] pMoniker = new IMoniker[1];
						while (pEnum.Next(1, pMoniker, IntPtr.Zero) == 0)
						{
							try
							{
								// The devs array now owns this object.  Don't
								// release it if we are going to be successfully
								// returning the devret array
								devs.Add(new DsDevice(pMoniker[0]));
							}
							catch
							{
								Marshal.ReleaseComObject(pMoniker[0]);
								throw;
							}
						}
					}
					finally
					{
						// Clean up.
						Marshal.ReleaseComObject(pEnum);
					}


					// Copy the ArrayList to the DsDevice[]
					devret = new DsDevice[devs.Count];
					devs.CopyTo(devret);
				}
				catch
				{
					foreach (DsDevice d in devs)
					{
						d.Dispose();
					}
					throw;
				}
			}
			else
			{
				devret = new DsDevice[0];
			}

			Marshal.ReleaseComObject(pMapper);

			return devret;
		}

		public static DsDevice[] GetMPEG2VideoDevices()
		{
			DsDevice[] devret;
			ArrayList devs = new ArrayList();

			IFilterMapper2 pMapper = (IFilterMapper2)new FilterMapper2();

			Guid[] arrayInTypes = new Guid[2] { MediaType.Video, MediaSubType.Mpeg2Video };
			IEnumMoniker pEnum = null;
			int hr = pMapper.EnumMatchingFilters(out pEnum,
					0,                  // Reserved.
					true,               // Use exact match?
					(Merit)((int)Merit.DoNotUse + 1), // Minimum merit.
					true,               // At least one input pin?
					1,                  // Number of major type/subtype pairs for input.
					arrayInTypes,       // Array of major type/subtype pairs for input.
					null,               // Input medium.
					null,               // Input pin category.
					false,              // Must be a renderer?
					true,               // At least one output pin?
					0,                  // Number of major type/subtype pairs for output.
					null,               // Array of major type/subtype pairs for output.
					null,               // Output medium.
					null);              // Output pin category.

			DsError.ThrowExceptionForHR(hr);

			if (hr >= 0 && pEnum != null)
			{
				try
				{
					try
					{
						// Enumerate the monikers.
						IMoniker[] pMoniker = new IMoniker[1];
						while (pEnum.Next(1, pMoniker, IntPtr.Zero) == 0)
						{
							try
							{
								// The devs array now owns this object.  Don't
								// release it if we are going to be successfully
								// returning the devret array
								devs.Add(new DsDevice(pMoniker[0]));
							}
							catch
							{
								Marshal.ReleaseComObject(pMoniker[0]);
								throw;
							}
						}
					}
					finally
					{
						// Clean up.
						Marshal.ReleaseComObject(pEnum);
					}


					// Copy the ArrayList to the DsDevice[]
					devret = new DsDevice[devs.Count];
					devs.CopyTo(devret);
				}
				catch
				{
					foreach (DsDevice d in devs)
					{
						d.Dispose();
					}
					throw;
				}
			}
			else
			{
				devret = new DsDevice[0];
			}

			Marshal.ReleaseComObject(pMapper);

			return devret;
		}

		public static DsDevice[] GetH264Devices()
		{
			DsDevice[] devret;
			ArrayList devs = new ArrayList();

			IFilterMapper2 pMapper = (IFilterMapper2)new FilterMapper2();

			Guid[] arrayInTypes = new Guid[2]
				{
					MediaType.Video,
					//new Guid(0x8d2d71cb, 0x243f, 0x45e3, 0xb2, 0xd8, 0x5f, 0xd7, 0x96, 0x7e, 0xc0, 0x9b)
					new Guid(0x34363248, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71) // FOURCC H264
				};
			IEnumMoniker pEnum = null;
			int hr = pMapper.EnumMatchingFilters(out pEnum,
					0,                  // Reserved.
					true,               // Use exact match?
					(Merit)((int)Merit.DoNotUse + 1), // Minimum merit.
					true,               // At least one input pin?
					1,                  // Number of major type/subtype pairs for input.
					arrayInTypes,       // Array of major type/subtype pairs for input.
					null,               // Input medium.
					null,               // Input pin category.
					false,              // Must be a renderer?
					true,               // At least one output pin?
					0,                  // Number of major type/subtype pairs for output.
					null,               // Array of major type/subtype pairs for output.
					null,               // Output medium.
					null);              // Output pin category.

			DsError.ThrowExceptionForHR(hr);

			if (hr >= 0 && pEnum != null)
			{
				try
				{
					try
					{
						// Enumerate the monikers.
						IMoniker[] pMoniker = new IMoniker[1];
						while (pEnum.Next(1, pMoniker, IntPtr.Zero) == 0)
						{
							try
							{
								// The devs array now owns this object.  Don't
								// release it if we are going to be successfully
								// returning the devret array
								devs.Add(new DsDevice(pMoniker[0]));
							}
							catch
							{
								Marshal.ReleaseComObject(pMoniker[0]);
								throw;
							}
						}
					}
					finally
					{
						// Clean up.
						Marshal.ReleaseComObject(pEnum);
					}


					// Copy the ArrayList to the DsDevice[]
					devret = new DsDevice[devs.Count];
					devs.CopyTo(devret);
				}
				catch
				{
					foreach (DsDevice d in devs)
					{
						d.Dispose();
					}
					throw;
				}
			}
			else
			{
				devret = new DsDevice[0];
			}

			Marshal.ReleaseComObject(pMapper);

			return devret;
		}

		//public static DsDevice GetVideoCaptureDeviceByName(string deviceName)
		//{
		//    DsDevice devret = null;

		//    IFilterMapper2 pMapper = (IFilterMapper2)new FilterMapper2();

		//    Guid[] arrayInTypes = new Guid[2] { MediaType.Video, MediaSubType.Null };
		//    IEnumMoniker pEnum = null;
		//    int hr = pMapper.EnumMatchingFilters(out pEnum,
		//            0,                  // Reserved.
		//            true,               // Use exact match?
		//            (Merit)((int)Merit.DoNotUse + 1), // Minimum merit.
		//            false,              // At least one input pin?
		//            0,                  // Number of major type/subtype pairs for input.
		//            null,			    // Array of major type/subtype pairs for input.
		//            null,               // Input medium.
		//            null,               // Input pin category.
		//            false,              // Must be a renderer?
		//            true,               // At least one output pin?
		//            0,//1,                  // Number of major type/subtype pairs for output.
		//            null, //arrayInTypes,       // Array of major type/subtype pairs for output.
		//            null,               // Output medium.
		//            null);              // Output pin category.

		//    DsError.ThrowExceptionForHR(hr);

		//    if (hr >= 0 && pEnum != null)
		//    {
		//        try
		//        {
		//            // Enumerate the monikers.
		//            IMoniker[] pMoniker = new IMoniker[1];
		//            while (pEnum.Next(1, pMoniker, IntPtr.Zero) == 0)
		//            {
		//                DsDevice device = new DsDevice(pMoniker[0]);
		//                System.Diagnostics.Trace.WriteLineIf(trace.TraceVerbose, device.Name);
		//                if (deviceName == device.Name)
		//                {
		//                    devret = device;
		//                    break;
		//                }
		//                else
		//                    Marshal.ReleaseComObject(pMoniker[0]);
		//            }
		//        }
		//        finally
		//        {
		//            // Clean up.
		//            Marshal.ReleaseComObject(pEnum);
		//        }
		//    }

		//    Marshal.ReleaseComObject(pMapper);

		//    return devret;
		//}

		//public void FreeMediaType(AMMediaType mt)
		//{
		//    if (mt.formatSize != 0)
		//    {
		//        Marshal.FreeCoTaskMem(mt.formatPtr);
		//        mt.formatSize = 0;
		//        mt.formatPtr = IntPtr.Zero;
		//    }
		//    if (mt.unkPtr != IntPtr.Zero)
		//    {
		//        // Unecessary because pUnk should not be used, but safest.
		//        Marshal.ReleaseComObject(mt.unkPtr);
		//        mt.unkPtr = IntPtr.Zero;
		//    }
		//}

		//public void DeleteMediaType(AMMediaType pmt)
		//{
		//    if (pmt != null)
		//    {
		//        FreeMediaType(pmt);
		//        Marshal.FreeCoTaskMem(pmt);
		//    }
		//}

		private static Hashtable hashtableMediaTypeByGUID = null;

		public static Hashtable MediaTypeByGUID
		{
			get
			{
				if (hashtableMediaTypeByGUID == null)
				{
					hashtableMediaTypeByGUID = new Hashtable();

					Type type = typeof(DirectShowLib.MediaType);
					MemberInfo[] memberInfo = type.GetMembers(BindingFlags.Public | BindingFlags.Static);

					foreach (MemberInfo mi in memberInfo)
					{
						if (mi.MemberType == MemberTypes.Field && ((FieldInfo)mi).FieldType == typeof(Guid))
						{
							FieldInfo fieldInfo = type.GetField(mi.Name, BindingFlags.Public | BindingFlags.Static);
							hashtableMediaTypeByGUID.Add(fieldInfo.GetValue(null), mi.Name);
						}
					}
				}

				return hashtableMediaTypeByGUID;
			}
		}

		private static Hashtable hashtableMediaSubTypeByGUID = null;

		public static Hashtable MediaSubTypeByGUID
		{
			get
			{
				if (hashtableMediaSubTypeByGUID == null)
				{
					hashtableMediaSubTypeByGUID = new Hashtable();

					Type type = typeof(DirectShowLib.MediaSubType);
					MemberInfo[] memberInfo = type.GetMembers(BindingFlags.Public | BindingFlags.Static);

					foreach (MemberInfo mi in memberInfo)
					{
						if (mi.MemberType == MemberTypes.Field && ((FieldInfo)mi).FieldType == typeof(Guid))
						{
							FieldInfo fieldInfo = type.GetField(mi.Name, BindingFlags.Public | BindingFlags.Static);
                            try
                            {
                                hashtableMediaSubTypeByGUID.Add(fieldInfo.GetValue(null), mi.Name);
                            }
                            catch (Exception)
                            {
                            }
						}
					}
				}

				return hashtableMediaSubTypeByGUID;
			}
		}

		public static void GetDesinterlaceMode()
		{
		}

		public static void GetDesinterlaceMode(IVMRDeinterlaceControl9 pDeinterlace)
		{
			VMR9VideoDesc VideoDesc = new VMR9VideoDesc();
			int dwNumModes = 0;
			// Fill in the VideoDesc structure (not shown).
			int hr = pDeinterlace.GetNumberOfDeinterlaceModes(ref VideoDesc, ref dwNumModes, null);
			if (hr >= 0 && dwNumModes != 0)
			{
				// Allocate an array for the GUIDs that identify the modes.
				Guid[] pModes = new Guid[dwNumModes];
				if (pModes != null)
				{
					// Fill the array.
					hr = pDeinterlace.GetNumberOfDeinterlaceModes(ref VideoDesc, ref dwNumModes, pModes);
					if (hr >= 0)
					{
						// Loop through each item and get the capabilities.
						for (int i = 0; i < dwNumModes; i++)
						{
							VMR9DeinterlaceCaps Caps = new VMR9DeinterlaceCaps();
							hr = pDeinterlace.GetDeinterlaceModeCaps(pModes[i], ref VideoDesc, ref Caps);
							if (hr >= 0)
							{
								// Examine the Caps structure.
							}
						}
					}
				}
			}
		}
	}


	public class HashtableSerializationProxy : ICollection
	{
		private Hashtable _hashTable;
		private IDictionaryEnumerator _enumerator = null;
		private int _position = -1;

		public HashtableSerializationProxy(Hashtable ht)
		{
			_hashTable = ht;
			_position = -1;
		}

		public Hashtable EmbeddedHashTable
		{
			get { return _hashTable; }
			set { _hashTable = value; }
		}

		// Serialization: XmlSerializer uses this one to get one item at the time
		public DictionaryEntry this[int index]
		{
			get
			{
				if (_enumerator == null)  // lazy initialization
					_enumerator = _hashTable.GetEnumerator();

				// Accessing an item that is before the current position is something that 
				// shouldn't normally happen because XmlSerializer calls indexer with a constantly 
				// increasing index that starts from zero. 
				// Trying to go backward requires the reset of the enumerator, followed by appropriate 
				// number of increments. (Enumerator acts as a forward-only iterator.)
				if (index < _position)
				{
					_enumerator.Reset();
					_position = -1;
				}

				while (_position < index)
				{
					_enumerator.MoveNext();
					_position++;
				}
				return _enumerator.Entry;
			}
		}
		// Deserialization: XmlSerializer uses this one to write content back
		public void Add(DictionaryEntry de)
		{
			_hashTable[de.Key] = de.Value;
		}

		// The rest is a simple redirection to Hashtable's ICollection implementation
		public int Count { get { return _hashTable.Count; } }
		public bool IsSynchronized { get { return _hashTable.IsSynchronized; } }
		public object SyncRoot { get { return _hashTable.SyncRoot; } }
		public void CopyTo(Array array, int index) { _hashTable.CopyTo(array, index); }
		public IEnumerator GetEnumerator() { return _hashTable.GetEnumerator(); }
	}

	public enum AM_MPEG2Profile
	{
		Simple = 1,
		Main,
		SNRScalable,
		SpatiallyScalable,
		High
	}

	public enum AM_MPEG2Level
	{
		Low = 1,
		Main,
		High1440,
		High
	}

	[Flags]
	public enum AMMPEG2
	{
		DoPanScan			= 0x00000001,
		DVDLine21Field1		= 0x00000002,
		DVDLine21Field2		= 0x00000004,
		SourceIsLetterboxed	= 0x00000008,
		FilmCameraMode		= 0x00000010,
		LetterboxAnalogOut	= 0x00000020,
		DSS_UserData		= 0x00000040,
		DVB_UserData		= 0x00000080,
		Timebase27Mhz		= 0x00000100,
		WidescreenAnalogOut	= 0x00000200
	}

	/// <summary>
    /// From MPEG2VIDEOINFO
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public class MPEG2VideoInfo
	{
		public VideoInfoHeader2 hdr;
		public uint StartTimeCode;
		public uint SequenceHeader;
		public AM_MPEG2Profile Profile;
		public AM_MPEG2Level Level;
		public AMMPEG2 Flags;            
    }

	/// <summary>
    /// From MPEG1WAVEFORMAT
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=2)]
	public class MPEG1WaveFormat //: WaveFormatEx // produce a bug in ngen 
	{
		// WaveFormatEx
		public short wFormatTag;
		public short nChannels;
		public int nSamplesPerSec;
		public int nAvgBytesPerSec;
		public short nBlockAlign;
		public short wBitsPerSample;
		public short cbSize;

		// MPEG1WaveFormat
		public ushort HeadLayer;
		public uint HeadBitrate;
		public ushort HeadMode;
		public ushort HeadModeExt;
		public ushort HeadEmphasis;
		public ushort HeadFlags;
		public uint PTSLow;
		public uint PTSHigh;
	}

	public enum VideoSizeMode { FromInside, FromOutside, Free, StretchToWindow }
	public enum VideoMode { Normal, TV, Fullscreen };

	public interface ITV
	{
		IFilterGraph2 FilterGraph { get; }
		IBaseFilter AudioRenderer { get; }
		IBaseFilter VideoRenderer { get; }

		Channel CurrentChannel { get; set; }

		//void BuildGraphWithNoRenderer();
		void BuildGraph();
		void SubmitTuneRequest(Channel channel);

		FilterState GetGraphState();
		void RunGraph();
		void PauseGraph();
		void StopGraph();
		void SaveGraph(string filepath);

		bool GetSignalStatistics(out bool locked, out bool present, out int strength, out int quality);
	}

	public interface IBDA
	{
		DsDevice TunerDevice { get; set; }
		DsDevice CaptureDevice { get; set; }

		IBaseFilter NetworkProvider { get; }
		IBaseFilter TunerFilter { get; }
		IBaseFilter CaptureFilter { get; }
		IBaseFilter SectionsAndTables { get; }
		IBaseFilter TransportInformationFilter { get; }

		ITuningSpace TuningSpace { get; set; }
		ITuneRequest TuneRequest { get; set; }

		string GetTablesInfos(Channel channel, bool allTransponderInfo);
	}

	public interface IWDM
	{
		DsDevice VideoCaptureDevice { get; set; }
		DsDevice AudioCaptureDevice { get; set; }

		IBaseFilter VideoCaptureFilter { get; }
		IBaseFilter AudioCaptureFilter { get; }
		IAMTVTuner Tuner { get; }
		IBaseFilter Crossbar { get; }
	}

	public interface IEPG
	{
		EPG EPG { get; }
	}

	public enum TimeShiftingStatus { Recording, Paused }

	public interface ITimeShifting
	{
		TimeShiftingStatus Status { get; }
		void Pause();
		void Resume();

		void GetPositions(out TimeSpan start, out TimeSpan stop);
		TimeSpan GetPosition();
		void SetPosition(TimeSpan position);

		double GetRate();
		void SetRate(double rate);
	}

	public enum RecorderStatus { Recording, Stopped }

	public interface IRecorder
	{
		RecorderStatus Status { get; }
		void Start(string filename);
		void Stop();
		void Play(string filename);
		TimeSpan GetDuration();
	}

	public enum PlayerStatus { Playing, Paused, Stopped }

	public interface IPlayer
	{
		PlayerStatus Status { get; }
		string FileName { get; }
		void Play();
		void Pause();
		void Stop();
		TimeSpan GetPosition();
		void SetPosition(TimeSpan position);
		TimeSpan GetDuration();

		double GetRate();
		void SetRate(double rate);
	}
}
