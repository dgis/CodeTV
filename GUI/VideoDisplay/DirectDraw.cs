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
using System.Runtime.InteropServices;

namespace CodeTV
{
	public class DirectDraw
	{
		////extern HRESULT WINAPI DirectDrawCreate( GUID *lpGUID, LPDIRECTDRAW *lplpDD, IUnknown *pUnkOuter );
		[DllImport("ddraw.dll", EntryPoint = "DirectDrawCreate")]
		private static extern void DirectDrawCreate(IntPtr gui, out IDirectDraw lpDD, IntPtr unkOuter);

	    enum DDWAITVBFLAGS
		{
			BLOCKBEGIN = 1,
			BLOCKBEGINEVENT = 2,
			BLOCKEND = 4
		};

		IDirectDraw directDraw = null;

		public void Init()
		{
			directDraw = null;
			DirectDrawCreate(IntPtr.Zero, out directDraw, IntPtr.Zero);
		}

		public void WaitForVerticalBlank()
		{
			if (directDraw != null)
			{
				//int hr = directDraw.WaitForVerticalBlank((int)DDWAITVBFLAGS.BLOCKBEGIN, 0);
				int hr = directDraw.WaitForVerticalBlank((int)DDWAITVBFLAGS.BLOCKBEGIN, 0);
			}
		}
	}

    [ComImport,
    Guid("6C14DB80-A733-11CE-A521-0020AF0BE560"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectDraw
    {
		[PreserveSig]
		int Compact();
		[PreserveSig]
		int CreateClipper();//DWORD, LPDIRECTDRAWCLIPPER FAR*, IUnknown FAR * );
		[PreserveSig]
		int CreatePalette();//DWORD, LPPALETTEENTRY, LPDIRECTDRAWPALETTE FAR*, IUnknown FAR * );
		[PreserveSig]
		int CreateSurface();//LPDDSURFACEDESC, LPDIRECTDRAWSURFACE FAR *, IUnknown FAR *);
		[PreserveSig]
		int DuplicateSurface();//LPDIRECTDRAWSURFACE, LPDIRECTDRAWSURFACE FAR * );
		[PreserveSig]
		int EnumDisplayModes();//DWORD, LPDDSURFACEDESC, LPVOID, LPDDENUMMODESCALLBACK );
		[PreserveSig]
		int EnumSurfaces();//DWORD, LPDDSURFACEDESC, LPVOID,LPDDENUMSURFACESCALLBACK );
		[PreserveSig]
		int FlipToGDISurface();
		[PreserveSig]
		int GetCaps();//LPDDCAPS, LPDDCAPS);
		[PreserveSig]
		int GetDisplayMode();//LPDDSURFACEDESC);
		[PreserveSig]
		int GetFourCCCodes();//LPDWORD, LPDWORD );
		[PreserveSig]
		int GetGDISurface();//LPDIRECTDRAWSURFACE FAR *);
		[PreserveSig]
		int GetMonitorFrequency();//LPDWORD);
		[PreserveSig]
		int GetScanLine();//LPDWORD);
		[PreserveSig]
		int GetVerticalBlankStatus();//LPBOOL );
		[PreserveSig]
		int Initialize();//GUID FAR *);
		[PreserveSig]
		int RestoreDisplayMode();
		[PreserveSig]
		int SetCooperativeLevel();//HWND, DWORD);
		[PreserveSig]
		int SetDisplayMode();//DWORD, DWORD,DWORD);
		[PreserveSig]
		int WaitForVerticalBlank(int flags, int handle); //DWORD, HANDLE);
		//int WaitForVerticalBlank(int flags, int handle, out IntPtr status); //DWORD, HANDLE);
    }

//    DECLARE_INTERFACE_( IDirectDraw, IUnknown )
//{
//    STDMETHOD(Compact)(THIS) PURE;
//    STDMETHOD(CreateClipper)(THIS_ DWORD, LPDIRECTDRAWCLIPPER FAR*, IUnknown FAR * ) PURE;
//    STDMETHOD(CreatePalette)(THIS_ DWORD, LPPALETTEENTRY, LPDIRECTDRAWPALETTE FAR*, IUnknown FAR * ) PURE;
//    STDMETHOD(CreateSurface)(THIS_  LPDDSURFACEDESC, LPDIRECTDRAWSURFACE FAR *, IUnknown FAR *) PURE;
//    STDMETHOD(DuplicateSurface)( THIS_ LPDIRECTDRAWSURFACE, LPDIRECTDRAWSURFACE FAR * ) PURE;
//    STDMETHOD(EnumDisplayModes)( THIS_ DWORD, LPDDSURFACEDESC, LPVOID, LPDDENUMMODESCALLBACK ) PURE;
//    STDMETHOD(EnumSurfaces)(THIS_ DWORD, LPDDSURFACEDESC, LPVOID,LPDDENUMSURFACESCALLBACK ) PURE;
//    STDMETHOD(FlipToGDISurface)(THIS) PURE;
//    STDMETHOD(GetCaps)( THIS_ LPDDCAPS, LPDDCAPS) PURE;
//    STDMETHOD(GetDisplayMode)( THIS_ LPDDSURFACEDESC) PURE;
//    STDMETHOD(GetFourCCCodes)(THIS_  LPDWORD, LPDWORD ) PURE;
//    STDMETHOD(GetGDISurface)(THIS_ LPDIRECTDRAWSURFACE FAR *) PURE;
//    STDMETHOD(GetMonitorFrequency)(THIS_ LPDWORD) PURE;
//    STDMETHOD(GetScanLine)(THIS_ LPDWORD) PURE;
//    STDMETHOD(GetVerticalBlankStatus)(THIS_ LPBOOL ) PURE;
//    STDMETHOD(Initialize)(THIS_ GUID FAR *) PURE;
//    STDMETHOD(RestoreDisplayMode)(THIS) PURE;
//    STDMETHOD(SetCooperativeLevel)(THIS_ HWND, DWORD) PURE;
//    STDMETHOD(SetDisplayMode)(THIS_ DWORD, DWORD,DWORD) PURE;
//    STDMETHOD(WaitForVerticalBlank)(THIS_ DWORD, HANDLE ) PURE;
//};
}
