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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using DirectShowLib;
using DirectShowLib.BDA;

namespace CodeTV
{
	// 'ONID:TSID:SID' = '8442:6:1537'

	//IGuideData.GetServices()
	//System.__ComObject
	//  name: 'Description.ID', value: '8442:6:1537'
	//  name: 'Description.Name', value: 'TF1'
	//  name: 'Provider.Name', value: ''
	//  name: 'Provider.NetworkName', value: 'Réseau Numérique Terrestre Français'

	//IGuideData.GetScheduleEntryIDs()
	//8442:6:1537:46
	//  name: 'Description.ID', value: '8442:6:1537:46'
	//  name: 'Time.Start', value: '830198865'
	//  name: 'Time.End', value: '830204878'
	//  name: 'ScheduleEntry.ProgramID', value: '8442:6:1537:46'
	//  name: 'ScheduleEntry.ServiceID', value: '8442:6:1537'
	//8442:6:1537:47
	//  name: 'Description.ID', value: '8442:6:1537:47'
	//  name: 'Time.Start', value: '830204878'
	//  name: 'Time.End', value: '830213733'
	//  name: 'ScheduleEntry.ProgramID', value: '8442:6:1537:47'
	//  name: 'ScheduleEntry.ServiceID', value: '8442:6:1537'

	//IGuideData.GetGuideProgramIDs()
	//8442:6:1537:46
	//  name: 'Description.ID', value: '8442:6:1537:46'
	//  name: 'Description.Title', value: 'Navarro'
	//  name: 'Description.One Sentence', value: ''
	//8442:6:1537:47
	//  name: 'Description.ID', value: '8442:6:1537:47'
	//  name: 'Description.Title', value: 'La méthode Cauet'
	//  name: 'Description.One Sentence', value: ''


	public class EPGProvider
	{
		string name;		//  name: 'Provider.Name', value: ''
		string networkName;	//  name: 'Provider.NetworkName', value: 'Réseau Numérique Terrestre Français'

		public EPGProvider() {}

		public string Name { get { return this.name; } set { this.name = value; } }
		public string NetworkName { get { return this.networkName; } set { this.networkName = value; } }
	}

	public class EPGService
	{
		string providerName;
		string serviceId;	//  name: 'Description.ID', value: '8442:6:1537'
		string name;		//  name: 'Description.Name', value: 'TF1'
		Hashtable programs = new Hashtable();

		public EPGService() {}

		public string ProviderName { get { return this.providerName; } set { this.providerName = value; } }
		public string ServiceId { get { return this.serviceId; } set { this.serviceId = value; } }
		public string Name { get { return this.name; } set { this.name = value; } }
		public Hashtable Programs { get { return this.programs; } }
	}

	public class EPGProgram
	{
		string serviceId;
		string programId;	//  name: 'Description.ID', value: '8442:6:1537:46'
		string title;		//  name: 'Description.Title', value: 'Navarro'
		string oneSentence;	//  name: 'Description.One Sentence', value: ''
		DateTime timeStart;	//  name: 'Time.Start', value: '830198865'
		DateTime timeEnd;	//  name: 'Time.End', value: '830204878'
		bool needProgramUpdate = true;
		bool needScheduleUpdate = true;

		public EPGProgram() { }

		public string ServiceId { get { return this.serviceId; } set { this.serviceId = value; } }
		public string ProgramId { get { return this.programId; } set { this.programId = value; } }
		public string Title { get { return this.title; } set { this.title = value; } }
		public string OneSentence { get { return this.oneSentence; } set { this.oneSentence = value; } }
		public DateTime TimeStart { get { return this.timeStart; } set { this.timeStart = value; } }
		public DateTime TimeEnd { get { return this.timeEnd; } set { this.timeEnd = value; } }
		public bool NeedProgramUpdate { get { return this.needProgramUpdate; } set { this.needProgramUpdate = value; } }
		public bool NeedScheduleUpdate { get { return this.needScheduleUpdate; } set { this.needScheduleUpdate = value; } }
	}

    public interface IGuideDataEvent // Deprecated!!!!!!!!!!!
    {
    }

    public interface IGuideData
    {
    }
    public interface IEnumGuideDataProperties
    {
    }
	public interface IGuideDataProperty
    {
    }

