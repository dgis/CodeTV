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
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

using DirectShowLib;

namespace CodeTV.PSI
{
	public class PSISection
	{
		private byte tableId;					// 8 bits
		private bool sectionSyntaxIndicator;	//  1 bit
		private bool privateIndicator;			//  1 bit
		private byte reserved;					//  2 bits
		private ushort sectionLength;			// 12 bits

		public virtual void Parse(byte[] data)
		{
			this.tableId = data[0];
			this.sectionSyntaxIndicator = (data[1] >> 7) != 0;
			this.privateIndicator = ((data[1] >> 6) & 0x01) != 0;
			this.reserved = (byte)((data[1] >> 4) & 0x03);
			this.sectionLength = (ushort)(((data[2] & 0x0F) << 8) | data[1]);
		}

		public byte TableId { get { return this.tableId; } set { this.tableId = value; } }
		public bool SectionSyntaxIndicator { get { return this.sectionSyntaxIndicator; } set { this.sectionSyntaxIndicator = value; } }
		public bool PrivateIndicator { get { return this.privateIndicator; } set { this.privateIndicator = value; } }
		public byte Reserved { get { return this.reserved; } set { this.reserved = value; } }
		public ushort SectionLength { get { return this.sectionLength; } set { this.sectionLength = value; } }

		public virtual string ToStringSectionOnly(string prefix)
		{
			string result = "";
			result += prefix + "TableId: " + this.tableId + "\r\n";
			result += prefix + "SectionSyntaxIndicator: " + this.sectionSyntaxIndicator + "\r\n";
			result += prefix + "PrivateIndicator: " + privateIndicator + "\r\n";
			result += prefix + "Reserved: " + reserved + "\r\n";
			result += prefix + "SectionLength: " + sectionLength + "\r\n";
			return result;
		}

		public override string ToString()
		{
			return "PSISection\r\n{\r\n" + ToStringSectionOnly("\t") + "}";
		}

		public static PSISection ParseTable(int tableId, byte[] data)
		{
			PSISection table;
			switch ((TABLE_IDS)tableId)
			{
				case TABLE_IDS.PAT:
					table = new PSIPAT(); break;
				case TABLE_IDS.SDT_ACTUAL:
					table = new PSISDT(); break;
				case TABLE_IDS.PMT:
					table = new PSIPMT(); break;
				case TABLE_IDS.NIT_ACTUAL:
					table = new PSINIT();break;
				default:
					table = new PSISection();
					break;
			}
			table.Parse(data);
			return table;
		}

		public static PSISection[] GetPSITable(int pid, int tableId, IMpeg2Data mpeg2Data)
		{
			ArrayList al = new ArrayList();
			ISectionList ppSectionList;
			int hr = mpeg2Data.GetTable((short)pid, (byte)tableId, null, 5000, out ppSectionList);
			if (ppSectionList != null)
			{
				short pidFound = -1;
				ppSectionList.GetProgramIdentifier(out pidFound);
				if (pidFound == (short)pid)
				{
					byte tableIdFound = 0;
					ppSectionList.GetTableIdentifier(out tableIdFound);
					if (tableIdFound == (byte)tableId)
					{
						short sectionCount = -1;
						ppSectionList.GetNumberOfSections(out sectionCount);

						short sectionNumber = 0;
						for (; sectionNumber < sectionCount; sectionNumber++)
						{
							int pdwRawPacketLength;
							IntPtr ppSection;
							ppSectionList.GetSectionData(sectionNumber, out pdwRawPacketLength, out ppSection);

							byte[] data = new byte[pdwRawPacketLength];
							Marshal.Copy(ppSection, data, 0, pdwRawPacketLength);

							PSISection table = ParseTable(tableId, data);
							Marshal.ReleaseComObject(ppSectionList);

							al.Add(table);
						}
					}
				}
				Marshal.ReleaseComObject(ppSectionList);
			}
			return (PSISection[])al.ToArray(typeof(PSISection));
		}

