CodeTV is a free DirectShow based TV software for Windows. 
The home page is located at http://regis.cosnier.free.fr.


Version 0.3 Copyright (c) 2006-2012 Regis Cosnier, All Rights Reserved.
This program is free software and may be distributed according to the terms of the GNU General Public License (GPL).


FEATURES:

- GPL
- Need of the Microsoft Framework.NET 3.5 (if not already install).
- Display analog and digital live video.
	* Analog tv or webcam device with WDM drivers.
	* Digital tv (DVB-T, DVB-S, DVB-C) device with BDA drivers.
- As fast as possible channel switching (This is the reason why I made this software).
- Still image capture (snapshot).
- Timeshifting (DVB only).
- Video recording (DVB only) in a ".dvr-ms" Microsoft proprietary file format
 (Because of the use of the DirectShow StreamBufferEngine).
	* Can be played with any DirectShow media player.
	* Information on Wikipedia http://en.wikipedia.org/wiki/DVR-MS
	* A tools to convert in mpeg2 http://msmvps.com/blogs/chrisl/archive/2006/08/10/107296.aspx


QUICK START:

- Launch the channel wizard.
- Select the parameters like the capture/tuner devices.
- Scan the channel over a frequency range.
- Double click on a channel and I hope for you it will work/


KEYBOARD SHORTCUTS:
- 'SPACE' : snapshot
- '+' : Next numbered channel
- '-' : Previous numbered channel
- PAGE UP : Volume up
- PAGE DOWN : Volume down
- MOUSE WHEEL UP : Volume up
- MOUSE WHEEL DOWN : Volume down


TIPS:

- Some DVB drivers use 1 or 2 codecs.
	* With a 2 drivers model, fill the "Tuner Device" and the "Capture Device" channel settings.
	* With a 1 driver model, fill the "Tuner Device" and leave the "Capture Device" blank.
