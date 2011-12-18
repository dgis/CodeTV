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
using System.Text;

namespace CodeTV.PSI
{
	class PSIDescriptor
	{
		private DESCRIPTOR_TAGS descriptorTag;
		private byte descriptorLength;
		private byte[] unparseData;

		public virtual void Parse(byte[] data, int offset)
		{
			this.descriptorTag = (DESCRIPTOR_TAGS)data[offset];
			this.descriptorLength = data[offset + 1];
		}

		public DESCRIPTOR_TAGS DescriptorTag { get { return this.descriptorTag; } set { this.descriptorTag = value; } }
		public byte DescriptorLength { get { return this.descriptorLength; } set { this.descriptorLength = value; } }
		public byte[] UnparseData { get { return this.unparseData; } }

		public virtual string ToStringDescriptorOnly(string prefix)
		{
			string result = "";
			result += prefix + "DescriptorTag: " + this.descriptorTag + string.Format(" 0x{0:x4} ({1})\r\n", (uint)this.descriptorTag, (uint)this.descriptorTag);
			result += prefix + "DescriptorLength: " + this.descriptorLength + "\r\n";
			if (this.unparseData != null)
				result += PSISection.ToStringByteArray(this.unparseData, 0, this.unparseData.Length, 8, prefix);
			return result;
		}

		public override string ToString()
		{
			return ToString("");
		}

		public virtual string ToString(string prefix)
		{
			return prefix + "PSIDescriptor\r\n" + prefix + "{\r\n" + ToStringDescriptorOnly(prefix + "\t") + prefix + "}";
		}

		public static PSIDescriptor ParseDescriptor(byte[] data, int offset, byte length)
		{
			DESCRIPTOR_TAGS descriptorTag = (DESCRIPTOR_TAGS)data[offset];
			PSIDescriptor descriptor;
			switch (descriptorTag)
			{
				case DESCRIPTOR_TAGS.DESCR_SERVICE:
					descriptor = new PSIDescriptorService(); break;
				case DESCRIPTOR_TAGS.DESCR_STD:
					descriptor = new PSIDescriptorSTD(); break;
				case DESCRIPTOR_TAGS.DESCR_ISO_639_LANGUAGE:
					descriptor = new PSIDescriptorISO639Language(); break;
				case DESCRIPTOR_TAGS.DESCR_SUBTITLING:
					descriptor = new PSIDescriptorSubtitling(); break;
				case DESCRIPTOR_TAGS.DESCR_TELETEXT:
					descriptor = new PSIDescriptorTeletext(); break;
				case DESCRIPTOR_TAGS.DESCR_AC3:
					descriptor = new PSIDescriptorAC3(); break;
				case DESCRIPTOR_TAGS.DESCR_CA_IDENT:
					descriptor = new PSIDescriptorCAIdentifier(); break;
				case DESCRIPTOR_TAGS.DESCR_CA_SYSTEM:
					descriptor = new PSIDescriptorCASystem(); break;
				case DESCRIPTOR_TAGS.DESCR_CA:
					descriptor = new PSIDescriptorCA(); break;
				case DESCRIPTOR_TAGS.DESCR_DATA_BROADCAST_ID:
					descriptor = new PSIDescriptorDataBroadcastId(); break;
				default:
					descriptor = new PSIDescriptor();
					descriptor.unparseData = new byte[length];
					Array.Copy(data, offset + 2, descriptor.unparseData, 0, length);
					break;
			}
			descriptor.Parse(data, offset);
			return descriptor;
		}

		public static PSIDescriptor[] ParseDescriptors(byte[] data, int offset, int descriptorsLength)
		{
			ArrayList al = new ArrayList();
			if (descriptorsLength >= 2)
			{
				PSIDescriptor descriptor;
				do
				{
					al.Add(descriptor = ParseDescriptor(data, offset, data[offset + 1]));
					descriptorsLength -= descriptor.DescriptorLength + 2;
					offset += descriptor.DescriptorLength + 2;
				}
				while (descriptorsLength >= 2);
			}
			return (PSIDescriptor[])al.ToArray(typeof(PSIDescriptor));
		}
	}

	class PSIDescriptorSTD : PSIDescriptor
	{
		// From Recommendation H.222.0 - ISO/IEC 13818-1
		// 2.6.32 _ Table 2-61 _ STD descriptor
		// STD_descriptor() {
		//		descriptor_tag							8
		//		descriptor_length						8
		//		reserved								7
		//		leak_valid_flag							1
		// }