		public static string ToStringByteArray(byte[] pData, int start, int size, int width, string prefix)
		{
			string result = "";
			string lineHex = "";
			string lineASCII = "";
			int iter = 0, iter2 = 0;
			for (; iter < size; iter++)
			{
				byte b = pData[iter + start];
				lineHex += String.Format("{0:X2} ", b);
				lineASCII += (b < 32 || b > 127 ? '.' : (char)b);
				if (iter % width == width - 1)
				{
					result += prefix + string.Format("0x{0:X8} ", iter2) + lineHex + " " + lineASCII + "\r\n";
					lineHex = "";
					lineASCII = "";
					iter2 += width;
				}
			}
			if (iter % width != width - 1)
			{
				string padding = " ";
				int paddingSize = 3 * (width - (iter % width));
				for(int i = 0; i < paddingSize; i++) padding += " ";
				result += prefix + string.Format("0x{0:X8} ", iter2) + lineHex + padding + lineASCII + "\r\n";
			}
			return result;
		}
	}

	public class PSILongSection : PSISection
	{
		private ushort transportStreamId;		//TableIdExtension;
		private byte reserved2;					//  2 bits
		private byte versionNumber;				//  5 bits
		private bool currentNextIndicator;		//  1 bit
		private byte sectionNumber;
		private byte lastSectionNumber;

		public override void Parse(byte[] data)
		{
			base.Parse(data);
			this.transportStreamId = (ushort)((data[4] << 8) | data[3]);
			this.reserved2 = (byte)(data[5] >> 6);
			this.VersionNumber = (byte)((data[5] >> 1) & 0x1F);
			this.currentNextIndicator = (data[5] & 0x01) != 0;
			this.sectionNumber = data[6];
			this.lastSectionNumber = data[7];
		}

		public ushort TransportStreamId { get { return this.transportStreamId; } set { this.transportStreamId = value; } }
		public byte Reserved2 { get { return this.reserved2; } set { this.reserved2 = value; } }
		public byte VersionNumber { get { return this.versionNumber; } set { this.versionNumber = value; } }
		public bool CurrentNextIndicator { get { return this.currentNextIndicator; } set { this.currentNextIndicator = value; } }
		public byte SectionNumber { get { return this.sectionNumber; } set { this.sectionNumber = value; } }
		public byte LastSectionNumber { get { return this.lastSectionNumber; } set { this.lastSectionNumber = value; } }

		public override string ToStringSectionOnly(string prefix)
		{
			string result = "";
			result += base.ToStringSectionOnly(prefix);
			result += prefix + string.Format("TransportStreamId: 0x{0:x4} ({1})\r\n", this.transportStreamId, this.transportStreamId);
			result += prefix + "Reserved2: " + this.reserved2 + "\r\n";
			result += prefix + "VersionNumber: " + this.versionNumber + "\r\n";
			result += prefix + "CurrentNextIndicator: " + this.currentNextIndicator + "\r\n";
			result += prefix + "SectionNumber: " + this.sectionNumber + "\r\n";
			result += prefix + "LastSectionNumber: " + this.lastSectionNumber + "\r\n";
			return result;
		}

		public override string ToString()
		{
			return "PSILongSection\r\n{\r\n" + ToStringSectionOnly("\t") + "}";
		}
	}

	public class PSIDSMCCSection : PSILongSection
	{
		private byte protocolDiscriminator;
		private byte dsmccType;
		private ushort messageId;
		private uint transactionId;
		private byte reserved3;
		private byte adaptationLength;
		private ushort messageLength;

		public override void Parse(byte[] data)
		{
		}

		public byte ProtocolDiscriminator { get { return this.protocolDiscriminator; } set { this.protocolDiscriminator = value; } }
		public byte DsmccType { get { return this.dsmccType; } set { this.dsmccType = value; } }
		public ushort MessageId { get { return this.messageId; } set { this.messageId = value; } }
		public uint TransactionId { get { return this.transactionId; } set { this.transactionId = value; } }
		public byte Reserved3 { get { return this.reserved3; } set { this.reserved3 = value; } }
		public byte AdaptationLength { get { return this.adaptationLength; } set { this.adaptationLength = value; } }
		public ushort MessageLength { get { return this.messageLength; } set { this.messageLength = value; } }