	public class EPG : IGuideDataEvent
	{
		public static TraceSwitch trace = new TraceSwitch("EPG", "EPG traces", "Info");

		Hashtable providers = new Hashtable();
		Hashtable services = new Hashtable();
		Hashtable programs = new Hashtable();

		int guideDataEventCookie = -1;
		IConnectionPoint guideDataEventConnectionPoint;

		Control control;

		public EPG(Control control)
		{
			this.control = control;
		}

		public void RegisterEvent(IConnectionPointContainer bdaTIFConnectionPointContainer)
		{
            //http://msdn.microsoft.com/en-us/library/ms779696
			try
			{
				Guid IID_IGuideDataEvent = typeof(IGuideDataEvent).GUID;
				bdaTIFConnectionPointContainer.FindConnectionPoint(ref IID_IGuideDataEvent, out this.guideDataEventConnectionPoint);
				if (this.guideDataEventConnectionPoint != null)
				{
					guideDataEventConnectionPoint.Advise(this, out this.guideDataEventCookie);
				}
			}
			catch (Exception ex)
			{
				// CONNECT_E_CANNOTCONNECT = 0x80040200
				Trace.WriteLineIf(trace.TraceError, ex.ToString());
			}
		}

		public void UnRegisterEvent()
		{
			if (this.guideDataEventConnectionPoint != null && this.guideDataEventCookie != -1)
			{
				try
				{
					this.guideDataEventConnectionPoint.Unadvise(this.guideDataEventCookie);
				}
				catch (Exception ex)
				{
					Trace.WriteLineIf(trace.TraceError, ex.ToString());
				}
			}
		}

		public string GetServiceIdFromProgramId(string programId)
		{
			int pos = programId.LastIndexOf(':');
			if (pos != -1)
				return programId.Substring(0, pos);
			else
				return programId;
		}

		public EPGProvider GetProvider(string providerId)
		{
			return (EPGProvider)providers[providerId];
		}

		public EPGService GetService(string serviceId)
		{
			return (EPGService)services[serviceId];
		}

		public EPGProgram GetProgram(string programId)
		{
			return (EPGProgram)programs[programId];
		}

		public bool NeedServiceUpdate(string serviceId)
		{
			return services[serviceId] != null;
		}

		public bool NeedProgramUpdate(string programId)
		{
			EPGProgram program = (EPGProgram)programs[programId];
			if (program != null)
				return program.NeedProgramUpdate;
			else
				return true;
		}

		public bool NeedScheduleUpdate(string programId)
		{
			EPGProgram program = (EPGProgram)programs[programId];
			if (program != null)
				return program.NeedScheduleUpdate;
			else
				return true;
		}

		public EPGProgram GetCurrentProgram(string serviceId)
		{
			DateTime dateTime = DateTime.Now;
			EPGService service = GetService(serviceId);
			if (service != null)
			{
				ICollection values = service.Programs.Values;
				foreach (EPGProgram program in values)
				{
					if (dateTime > program.TimeStart && dateTime < program.TimeEnd)
						return program;
				}
			}
			return null;
		}

		// GPS time was zero at 0h 6-Jan-1980
		// and since it is not perturbed by leap seconds GPS is now ahead of UTC by 14 seconds.
		private DateTime GPSTimeToDateTime(int gpsTime)
		{
			int seconds, minutes, hours = Math.DivRem(gpsTime, 3600, out minutes);
			minutes = Math.DivRem(minutes, 60, out seconds);
			TimeSpan ts = new TimeSpan(hours, minutes, seconds);
			DateTime dt = new DateTime(1980, 1, 6, 0, 0, 0);
			return TimeZone.CurrentTimeZone.ToLocalTime(dt + ts);
		}

		//ServiceChanged(object varProgramDescriptionID)
		//ProgramChanged(object varProgramDescriptionID)
		//ScheduleEntryChanged(object varProgramDescriptionID)