		private byte reserved;
		private bool leakValidFlag;

		public override void Parse(byte[] data, int offset)
		{
			base.Parse(data, offset);
			this.reserved = (byte)(data[offset + 2] & 0x7F);
			this.leakValidFlag = (data[offset + 2] & 0x80) != 0;
		}

		public byte Reserved { get { return this.reserved; } set { this.reserved = value; } }
		public bool LeakValidFlag { get { return this.leakValidFlag; } set { this.leakValidFlag = value; } }

		public override string ToStringDescriptorOnly(string prefix)
		{
			string result = "";
			result += base.ToStringDescriptorOnly(prefix);
			result += prefix + "Reserved: " + this.reserved + "\r\n";
			result += prefix + "LeakValidFlag: " + this.leakValidFlag + "\r\n";
			return result;
		}

		public override string ToString(string prefix)
		{
			return prefix + "PSIDescriptorSTD\r\n" + prefix + "{\r\n" + ToStringDescriptorOnly(prefix + "\t") + prefix + "}";
		}
	}

	class PSIDescriptorISO639Language : PSIDescriptor
	{
		// From Recommendation H.222.0 - ISO/IEC 13818-1
		// 2.6.18 _ Table 2-53 _ ISO 639 language descriptor
		// ISO_639_language_descriptor() {
		//		descriptor_tag							8
		//		descriptor_length						8
		//		for (i = 0; i < N; i++) {
		//			ISO_639_language_code				24
		//			audio_type							8
		//		}
		// }

		public class Language
		{
			private string iso639LanguageCode = "";
			private AUDIO_TYPE audioType;

			public string Iso639LanguageCode { get { return this.iso639LanguageCode; } set { this.iso639LanguageCode = value; } }
			public AUDIO_TYPE AudioType { get { return this.audioType; } set { this.audioType = value; } }

			public string ToStringSectionOnly(string prefix)
			{
				string result = "";
				result += prefix + "Iso639LanguageCode: " + this.iso639LanguageCode + "\r\n";
				result += prefix + "AudioType: " + this.audioType + string.Format(" 0x{0:x4} ({1})\r\n", (uint)this.audioType, (uint)this.audioType);
				return result;
			}

			public override string ToString()
			{
				return "Language\r\n{\r\n" + ToStringSectionOnly("\t") + "}";
			}
		}

		private ArrayList languages = new ArrayList();

		public ArrayList Languages { get { return this.languages; } }

		public override void Parse(byte[] data, int offset)
		{
			base.Parse(data, offset);
			this.languages.Clear();
			for (int offset2 = offset + 2; offset2 < offset + this.DescriptorLength - 1; offset2 += 4)
			{
				Language language = new Language();
				language.Iso639LanguageCode = "" + (char)data[offset2] + (char)data[offset2 + 1] + (char)data[offset2 + 2];
				language.AudioType = (AUDIO_TYPE)data[offset2 + 3];
				this.languages.Add(language);
			}
		}

		public override string ToStringDescriptorOnly(string prefix)
		{
			string result = "";
			result += base.ToStringDescriptorOnly(prefix);
			result += prefix + "for (i = 0; i < " + this.languages.Count + ")\r\n";
			foreach (Language language in this.languages)
			{
				result += prefix + "{\r\n";
				result += language.ToStringSectionOnly(prefix + "\t");
				result += prefix + "}\r\n";
			}
			return result;
		}

		public override string ToString(string prefix)
		{
			return prefix + "PSIDescriptorISO639Language\r\n" + prefix + "{\r\n" + ToStringDescriptorOnly(prefix + "\t") + prefix + "}";
		}
	}

	class PSIDescriptorSubtitling : PSIDescriptor
	{
		// From ETSI EN 300 468 - V1.5,1
		// 6.2.38 _ Table 82 _ Subtitling descriptor
		// subtitling_descriptor() {
		//		descriptor_tag							8
		//		descriptor_length						8
		//		for (i = 0; i < N; i++) {
		//			ISO_639_language_code				24
		//			subtitling_type						8
		//			composition_page_id					16
		//			ancillary_page_id					16
		//		}
		// }

		public class Language
		{
			private string iso639LanguageCode = "";
			private byte subtitlingType;
			private ushort compositionPageId;
			private ushort ancillaryPageId;