		public override string ToStringSectionOnly(string prefix)
		{
			string result = "";
			result += base.ToStringSectionOnly(prefix);
			result += prefix + "ProtocolDiscriminator: " + this.protocolDiscriminator + "\r\n";
			result += prefix + "DsmccType: " + this.dsmccType + "\r\n";
			result += prefix + "MessageId: " + this.messageId + "\r\n";
			result += prefix + "TransactionId: " + this.transactionId + "\r\n";
			result += prefix + "Reserved3: " + this.reserved3 + "\r\n";
			result += prefix + "AdaptationLength: " + this.adaptationLength + "\r\n";
			result += prefix + "MessageLength: " + this.messageLength + "\r\n";
			return result;
		}

		public override string ToString()
		{
			return "PSILongSection\r\n{\r\n" + ToStringSectionOnly("\t") + "}";
		}
	}

	public class PSIPAT : PSILongSection
	{
		public class Data
		{
			private ushort programNumber;
			private byte reserved;
			private ushort pid;
			private bool isNetworkPID;

			public ushort ProgramNumber { get { return this.programNumber; } set { this.programNumber = value; } }
			public byte Reserved { get { return this.reserved; } set { this.reserved = value; } }
			public ushort Pid { get { return this.pid; } set { this.pid = value; } }
			public bool IsNetworkPID { get { return this.isNetworkPID; } set { this.isNetworkPID = value; } }

			public string ToStringSectionOnly(string prefix)
			{
				string result = "";
				result += prefix + string.Format("ProgramNumber: 0x{0:x4} ({1})\r\n", this.programNumber, this.programNumber);
				result += prefix + "Reserved: " + this.reserved + "\r\n";
				result += prefix + string.Format("Pid: 0x{0:x4} ({1})\r\n", this.pid, this.pid);
				result += prefix + "IsNetworkPID: " + this.isNetworkPID + "\r\n";
				return result;
			}

			public override string ToString()
			{
				return "PATData\r\n{\r\n" + ToStringSectionOnly("\t") + "}";
			}
		}

		private ArrayList programIds = new ArrayList();
		private Hashtable programIdsByProgramNumber = new Hashtable();

		public ArrayList ProgramIds { get { return this.programIds; } }

		public Data GetProgramIdsByProgramNumber(int programNumber)
		{
			return (Data)programIdsByProgramNumber[programNumber];
		}

		public override void Parse(byte[] data)
		{
			//		for (i = 0; i < N; i++) {
			//			program_number				// 16
			//			reserved					// 3
			//			if (program_number == 0)
			//				network_PID				// 13
			//			else
			//				program_map_PID			// 13
			//		}
			//		CRC_32
			base.Parse(data);
			programIds.Clear();
			for (int offset = 8; offset < this.SectionLength - 1; offset += 4)
			{
				Data patData = new Data();
				patData.ProgramNumber = (ushort)((data[offset] << 8) | data[offset+1]);
				patData.Reserved = (byte)(data[offset+2] >> 5);
				patData.Pid = (ushort)(((data[offset+2] & 0x1F) << 8) | data[offset+3]);
				patData.IsNetworkPID = (patData.ProgramNumber == 0);
				programIds.Add(patData);
				programIdsByProgramNumber[patData.ProgramNumber] = patData;
			}
		}

		public override string ToStringSectionOnly(string prefix)
		{
			string result = "";
			result += base.ToStringSectionOnly(prefix);
			result += prefix + "for (i = 0; i < " + ProgramIds.Count + ")\r\n";
			foreach (Data pat in ProgramIds)
			{
				result += prefix + "{\r\n";
				result += pat.ToStringSectionOnly(prefix + "\t");
				result += prefix + "}\r\n";
			}
			return result;
		}

		public override string ToString()
		{
			return "PSIPAT\r\n{\r\n" + ToStringSectionOnly("\t") + "}\r\n";
		}
	}

	public class PSIPMT : PSILongSection
	{
		// From Recommendation H.222.0 - ISO/IEC 13818-1
		// 2.4.4.8 Program Map Table - Table 2.29
		// TS_program_map_section() {
		//		table_id						// 8
		//		section_syntax_indicator		// 1
		//		'0'								// 1
		//		reserved						// 2
		//		section_length					// 12
		//		program_number					// 16
		//		reserved						// 2
		//		version_number					// 5
		//		current_next_indicator			// 1
		//		section_number					// 8
		//		last_section_number				// 8
		//		reserved						// 3
		//		PCR_PID							// 13
		//		reserved						// 4
		//		program_info_length				// 12
		//		for (i = 0; i < N; i++) {
		//			descriptor()
		//		}
		//		for (i = 0; i < N1; i++) {
		//			stream_type					// 8
		//			reserved					// 3
		//			elementary_PID				// 13
		//			reserved					// 4
		//			ES_info_length				// 12
		//			for (i = 0; i < N2; i++) {
		//				descriptor()
		//			}
		//		}
		//		CRC_32
		// }