		public void UpdateProgram(string programmId, IGuideData guideData)
		{
			Trace.WriteLineIf(trace.TraceInfo, "EPG.UpdateProgram(" + programmId + ")");

            //if (programmId == null)
            //    UpdateAllProgram(guideData);
            //else
            //{
            //    if (guideData != null && NeedProgramUpdate(programmId))
            //    {
            //        IEnumGuideDataProperties ppEnumProperties;
            //        int hr = guideData.GetProgramProperties(programmId, out ppEnumProperties);
            //        if (ppEnumProperties != null)
            //        {
            //            Hashtable hashtable = new Hashtable();
            //            IGuideDataProperty[] ppProp = new IGuideDataProperty[1];
            //            //while (ppEnumProperties.Next(1, out ppProp, out pcelt) == 0 && pcelt > 0)
            //            //22 int pcelt = 1;
            //            //22 while (ppEnumProperties.Next(1, ppProp, out pcelt) == 0 && pcelt > 0)
            //            IntPtr p = Marshal.AllocCoTaskMem(4);
            //            try
            //            {
            //                //x = Marshal.ReadInt32(p);
            //                while (ppEnumProperties.Next(1, ppProp, p) == 0)
            //                {
            //                    string name;
            //                    ppProp[0].get_Name(out name);
            //                    int language;
            //                    ppProp[0].get_Language(out language);
            //                    object value;
            //                    ppProp[0].get_Value(out value);

            //                    hashtable[name] = value;

            //                    Trace.WriteLineIf(trace.TraceVerbose, "  name: '" + name + "', value: '" + value.ToString() + "', language: " + language);

            //                    Marshal.ReleaseComObject(ppProp[0]);
            //                    ppProp[0] = null;
            //                }
            //            }
            //            finally
            //            {
            //                Marshal.FreeCoTaskMem(p);
            //            }

            //            // Character seems to be encoded in ISO/IEC 6937
            //            // http://en.wikipedia.org/wiki/ISO/IEC_6937

            //            //IGuideData.GetGuideProgramIDs()
            //            //8442:6:1537:46
            //            //  name: 'Description.ID', value: '8442:6:1537:46'
            //            //  name: 'Description.Title', value: 'Navarro'
            //            //  name: 'Description.One Sentence', value: ''
            //            //8442:6:1537:47
            //            //  name: 'Description.ID', value: '8442:6:1537:47'
            //            //  name: 'Description.Title', value: 'La méthode Cauet'
            //            //  name: 'Description.One Sentence', value: ''

            //            object o = hashtable["Description.ID"];
            //            if (o != null)
            //            {
            //                EPGProgram program = (EPGProgram)programs[o];
            //                if (program == null)
            //                {
            //                    program = new EPGProgram();
            //                    program.ProgramId = (string)hashtable["Description.ID"];
            //                    program.Title = (string)hashtable["Description.Title"];
            //                    program.OneSentence = (string)hashtable["Description.One Sentence"];
            //                    program.NeedProgramUpdate = false;
            //                    programs[program.ProgramId] = program;
            //                }
            //            }

            //            ppEnumProperties = null;
            //        }
            //    }
            //}
		}

		public void UpdateAllProgram(IGuideData guideData)
		{
			Trace.WriteLineIf(trace.TraceInfo, "EPG.UpdateAllProgram()");

            //if (guideData != null)
            //{
            //    IEnumVARIANT pEnumPrograms;
            //    guideData.GetGuideProgramIDs(out pEnumPrograms);
            //    //pEnumPrograms.Reset();
            //    if (pEnumPrograms != null)
            //    {
            //        object[] varProgramEntryId = new object[1];
            //        //unsafe
            //        {
            //            //22 int fetched = 0;
            //            //22 IntPtr pFetched = new IntPtr(&fetched); //new IntPtr(fetched); // IntPtr.Zero; // new IntPtr(&fetched);
            //            IntPtr p = Marshal.AllocCoTaskMem(4);
            //            try
            //            {
            //                while (pEnumPrograms.Next(1, varProgramEntryId, p) == 0)
            //                {
            //                    Trace.WriteLineIf(trace.TraceVerbose, varProgramEntryId[0].ToString());
            //                    UpdateProgram((string)varProgramEntryId[0], guideData);
            //                    varProgramEntryId[0] = null;
            //                }
            //            }
            //            finally
            //            {
            //                Marshal.FreeCoTaskMem(p);
            //            }
            //        }
            //        pEnumPrograms = null;
            //    }
            //}
		}