			public string Iso639LanguageCode { get { return this.iso639LanguageCode; } set { this.iso639LanguageCode = value; } }
			public byte SubtitlingType { get { return this.subtitlingType; } set { this.subtitlingType = value; } }
			public ushort CompositionPageId { get { return this.compositionPageId; } set { this.compositionPageId = value; } }
			public ushort AncillaryPageId { get { return this.ancillaryPageId; } set { this.ancillaryPageId = value; } }

			public string ToStringSectionOnly(string prefix)
			{
				string result = "";
				result += prefix + "Iso639LanguageCode: " + this.iso639LanguageCode + "\r\n";
				result += prefix + "SubtitlingType: " + string.Format(" 0x{0:x4} ({1})\r\n", this.subtitlingType, this.subtitlingType);
				result += prefix + "CompositionPageId: " + string.Format(" 0x{0:x4} ({1})\r\n", this.compositionPageId, this.compositionPageId);
				result += prefix + "AncillaryPageId: " + string.Format(" 0x{0:x4} ({1})\r\n", this.ancillaryPageId, this.ancillaryPageId);
				return result;
			}

			public override string ToString()
			{
				return "Language\r\n{\r\n" + ToStringSectionOnly("\t") + "}";
			}
		}

		private ArrayList languages = new ArrayList();

		public ArrayList Languages { get { return this.languages; } }

		public override void Parse(byte[] data, int offset)
		{
			base.Parse(data, offset);
			this.languages.Clear();
			for (int offset2 = offset + 2; offset2 < offset + this.DescriptorLength - 1; offset2 += 8)
			{
				Language language = new Language();
				language.Iso639LanguageCode = "" + (char)data[offset2] + (char)data[offset2 + 1] + (char)data[offset2 + 2];
				language.SubtitlingType = data[offset2 + 3];
				language.CompositionPageId = (ushort)((data[offset2 + 4] << 8) | data[offset2 + 5]);
				language.AncillaryPageId = (ushort)((data[offset2 + 6] << 8) | data[offset2 + 7]);
				this.languages.Add(language);
			}
		}

		public override string ToStringDescriptorOnly(string prefix)
		{
			string result = "";
			result += base.ToStringDescriptorOnly(prefix);
			result += prefix + "for (i = 0; i < " + this.languages.Count + ")\r\n";
			foreach (Language language in this.languages)
			{
				result += prefix + "{\r\n";
				result += language.ToStringSectionOnly(prefix + "\t");
				result += prefix + "}\r\n";
			}
			return result;
		}

		public override string ToString(string prefix)
		{
			return prefix + "PSIDescriptorSubtitling\r\n" + prefix + "{\r\n" + ToStringDescriptorOnly(prefix + "\t") + prefix + "}";
		}
	}

	class PSIDescriptorTeletext : PSIDescriptor
	{
		// From ETSI EN 300 468 - V1.6,1
		// 6.2.41 _ Table 87 _ Teletext descriptor
		// teletext_descriptor() {
		//		descriptor_tag							8
		//		descriptor_length						8
		//		for (i = 0; i < N; i++) {
		//			ISO_639_language_code				24
		//			teletext_type						5
		//			teletext_magazine_number			3
		//			teletext_page_number				8
		//		}
		// }

		public class Language
		{
			private string iso639LanguageCode = "";
			private byte teletextType;
			private byte teletextMagazineNumber;
			private byte teletextPageNumber;

			public string Iso639LanguageCode { get { return this.iso639LanguageCode; } set { this.iso639LanguageCode = value; } }
			public byte TeletextType { get { return this.teletextType; } set { this.teletextType = value; } }
			public byte TeletextMagazineNumber { get { return this.teletextMagazineNumber; } set { this.teletextMagazineNumber = value; } }
			public byte TeletextPageNumber { get { return this.teletextPageNumber; } set { this.teletextPageNumber = value; } }

			public string ToStringSectionOnly(string prefix)
			{
				string result = "";
				result += prefix + "Iso639LanguageCode: " + this.iso639LanguageCode + "\r\n";
				result += prefix + "TeletextType: " + string.Format(" 0x{0:x4} ({1})\r\n", this.teletextType, this.teletextType);
				result += prefix + "TeletextMagazineNumber: " + string.Format(" 0x{0:x4} ({1})\r\n", this.teletextMagazineNumber, this.teletextMagazineNumber);
				result += prefix + "TeletextPageNumber: " + string.Format(" 0x{0:x4} ({1})\r\n", this.teletextPageNumber, this.teletextPageNumber);
				return result;
			}