		public class Data
		{
			private STREAM_TYPES streamType;
			private byte reserved;
			private ushort pid;
			private byte reserved2;
			private ushort esInfoLen;
			private PSIDescriptor[] descriptors;

			public STREAM_TYPES StreamType { get { return this.streamType; } set { this.streamType = value; } }
			public byte Reserved { get { return this.reserved; } set { this.reserved = value; } }
			public ushort Pid { get { return this.pid; } set { this.pid = value; } }
			public byte Reserved2 { get { return this.reserved2; } set { this.reserved2 = value; } }
			public ushort EsInfoLen { get { return this.esInfoLen; } set { this.esInfoLen = value; } }
			public PSIDescriptor[] Descriptors { get { return this.descriptors; } set { this.descriptors = value; } }

			public string ToStringSectionOnly(string prefix)
			{
				string result = "";
				result += prefix + "StreamType: " + this.streamType.ToString() + string.Format(" 0x{0:x4} ({1})\r\n", (int)this.streamType, (int)this.streamType);
				result += prefix + "Reserved: " + this.reserved + "\r\n";
				result += prefix + string.Format("Pid: 0x{0:x4} ({1})\r\n", this.pid, this.pid);
				result += prefix + "Reserved2: " + this.reserved2 + "\r\n";
				result += prefix + "EsInfoLen: " + this.esInfoLen + "\r\n";
				result += prefix + "for (i = 0; i < " + this.descriptors.Length + ")\r\n";
				foreach (PSIDescriptor descriptor in this.descriptors)
				{
					result += prefix + "{\r\n";
					result += descriptor.ToStringDescriptorOnly(prefix + "\t");
					result += prefix + "}\r\n";
				}
				return result;
			}

			public override string ToString()
			{
				return "PMTData\r\n{\r\n" + ToStringSectionOnly("\t") + "}\r\n";
			}
		}

		private byte reserved3;
		private ushort pcrPid;
		private byte reserved4;
		private ushort programInfoLength;
		private PSIDescriptor[] descriptors;

		private ArrayList streams = new ArrayList();
		private Hashtable streamsByType = new Hashtable();


		public byte Reserved3 { get { return this.reserved3; } set { this.reserved3 = value; } }
		public ushort PcrPid { get { return this.pcrPid; } set { this.pcrPid = value; } }
		public byte Reserved4 { get { return this.reserved4; } set { this.reserved4 = value; } }
		public ushort ProgramInfoLength { get { return this.programInfoLength; } set { this.programInfoLength = value; } }
		public PSIDescriptor[] Descriptors { get { return this.descriptors; } set { this.descriptors = value; } }

		public ArrayList Streams { get { return this.streams; } }

		public Data GetStreamByType(STREAM_TYPES streamType)
		{
			return (Data)streamsByType[streamType];
		}

		public override void Parse(byte[] data)
		{
			base.Parse(data);

			this.reserved3 = (byte)(data[8] >> 5);
			this.pcrPid = (ushort)((data[8] & 0x1F) << 8 | data[9]);
			this.reserved4 = (byte)(data[10] >> 4);
			this.programInfoLength = (byte)((data[10] & 0x0F) << 8 | data[11]);
			int offset = 12;
			if (this.programInfoLength > 0)
			{
				// Parse program descriptors
				this.descriptors = PSIDescriptor.ParseDescriptors(data, offset, this.programInfoLength);
			}


			offset += this.programInfoLength;
			int streamLen = data.Length - this.programInfoLength - 16;
			while (streamLen >= 5)
			{
				Data pmtData = new Data();
				pmtData.StreamType = (STREAM_TYPES)data[offset];
				pmtData.Pid = (ushort)(((data[offset + 1] & 0x1F) << 8) + data[offset + 2]);
				pmtData.EsInfoLen = (ushort)(((data[offset + 3] & 0x0F) << 8) + data[offset + 4]);
				pmtData.Descriptors = PSIDescriptor.ParseDescriptors(data, offset + 5, pmtData.EsInfoLen);

				this.streams.Add(pmtData);
				this.streamsByType[pmtData.StreamType] = pmtData;
				offset += pmtData.EsInfoLen + 5;
				streamLen -= pmtData.EsInfoLen + 5;
			}
		}