		public void UpdateSchedule(string programmId, IGuideData guideData)
		{
			Trace.WriteLineIf(trace.TraceInfo, "EPG.UpdateSchedule(" + programmId + ")");
            //if (programmId == null)
            //    UpdateAllSchedule(guideData);
            //else
            //{
            //    if (guideData != null && NeedScheduleUpdate(programmId))
            //    {
            //        IEnumGuideDataProperties ppEnumProperties;
            //        int hr = guideData.GetScheduleEntryProperties(programmId, out ppEnumProperties);
            //        if (ppEnumProperties != null)
            //        {
            //            Hashtable hashtable = new Hashtable();
            //            IGuideDataProperty[] ppProp = new IGuideDataProperty[1];
            //            //22 int pcelt = 1;
            //            //22 while (ppEnumProperties.Next(1, ppProp, out pcelt) == 0 && pcelt > 0)
            //            IntPtr p = Marshal.AllocCoTaskMem(4);
            //            try
            //            {
            //                while (ppEnumProperties.Next(1, ppProp, p) == 0)
            //                {
            //                    int language;
            //                    ppProp[0].get_Language(out language);
            //                    string name;
            //                    ppProp[0].get_Name(out name);
            //                    object value;
            //                    ppProp[0].get_Value(out value);

            //                    hashtable[name] = value;

            //                    Trace.WriteLineIf(trace.TraceVerbose, "  name: '" + name + "', value: '" + value.ToString() + "', language: " + language);

            //                    Marshal.ReleaseComObject(ppProp[0]);
            //                    ppProp[0] = null;
            //                    //ppProp = null;
            //                }
            //            }
            //            finally
            //            {
            //                Marshal.FreeCoTaskMem(p);
            //            }

            //            //IGuideData.GetScheduleEntryIDs()
            //            //8442:6:1537:46
            //            //  name: 'Description.ID', value: '8442:6:1537:46'
            //            //  name: 'Time.Start', value: '830198865'
            //            //  name: 'Time.End', value: '830204878'
            //            //  name: 'ScheduleEntry.ProgramID', value: '8442:6:1537:46'
            //            //  name: 'ScheduleEntry.ServiceID', value: '8442:6:1537'
            //            //8442:6:1537:47
            //            //  name: 'Description.ID', value: '8442:6:1537:47'
            //            //  name: 'Time.Start', value: '830204878'
            //            //  name: 'Time.End', value: '830213733'
            //            //  name: 'ScheduleEntry.ProgramID', value: '8442:6:1537:47'
            //            //  name: 'ScheduleEntry.ServiceID', value: '8442:6:1537'

            //            object o = hashtable["ScheduleEntry.ProgramID"];
            //            if (o != null)
            //            {
            //                EPGProgram program = (EPGProgram)programs[o];
            //                if (program == null)
            //                {
            //                    program = new EPGProgram();
            //                    program.ProgramId = (string)hashtable["ScheduleEntry.ProgramID"];
            //                    programs[program.ProgramId] = program;
            //                }

            //                EPGService service = (EPGService)services[hashtable["ScheduleEntry.ServiceID"]];
            //                if (service == null)
            //                {
            //                    service = new EPGService();
            //                    service.ServiceId = (string)hashtable["ScheduleEntry.ServiceID"];
            //                    services[service.ServiceId] = service;
            //                }

            //                program.TimeStart = GPSTimeToDateTime((int)hashtable["Time.Start"]);
            //                program.TimeEnd = GPSTimeToDateTime((int)hashtable["Time.End"]);
            //                program.ServiceId = service.ServiceId;
            //                program.NeedScheduleUpdate = false;

            //                service.Programs[program.ProgramId] = program;

            //                //EPGProgram program = (EPGProgram)programs[o];
            //                //if (program == null)
            //                //{
            //                //    program = new EPGProgram();
            //                //    program.ProgramId = (string)hashtable["ScheduleEntry.ProgramID"];
            //                //    programs[program.ProgramId] = program;
            //                //}

            //                //EPGService service = (EPGService)services[hashtable["ScheduleEntry.ServiceID"]];
            //                //if (service == null)
            //                //{
            //                //    service = new EPGService();
            //                //    service.ServiceId = (string)hashtable["ScheduleEntry.ServiceID"];
            //                //    services[service.ServiceId] = service;
            //                //    service.Programs[program.ProgramId] = program;
            //                //}

            //                //program.TimeStart = GPSTimeToDateTime((int)hashtable["Time.Start"]);
            //                //program.TimeEnd = GPSTimeToDateTime((int)hashtable["Time.End"]);
            //                //program.ServiceId = service.ServiceId;
            //                //program.NeedScheduleUpdate = false;
            //            }

            //            ppEnumProperties = null;
            //        }
            //    }
            //}
		}

