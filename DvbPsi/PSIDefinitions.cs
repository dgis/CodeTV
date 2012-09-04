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
using System.Collections.Generic;
using System.Text;

namespace CodeTV.PSI
{
	public enum TABLE_IDS
	{
		PAT = 0x00,
		CAT = 0x01,
		PMT = 0x02,
		NIT_ACTUAL = 0x40,
		NIT_OTHER = 0x41,
		SDT_ACTUAL = 0x42,
		SDT_OTHER = 0x46,
		BAT = 0x4A,
	}

	public enum PIDS
	{
		PAT = 0x00,
		CAT = 0x01,
		NIT = 0x10,
		SDT = 0x11,
		EIT = 0x12,
		TDT = 0x14,
	}

	public enum FILTER_STATUS
	{
		FILTER_COMPLETED,
		FILTER_DATA_ARRIVAL,
		FILTER_NO_SIGNAL,
		FILTER_START_ERROR,
		FILTER_INVALID_TABLE_ID,
		FILTER_CRC_ERROR,
		FILTER_ID_DONT_MATCH,
		FILTER_TIMEOUT,
		FILTER_ABORTED,
		FILTER_DATA_ERROR,
	}

	public enum CONTROL_ACCESS
	{
		ID_CRYPT_SECA = 0x0100,			// Seca
		ID_CRYPT_VIACCESS = 0x0500,		// Viaccess (France Telecom)
		ID_CRYPT_IRDETO = 0x0600,		//Irdeto (Irdeto)
		//ID_CRYPT_VIDEOGUARD = 0x0900,	//  (News Datacom)
		//ID_CRYPT_CRYPTOWORKS = 0x0d00,//  (Philips)
		//ID_CRYPT_POWERVU = 0x0e00,	//  (Scientific Atlanta)
		//ID_CRYPT_BETACRYPT = 0x1700,	//  (Beta Technik)
		ID_CRYPT_NAGRAVISION = 0x1800,	// Nagravision
	}

	public enum PROVIDERS
	{
		ID_ABSAT = 0x0025,			// ABSTA

		ID_DIGITAL = 0x0064,		// Digital+
		ID_CYFRA = 0x0065,			// Cyfra
		ID_CSN = 0x0080,			// S2 CSN
		ID_S2CANAL = 0x0081,		// S2 CANAL+
		ID_S2NUMERICABLE = 0x0084,	// S2 Numericable
		ID_S2NUMERICABLE2 = 0x0085,	// S2 Numericable2

		ID_CSNUPC = 0x007800,		// S2 CSN/UPC
		ID_TPS = 0x007C00,			// TPS

		ID_V2TPS = 0x015000,		// V2_TPS
	}



	// Descriptor Identifiers
	public enum DESCRIPTOR_TAGS
	{
		/* defined by ISO/IEC 13818-1 */

		/* 0x00 - 0x01 */
		/* Reserved */
		DESCR_VIDEO_STREAM = 0x02,
		DESCR_AUDIO_STREAM = 0x03,
		DESCR_HIERARCHY = 0x04,
		DESCR_REGISTRATION = 0x05,
		DESCR_DATA_STREAM_ALIGN = 0x06,
		DESCR_TARGET_BACKGRID = 0x07,
		DESCR_VIDEO_WINDOW = 0x08,
		DESCR_CA = 0x09,
		DESCR_ISO_639_LANGUAGE = 0x0A,
		DESCR_SYSTEM_CLOCK = 0x0B,
		DESCR_MULTIPLEX_BUFFER_UTIL = 0x0C,
		DESCR_COPYRIGHT = 0x0D,
		DESCR_MAXIMUM_BITRATE = 0x0E,
		DESCR_PRIVATE_DATA_IND = 0x0F,
		DESCR_SMOOTHING_BUFFER = 0x10,
		DESCR_STD = 0x11,
		DESCR_IBP = 0x12,
		/* 0x13 - 0x3F */
		/* Reserved */

		/* defined by ETSI */
		DESCR_NW_NAME = 0x40,
		DESCR_SERVICE_LIST = 0x41,
		DESCR_STUFFING = 0x42,
		DESCR_SAT_DEL_SYS = 0x43,
		DESCR_CABLE_DEL_SYS = 0x44,
		DESCR_VBI_DATA = 0x45,
		DESCR_VBI_TELETEXT = 0x46,
		DESCR_BOUQUET_NAME = 0x47,
		DESCR_SERVICE = 0x48,
		DESCR_COUNTRY_AVAIL = 0x49,
		DESCR_LINKAGE = 0x4A,
		DESCR_NVOD_REF = 0x4B,
		DESCR_TIME_SHIFTED_SERVICE = 0x4C,
		DESCR_SHORT_EVENT = 0x4D,
		DESCR_EXTENDED_EVENT = 0x4E,
		DESCR_TIME_SHIFTED_EVENT = 0x4F,
		DESCR_COMPONENT = 0x50,
		DESCR_MOSAIC = 0x51,
		DESCR_STREAM_ID = 0x52,
		DESCR_CA_IDENT = 0x53,
		DESCR_CONTENT = 0x54,
		DESCR_PARENTAL_RATING = 0x55,
		DESCR_TELETEXT = 0x56,
		DESCR_TELEPHONE = 0x57,
		DESCR_LOCAL_TIME_OFF = 0x58,
		DESCR_SUBTITLING = 0x59,
		DESCR_TERR_DEL_SYS = 0x5A,
		DESCR_ML_NW_NAME = 0x5B,
		DESCR_ML_BQ_NAME = 0x5C,
		DESCR_ML_SERVICE_NAME = 0x5D,
		DESCR_ML_COMPONENT = 0x5E,
		DESCR_PRIV_DATA_SPEC = 0x5F,
		DESCR_SERVICE_MOVE = 0x60,
		DESCR_SHORT_SMOOTH_BUF = 0x61,
		DESCR_FREQUENCY_LIST = 0x62,
		DESCR_PARTIAL_TP_STREAM = 0x63,
		DESCR_DATA_BROADCAST = 0x64,
		DESCR_CA_SYSTEM = 0x65,
		DESCR_DATA_BROADCAST_ID = 0x66,
		DESCR_TRANSPORT_STREAM = 0x67,
		DESCR_DSNG = 0x68,
		DESCR_PDC = 0x69,
		DESCR_AC3 = 0x6A,
		DESCR_ANCILLARY_DATA = 0x6B,
		DESCR_CELL_LIST = 0x6C,
		DESCR_CELL_FREQ_LINK = 0x6D,
		DESCR_ANNOUNCEMENT_SUPPORT = 0x6E,
        // Ajout Hervé Stalin
        DESCR_ENHANCED_AC3 = 0x7A,
        DESCR_AAC = 0x7C,
        DESCR_LOGICAL_CHANNEL = 0x83,
        DESCR_HD_SIMULCAST_LOGICAL_CHANNEL = 0x88,
		/* 0x6f - 0x7f */
		/* Reserved */
		/* 0x80 - 0xfe */
		/* User private */
		/* 0xff */
		/* Forbidden */
	}