		public override string ToStringSectionOnly(string prefix)
		{
			string result = "";
			result += base.ToStringSectionOnly(prefix);
			result += prefix + "Reserved3: " + this.reserved3 + "\r\n";
			result += prefix + string.Format("PcrPid: 0x{0:x4} ({1})\r\n", this.pcrPid, this.pcrPid);
			result += prefix + "Reserved4: " + this.reserved4 + "\r\n";
			result += prefix + "ProgramInfoLength: " + this.programInfoLength + "\r\n";
			if (this.descriptors != null)
			{
				result += prefix + "for (i = 0; i < " + this.descriptors.Length + ")\r\n";
				foreach (PSIDescriptor descriptor in this.descriptors)
				{
					result += prefix + "{\r\n";
					result += descriptor.ToStringDescriptorOnly(prefix + "\t");
					result += prefix + "}\r\n";
				}
			}
			result += prefix + "for (i = 0; i < " + this.streams.Count + ")\r\n";
			foreach (Data stream in this.streams)
			{
				result += prefix + "{\r\n";
				result += stream.ToStringSectionOnly(prefix + "\t");
				result += prefix + "}\r\n";
			}
			return result;
		}

		public override string ToString()
		{
			return "PSIPMT\r\n{\r\n" + ToStringSectionOnly("\t") + "}\r\n";
		}
	}

	public class PSISDT : PSILongSection
	{
		// From ETSI EN 300 468 V1.5.1 (2003-5)
		// 5.2.3 Service Description Table - Table 5
		// service_description_section() {
		//		table_id						// 8
		//		section_syntax_indicator		// 1
		//		'0'								// 1
		//		reserved						// 2
		//		section_length					// 12
		//		transport_stream_id				// 16
		//		reserved						// 2
		//		version_number					// 5
		//		current_next_indicator			// 1
		//		section_number					// 8
		//		last_section_number				// 8
		//		original_network_id				// 16
		//		reserved						// 8
		//		for (i = 0; i < N; i++) {
		//			service_id					// 16
		//			reserved					// 6
		//			EIT_schedule_flag			// 1
		//			EIT_present_following_flag	// 1
		//			running_status				// 3
		//			free_CA_mode				// 1
		//			descriptors_loop_length		// 12
		//			for (i = 0; i < N1; i++) {
		//				descriptor()
		//			}
		//		}
		//		CRC_32
		// }

		public class Data
		{
			public enum RUNNING_STATUS
			{
				UNDEFINED = 0,
				NOT_RUNNING = 1,
				STARTS_IN_FEW_SECONDS = 2,
				PAUSING = 3,
				RUNNING = 4,
				RESERVED_1 = 5,
				RESERVED_2 = 6,
				RESERVED_3 = 7,
			}

			private ushort serviceId;
			private byte reserved;
			private bool eitScheduleFlag;
			private bool eitPresentFollowingFlag;
			private RUNNING_STATUS runningStatus;
			private bool freeCaMode;
			private ushort descriptorsLoopLength;
			private PSIDescriptor[] descriptors;

			public ushort ServiceId { get { return this.serviceId; } set { this.serviceId = value; } }
			public byte Reserved { get { return this.reserved; } set { this.reserved = value; } }
			public bool EitScheduleFlag { get { return this.eitScheduleFlag; } set { this.eitScheduleFlag = value; } }
			public bool EitPresentFollowingFlag { get { return this.eitPresentFollowingFlag; } set { this.eitPresentFollowingFlag = value; } }
			public RUNNING_STATUS RunningStatus { get { return this.runningStatus; } set { this.runningStatus = value; } }
			public bool FreeCaMode { get { return this.freeCaMode; } set { this.freeCaMode = value; } }
			public ushort DescriptorsLoopLength { get { return this.descriptorsLoopLength; } set { this.descriptorsLoopLength = value; } }
			public PSIDescriptor[] Descriptors { get { return this.descriptors; } set { this.descriptors = value; } }