		public void UpdateAllSchedule(IGuideData guideData)
		{
			Trace.WriteLineIf(trace.TraceInfo, "EPG.UpdateAllSchedule()");

            //if (guideData != null)
            //{
            //    IEnumVARIANT pEnumPrograms;
            //    guideData.GetScheduleEntryIDs(out pEnumPrograms);
            //    //pEnumPrograms.Reset();
            //    if (pEnumPrograms != null)
            //    {
            //        object[] varScheduleEntryDescriptionID = new object[1];
            //        //unsafe
            //        {
            //            //22 int fetched = 0;
            //            //22 IntPtr pFetched = new IntPtr(&fetched); //new IntPtr(fetched); // IntPtr.Zero; // new IntPtr(&fetched);
            //            //22 int hr = 0;
            //            IntPtr p = Marshal.AllocCoTaskMem(4);
            //            try
            //            {
            //                while (pEnumPrograms.Next(1, varScheduleEntryDescriptionID, p) == 0)
            //                {
            //                    Trace.WriteLineIf(trace.TraceVerbose, varScheduleEntryDescriptionID[0].ToString());
            //                    UpdateSchedule((string)varScheduleEntryDescriptionID[0], guideData);
            //                    varScheduleEntryDescriptionID[0] = null;
            //                }
            //            }
            //            finally
            //            {
            //                Marshal.FreeCoTaskMem(p);
            //            }
            //        }
            //        pEnumPrograms = null;
            //    }
            //}
		}

		public void UpdateService(string serviceId, ITuningSpace tuningSpace, IGuideData guideData)
		{
			Trace.WriteLineIf(trace.TraceInfo, "EPG.UpdateService(" + serviceId + ")");
			if (serviceId == null)
				UpdateAllService(guideData);
			else if (tuningSpace != null && NeedServiceUpdate(serviceId))
			{
				// serviceId = "ONID:TSID:SID"
				string[] serviceIds = serviceId.Split(new char[] { ':' });
				int onid = int.Parse(serviceIds[0]);
				int tsid = int.Parse(serviceIds[1]);
				int sid = int.Parse(serviceIds[2]);

				ITuneRequest tr;
				tuningSpace.CreateTuneRequest(out tr);
				IDVBTuneRequest tuneRequest = (IDVBTuneRequest)tr;
				tuneRequest.put_ONID(onid);
				tuneRequest.put_TSID(tsid);
				tuneRequest.put_SID(sid);
				UpdateService(tuneRequest, guideData);
			}
		}

		public void UpdateService(ITuneRequest tuneRequest, IGuideData guideData)
		{
			Trace.WriteLineIf(trace.TraceInfo, "EPG.UpdateService(" + tuneRequest.ToString() + ")");
            //if (guideData != null)
            //{
            //    IEnumGuideDataProperties ppEnumProperties;
            //    int hr = guideData.GetServiceProperties(tuneRequest, out ppEnumProperties);
            //    if (ppEnumProperties != null)
            //    {
            //        Hashtable hashtable = new Hashtable();
            //        IGuideDataProperty[] ppProp = new IGuideDataProperty[1];
            //        //22 int pcelt = 1;
            //        //22 while (ppEnumProperties.Next(1, ppProp, out pcelt) == 0 && pcelt > 0)
            //        IntPtr p = Marshal.AllocCoTaskMem(4);
            //        try
            //        {
            //            while (ppEnumProperties.Next(1, ppProp, p) == 0)
            //            {
            //                int language;
            //                ppProp[0].get_Language(out language);
            //                string name;
            //                ppProp[0].get_Name(out name);
            //                object value;
            //                ppProp[0].get_Value(out value);

            //                //  name: 'Description.ID', value: '8442:6:1537'
            //                //  name: 'Description.Name', value: 'TF1'
            //                //  name: 'Provider.Name', value: ''
            //                //  name: 'Provider.NetworkName', value: 'Réseau Numérique Terrestre Français'

            //                hashtable[name] = value;

            //                Trace.WriteLineIf(trace.TraceVerbose, "  name: '" + name + "', value: '" + value.ToString() + "', language: " + language);
            //                Marshal.ReleaseComObject(ppProp[0]);
            //                ppProp[0] = null;
            //            }
            //        }
            //        finally
            //        {
            //            Marshal.FreeCoTaskMem(p);
            //        }

            //        EPGProvider provider = (EPGProvider)providers[hashtable["Provider.Name"]];
            //        if (provider == null)
            //        {
            //            provider = new EPGProvider();
            //            provider.Name = (string)hashtable["Provider.Name"];
            //            provider.NetworkName = (string)hashtable["Provider.NetworkName"];
            //            providers[provider.Name] = provider;
            //        }
            //        EPGService service = (EPGService)services[hashtable["Description.ID"]];
            //        if (service == null)
            //        {
            //            service = new EPGService();
            //            service.ServiceId = (string)hashtable["Description.ID"];
            //            services[service.ServiceId] = service;
            //        }
            //        service.Name = (string)hashtable["Description.Name"];
            //        service.ProviderName = provider.Name;

            //        Marshal.ReleaseComObject(ppEnumProperties);
            //    }
            //}
		}