			public override string ToString()
			{
				return "Language\r\n{\r\n" + ToStringSectionOnly("\t") + "}";
			}
		}

		private ArrayList languages = new ArrayList();

		public ArrayList Languages { get { return this.languages; } }

		public override void Parse(byte[] data, int offset)
		{
			base.Parse(data, offset);
			this.languages.Clear();
			for (int offset2 = offset + 2; offset2 < offset + this.DescriptorLength - 1; offset2 += 5)
			{
				Language language = new Language();
				language.Iso639LanguageCode = "" + (char)data[offset2] + (char)data[offset2 + 1] + (char)data[offset2 + 2];
				language.TeletextType = (byte)(data[offset2 + 3] & 0x1F);
				language.TeletextMagazineNumber = (byte)(data[offset2 + 3] >> 5);
				language.TeletextPageNumber = data[offset2 + 4];
				this.languages.Add(language);
			}
		}

		public override string ToStringDescriptorOnly(string prefix)
		{
			string result = "";
			result += base.ToStringDescriptorOnly(prefix);
			result += prefix + "for (i = 0; i < " + this.languages.Count + ")\r\n";
			foreach (Language language in this.languages)
			{
				result += prefix + "{\r\n";
				result += language.ToStringSectionOnly(prefix + "\t");
				result += prefix + "}\r\n";
			}
			return result;
		}

		public override string ToString(string prefix)
		{
			return prefix + "PSIDescriptorTeletext\r\n" + prefix + "{\r\n" + ToStringDescriptorOnly(prefix + "\t") + prefix + "}";
		}
	}

	class PSIDescriptorAC3 : PSIDescriptor
	{
		// From ETSI EN 300 468 - V1.5,1
		// Annex D (informativ) _ Table D.2: AC3 descriptor syntax
		// AC3_descriptor() {
		//		descriptor_tag							8
		//		descriptor_length						N*8
		//		component_type_flag						1
		//		bsid_flag								1
		//		mainid_flag								1
		//		asvc_flag								1
		//		reserved								4
		//		if (component_type_flag == 1) {
		//			AC3_type							8
		//		}
		//		if (bsid_flag == 1) {
		//			bsid								8
		//		}
		//		if (mainid_flag == 1) {
		//			mainid								8
		//		}
		//		if (asvc_flag == 1) {
		//			asvc								8
		//		}
		//		for (i = 0; i < N; i++) {
		//			additional_info[i]					N*8
		//		}
		// }

		public override void Parse(byte[] data, int offset)
		{
			base.Parse(data, offset);

		}

		public override string ToStringDescriptorOnly(string prefix)
		{
			string result = "";
			result += base.ToStringDescriptorOnly(prefix);

			return result;
		}

		public override string ToString(string prefix)
		{
			return prefix + "PSIDescriptorAC3\r\n" + prefix + "{\r\n" + ToStringDescriptorOnly(prefix + "\t") + prefix + "}";
		}
	}

	class PSIDescriptorCAIdentifier : PSIDescriptor
	{
	// DESCRIPTOR_TAGS.DESCR_CA_IDENT
	// From ETSI EN 300 468 - V1.6,1
	// 6.2.5 _ Table 22 _ CA identifier descriptor
	// CA_identifier_descriptor() {
	//		descriptor_tag							8
	//		descriptor_length						8
	//		for (i - 0; i < N; i++) {
	//			CA_system_id						16
	//		}
	// }

		private ushort[] caSystemIds = new ushort[0];

		public ushort[] CaSystemIds { get { return this.caSystemIds; } }

		public override void Parse(byte[] data, int offset)
		{
			base.Parse(data, offset);
			ArrayList al = new ArrayList();
			for (int offset2 = offset + 2; offset2 < offset + this.DescriptorLength - 1; offset2 += 2)
				al.Add((ushort)((data[offset2] << 8) | data[offset2 + 1]));
			this.caSystemIds = (ushort[])al.ToArray(typeof(ushort));
		}

		public override string ToStringDescriptorOnly(string prefix)
		{
			string result = "";
			result += base.ToStringDescriptorOnly(prefix);
			result += prefix + "for (i = 0; i < " + this.caSystemIds.Length + ")\r\n";
			for (int i = 0; i < this.caSystemIds.Length; i++)
			{
				uint caSystemId = this.caSystemIds[i];
				result += prefix + "CaSystemIds" + string.Format("[{0}]: 0x{1:x4} ({2})\r\n", i, caSystemId, caSystemId);
			}
			return result;
		}