			public string GetServiceName()
			{
				foreach(PSIDescriptor descriptor in this.Descriptors)
				{
					if(descriptor is PSIDescriptorService)
					{
						PSIDescriptorService descriptorService = descriptor as PSIDescriptorService;
						return descriptorService.ServiceName;
                        
					}
				}
				return "";
			}
            // Hervé Stalin : retourne le service_type
            public SERVICE_TYPES GetServiceType()
            {
                foreach (PSIDescriptor descriptor in this.Descriptors)
                {
                    if (descriptor is PSIDescriptorService)
                    {
                        PSIDescriptorService descriptorService = descriptor as PSIDescriptorService;
                        return descriptorService.ServiceType;

                    }
                }
                return SERVICE_TYPES.RESERVED_SERVICE;
            }




			public string ToStringSectionOnly(string prefix)
			{
				string result = "";
				result += prefix + "ServiceId: " + string.Format(" 0x{0:x4} ({1})\r\n", this.serviceId, this.serviceId);
				result += prefix + "Reserved: " + string.Format(" 0x{0:x4} ({1})\r\n", this.reserved, this.reserved);
				result += prefix + "EitScheduleFlag: " + this.eitScheduleFlag + "\r\n";
				result += prefix + "EitPresentFollowingFlag: " + this.eitPresentFollowingFlag + "\r\n";
				result += prefix + "RunningStatus: " + this.runningStatus + " " + string.Format(" 0x{0:x4} ({1})\r\n", (byte)this.runningStatus, (byte)this.runningStatus);
				result += prefix + "FreeCaMode: " + this.freeCaMode + "\r\n";
				result += prefix + "DescriptorsLoopLength: " + string.Format(" 0x{0:x4} ({1})\r\n", this.descriptorsLoopLength, this.descriptorsLoopLength);
				result += prefix + "for (i = 0; i < " + this.descriptors.Length + ")\r\n";
				foreach (PSIDescriptor descriptor in this.descriptors)
				{
					result += prefix + "{\r\n";
					result += descriptor.ToStringDescriptorOnly(prefix + "\t");
					result += prefix + "}\r\n";
				}
				return result;
			}

			public override string ToString()
			{
				return "STDData\r\n{\r\n" + ToStringSectionOnly("\t") + "}\r\n";
			}
		}

		private ushort originalNetworkId;
		private byte reserved3;
		private ArrayList services = new ArrayList();

		public ushort OriginalNetworkId { get { return this.originalNetworkId; } set { this.originalNetworkId = value; } }
		public byte Reserved3 { get { return this.reserved3; } set { this.reserved3 = value; } }
		public ArrayList Services { get { return this.services; } }

		public override void Parse(byte[] data)
		{
			base.Parse(data);

			this.originalNetworkId = (ushort)(data[8] << 8 | data[9]);
			this.reserved3 = data[10];

			int offset = 11;
			while (offset < SectionLength - 1)
			{
				Data sdtData = new Data();
				sdtData.ServiceId = (ushort)((data[offset] << 8) + data[offset + 1]);
				sdtData.Reserved = (byte)(data[offset + 2] >> 2);
				sdtData.EitScheduleFlag = ((data[offset + 2] & 0x02) != 0);
				sdtData.EitPresentFollowingFlag = ((data[offset + 2] & 0x01) != 0);
				sdtData.RunningStatus = (Data.RUNNING_STATUS)((data[offset + 3] >> 5) & 0x07);
				sdtData.FreeCaMode = (((data[offset + 3] >> 4) & 0x01) != 0);
				sdtData.DescriptorsLoopLength = (ushort)(((data[offset + 3] & 0x0F) << 8) | data[offset + 4]);
				sdtData.Descriptors = PSIDescriptor.ParseDescriptors(data, offset + 5, sdtData.DescriptorsLoopLength);

				this.services.Add(sdtData);
				offset += sdtData.DescriptorsLoopLength + 5;
			}
		}