		public void UpdateAllService(IGuideData guideData)
		{
			Trace.WriteLineIf(trace.TraceInfo, "EPG.UpdateAllService()");
            //if (guideData != null)
            //{
            //    IEnumTuneRequests ppEnumTuneRequests;
            //    guideData.GetServices(out ppEnumTuneRequests);
            //    if (ppEnumTuneRequests != null)
            //    {
            //        ITuneRequest[] tuneRequest = new ITuneRequest[1];
            //        //22 int fetched = 0;
            //        //22 while (ppEnumTuneRequests.Next(1, tuneRequest, out fetched) == 0)
            //        IntPtr p = Marshal.AllocCoTaskMem(4);
            //        try
            //        {
            //            while (ppEnumTuneRequests.Next(1, tuneRequest, p) == 0)
            //            {
            //                Trace.WriteLineIf(trace.TraceVerbose, tuneRequest[0].ToString());
            //                UpdateService(tuneRequest[0], guideData);
            //                Marshal.ReleaseComObject(tuneRequest[0]);
            //            }
            //        }
            //        finally
            //        {
            //            Marshal.FreeCoTaskMem(p);
            //        }
            //        Marshal.ReleaseComObject(ppEnumTuneRequests);
            //        tuneRequest = null;
            //    }
            //}
		}

		//public event EventHandler UpdateAsynchroneEnded;

		//public void UpdateAsynchrone(IGuideData guideData)
		//{
		//    Trace.WriteLineIf(trace.TraceInfo, "UpdateAsynchrone() threadid=" + Thread.CurrentThread.ManagedThreadId);
		//    UpdateDelegate updateDelegate = new UpdateDelegate(this.Update);
		//    updateDelegate.BeginInvoke(guideData, new AsyncCallback(UpdateAsynchroneFinished), updateDelegate);
		//}

		//private void UpdateAsynchroneFinished(IAsyncResult ar)
		//{
		//    Trace.WriteLineIf(trace.TraceInfo, "UpdateAsynchroneFinished() threadid=" + Thread.CurrentThread.ManagedThreadId);

		//    UpdateDelegate caller = (UpdateDelegate)ar.AsyncState;
		//    //caller.EndInvoke(ar);

		//    if (UpdateAsynchroneEnded != null)
		//        UpdateAsynchroneEnded(this, new EventArgs());
		//}

		//private delegate void UpdateDelegate(IGuideData guideData);
		public void UpdateAll(IGuideData guideData)
		{
			Trace.WriteLineIf(trace.TraceInfo, "UpdateAll() threadid=" + Thread.CurrentThread.ManagedThreadId);

			UpdateAllService(guideData);
			UpdateAllProgram(guideData);
			UpdateAllSchedule(guideData);
		}

		public enum GuideDataEventType { GuideDataAcquired, ProgramChanged, ServiceChanged, ScheduleEntryChanged, ProgramDeleted, ServiceDeleted, ScheduleDeleted };