	public enum SERVICE_TYPES
	{
		RESERVED_SERVICE = 0x00,
		DIGITAL_TELEVISION_SERVICE = 0x01,
		DIGITAL_RADIO_SOUND_SERVICE = 0x02,
		TELETEXT_SERVICE = 0x03,
		NVOD_REFERENCE_SERVICE = 0x04,
		NVOD_TIME_SHIFTED_SERVICE = 0x05,
		MOSAIC_SERVICE = 0x06,
		PAL_CODED_SIGNAL = 0x07,
		SECAM_CODED_SIGNAL = 0x08,
		D_D2_MAC = 0x09,
        //Hervé stalin : remplacement de service type obsolete 
        //FM_RADIO = 0x0A,
        //NTSC_CODED_SIGNAL = 0x0B,
        ADVANCED_CODEC_DIGITAL_RADIO_SOUND_SERVICE = 0x0A,
        ADVANCED_CODEC_MOSAIC_SERVICE = 0x0B,
		DATA_BROADCAST_SERVICE = 0x0C,
		RESERVED_FOR_COMMON_INTERFACE_USAGE = 0x0D,
		RCS_MAP = 0x0E,
		RCS_FLS = 0x0F,
		DVB_MHP_SERVICE = 0x10,
        //Hervé Stalin : ajout de service_type
        MPEG2_HD_DIGITAL_TELEVISION_SERVICE = 0x11,
        ADVANCE_CODEC_SD_DIGITAL_SERVICE = 0x16,
        ADVANCE_CODEC_SD_NVOD_TIME_SHIFTED_SERVICE = 0x17,
        ADVANCE_CODEC_SD_NVOD_REFERENCE_SERVICE = 0x18,
        ADVANCE_CODEC_HD_DIGITAL_SERVICE = 0x19,
        ADVANCE_CODEC_HD_NVOD_TIME_SHIFTED_SERVICE = 0x1A,
        ADVANCE_CODEC_HD_NVOD_REFERENCE_SERVICE = 0x1B,
        RESERVED_SERVICE2 = 0xFF,
    };

	// Elementary stream identifiers
	public enum STREAM_TYPES
	{
		/* 0x00 */
		/* Reserved */
		STREAMTYPE_11172_VIDEO = 0x01,
		STREAMTYPE_13818_VIDEO = 0x02,
		STREAMTYPE_11172_AUDIO = 0x03,
		STREAMTYPE_13818_AUDIO = 0x04,
		STREAMTYPE_13818_PRIVATE = 0x05,
		STREAMTYPE_13818_PES_PRIVATE = 0x06,
		STREAMTYPE_13522_MHPEG = 0x07,
		STREAMTYPE_13818_DSMCC = 0x08,
		STREAMTYPE_ITU_222_1 = 0x09,
		STREAMTYPE_13818_A = 0x0a,
		STREAMTYPE_13818_B = 0x0b,
		STREAMTYPE_13818_C = 0x0c,
		STREAMTYPE_13818_D = 0x0d,
		STREAMTYPE_13818_AUX = 0x0e,
		/* 0x0F - 0x7F */
		/* Reserved */
		/* 0x80 - 0xFF */
		/* User private */
	}

	public enum AUDIO_TYPE
	{
		UNDEFINED = 0x00,
		CLEAN_EFFECTS = 0x01,
		HEARING_IMPAIRED = 0x02,
		VISUAL_IMPAIRED_COMMENTARY = 0x03,
		RESERVED = 0x04,
	}

	public class DVBServiceDescriptor
	{
		public SERVICE_TYPES _serviceType;
		public string _providerName;
		public string _serviceName;
	}

	public class DVBAudioInfo
	{
		public ushort _audioId;
		public string _name;
		public AUDIO_TYPE _type;
	}

	public class DVBAC3AudioInfo
	{
		public ushort _audioId;
		public string _name;
		public AUDIO_TYPE _type;
	}

	public class DVBSubtitleInfo
	{
		public ushort _subtitleId;
		public string _name;
		public byte _type;
		public ushort _compositionPageId;
		public ushort _auxiliaryPageId;
	}

	public class DVBECMInfo
	{
		public CONTROL_ACCESS _caType;
		public PROVIDERS _providerID;
		public ushort _ecmId;
	}

	public class DVBEMMInfo
	{
		public CONTROL_ACCESS _caType;
		public PROVIDERS _providerID;
		public ushort _emmId;
	}
}