		public override string ToStringSectionOnly(string prefix)
		{
			string result = "";
			result += base.ToStringSectionOnly(prefix);
			result += prefix + "OriginalNetworkId: " + string.Format(" 0x{0:x4} ({1})\r\n", this.originalNetworkId, this.originalNetworkId);
			result += prefix + "Reserved3: " + string.Format(" 0x{0:x4} ({1})\r\n", this.reserved3, this.reserved3);
			result += prefix + "for (i = 0; i < " + this.services.Count + ")\r\n";
			foreach (Data service in this.services)
			{
				result += prefix + "{\r\n";
				result += service.ToStringSectionOnly(prefix + "\t");
				result += prefix + "}\r\n";
			}
			return result;
		}

		public override string ToString()
		{
			return "PSILongSection\r\n{\r\n" + ToStringSectionOnly("\t") + "}\r\n";
		}
	}


    //Hervé stalin : class pour parser la table NIT
	public class PSINIT : PSILongSection
	{
		//network_information_section( ) {
		//      table_id                            8   PSISection          8       Byte[0]
		//      section_syntax_indicator            1   PSISection          9       Byte[1] bit 0
		//      reserved_future_use                 1   PSISection          10      Byte[1] bit 1
		//      reserved                            2   PSISection          12      Byte[1] bit 2 à 3
		//      section_length                      12  PSISection          24      byte[1] bit 4 à 7 + Byte[2] 
		//      network_id                          16  PSILongSection      40      Byte[3] + Byte[4]
		//      reserved                            2   PSILongSection      42      Byte[5] bit 0 à 1
		//      version_number                      5   PSILongSection      47      Byte[5] bit 2 à 6
		//      current_next_indicator              1   PSILongSection      48      Byte[6] bit 7
		//      section_number                      8   PSILongSection      56      Byte[7]
		//      last_section number                 8   PSILongSection      64      Byte[8]
		//      reserved_future_use                 4                       68      Byte[9] bit 0 à 3
		//      network_descriptors_length          12                      80      Byte[9} bit 4 à 7 +byte[10]
		//      for(i=0;i<N;i++){
		//          descriptor()
		//      }
		//      reserved_future_use                 4                       
		//      transport_stream_loop_length        12
		//      for(i=0;i<N;i++){
		//          transport_stream_id             16
		//          original_network_id             16
		//          reserved_future_use             4
		//          transport_descriptors_length    12
		//          for(j=0;j<N;j++){
		//              descriptor()
		//          }
		//      }
		//  CRC_32 32
		//}
		public class Data
		{
			private ushort transportStreamId;
			private ushort originalNetworkId;
			private byte reserved1;
			private ushort transportDescriptorLenght;
			private PSIDescriptor[] descriptors;

			public ushort TransportStreamId { get { return this.transportStreamId; } set { this.transportStreamId = value; } }
			public ushort OriginalNetworkId { get { return this.originalNetworkId; } set { this.originalNetworkId = value; } }
			public byte Reserved1 { get { return this.reserved1; } set { this.reserved1 = value; } }
			public ushort TransportDescriptorLenght { get { return this.transportDescriptorLenght; } set { this.transportDescriptorLenght = value; } }
			public PSIDescriptor[] Descriptors { get { return this.descriptors; } set { this.descriptors = value; } }

            public string ToStringSectionOnly(string prefix)
            {
                string result = "";
                result += prefix + "TransportStreamId: " + this.transportStreamId.ToString() + string.Format(" 0x{0:x4} ({1})\r\n", (int)this.transportStreamId, (int)this.transportStreamId);
                result += prefix + "OriginalNetworkId: " + this.originalNetworkId + "\r\n";
                result += prefix + "Reserved1: " + this.reserved1 + "\r\n";
                result += prefix + "transportDescriptorLenght: " + this.transportDescriptorLenght + "\r\n";
                result += prefix + "for (i = 0; i < " + this.descriptors.Length + ")\r\n";
                foreach (PSIDescriptor descriptor in this.descriptors)
                {
                    result += prefix + "{\r\n";
                    result += descriptor.ToStringDescriptorOnly(prefix + "\t");
                    result += prefix + "}\r\n";
                }
                return result;
            }