		public class GuideDataEventArgs : EventArgs
		{
			private GuideDataEventType type; 
			private string identifier;

			public GuideDataEventArgs(GuideDataEventType type, string identifier)
			{
				this.type = type;
				this.identifier = identifier;
			}

			public GuideDataEventType Type { get { return this.type; } }
			public string Identifier { get { return this.identifier; } }
		}

		public delegate void GuideDataEventHandler(object sender, GuideDataEventArgs fe);
		public event GuideDataEventHandler GuideDataEvent;

		private delegate void GuideDataEventDelegate(GuideDataEventType type, string identifier);
		private void AsynchroneEventCallback(GuideDataEventType type, string identifier)
		{
			if (this.control != null && this.control.InvokeRequired)
				this.control.BeginInvoke(new GuideDataEventDelegate(this.GUIThreadAsynchroneEventCallback), new object[] { type, identifier });
			else
				GUIThreadAsynchroneEventCallback(type, identifier);
		}

		private void GUIThreadAsynchroneEventCallback(GuideDataEventType type, string identifier)
		{
			if (GuideDataEvent != null)
				GuideDataEvent(this, new GuideDataEventArgs(type, identifier));
		}

		#region IGuideDataEvent Members

		public int GuideDataAcquired()
		{
			Trace.WriteLineIf(trace.TraceInfo, "EPG.IGuideDataEvent_GuideDataAcquired()");
			(new GuideDataEventDelegate(this.AsynchroneEventCallback)).BeginInvoke(GuideDataEventType.GuideDataAcquired, "", null, null);
			return 0;
		}

		public int ProgramChanged(object varProgramDescriptionID)
		{
			Trace.WriteLineIf(trace.TraceInfo, "EPG.IGuideDataEvent_ProgramChanged(" + varProgramDescriptionID + ")");
			(new GuideDataEventDelegate(this.AsynchroneEventCallback)).BeginInvoke(GuideDataEventType.ProgramChanged, (string)varProgramDescriptionID, null, null);
			return 0;
		}

		public int ServiceChanged(object varServiceDescriptionID)
		{
			Trace.WriteLineIf(trace.TraceInfo, "EPG.IGuideDataEvent_ServiceChanged(" + varServiceDescriptionID + ")");
			(new GuideDataEventDelegate(this.AsynchroneEventCallback)).BeginInvoke(GuideDataEventType.ServiceChanged, (string)varServiceDescriptionID, null, null);
			return 0;
		}

		public int ScheduleEntryChanged(object varProgramDescriptionID)
		{
			Trace.WriteLineIf(trace.TraceInfo, "EPG.IGuideDataEvent_ScheduleEntryChanged(" + varProgramDescriptionID + ")");
			(new GuideDataEventDelegate(this.AsynchroneEventCallback)).BeginInvoke(GuideDataEventType.ScheduleEntryChanged, (string)varProgramDescriptionID, null, null);
			return 0;
		}

		public int ProgramDeleted(object varProgramDescriptionID)
		{
			Trace.WriteLineIf(trace.TraceInfo, "EPG.IGuideDataEvent_ProgramDeleted(" + varProgramDescriptionID + ")");
			(new GuideDataEventDelegate(this.AsynchroneEventCallback)).BeginInvoke(GuideDataEventType.ProgramDeleted, (string)varProgramDescriptionID, null, null);
			return 0;
		}

		public int ServiceDeleted(object varServiceDescriptionID)
		{
			Trace.WriteLineIf(trace.TraceInfo, "EPG.IGuideDataEvent_ServiceDeleted(" + varServiceDescriptionID + ")");
			(new GuideDataEventDelegate(this.AsynchroneEventCallback)).BeginInvoke(GuideDataEventType.ServiceDeleted, (string)varServiceDescriptionID, null, null);
			return 0;
		}

		public int ScheduleDeleted(object varProgramDescriptionID)
		{
			Trace.WriteLineIf(trace.TraceInfo, "EPG.IGuideDataEvent_ScheduleDeleted(" + varProgramDescriptionID + ")");
			(new GuideDataEventDelegate(this.AsynchroneEventCallback)).BeginInvoke(GuideDataEventType.ScheduleDeleted, (string)varProgramDescriptionID, null, null);
			return 0;
		}

		#endregion
	}
}