		public override string ToString(string prefix)
		{
			return prefix + "PSIDescriptorCAIdentifier\r\n" + prefix + "{\r\n" + ToStringDescriptorOnly(prefix + "\t") + prefix + "}";
		}
	}

	class PSIDescriptorCASystem : PSIDescriptor
	{
	// DESCRIPTOR_TAGS.DESCR_CA_SYSTEM
	// From ETSI EN 300 468 - V1.5,1
	//	RESERVED FOR Future use !!!
	// CA_system_descriptor() {
	//		descriptor_tag							8
	//		descriptor_length						8
	//		!!!!!
	// }

		public override void Parse(byte[] data, int offset)
		{
			base.Parse(data, offset);

		}

		public override string ToStringDescriptorOnly(string prefix)
		{
			string result = "";
			result += base.ToStringDescriptorOnly(prefix);

			return result;
		}

		public override string ToString(string prefix)
		{
			return prefix + "PSIDescriptorCASystem\r\n" + prefix + "{\r\n" + ToStringDescriptorOnly(prefix + "\t") + prefix + "}";
		}
	}

	class PSIDescriptorCA : PSIDescriptor
	{
	// DESCRIPTOR_TAGS.DESCR_CA
	// From Recommendation H.222.0 - ISO/IEC 13818-1
	// 2.6.16 _ Table 2-52 _ Conditional access descriptor
	// CA_descriptor() {
	//		descriptor_tag							8
	//		descriptor_length						8
	//		CA_system_ID							16
	//		reserved								3
	//		CA_PID									13		: ECM
	//		for(i = 0; i < N; i++) {
	//			private_data_byte					8
	//		}
	// }
	
		private ushort caSystemId;
		private byte reserved;
		private ushort caPid;
		private byte[] privateDataBytes = new byte[0];

		public ushort CaSystemId { get { return this.caSystemId; } set { this.caSystemId = value; } }
		public byte Reserved { get { return this.reserved; } set { this.reserved = value; } }
		public ushort CaPid { get { return this.caPid; } set { this.caPid = value; } }
		public byte[] PrivateDataBytes { get { return this.privateDataBytes; } }

		public override void Parse(byte[] data, int offset)
		{
			base.Parse(data, offset);
			this.caSystemId = (ushort)((data[offset + 2] << 8) | data[offset + 3]);
			this.reserved = (byte)((data[offset + 4] & 0xe0) >> 5);
			this.caPid = (ushort)(((data[offset + 4] & 0x07) << 8) | data[offset + 5]);
			this.privateDataBytes = new byte[this.DescriptorLength - 4];
			Array.Copy(data, offset + 6, this.privateDataBytes, 0, this.DescriptorLength - 4);
		}

		public override string ToStringDescriptorOnly(string prefix)
		{
			string result = "";
			result += base.ToStringDescriptorOnly(prefix);
			result += prefix + "CaSystemId: " + string.Format(" 0x{0:x4} ({1})\r\n", this.caSystemId, this.caSystemId);
			result += prefix + "Reserved: " + string.Format(" 0x{0:x4} ({1})\r\n", this.reserved, this.reserved);
			result += prefix + "CaPid: " + string.Format(" 0x{0:x4} ({1})\r\n", this.caPid, this.caPid);
			result += PSISection.ToStringByteArray(this.privateDataBytes, 0, this.privateDataBytes.Length, 8, prefix);
			return result;
		}

		public override string ToString(string prefix)
		{
			return prefix + "PSIDescriptorCA\r\n" + prefix + "{\r\n" + ToStringDescriptorOnly(prefix + "\t") + prefix + "}";
		}
	}

	class PSIDescriptorDataBroadcastId : PSIDescriptor
	{
		//DESCRIPTOR_TAGS.DESCR_DATA_BROADCAST_ID:
		// From ETSI EN 300 468 - V1.5,1
		// 6.2.12 _ Table 31 _ Data broadcast id descriptor
		// data_broadcast_id_descriptor() {
		//		descriptor_tag							8
		//		descriptor_length						8
		//		data_broadcast_id						16
		//		for (i = 0; i < N; i++) {
		//			id_selector_byte					8
		//		}
		// }

		private ushort dataBroadcastId;
		private byte[] idSelectorBytes = new byte[0];

		public ushort DataBroadcastId { get { return this.dataBroadcastId; } set { this.dataBroadcastId = value; } }
		public byte[] IdSelectorBytes { get { return this.idSelectorBytes; } }