            public override string ToString()
            {
                return "PSINIT\r\n{\r\n" + ToStringSectionOnly("\t") + "}\r\n";
            }

		}

		private byte reserved3;
		private ushort networkDescriptorLength;
		private PSIDescriptor[] descriptors;

		private byte reserved4;
		private ushort transportStreamLoopLength;       

		public byte Reserved3 { get { return this.reserved3; } set { this.reserved3 = value; } }
		public ushort NetworkDescriptorLength { get { return this.networkDescriptorLength; } set { this.networkDescriptorLength = value; } }
		public PSIDescriptor[] Descriptors { get { return this.descriptors; } set { this.descriptors = value; } }
		public byte Reserved4 { get { return this.reserved4; } set { this.reserved4 = value; } }
		public ushort TransportStreamLoopLength { get { return this.transportStreamLoopLength; } set { this.transportStreamLoopLength = value; } }


		private ArrayList streams = new ArrayList();
        public ArrayList Streams { get { return this.streams; } }

		public override void Parse(byte[] data)
		{
			base.Parse(data);
			this.reserved3 = (byte)(data[8] >> 4);
			this.networkDescriptorLength = (byte)(((data[8] & 0x0F) << 8) | (data[9]));

			int offset = 10; //taille de l'entete de la table
			if (this.NetworkDescriptorLength > 0)
			{
				// Parse program descriptors
				this.descriptors = PSIDescriptor.ParseDescriptors(data, offset, this.NetworkDescriptorLength);
			}

            offset += this.NetworkDescriptorLength; // rajout la taille du tableau de descriptors
			this.reserved4 = (byte)(data[offset] >> 4);
			this.transportStreamLoopLength =(ushort)(((data[offset] & 0x0F) << 8) | (data[offset + 1]));

			offset += 2; //rajout de la taille de reserved4 et de transportStreamLoopLength
            int streamLen = data.Length - this.NetworkDescriptorLength - 14;
			//int streamLen = this.transportStreamLoopLength;
			while (streamLen >= 6)
			{
				Data nitData = new Data();
				nitData.TransportStreamId = (ushort)(((data[offset])<<8) | (data[offset+1]));
				nitData.OriginalNetworkId = (ushort)(((data[offset+2]) << 8) | (data[offset + 3])); ;
				nitData.Reserved1 =(byte) (data[offset+4]>>4) ;
				nitData.TransportDescriptorLenght = (ushort)((((data[offset + 4]) & 0x0F) <<8) | (data[offset+5]));
				nitData.Descriptors = PSIDescriptor.ParseDescriptors(data, offset + 6, nitData.TransportDescriptorLenght);

				this.streams.Add(nitData);
				offset += nitData.TransportDescriptorLenght + 6;//offset increnté de la taille de la loop
				streamLen -= nitData.TransportDescriptorLenght + 6;// taille de la loop restante décrémenté 
			}
			
		}

        public override string ToStringSectionOnly(string prefix)
        {
            string result = "";
            result += base.ToStringSectionOnly(prefix);
            result += prefix + "Reserved3: " + this.reserved3 + "\r\n";
            result += prefix + "NetworkDescriptorLength: " + this.networkDescriptorLength + "\r\n";
            if (this.descriptors != null)
            {
                result += prefix + "for (i = 0; i < " + this.descriptors.Length + ")\r\n";
                foreach (PSIDescriptor descriptor in this.descriptors)
                {
                    result += prefix + "{\r\n";
                    result += descriptor.ToStringDescriptorOnly(prefix + "\t");
                    result += prefix + "}\r\n";
                }
            }

            result += prefix + "Reserved4: " + this.reserved4 + "\r\n";
            result += prefix + "TransportStreamLoopLength: " + this.transportStreamLoopLength + "\r\n";

           result += prefix + "for (i = 0; i < " + this.streams.Count + ")\r\n";
            foreach (Data stream in this.streams)
            {
                result += prefix + "{\r\n";
                result += stream.ToStringSectionOnly(prefix + "\t");
                result += prefix + "}\r\n";
            }
            return result;
        }

        public override string ToString()
        {
            return "PSINIT\r\n{\r\n" + ToStringSectionOnly("\t") + "}\r\n";
        }

}
	// pour le decodage des ECM regarder dans _mainForm_OnECMFilterData
}