- Codecs
	* Video
		- MPEG2 Decoder Device="ffdshow Video Decoder" (http://ffdshow-tryout.sourceforge.net/)
		- MPEG2 Decoder Device="Microsoft DTV-DVD Video Decoder" found in Windows 7.
		- H264 Decoder Device="ffdshow Video Decoder"
		- H264 Decoder Device="Microsoft DTV-DVD Video Decoder".
		- H264 Decoder Device="CyberLink H.264/AVC Decoder (PDVD7)" found in PowerDVD.
	* Audio
		- Audio Decoder Device="ffdshow Audio Decoder"
			but AC3Filter Lite (http://ac3filter.net/) must be installed for HD audio (AC3 or EAC3).

- Select several channels (with CONTROL and/or SHIFT keys) and select "Property" with
	the contextual menu to change the common properties (i.e.: If you want to change the
	video decoder device for several channels at once).
- Once a channel is tuned, you can tweak lot of codecs settings under the "Video/Filters" menu.
- To avoid synchronisation issue on TV out, just put the TV out as the main monitor. The
	synchronisation issue seems to appear on the secondary monitor. If it still not works,
	try the ReClock filter (http://reclock.free.fr/main.htm)
- For european, in order to avoid some jerky video you should put the monitor frequency to be
	a multiple of 25. For instance:
	* A PAL DVB video with 25 frames per second should be fluid on a LCD monitor set to 75 Hz.
	* A NTSC DVB video with 30 frames per second should be fluid on a LCD monitor set to 60 Hz.
- If an aspect ratio issue occurs, try another Mpeg2 codec (Mpeg2Dec, DScaler Mpeg2 decoder,
	Elecard Mpeg-2 decoder, ...), or try to adjust the ratio and save it to the channel.
- The GeForce 6600GT TV out seems a lot better than the ATI 9600.
- To read recorded .ts file you need the TSFileSource filter
	(http://nate.dynalias.net/TSFileSource/TSFileSource.html).
- You can reorder the toolbar element with the mouse by maintaining the ALT key.
- Microsoft DirectShow upgrades
	* Windows XP TV tuner program stops responding or displays corrupted video
		http://support.microsoft.com/?kbid=896626
	* Some USB audio devices and some USB audio TV tuners do not work correctly together
		with a Windows Vista-based computer http://support.microsoft.com/?kbid=933262


TODO LIST:

- Add a log file!
- Add the possibility to stop the live video
- Bug: Snapshot in video file mode does not work
- Bug in the remote command REC/PAUSE
- Programable recording
- Save the normal and restore window position
- Bug: In TV mode, if we minimize from the task bar, then the restoration display the TV mode in a very small window.
- Add a timeout to the message display in the statusbar
- Put only one TimeShiftingBufferLength (=TimeShiftingBufferLengthMin=TimeShiftingBufferLengthMax)
- Add an explorer access to the snapshot and recorded video folder
- Put the process priority in the settings
- Bug: Sometimes the video is badly interlaced (Look at the "VIDEOINFOHEADER2 struct")
	-> Maybe come from terratec driver?
	-> It seems to come from the DirectShow VMR Renderer
- Bug: Recording start on the live timestamp and not on what it is displayed!
- In TV mode, moving the windows by dragging inside the window
- Add audio/subtitle selection (See document ETSI en_300743v010201o-Subtitling systems.pdf)
- Image (brigthness/contrast/hue/saturation) tuning in Rendering Settings (by channel ?)
- Put the already built stopped graph in a cache in order to get quicker rebuild!
- Add key mapping settings
- Add OSD (in XAML?)
- Add an audio/video sync delay filter
- Mosaic
- Add KastorTV plugin for analogic
- Teletext
- Use a PropertyGrid to show the PAT/PMT/SDT result
- Make a plugin system to do anythings with the directshow graph


CHANGES:

version 0.3 (2012-11-19)
- Add support for AC3 and EAC3 audio.
- Add support for EVR instead of VMR. EVR gives a better quality on Vista or Seven.
- The DVB frequency scanner now wait 500ms before trying to get the channels.
- Add the project to GitHub: https://github.com/dgis/CodeTV
- Clean up the project file to target the corresponding Visual Studio version (2005, 2008, 2010, 2012)
- Add DirectShowNet version 2.1
- Fix the proppage.dll on 64bit system.
- Add the Hervé changes:
	* Parser
		- The parser tables is now in an external dll to be reused in another program.
		- Parsing of NIT table.
		- Parsing the descriptors logical_channel_number and HD_simulcast_logical_channel_number.
	* Interface
		- Add the logical_channel_number to the channels.
		- Add data from the NIT table in the channel and the tranponder.

version 0.2 (2007-11-04)
- Change the WPF initialization if used
- Set the focus to the video control in order to get the keyboard shortcut working

version 0.1 (2007-11-01) (first public release)
- The transponders files puts in the resources!
- Set "CodeTV" as prefix for all the setting files
- Upgrade to DirectShowNet Library v 2.0
- Add the possibility to number the channels
- Put all hardcoded strings in the resource for future localization
	(Except for the PropertyGrid Category/Display/Description
	 Solution here: http://www.codeproject.com/csharp/wdzpropertygridutils.asp
	      and here: http://www.codeproject.com/cs/miscctrl/gpg_revisited.asp)
- Cosmetic: Icon, tooltips, about...
- Add the possibility to play a regular video file
- Remove scanner panel and add a wizard to create the channels.
- Remove the automatic selection of the channel property when changing the channel
- Replace Recording and Timeshifting panel with a toolbar
- Put the recorder file path configurable in the general settings
- Remove the snapshot panel and the save to a folder configured in the general settings!!!
- In TV mode, the resizing must be adjust following the aspect ratio
- Set the panel size to a fix value when resizing the main window (and not in percentage)
- In analogic WDM, add the crossbar audio and video source
- Add Mpeg2 audio decoder
- Add VideoAnalyzer for MPEG2 FastForward in the time shifting mode
- New GUI using the DockPanel Suite (http://sourceforge.net/projects/dockpanelsuite/)
- Video zoom/ratio/... editor
- Recorder
- TimeShifting
- Channel pids update (Pids seem to change between MPEG2 and H264)
- PanScan mode + ratio choice + Zoom
- Put global preferences
- Allowing to choose the prefered resolution in analogic (WDM)
- WinLIRC support
- Pseudo support for the H264 added (for DVB-T TPS-Star/Sport+)
- Allowing to choose the audio/Video decoder and renderer
- Add a method NeedRebuildGraph in the channel (like comparaison operator)
- Multiselect treeview
- Bug Fixed: Audio/video desynchronisation in analogic (Bad input device)
- Add analogic tuner + cam capture
- Frequency scan
- Decode PSI descriptors
- Logo on channels with relative path
- Use the PropertyGrid for the Channel property
- Transponder mosaic (sync problem between channels!)
- Snapshot


LICENSE:

Copyright (c) 2006-2012 Regis COSNIER, All Rights Reserved.

This program is free software and may be distributed
according to the terms of the GNU General Public License (GPL).


CREDITS:

Thanks to the DirectShowLibNET library (http://directshownet.sourceforge.net).
The panels windows are credited to Weifen Luo (http://sourceforge.net/projects/dockpanelsuite).
Some icons are credited to Mark James (http://www.famfamfam.com/lab/icons/silk/).

Hervé Stalin for the PSI parsing and channel numbering.