		public override void Parse(byte[] data, int offset)
		{
			base.Parse(data, offset);
			this.dataBroadcastId = (ushort)((data[offset + 2] << 8) | data[offset + 3]);
			this.idSelectorBytes = new byte[this.DescriptorLength - 2];
			Array.Copy(data, offset + 4, this.idSelectorBytes, 0, this.DescriptorLength - 2);
		}

		public override string ToStringDescriptorOnly(string prefix)
		{
			string result = "";
			result += base.ToStringDescriptorOnly(prefix);
			result += prefix + "DataBroadcastId: " + string.Format(" 0x{0:x4} ({1})\r\n", this.dataBroadcastId, this.dataBroadcastId);
			result += PSISection.ToStringByteArray(this.idSelectorBytes, 0, this.idSelectorBytes.Length, 8, prefix);
			return result;
		}

		public override string ToString(string prefix)
		{
			return prefix + "PSIDescriptorDataBroadcastId\r\n" + prefix + "{\r\n" + ToStringDescriptorOnly(prefix + "\t") + prefix + "}";
		}
	}

	class PSIDescriptorService : PSIDescriptor
	{
		// DESCRIPTOR_TAGS.DESCR_SERVICE
		// From ETSI EN 300 468 - V1.6,1
		// 6.2.31 _ Table 74 _ Service descriptor
		// service_descriptor() {
		//		descriptor_tag							8
		//		descriptor_length						8
		//		service_type							8
		//		service_provider_name_length			8
		//		for (i = 0; i < N; i++) {
		//			char								8
		//		}
		//		service_name_length						8
		//		for (i = 0; i < N; i++) {
		//			char								8
		//		}
		// }

		private SERVICE_TYPES serviceType;
		private byte serviceProviderNameLength;
		private string serviceProviderName;
		private byte serviceNameLength;
		private string serviceName;

		public SERVICE_TYPES ServiceType { get { return this.serviceType; } set { this.serviceType = value; } }
		public byte ServiceProviderNameLength { get { return this.serviceProviderNameLength; } set { this.serviceProviderNameLength = value; } }
		public string ServiceProviderName { get { return this.serviceProviderName; } set { this.serviceProviderName = value; } }
		public byte ServiceNameLength { get { return this.serviceNameLength; } set { this.serviceNameLength = value; } }
		public string ServiceName { get { return this.serviceName; } set { this.serviceName = value; } }

		public override void Parse(byte[] data, int offset)
		{
			base.Parse(data, offset);
			this.serviceType = (SERVICE_TYPES)data[offset + 2];
			this.serviceProviderNameLength = data[offset + 3];
			char[] serviceProviderName = new char[this.serviceProviderNameLength];
			Array.Copy(data, offset + 4, serviceProviderName, 0, this.serviceProviderNameLength);
			this.serviceProviderName = new string(serviceProviderName);
			this.serviceNameLength = data[offset + 4 + this.serviceProviderNameLength];
			char[] serviceName = new char[this.serviceNameLength];
			Array.Copy(data, offset + 5 + this.serviceProviderNameLength, serviceName, 0, this.serviceNameLength);
            foreach (var c in serviceName)
                if (c >= 32)
                    this.serviceName += c;
			//this.serviceName = new string(serviceName);
		}

		public override string ToStringDescriptorOnly(string prefix)
		{
			string result = "";
			result += base.ToStringDescriptorOnly(prefix);
			result += prefix + "ServiceType: " + this.serviceType + " " + string.Format(" 0x{0:x4} ({1})\r\n", (byte)this.serviceType, (byte)this.serviceType);
			result += prefix + "ServiceProviderNameLength: " + string.Format(" 0x{0:x4} ({1})\r\n", this.serviceProviderNameLength, this.serviceProviderNameLength);
			result += prefix + "ServiceProviderName: \"" + this.serviceProviderName + "\"\r\n";
			result += prefix + "ServiceNameLength: " + string.Format(" 0x{0:x4} ({1})\r\n", this.serviceNameLength, this.serviceNameLength);
			result += prefix + "ServiceName: \"" + this.serviceName + "\"\r\n";
			return result;
		}

		public override string ToString(string prefix)
		{
			return prefix + "PSIDescriptorService\r\n" + prefix + "{\r\n" + ToStringDescriptorOnly(prefix + "\t") + prefix + "}";
		}
	}
}
