// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
// *** When you make some changes to this file, dont forget to update Tests/TestHelper class ***

// NOT COMPLETE

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using static System.Windows.Forms.XplatUIDriver;

#if PUBLIC_TYPES
namespace Mono.Winforms 
#else
namespace System.Windows.Forms
#endif
{
    #region Private Structs
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct WNDCLASS
    {
        internal int style;
        internal WndProc lpfnWndProc;
        internal int cbClsExtra;
        internal int cbWndExtra;
        internal IntPtr hInstance;
        internal IntPtr hIcon;
        internal IntPtr hCursor;
        internal IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string lpszClassName;
    }


    internal enum SPIAction
    {
        SPI_GETACTIVEWINDOWTRACKING = 0x1000,
        SPI_GETACTIVEWNDTRKTIMEOUT = 0x2002,
        SPI_GETANIMATION = 0x0048,
        SPI_GETCARETWIDTH = 0x2006,
        SPI_GETCOMBOBOXANIMATION = 0x1004,
        SPI_GETDRAGFULLWINDOWS = 0x0026,
        SPI_GETDROPSHADOW = 0x1024,
        SPI_GETFONTSMOOTHING = 0x004A,
        SPI_GETFONTSMOOTHINGCONTRAST = 0x200C,
        SPI_GETFONTSMOOTHINGTYPE = 0x200A,
        SPI_GETGRADIENTCAPTIONS = 0x1008,
        SPI_GETHOTTRACKING = 0x100E,
        SPI_GETICONTITLEWRAP = 0x0019,
        SPI_GETKEYBOARDSPEED = 0x000A,
        SPI_GETKEYBOARDDELAY = 0x0016,
        SPI_GETKEYBOARDCUES = 0x100A,
        SPI_GETKEYBOARDPREF = 0x0044,
        SPI_GETLISTBOXSMOOTHSCROLLING = 0x1006,
        SPI_GETMENUANIMATION = 0x1002,
        SPI_GETMENUDROPALIGNMENT = 0x001B,
        SPI_GETMENUFADE = 0x1012,
        SPI_GETMENUSHOWDELAY = 0x006A,
        SPI_GETMOUSESPEED = 0x0070,
        SPI_GETSELECTIONFADE = 0x1014,
        SPI_GETSNAPTODEFBUTTON = 0x005F,
        SPI_GETTOOLTIPANIMATION = 0x1016,
        SPI_GETWORKAREA = 0x0030,
        SPI_GETMOUSEHOVERWIDTH = 0x0062,
        SPI_GETMOUSEHOVERHEIGHT = 0x0064,
        SPI_GETMOUSEHOVERTIME = 0x0066,
        SPI_GETUIEFFECTS = 0x103E,
        SPI_GETWHEELSCROLLLINES = 0x0068
    }

    internal enum WindowPlacementFlags
    {
        SW_HIDE = 0,
        SW_SHOWNORMAL = 1,
        SW_NORMAL = 1,
        SW_SHOWMINIMIZED = 2,
        SW_SHOWMAXIMIZED = 3,
        SW_MAXIMIZE = 3,
        SW_SHOWNOACTIVATE = 4,
        SW_SHOW = 5,
        SW_MINIMIZE = 6,
        SW_SHOWMINNOACTIVE = 7,
        SW_SHOWNA = 8,
        SW_RESTORE = 9,
        SW_SHOWDEFAULT = 10,
        SW_FORCEMINIMIZE = 11,
        SW_MAX = 11
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WINDOWPLACEMENT
    {
        internal uint length;
        internal uint flags;
        internal WindowPlacementFlags showCmd;
        internal POINT ptMinPosition;
        internal POINT ptMaxPosition;
        internal RECT rcNormalPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NCCALCSIZE_PARAMS
    {
        public RECT rgrc1;
        public RECT rgrc2;
        public RECT rgrc3;
        public IntPtr lppos;
    }

    [Flags]
    internal enum TMEFlags
    {
        TME_HOVER = 0x00000001,
        TME_LEAVE = 0x00000002,
        TME_NONCLIENT = 0x00000010,
        TME_QUERY = unchecked((int)0x40000000),
        TME_CANCEL = unchecked((int)0x80000000)
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TRACKMOUSEEVENT
    {
        internal int size;
        internal TMEFlags dwFlags;
        internal IntPtr hWnd;
        internal int dwHoverTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PAINTSTRUCT
    {
        internal IntPtr hdc;
        internal int fErase;
        internal RECT rcPaint;
        internal int fRestore;
        internal int fIncUpdate;
        internal int Reserved1;
        internal int Reserved2;
        internal int Reserved3;
        internal int Reserved4;
        internal int Reserved5;
        internal int Reserved6;
        internal int Reserved7;
        internal int Reserved8;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        internal short wVk;
        internal short wScan;
        internal Int32 dwFlags;
        internal Int32 time;
        internal UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEINPUT
    {
        internal Int32 dx;
        internal Int32 dy;
        internal Int32 mouseData;
        internal Int32 dwFlags;
        internal Int32 time;
        internal UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HARDWAREINPUT
    {
        internal Int32 uMsg;
        internal short wParamL;
        internal short wParamH;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ICONINFO
    {
        internal bool fIcon;
        internal Int32 xHotspot;
        internal Int32 yHotspot;
        internal IntPtr hbmMask;
        internal IntPtr hbmColor;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct INPUT
    {
        [FieldOffset(0)]
        internal Int32 type;

        [FieldOffset(4)]
        internal MOUSEINPUT mi;

        [FieldOffset(4)]
        internal KEYBDINPUT ki;

        [FieldOffset(4)]
        internal HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ANIMATIONINFO
    {
        internal uint cbSize;
        internal int iMinAnimate;
    }

    internal enum InputFlags
    {
        KEYEVENTF_EXTENDEDKEY = 0x0001,
        KEYEVENTF_KEYUP = 0x0002,
        KEYEVENTF_SCANCODE = 0x0003,
        KEYEVENTF_UNICODE = 0x0004,
    }

    internal enum ClassStyle
    {
        CS_VREDRAW = 0x00000001,
        CS_HREDRAW = 0x00000002,
        CS_KEYCVTWINDOW = 0x00000004,
        CS_DBLCLKS = 0x00000008,
        CS_OWNDC = 0x00000020,
        CS_CLASSDC = 0x00000040,
        CS_PARENTDC = 0x00000080,
        CS_NOKEYCVT = 0x00000100,
        CS_NOCLOSE = 0x00000200,
        CS_SAVEBITS = 0x00000800,
        CS_BYTEALIGNCLIENT = 0x00001000,
        CS_BYTEALIGNWINDOW = 0x00002000,
        CS_GLOBALCLASS = 0x00004000,
        CS_IME = 0x00010000,
        // Windows XP+
        CS_DROPSHADOW = 0x00020000
    }

    internal enum SetWindowPosZOrder
    {
        HWND_TOP = 0,
        HWND_BOTTOM = 1,
        HWND_TOPMOST = -1,
        HWND_NOTOPMOST = -2
    }

    [Flags]
    internal enum SetWindowPosFlags
    {
        SWP_ASYNCWINDOWPOS = 0x4000,
        SWP_DEFERERASE = 0x2000,
        SWP_DRAWFRAME = 0x0020,
        SWP_FRAMECHANGED = 0x0020,
        SWP_HIDEWINDOW = 0x0080,
        SWP_NOACTIVATE = 0x0010,
        SWP_NOCOPYBITS = 0x0100,
        SWP_NOMOVE = 0x0002,
        SWP_NOOWNERZORDER = 0x0200,
        SWP_NOREDRAW = 0x0008,
        SWP_NOREPOSITION = 0x0200,
        SWP_NOENDSCHANGING = 0x0400,
        SWP_NOSIZE = 0x0001,
        SWP_NOZORDER = 0x0004,
        SWP_SHOWWINDOW = 0x0040
    }


    internal enum LoadCursorType
    {
        First = 32512,
        IDC_ARROW = 32512,
        IDC_IBEAM = 32513,
        IDC_WAIT = 32514,
        IDC_CROSS = 32515,
        IDC_UPARROW = 32516,
        IDC_SIZE = 32640,
        IDC_ICON = 32641,
        IDC_SIZENWSE = 32642,
        IDC_SIZENESW = 32643,
        IDC_SIZEWE = 32644,
        IDC_SIZENS = 32645,
        IDC_SIZEALL = 32646,
        IDC_NO = 32648,
        IDC_HAND = 32649,
        IDC_APPSTARTING = 32650,
        IDC_HELP = 32651,
        Last = 32651
    }

    internal enum AncestorType
    {
        GA_PARENT = 1,
        GA_ROOT = 2,
        GA_ROOTOWNER = 3
    }

    [Flags]
    internal enum WindowLong
    {
        GWL_WNDPROC = -4,
        GWL_HINSTANCE = -6,
        GWL_HWNDPARENT = -8,
        GWL_STYLE = -16,
        GWL_EXSTYLE = -20,
        GWL_USERDATA = -21,
        GWL_ID = -12
    }

    [Flags]
    internal enum LogBrushStyle
    {
        BS_SOLID = 0,
        BS_NULL = 1,
        BS_HATCHED = 2,
        BS_PATTERN = 3,
        BS_INDEXED = 4,
        BS_DIBPATTERN = 5,
        BS_DIBPATTERNPT = 6,
        BS_PATTERN8X8 = 7,
        BS_DIBPATTERN8X8 = 8,
        BS_MONOPATTERN = 9
    }

    [Flags]
    internal enum LogBrushHatch
    {
        HS_HORIZONTAL = 0,       /* ----- */
        HS_VERTICAL = 1,       /* ||||| */
        HS_FDIAGONAL = 2,       /* \\\\\ */
        HS_BDIAGONAL = 3,       /* ///// */
        HS_CROSS = 4,       /* +++++ */
        HS_DIAGCROSS = 5,       /* xxxxx */
    }

    internal struct COLORREF
    {
        internal byte R;
        internal byte G;
        internal byte B;
        internal byte A;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LOGBRUSH
    {
        internal LogBrushStyle lbStyle;
        internal COLORREF lbColor;
        internal LogBrushHatch lbHatch;
    }


    public enum TernaryRasterOperations : uint
    {
        SRCCOPY = 0x00CC0020,
        SRCPAINT = 0x00EE0086,
        SRCAND = 0x008800C6,
        SRCINVERT = 0x00660046,
        SRCERASE = 0x00440328,
        NOTSRCCOPY = 0x00330008,
        NOTSRCERASE = 0x001100A6,
        MERGECOPY = 0x00C000CA,
        MERGEPAINT = 0x00BB0226,
        PATCOPY = 0x00F00021,
        PATPAINT = 0x00FB0A09,
        PATINVERT = 0x005A0049,
        DSTINVERT = 0x00550009,
        BLACKNESS = 0x00000042,
        WHITENESS = 0x00FF0062
    }

    [Flags]
    internal enum ScrollWindowExFlags
    {
        SW_NONE = 0x0000,
        SW_SCROLLCHILDREN = 0x0001,
        SW_INVALIDATE = 0x0002,
        SW_ERASE = 0x0004,
        SW_SMOOTHSCROLL = 0x0010
    }

    internal enum SystemMetrics
    {
        SM_CXSCREEN = 0,
        SM_CYSCREEN = 1,
        SM_CXVSCROLL = 2,
        SM_CYHSCROLL = 3,
        SM_CYCAPTION = 4,
        SM_CXBORDER = 5,
        SM_CYBORDER = 6,
        SM_CXDLGFRAME = 7,
        SM_CYDLGFRAME = 8,
        SM_CYVTHUMB = 9,
        SM_CXHTHUMB = 10,
        SM_CXICON = 11,
        SM_CYICON = 12,
        SM_CXCURSOR = 13,
        SM_CYCURSOR = 14,
        SM_CYMENU = 15,
        SM_CXFULLSCREEN = 16,
        SM_CYFULLSCREEN = 17,
        SM_CYKANJIWINDOW = 18,
        SM_MOUSEPRESENT = 19,
        SM_CYVSCROLL = 20,
        SM_CXHSCROLL = 21,
        SM_DEBUG = 22,
        SM_SWAPBUTTON = 23,
        SM_RESERVED1 = 24,
        SM_RESERVED2 = 25,
        SM_RESERVED3 = 26,
        SM_RESERVED4 = 27,
        SM_CXMIN = 28,
        SM_CYMIN = 29,
        SM_CXSIZE = 30,
        SM_CYSIZE = 31,
        SM_CXFRAME = 32,
        SM_CYFRAME = 33,
        SM_CXMINTRACK = 34,
        SM_CYMINTRACK = 35,
        SM_CXDOUBLECLK = 36,
        SM_CYDOUBLECLK = 37,
        SM_CXICONSPACING = 38,
        SM_CYICONSPACING = 39,
        SM_MENUDROPALIGNMENT = 40,
        SM_PENWINDOWS = 41,
        SM_DBCSENABLED = 42,
        SM_CMOUSEBUTTONS = 43,
        SM_CXFIXEDFRAME = SM_CXDLGFRAME,
        SM_CYFIXEDFRAME = SM_CYDLGFRAME,
        SM_CXSIZEFRAME = SM_CXFRAME,
        SM_CYSIZEFRAME = SM_CYFRAME,
        SM_SECURE = 44,
        SM_CXEDGE = 45,
        SM_CYEDGE = 46,
        SM_CXMINSPACING = 47,
        SM_CYMINSPACING = 48,
        SM_CXSMICON = 49,
        SM_CYSMICON = 50,
        SM_CYSMCAPTION = 51,
        SM_CXSMSIZE = 52,
        SM_CYSMSIZE = 53,
        SM_CXMENUSIZE = 54,
        SM_CYMENUSIZE = 55,
        SM_ARRANGE = 56,
        SM_CXMINIMIZED = 57,
        SM_CYMINIMIZED = 58,
        SM_CXMAXTRACK = 59,
        SM_CYMAXTRACK = 60,
        SM_CXMAXIMIZED = 61,
        SM_CYMAXIMIZED = 62,
        SM_NETWORK = 63,
        SM_CLEANBOOT = 67,
        SM_CXDRAG = 68,
        SM_CYDRAG = 69,
        SM_SHOWSOUNDS = 70,
        SM_CXMENUCHECK = 71,
        SM_CYMENUCHECK = 72,
        SM_SLOWMACHINE = 73,
        SM_MIDEASTENABLED = 74,
        SM_MOUSEWHEELPRESENT = 75,
        SM_XVIRTUALSCREEN = 76,
        SM_YVIRTUALSCREEN = 77,
        SM_CXVIRTUALSCREEN = 78,
        SM_CYVIRTUALSCREEN = 79,
        SM_CMONITORS = 80,
        SM_SAMEDISPLAYFORMAT = 81,
        SM_IMMENABLED = 82,
        SM_CXFOCUSBORDER = 83,
        SM_CYFOCUSBORDER = 84,
        SM_TABLETPC = 86,
        SM_MEDIACENTER = 87,
        SM_CMETRICS = 88
    }

    // We'll only support _WIN32_IE < 0x0500 for now
    internal enum NotifyIconMessage
    {
        NIM_ADD = 0x00000000,
        NIM_MODIFY = 0x00000001,
        NIM_DELETE = 0x00000002,
    }

    [Flags]
    internal enum NotifyIconFlags
    {
        NIF_MESSAGE = 0x00000001,
        NIF_ICON = 0x00000002,
        NIF_TIP = 0x00000004,
        NIF_STATE = 0x00000008,
        NIF_INFO = 0x00000010
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct NOTIFYICONDATA
    {
        internal uint cbSize;
        internal IntPtr hWnd;
        internal uint uID;
        internal NotifyIconFlags uFlags;
        internal uint uCallbackMessage;
        internal IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        internal string szTip;
        internal int dwState;
        internal int dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        internal string szInfo;
        internal int uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        internal string szInfoTitle;
        internal ToolTipIcon dwInfoFlags;
    }

    [Flags]
    internal enum DCExFlags
    {
        DCX_WINDOW = 0x00000001,
        DCX_CACHE = 0x00000002,
        DCX_NORESETATTRS = 0x00000004,
        DCX_CLIPCHILDREN = 0x00000008,
        DCX_CLIPSIBLINGS = 0x00000010,
        DCX_PARENTCLIP = 0x00000020,
        DCX_EXCLUDERGN = 0x00000040,
        DCX_INTERSECTRGN = 0x00000080,
        DCX_EXCLUDEUPDATE = 0x00000100,
        DCX_INTERSECTUPDATE = 0x00000200,
        DCX_LOCKWINDOWUPDATE = 0x00000400,
        DCX_USESTYLE = 0x00010000,
        DCX_VALIDATE = 0x00200000
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct CLIENTCREATESTRUCT
    {
        internal IntPtr hWindowMenu;
        internal uint idFirstChild;
    }

    internal enum ClassLong : int
    {
        GCL_MENUNAME = -8,
        GCL_HBRBACKGROUND = -10,
        GCL_HCURSOR = -12,
        GCL_HICON = -14,
        GCL_HMODULE = -16,
        GCL_CBWNDEXTRA = -18,
        GCL_CBCLSEXTRA = -20,
        GCL_WNDPROC = -24,
        GCL_STYLE = -26,
        GCW_ATOM = -32,
        GCL_HICONSM = -34
    }

    [Flags]
    internal enum GAllocFlags : uint
    {
        GMEM_FIXED = 0x0000,
        GMEM_MOVEABLE = 0x0002,
        GMEM_NOCOMPACT = 0x0010,
        GMEM_NODISCARD = 0x0020,
        GMEM_ZEROINIT = 0x0040,
        GMEM_MODIFY = 0x0080,
        GMEM_DISCARDABLE = 0x0100,
        GMEM_NOT_BANKED = 0x1000,
        GMEM_SHARE = 0x2000,
        GMEM_DDESHARE = 0x2000,
        GMEM_NOTIFY = 0x4000,
        GMEM_LOWER = GMEM_NOT_BANKED,
        GMEM_VALID_FLAGS = 0x7F72,
        GMEM_INVALID_HANDLE = 0x8000,
        GHND = (GMEM_MOVEABLE | GMEM_ZEROINIT),
        GPTR = (GMEM_FIXED | GMEM_ZEROINIT)
    }

    internal enum ROP2DrawMode : int
    {
        R2_BLACK = 1,
        R2_NOTMERGEPEN = 2,
        R2_MASKNOTPEN = 3,
        R2_NOTCOPYPEN = 4,
        R2_MASKPENNOT = 5,
        R2_NOT = 6,
        R2_XORPEN = 7,
        R2_NOTMASKPEN = 8,
        R2_MASKPEN = 9,
        R2_NOTXORPEN = 10,
        R2_NOP = 11,
        R2_MERGENOTPEN = 12,
        R2_COPYPEN = 13,
        R2_MERGEPENNOT = 14,
        R2_MERGEPEN = 15,
        R2_WHITE = 16,
        R2_LAST = 16
    }

    internal enum PenStyle : int
    {
        PS_SOLID = 0,
        PS_DASH = 1,
        PS_DOT = 2,
        PS_DASHDOT = 3,
        PS_DASHDOTDOT = 4,
        PS_NULL = 5,
        PS_INSIDEFRAME = 6,
        PS_USERSTYLE = 7,
        PS_ALTERNATE = 8
    }

    internal enum PatBltRop : int
    {
        PATCOPY = 0xf00021,
        PATINVERT = 0x5a0049,
        DSTINVERT = 0x550009,
        BLACKNESS = 0x000042,
        WHITENESS = 0xff0062,
    }

    internal enum StockObject : int
    {
        WHITE_BRUSH = 0,
        LTGRAY_BRUSH = 1,
        GRAY_BRUSH = 2,
        DKGRAY_BRUSH = 3,
        BLACK_BRUSH = 4,
        NULL_BRUSH = 5,
        HOLLOW_BRUSH = NULL_BRUSH,
        WHITE_PEN = 6,
        BLACK_PEN = 7,
        NULL_PEN = 8,
        OEM_FIXED_FONT = 10,
        ANSI_FIXED_FONT = 11,
        ANSI_VAR_FONT = 12,
        SYSTEM_FONT = 13,
        DEVICE_DEFAULT_FONT = 14,
        DEFAULT_PALETTE = 15,
        SYSTEM_FIXED_FONT = 16
    }



    [Flags]
    internal enum SndFlags : int
    {
        SND_SYNC = 0x0000,
        SND_ASYNC = 0x0001,
        SND_NODEFAULT = 0x0002,
        SND_MEMORY = 0x0004,
        SND_LOOP = 0x0008,
        SND_NOSTOP = 0x0010,
        SND_NOWAIT = 0x00002000,
        SND_ALIAS = 0x00010000,
        SND_ALIAS_ID = 0x00110000,
        SND_FILENAME = 0x00020000,
        SND_RESOURCE = 0x00040004,
        SND_PURGE = 0x0040,
        SND_APPLICATION = 0x0080,
    }

    [Flags]
    internal enum LayeredWindowAttributes : int
    {
        LWA_COLORKEY = 0x1,
        LWA_ALPHA = 0x2,
    }

    public enum ACLineStatus : byte
    {
        Offline = 0,
        Online = 1,
        Unknown = 255
    }

    public enum BatteryFlag : byte
    {
        High = 1,
        Low = 2,
        Critical = 4,
        Charging = 8,
        NoSystemBattery = 128,
        Unknown = 255
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SYSTEMPOWERSTATUS
    {
        public ACLineStatus _ACLineStatus;
        public BatteryFlag _BatteryFlag;
        public Byte _BatteryLifePercent;
        public Byte _Reserved1;
        public Int32 _BatteryLifeTime;
        public Int32 _BatteryFullLifeTime;
    }
    #endregion
    internal enum GetSysColorIndex
    {
        COLOR_SCROLLBAR = 0,
        COLOR_BACKGROUND = 1,
        COLOR_ACTIVECAPTION = 2,
        COLOR_INACTIVECAPTION = 3,
        COLOR_MENU = 4,
        COLOR_WINDOW = 5,
        COLOR_WINDOWFRAME = 6,
        COLOR_MENUTEXT = 7,
        COLOR_WINDOWTEXT = 8,
        COLOR_CAPTIONTEXT = 9,
        COLOR_ACTIVEBORDER = 10,
        COLOR_INACTIVEBORDER = 11,
        COLOR_APPWORKSPACE = 12,
        COLOR_HIGHLIGHT = 13,
        COLOR_HIGHLIGHTTEXT = 14,
        COLOR_BTNFACE = 15,
        COLOR_BTNSHADOW = 16,
        COLOR_GRAYTEXT = 17,
        COLOR_BTNTEXT = 18,
        COLOR_INACTIVECAPTIONTEXT = 19,
        COLOR_BTNHIGHLIGHT = 20,
        COLOR_3DDKSHADOW = 21,
        COLOR_3DLIGHT = 22,
        COLOR_INFOTEXT = 23,
        COLOR_INFOBK = 24,

        COLOR_HOTLIGHT = 26,
        COLOR_GRADIENTACTIVECAPTION = 27,
        COLOR_GRADIENTINACTIVECAPTION = 28,
        COLOR_MENUHIGHLIGHT = 29,
        COLOR_MENUBAR = 30,

        COLOR_DESKTOP = 1,
        COLOR_3DFACE = 16,
        COLOR_3DSHADOW = 16,
        COLOR_3DHIGHLIGHT = 20,
        COLOR_3DHILIGHT = 20,
        COLOR_BTNHILIGHT = 20,
        COLOR_MAXVALUE = 24,/* Maximum value */
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct TEXTMETRIC
    {
        internal int tmHeight;
        internal int tmAscent;
        internal int tmDescent;
        internal int tmInternalLeading;
        internal int tmExternalLeading;
        internal int tmAveCharWidth;
        internal int tmMaxCharWidth;
        internal int tmWeight;
        internal int tmOverhang;
        internal int tmDigitizedAspectX;
        internal int tmDigitizedAspectY;
        internal short tmFirstChar;
        internal short tmLastChar;
        internal short tmDefaultChar;
        internal short tmBreakChar;
        internal byte tmItalic;
        internal byte tmUnderlined;
        internal byte tmStruckOut;
        internal byte tmPitchAndFamily;
        internal byte tmCharSet;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        #region Instance Properties
        public int Height { get { return bottom - top; } }
        public int Width { get { return right - left; } }
        public Size Size { get { return new Size(Width, Height); } }
        public Point Location { get { return new Point(left, top); } }
        #endregion

        #region Instance Methods
        public Rectangle ToRectangle()
        {
            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        public static RECT FromRectangle(Rectangle rectangle)
        {
            return new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }

        public override int GetHashCode()
        {
            return left ^ ((top << 13) | (top >> 0x13))
                        ^ ((Width << 0x1a) | (Width >> 6))
                        ^ ((Height << 7) | (Height >> 0x19));
        }

        public override string ToString()
        {
            return String.Format("RECT left={0}, top={1}, right={2}, bottom={3}, width={4}, height={5}", left, top, right, bottom, right - left, bottom - top);
        }
        #endregion

        #region Operator overloads
        public static implicit operator Rectangle(RECT rect)
        {
            return Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
        }

        public static implicit operator RECT(Rectangle rect)
        {
            return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
        #endregion
    }
    [Flags]

	public

	enum WindowStyles : int {
		WS_OVERLAPPED		= 0x00000000,
		WS_POPUP		= unchecked((int)0x80000000),
		WS_CHILD		= 0x40000000,
		WS_MINIMIZE		= 0x20000000,
		WS_VISIBLE		= 0x10000000,
		WS_DISABLED		= 0x08000000,
		WS_CLIPSIBLINGS		= 0x04000000,
		WS_CLIPCHILDREN		= 0x02000000,
		WS_MAXIMIZE		= 0x01000000,
		WS_CAPTION		= 0x00C00000, // == WS_BORDER | WS_DLGFRAME
		WS_BORDER		= 0x00800000,
		WS_DLGFRAME		= 0x00400000,
		WS_VSCROLL		= 0x00200000,
		WS_HSCROLL		= 0x00100000,
		WS_SYSMENU		= 0x00080000,
		WS_THICKFRAME		= 0x00040000,
		WS_GROUP		= 0x00020000,
		WS_TABSTOP		= 0x00010000,
		WS_MINIMIZEBOX		= 0x00020000,
		WS_MAXIMIZEBOX		= 0x00010000,
		WS_TILED		= 0x00000000,
		WS_ICONIC		= 0x20000000,
		WS_SIZEBOX		= 0x00040000,
		WS_POPUPWINDOW		= unchecked((int)0x80880000),
		WS_OVERLAPPEDWINDOW	= WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
		WS_TILEDWINDOW		= WS_OVERLAPPEDWINDOW,
		WS_CHILDWINDOW		= WS_CHILD,
	}

	[Flags]

	public

	enum WindowExStyles : int {
		// Extended Styles
		WS_EX_DLGMODALFRAME	= 0x00000001,
		WS_EX_DRAGDETECT	= 0x00000002,
		WS_EX_NOPARENTNOTIFY	= 0x00000004,
		WS_EX_TOPMOST		= 0x00000008,
		WS_EX_ACCEPTFILES	= 0x00000010,
		WS_EX_TRANSPARENT	= 0x00000020,

		WS_EX_MDICHILD		= 0x00000040,
		WS_EX_TOOLWINDOW	= 0x00000080,
		WS_EX_WINDOWEDGE	= 0x00000100,
		WS_EX_CLIENTEDGE	= 0x00000200,
		WS_EX_CONTEXTHELP	= 0x00000400,

		WS_EX_RIGHT		= 0x00001000,
		WS_EX_LEFT		= 0x00000000,
		WS_EX_RTLREADING	= 0x00002000,
		WS_EX_LTRREADING	= 0x00000000,
		WS_EX_LEFTSCROLLBAR	= 0x00004000,
		WS_EX_LAYERED		= 0x00080000,
		WS_EX_RIGHTSCROLLBAR	= 0x00000000,

		WS_EX_CONTROLPARENT	= 0x00010000,
		WS_EX_STATICEDGE	= 0x00020000,
		WS_EX_APPWINDOW		= 0x00040000,
		WS_EX_NOINHERITLAYOUT	= 0x00100000,
		WS_EX_LAYOUTRTL		= 0x00400000,
		WS_EX_COMPOSITED	= 0x02000000,
		WS_EX_NOACTIVATE	= 0x08000000,

		WS_EX_OVERLAPPEDWINDOW	= WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
		WS_EX_PALETTEWINDOW	= WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST
	}

	[Flags]

	public

	enum ToolBarStyles : int {
		TBSTYLE_TOOLTIPS    = 0x100,
		TBSTYLE_FLAT        = 0x800,
		TBSTYLE_LIST        = 0x1000,
		TBSTYLE_TRANSPARENT = 0x8000
	}


	public

	enum Msg {
		WM_NULL                   = 0x0000,
		WM_CREATE                 = 0x0001,
		WM_DESTROY                = 0x0002,
		WM_MOVE                   = 0x0003,
		WM_SIZE                   = 0x0005,
		WM_ACTIVATE               = 0x0006,
		WM_SETFOCUS               = 0x0007,
		WM_KILLFOCUS              = 0x0008,
		//              public const uint WM_SETVISIBLE           = 0x0009;
		WM_ENABLE                 = 0x000A,
		WM_SETREDRAW              = 0x000B,
		WM_SETTEXT                = 0x000C,
		WM_GETTEXT                = 0x000D,
		WM_GETTEXTLENGTH          = 0x000E,
		WM_PAINT                  = 0x000F,
		WM_CLOSE                  = 0x0010,
		WM_QUERYENDSESSION        = 0x0011,
		WM_QUIT                   = 0x0012,
		WM_QUERYOPEN              = 0x0013,
		WM_ERASEBKGND             = 0x0014,
		WM_SYSCOLORCHANGE         = 0x0015,
		WM_ENDSESSION             = 0x0016,
		//              public const uint WM_SYSTEMERROR          = 0x0017;
		WM_SHOWWINDOW             = 0x0018,
		WM_CTLCOLOR               = 0x0019,
		WM_WININICHANGE           = 0x001A,
		WM_SETTINGCHANGE          = 0x001A,
		WM_DEVMODECHANGE          = 0x001B,
		WM_ACTIVATEAPP            = 0x001C,
		WM_FONTCHANGE             = 0x001D,
		WM_TIMECHANGE             = 0x001E,
		WM_CANCELMODE             = 0x001F,
		WM_SETCURSOR              = 0x0020,
		WM_MOUSEACTIVATE          = 0x0021,
		WM_CHILDACTIVATE          = 0x0022,
		WM_QUEUESYNC              = 0x0023,
		WM_GETMINMAXINFO          = 0x0024,
		WM_PAINTICON              = 0x0026,
		WM_ICONERASEBKGND         = 0x0027,
		WM_NEXTDLGCTL             = 0x0028,
		//              public const uint WM_ALTTABACTIVE         = 0x0029;
		WM_SPOOLERSTATUS          = 0x002A,
		WM_DRAWITEM               = 0x002B,
		WM_MEASUREITEM            = 0x002C,
		WM_DELETEITEM             = 0x002D,
		WM_VKEYTOITEM             = 0x002E,
		WM_CHARTOITEM             = 0x002F,
		WM_SETFONT                = 0x0030,
		WM_GETFONT                = 0x0031,
		WM_SETHOTKEY              = 0x0032,
		WM_GETHOTKEY              = 0x0033,
		//              public const uint WM_FILESYSCHANGE        = 0x0034;
		//              public const uint WM_ISACTIVEICON         = 0x0035;
		//              public const uint WM_QUERYPARKICON        = 0x0036;
		WM_QUERYDRAGICON          = 0x0037,
		WM_COMPAREITEM            = 0x0039,
		//              public const uint WM_TESTING              = 0x003a;
		//              public const uint WM_OTHERWINDOWCREATED = 0x003c;
		WM_GETOBJECT              = 0x003D,
		//                      public const uint WM_ACTIVATESHELLWINDOW        = 0x003e;
		WM_COMPACTING             = 0x0041,
		WM_COMMNOTIFY             = 0x0044 ,
		WM_WINDOWPOSCHANGING      = 0x0046,
		WM_WINDOWPOSCHANGED       = 0x0047,
		WM_POWER                  = 0x0048,
		WM_COPYDATA               = 0x004A,
		WM_CANCELJOURNAL          = 0x004B,
		WM_NOTIFY                 = 0x004E,
		WM_INPUTLANGCHANGEREQUEST = 0x0050,
		WM_INPUTLANGCHANGE        = 0x0051,
		WM_TCARD                  = 0x0052,
		WM_HELP                   = 0x0053,
		WM_USERCHANGED            = 0x0054,
		WM_NOTIFYFORMAT           = 0x0055,
		WM_CONTEXTMENU            = 0x007B,
		WM_STYLECHANGING          = 0x007C,
		WM_STYLECHANGED           = 0x007D,
		WM_DISPLAYCHANGE          = 0x007E,
		WM_GETICON                = 0x007F,

		// Non-Client messages
		WM_SETICON                = 0x0080,
		WM_NCCREATE               = 0x0081,
		WM_NCDESTROY              = 0x0082,
		WM_NCCALCSIZE             = 0x0083,
		WM_NCHITTEST              = 0x0084,
		WM_NCPAINT                = 0x0085,
		WM_NCACTIVATE             = 0x0086,
		WM_GETDLGCODE             = 0x0087,
		WM_SYNCPAINT              = 0x0088,
		//              public const uint WM_SYNCTASK       = 0x0089;
		WM_NCMOUSEMOVE            = 0x00A0,
		WM_NCLBUTTONDOWN          = 0x00A1,
		WM_NCLBUTTONUP            = 0x00A2,
		WM_NCLBUTTONDBLCLK        = 0x00A3,
		WM_NCRBUTTONDOWN          = 0x00A4,
		WM_NCRBUTTONUP            = 0x00A5,
		WM_NCRBUTTONDBLCLK        = 0x00A6,
		WM_NCMBUTTONDOWN          = 0x00A7,
		WM_NCMBUTTONUP            = 0x00A8,
		WM_NCMBUTTONDBLCLK        = 0x00A9,
		//              public const uint WM_NCXBUTTONDOWN    = 0x00ab;
		//              public const uint WM_NCXBUTTONUP      = 0x00ac;
		//              public const uint WM_NCXBUTTONDBLCLK  = 0x00ad;
		WM_KEYDOWN                = 0x0100,
		WM_KEYFIRST               = 0x0100,
		WM_KEYUP                  = 0x0101,
		WM_CHAR                   = 0x0102,
		WM_DEADCHAR               = 0x0103,
		WM_SYSKEYDOWN             = 0x0104,
		WM_SYSKEYUP               = 0x0105,
		WM_SYSCHAR                = 0x0106,
		WM_SYSDEADCHAR            = 0x0107,
		WM_KEYLAST                = 0x0108,
		WM_IME_STARTCOMPOSITION   = 0x010D,
		WM_IME_ENDCOMPOSITION     = 0x010E,
		WM_IME_COMPOSITION        = 0x010F,
		WM_IME_KEYLAST            = 0x010F,
		WM_INITDIALOG             = 0x0110,
		WM_COMMAND                = 0x0111,
		WM_SYSCOMMAND             = 0x0112,
		WM_TIMER                  = 0x0113,
		WM_HSCROLL                = 0x0114,
		WM_VSCROLL                = 0x0115,
		WM_INITMENU               = 0x0116,
		WM_INITMENUPOPUP          = 0x0117,
		//              public const uint WM_SYSTIMER       = 0x0118;
		WM_MENUSELECT             = 0x011F,
		WM_MENUCHAR               = 0x0120,
		WM_ENTERIDLE              = 0x0121,
		WM_MENURBUTTONUP          = 0x0122,
		WM_MENUDRAG               = 0x0123,
		WM_MENUGETOBJECT          = 0x0124,
		WM_UNINITMENUPOPUP        = 0x0125,
		WM_MENUCOMMAND            = 0x0126,
		
		WM_CHANGEUISTATE          = 0x0127,
		WM_UPDATEUISTATE          = 0x0128,
		WM_QUERYUISTATE           = 0x0129,

		//              public const uint WM_LBTRACKPOINT     = 0x0131;
		WM_CTLCOLORMSGBOX         = 0x0132,
		WM_CTLCOLOREDIT           = 0x0133,
		WM_CTLCOLORLISTBOX        = 0x0134,
		WM_CTLCOLORBTN            = 0x0135,
		WM_CTLCOLORDLG            = 0x0136,
		WM_CTLCOLORSCROLLBAR      = 0x0137,
		WM_CTLCOLORSTATIC         = 0x0138,
		WM_MOUSEMOVE              = 0x0200,
		WM_MOUSEFIRST                     = 0x0200,
		WM_LBUTTONDOWN            = 0x0201,
		WM_LBUTTONUP              = 0x0202,
		WM_LBUTTONDBLCLK          = 0x0203,
		WM_RBUTTONDOWN            = 0x0204,
		WM_RBUTTONUP              = 0x0205,
		WM_RBUTTONDBLCLK          = 0x0206,
		WM_MBUTTONDOWN            = 0x0207,
		WM_MBUTTONUP              = 0x0208,
		WM_MBUTTONDBLCLK          = 0x0209,
		WM_MOUSEWHEEL             = 0x020A,
		WM_MOUSELAST             = 0x020D,
		//              public const uint WM_XBUTTONDOWN      = 0x020B;
		//              public const uint WM_XBUTTONUP        = 0x020C;
		//              public const uint WM_XBUTTONDBLCLK    = 0x020D;
		WM_PARENTNOTIFY           = 0x0210,
		WM_ENTERMENULOOP          = 0x0211,
		WM_EXITMENULOOP           = 0x0212,
		WM_NEXTMENU               = 0x0213,
		WM_SIZING                 = 0x0214,
		WM_CAPTURECHANGED         = 0x0215,
		WM_MOVING                 = 0x0216,
		//              public const uint WM_POWERBROADCAST   = 0x0218;
		WM_DEVICECHANGE           = 0x0219,
		WM_MDICREATE              = 0x0220,
		WM_MDIDESTROY             = 0x0221,
		WM_MDIACTIVATE            = 0x0222,
		WM_MDIRESTORE             = 0x0223,
		WM_MDINEXT                = 0x0224,
		WM_MDIMAXIMIZE            = 0x0225,
		WM_MDITILE                = 0x0226,
		WM_MDICASCADE             = 0x0227,
		WM_MDIICONARRANGE         = 0x0228,
		WM_MDIGETACTIVE           = 0x0229,
		/* D&D messages */
		//              public const uint WM_DROPOBJECT     = 0x022A;
		//              public const uint WM_QUERYDROPOBJECT  = 0x022B;
		//              public const uint WM_BEGINDRAG      = 0x022C;
		//              public const uint WM_DRAGLOOP       = 0x022D;
		//              public const uint WM_DRAGSELECT     = 0x022E;
		//              public const uint WM_DRAGMOVE       = 0x022F;
		WM_MDISETMENU             = 0x0230,
		WM_ENTERSIZEMOVE          = 0x0231,
		WM_EXITSIZEMOVE           = 0x0232,
		WM_DROPFILES              = 0x0233,
		WM_MDIREFRESHMENU         = 0x0234,
		WM_IME_SETCONTEXT         = 0x0281,
		WM_IME_NOTIFY             = 0x0282,
		WM_IME_CONTROL            = 0x0283,
		WM_IME_COMPOSITIONFULL    = 0x0284,
		WM_IME_SELECT             = 0x0285,
		WM_IME_CHAR               = 0x0286,
		WM_IME_REQUEST            = 0x0288,
		WM_IME_KEYDOWN            = 0x0290,
		WM_IME_KEYUP              = 0x0291,
		WM_NCMOUSEHOVER           = 0x02A0,
		WM_MOUSEHOVER             = 0x02A1,
		WM_NCMOUSELEAVE           = 0x02A2,
		WM_MOUSELEAVE             = 0x02A3,
		WM_CUT                    = 0x0300,
		WM_COPY                   = 0x0301,
		WM_PASTE                  = 0x0302,
		WM_CLEAR                  = 0x0303,
		WM_UNDO                   = 0x0304,
		WM_RENDERFORMAT           = 0x0305,
		WM_RENDERALLFORMATS       = 0x0306,
		WM_DESTROYCLIPBOARD       = 0x0307,
		WM_DRAWCLIPBOARD          = 0x0308,
		WM_PAINTCLIPBOARD         = 0x0309,
		WM_VSCROLLCLIPBOARD       = 0x030A,
		WM_SIZECLIPBOARD          = 0x030B,
		WM_ASKCBFORMATNAME        = 0x030C,
		WM_CHANGECBCHAIN          = 0x030D,
		WM_HSCROLLCLIPBOARD       = 0x030E,
		WM_QUERYNEWPALETTE        = 0x030F,
		WM_PALETTEISCHANGING      = 0x0310,
		WM_PALETTECHANGED         = 0x0311,
		WM_HOTKEY                 = 0x0312,
		WM_PRINT                  = 0x0317,
		WM_PRINTCLIENT            = 0x0318,
		WM_HANDHELDFIRST          = 0x0358,
		WM_HANDHELDLAST           = 0x035F,
		WM_AFXFIRST               = 0x0360,
		WM_AFXLAST                = 0x037F,
		WM_PENWINFIRST            = 0x0380,
		WM_PENWINLAST             = 0x038F,
		WM_APP                    = 0x8000,
		WM_USER                   = 0x0400,

		// Our "private" ones
		WM_MOUSE_ENTER            = 0x0401,
		WM_ASYNC_MESSAGE          = 0x0403,
		WM_REFLECT                = WM_USER + 0x1c00,
		WM_CLOSE_INTERNAL         = WM_USER + 0x1c01,

		// private messages to support on-the-spot IME editing.
		WM_XIM_PREEDITSTART		  = WM_USER + 0x1d00,
		WM_XIM_PREEDITDONE		  = WM_USER + 0x1d01,
		WM_XIM_PREEDITDRAW		  = WM_USER + 0x1d02,
		WM_XIM_PREEDITCARET		  = WM_USER + 0x1d03,

		// NotifyIcon (Systray) Balloon messages 
		NIN_BALLOONSHOW           = WM_USER + 0x0002,
		NIN_BALLOONHIDE           = WM_USER + 0x0003,
		NIN_BALLOONTIMEOUT        = WM_USER + 0x0004,
		NIN_BALLOONUSERCLICK      = WM_USER + 0x0005 
	}


	public

	enum MsgButtons {
		MK_LBUTTON		= 0x0001,
		MK_RBUTTON		= 0x0002,
		MK_SHIFT		= 0x0004,
		MK_CONTROL		= 0x0008,
		MK_MBUTTON          	= 0x0010,
		MK_XBUTTON1		= 0x0020,
		MK_XBUTTON2		= 0x0040,
	}


	public

	enum MsgUIState {
		UIS_SET        = 1,
		UIS_CLEAR      = 2,
		UIS_INITIALIZE = 3,
		UISF_HIDEFOCUS = 0x1,
		UISF_HIDEACCEL = 0x2,
		UISF_ACTIVE    = 0x4
	}

	[StructLayout(LayoutKind.Sequential)]

	public

	struct POINT {
		public int x;
		public int y;

		public POINT (int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public Point ToPoint ()
		{
			return new Point (x, y);
		}

		public override string ToString ()
		{
			return "Point {" + x.ToString () + ", " + y.ToString () + "}";
		}
	}
	
	[StructLayout(LayoutKind.Sequential)] 

        public

	struct MSG {
		public IntPtr	hwnd;
        public Msg	message;
        public IntPtr	wParam;
        public IntPtr	lParam;
		internal uint	time;  
		internal POINT	pt;
		public object refobject;

		public override string ToString ()
		{
			return String.Format ("msg=0x{0:x} ({1}) hwnd=0x{2:x} wparam=0x{3:x} lparam=0x{4:x} pt={5}", (int) message, message.ToString (), hwnd.ToInt32 (), wParam.ToInt32 (), lParam.ToInt32 (), pt);
		}
	}

	[Flags]

	public

	enum TransparencySupport {
		None = 0x00,
		Get = 0x01,
		Set = 0x02,
		GetSet = 0x03
	}

#if PUBLIC_TYPES
	public
#else
	internal
#endif
	enum WindowActiveFlags {
		WA_INACTIVE		= 0,
		WA_ACTIVE		= 1,
		WA_CLICKACTIVE		= 2
	}

#if PUBLIC_TYPES
	public
#else
	internal
#endif
	enum KeybdEventFlags {
		None			= 0,
		ExtendedKey		= 0x0001,
		KeyUp			= 0x0002
	}


	public

	enum VirtualKeys {
		VK_LBUTTON		= 0x01,
		VK_RBUTTON              = 0x02,
		VK_CANCEL		= 0x03,
		VK_MBUTTON              = 0x04,
		VK_XBUTTON1             = 0x05,
		VK_XBUTTON2             = 0x06,
		VK_BACK			= 0x08,
		VK_TAB			= 0x09,
		VK_CLEAR		= 0x0C,
		VK_RETURN		= 0x0D,
		VK_SHIFT		= 0x10,
		VK_CONTROL		= 0x11,
		VK_MENU			= 0x12,
		VK_PAUSE		= 0x13,
		VK_CAPITAL		= 0x14,
		VK_ESCAPE		= 0x1B,
		VK_CONVERT		= 0x1C,
		VK_NONCONVERT	= 0x1D,
		VK_SPACE		= 0x20,
		VK_PRIOR		= 0x21,
		VK_NEXT			= 0x22,
		VK_END			= 0x23,
		VK_HOME			= 0x24,
		VK_LEFT			= 0x25,
		VK_UP			= 0x26,
		VK_RIGHT		= 0x27,
		VK_DOWN			= 0x28,
		VK_SELECT		= 0x29,
		VK_PRINT		= 0x2A,
		VK_EXECUTE		= 0x2B,
		VK_SNAPSHOT		= 0x2C,
		VK_INSERT		= 0x2D,
		VK_DELETE		= 0x2E,
		VK_HELP			= 0x2F,
		VK_0			= 0x30,
		VK_1			= 0x31,
		VK_2			= 0x32,
		VK_3			= 0x33,
		VK_4			= 0x34,
		VK_5			= 0x35,
		VK_6			= 0x36,
		VK_7			= 0x37,
		VK_8			= 0x38,
		VK_9			= 0x39,
		VK_A			= 0x41,
		VK_B			= 0x42,
		VK_C			= 0x43,
		VK_D			= 0x44,
		VK_E			= 0x45,
		VK_F			= 0x46,
		VK_G			= 0x47,
		VK_H			= 0x48,
		VK_I			= 0x49,
		VK_J			= 0x4A,
		VK_K			= 0x4B,
		VK_L			= 0x4C,
		VK_M			= 0x4D,
		VK_N			= 0x4E,
		VK_O			= 0x4F,
		VK_P			= 0x50,
		VK_Q			= 0x51,
		VK_R			= 0x52,
		VK_S			= 0x53,
		VK_T			= 0x54,
		VK_U			= 0x55,
		VK_V			= 0x56,
		VK_W			= 0x57,
		VK_X			= 0x58,
		VK_Y			= 0x59,
		VK_Z			= 0x5A,
		VK_LWIN			= 0x5B,
		VK_RWIN			= 0x5C,
		VK_APPS			= 0x5D,
		VK_NUMPAD0		= 0x60,
		VK_NUMPAD1		= 0x61,
		VK_NUMPAD2		= 0x62,
		VK_NUMPAD3		= 0x63,
		VK_NUMPAD4		= 0x64,
		VK_NUMPAD5		= 0x65,
		VK_NUMPAD6		= 0x66,
		VK_NUMPAD7		= 0x67,
		VK_NUMPAD8		= 0x68,
		VK_NUMPAD9		= 0x69,
		VK_MULTIPLY		= 0x6A,
		VK_ADD			= 0x6B,
		VK_SEPARATOR		= 0x6C,
		VK_SUBTRACT		= 0x6D,
		VK_DECIMAL		= 0x6E,
		VK_DIVIDE		= 0x6F,
		VK_F1			= 0x70,
		VK_F2			= 0x71,
		VK_F3			= 0x72,
		VK_F4			= 0x73,
		VK_F5			= 0x74,
		VK_F6			= 0x75,
		VK_F7			= 0x76,
		VK_F8			= 0x77,
		VK_F9			= 0x78,
		VK_F10			= 0x79,
		VK_F11			= 0x7A,
		VK_F12			= 0x7B,
		VK_F13			= 0x7C,
		VK_F14			= 0x7D,
		VK_F15			= 0x7E,
		VK_F16			= 0x7F,
		VK_F17			= 0x80,
		VK_F18			= 0x81,
		VK_F19			= 0x82,
		VK_F20			= 0x83,
		VK_F21			= 0x84,
		VK_F22			= 0x85,
		VK_F23			= 0x86,
		VK_F24			= 0x87,
		VK_NUMLOCK		= 0x90,
		VK_SCROLL		= 0x91,
		VK_LSHIFT		= 0xA0,   
		VK_RSHIFT		= 0xA1,   
		VK_LCONTROL		= 0xA2,   
		VK_RCONTROL		= 0xA3,   
		VK_LMENU		= 0xA4,   
		VK_RMENU		= 0xA5,
		VK_OEM_1		= 0xBA,
		VK_OEM_PLUS		= 0xBB,
		VK_OEM_COMMA		= 0xBC,
		VK_OEM_MINUS		= 0xBD,
		VK_OEM_PERIOD		= 0xBE,
		VK_OEM_2		= 0xBF, 
		VK_OEM_3		= 0xC0,
		VK_OEM_4		= 0xDB,
		VK_OEM_5		= 0xDC,
		VK_OEM_6		= 0xDD,
		VK_OEM_7		= 0xDE,
		VK_OEM_8		= 0xDF,
		VK_OEM_AX		= 0xE1,
		VK_OEM_102		= 0xE2,
		VK_ICO_HELP		= 0xE3,
		VK_ICO_00		= 0xE4,
		VK_PROCESSKEY		= 0xE5,
		VK_OEM_ATTN		= 0xF0,
		VK_OEM_COPY		= 0xF2,
		VK_OEM_AUTO		= 0xF3,
		VK_OEM_ENLW		= 0xF4,
		VK_OEM_BACKTAB		= 0xF5,
		VK_ATTN			= 0xF6,
		VK_CRSEL		= 0xF7,
		VK_EXSEL		= 0xF8,
		VK_EREOF		= 0xF9,
		VK_PLAY			= 0xFA,
		VK_ZOOM			= 0xFB,
		VK_NONAME		= 0xFC,
		VK_PA1			= 0xFD,
		VK_OEM_CLEAR		= 0xFE,
	}

#if PUBLIC_TYPES
	public
#else
	internal
#endif
	enum TtyKeys {
		XK_BackSpace		= 0xff08,  /* Back space, back char */
		XK_Tab			= 0xff09,
		XK_Linefeed		= 0xff0a,  /* Linefeed, LF */
		XK_Clear		= 0xff0b,
		XK_Return		= 0xff0d,  /* Return, enter */
		XK_Pause		= 0xff13,  /* Pause, hold */
		XK_Scroll_Lock		= 0xff14,
		XK_Sys_Req		= 0xff15,
		XK_Escape		= 0xff1b,
		XK_Delete		= 0xffff  /* Delete, rubout */
	}

#if PUBLIC_TYPES
	public
#else
	internal
#endif
	enum MiscKeys {
		XK_ISO_Lock             = 0xfe01,
		XK_ISO_Last_Group_Lock	= 0xfe0f,
		XK_Select		= 0xff60,
		XK_Print		= 0xff61,
		XK_Execute		= 0xff62,
		XK_Insert		= 0xff63,
		XK_Undo			= 0xff65,
		XK_Redo			= 0xff66,
		XK_Menu			= 0xff67,
		XK_Find			= 0xff68,
		XK_Cancel		= 0xff69,
		XK_Help			= 0xff6a,
		XK_Break		= 0xff6b,
		XK_Mode_switch		= 0xff7e,
		XK_script_switch	= 0xff7e,
		XK_Num_Lock		= 0xff7f
	}

#if PUBLIC_TYPES
	public
#else
	internal
#endif
	enum KeypadKeys {
		XK_KP_Space		= 0xff80,
		XK_KP_Tab		= 0xff89,
		XK_KP_Enter		= 0xff8d,  /* Enter */
		XK_KP_F1		= 0xff91,  /* PF1, KP_A, ... */
		XK_KP_F2		= 0xff92,
		XK_KP_F3		= 0xff93,
		XK_KP_F4		= 0xff94,
		XK_KP_Home		= 0xff95,
		XK_KP_Left		= 0xff96,
		XK_KP_Up		= 0xff97,
		XK_KP_Right		= 0xff98,
		XK_KP_Down		= 0xff99,
		XK_KP_Prior		= 0xff9a,
		XK_KP_Page_Up		= 0xff9a,
		XK_KP_Next		= 0xff9b,
		XK_KP_Page_Down		= 0xff9b,
		XK_KP_End		= 0xff9c,
		XK_KP_Begin		= 0xff9d,
		XK_KP_Insert		= 0xff9e,
		XK_KP_Delete		= 0xff9f,
		XK_KP_Equal		= 0xffbd,  /* Equals */
		XK_KP_Multiply		= 0xffaa,
		XK_KP_Add		= 0xffab,
		XK_KP_Separator		= 0xffac,  /* Separator, often comma */
		XK_KP_Subtract		= 0xffad,
		XK_KP_Decimal		= 0xffae,
		XK_KP_Divide		= 0xffaf,

		XK_KP_0			= 0xffb0,
		XK_KP_1			= 0xffb1,
		XK_KP_2			= 0xffb2,
		XK_KP_3			= 0xffb3,
		XK_KP_4			= 0xffb4,
		XK_KP_5			= 0xffb5,
		XK_KP_6			= 0xffb6,
		XK_KP_7			= 0xffb7,
		XK_KP_8			= 0xffb8,
		XK_KP_9			= 0xffb9
	}

#if PUBLIC_TYPES
	public
#else
        internal
#endif
	enum DeadKeys {
                XK_dead_grave           = 0xfe50,
                XK_dead_acute           = 0xfe51,
                XK_dead_circumflex      = 0xfe52,
                XK_dead_tilde           = 0xfe53,
                XK_dead_macron          = 0xfe54,
                XK_dead_breve           = 0xfe55,
                XK_dead_abovedot        = 0xfe56,
                XK_dead_diaeresis       = 0xfe57,
                XK_dead_abovering       = 0xfe58,
                XK_dead_doubleacute     = 0xfe59,
                XK_dead_caron           = 0xfe5a,
                XK_dead_cedilla         = 0xfe5b,
                XK_dead_ogonek          = 0xfe5c,
                XK_dead_iota            = 0xfe5d,
                XK_dead_voiced_sound    = 0xfe5e,
                XK_dead_semivoiced_sound  = 0xfe5f,
                XK_dead_belowdot        = 0xfe60,
                XK_dead_hook            = 0xfe61,
                XK_dead_horn            = 0xfe62

        }

	[StructLayout(LayoutKind.Sequential)]
#if PUBLIC_TYPES
	 public
#else
	internal
#endif
	struct HELPINFO {
		internal uint		cbSize;
		internal int		iContextType;
		internal int		iCtrlId;
		internal IntPtr		hItemHandle;
		internal uint		dwContextId;
		internal POINT		MousePos;
	}


	public

	enum PeekMessageFlags {
		PM_NOREMOVE			= 0x00000000,
		PM_REMOVE			= 0x00000001,
		PM_NOYIELD			= 0x00000002
	}


	public

	enum StdCursor {
		Default				= 0,
		AppStarting			= 1,
		Arrow				= 2,
		Cross				= 3,
		Hand				= 4,
		Help				= 5,
		HSplit				= 6,
		IBeam				= 7,
		No				= 8,
		NoMove2D			= 9,
		NoMoveHoriz			= 10,
		NoMoveVert			= 11,
		PanEast				= 12,
		PanNE				= 13,
		PanNorth			= 14,
		PanNW				= 15,
		PanSE				= 16,
		PanSouth			= 17,
		PanSW				= 18,
		PanWest				= 19,
		SizeAll				= 20,
		SizeNESW			= 21,
		SizeNS				= 22,
		SizeNWSE			= 23,
		SizeWE				= 24,
		UpArrow				= 25,
		VSplit				= 26,
		WaitCursor			= 27
	}

#if PUBLIC_TYPES
	public
#else
	internal
#endif
	enum HitTest {
		HTERROR				= -2,
		HTTRANSPARENT			= -1,
		HTNOWHERE			= 0,
		HTCLIENT			= 1,
		HTCAPTION			= 2,
		HTSYSMENU			= 3,
		HTGROWBOX			= 4,
		HTSIZE				= HTGROWBOX,
		HTMENU				= 5,
		HTHSCROLL			= 6,
		HTVSCROLL			= 7,
		HTMINBUTTON			= 8,
		HTMAXBUTTON			= 9,
		HTLEFT				= 10,
		HTRIGHT				= 11,
		HTTOP				= 12,
		HTTOPLEFT			= 13,
		HTTOPRIGHT			= 14,
		HTBOTTOM			= 15,
		HTBOTTOMLEFT			= 16,
		HTBOTTOMRIGHT			= 17,
		HTBORDER			= 18,
		HTREDUCE			= HTMINBUTTON,
		HTZOOM				= HTMAXBUTTON,
		HTSIZEFIRST			= HTLEFT,
		HTSIZELAST			= HTBOTTOMRIGHT,
		HTOBJECT			= 19,
		HTCLOSE				= 20,
		HTHELP				= 21
	}


	public

	enum TitleStyle {
		None				= 0,
		Normal				= 1,
		Tool				= 2
	}

	[StructLayout(LayoutKind.Sequential)]
#if PUBLIC_TYPES
	 public
#else
	internal
#endif
	struct BITMAPINFOHEADER {
		internal uint     biSize;
		internal int      biWidth;
		internal int      biHeight;
		internal ushort   biPlanes;
		internal ushort   biBitCount;
		internal uint     biCompression;
		internal uint     biSizeImage;
		internal int      biXPelsPerMeter;
		internal int      biYPelsPerMeter;
		internal uint     biClrUsed;
		internal uint     biClrImportant;
	}

#if PUBLIC_TYPES
	public
#else
	internal
#endif
	enum ClipboardFormats : ushort {
		CF_TEXT				= 1,
		CF_BITMAP           		= 2,
		CF_METAFILEPICT     		= 3,
		CF_SYLK             		= 4,
		CF_DIF             		= 5,
		CF_TIFF            		= 6,
		CF_OEMTEXT         		= 7,
		CF_DIB              		= 8,
		CF_PALETTE          		= 9,
		CF_PENDATA          		= 10,
		CF_RIFF             		= 11,
		CF_WAVE             		= 12,
		CF_UNICODETEXT      		= 13,
		CF_ENHMETAFILE      		= 14,
		CF_HDROP            		= 15,
		CF_LOCALE           		= 16,
		CF_DIBV5            		= 17
	}

	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
#if PUBLIC_TYPES
	 public
#else
	internal
#endif
	struct MINMAXINFO {
		internal POINT			ptReserved;
		internal POINT			ptMaxSize;
		internal POINT			ptMaxPosition;
		internal POINT			ptMinTrackSize;
		internal POINT			ptMaxTrackSize;
	}

	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
#if PUBLIC_TYPES
	 public
#else
	internal
#endif
	struct KeyFilterData {
			internal bool Down;
			internal int keycode;
			internal int keysym;
			internal Keys ModifierKeys;
			internal String str;
		}

#if PUBLIC_TYPES
	public
#else
	internal
#endif
	enum ScrollBarCommands {
		SB_LINEUP		= 0,
		SB_LINELEFT		= 0,
		SB_LINEDOWN		= 1,
		SB_LINERIGHT		= 1,
		SB_PAGEUP		= 2,
		SB_PAGELEFT		= 2,
		SB_PAGEDOWN		= 3,
		SB_PAGERIGHT		= 3,
		SB_THUMBPOSITION	= 4,
		SB_THUMBTRACK		= 5,
		SB_TOP			= 6,
		SB_LEFT			= 6,
		SB_BOTTOM		= 7,
		SB_RIGHT		= 7,
		SB_ENDSCROLL		= 8
	}

#if PUBLIC_TYPES
	public
#else
	internal
#endif
	enum ClipCombineMode {
		RGN_AND = 1,
		RGN_OR,
		RGN_XOR,
		RGN_DIFF,
		RGN_COPY,

		RGN_MIN = RGN_AND,
		RGN_MAX = RGN_COPY
	}

#if PUBLIC_TYPES
	public
#else
	internal
#endif
	enum SystemCommands {
		SC_SIZE         = 0xF000,
		SC_MOVE         = 0xF010,
		SC_MINIMIZE     = 0xF020,
		SC_MAXIMIZE     = 0xF030,
		SC_NEXTWINDOW   = 0xF040,
		SC_PREVWINDOW   = 0xF050,
		SC_CLOSE        = 0xF060,
		SC_VSCROLL      = 0xF070,
		SC_HSCROLL      = 0xF080,
		SC_MOUSEMENU    = 0xF090,
		SC_KEYMENU      = 0xF100,
		SC_ARRANGE      = 0xF110,
		SC_RESTORE      = 0xF120,
		SC_TASKLIST     = 0xF130,
		SC_SCREENSAVE   = 0xF140,
		SC_HOTKEY       = 0xF150,
		SC_DEFAULT      = 0xF160,
		SC_MONITORPOWER = 0xF170,
		SC_CONTEXTHELP  = 0xF180
	}
    
	public

	enum AlertType {
		Default		= 1,
		Error		= 2,
		Question	= 3,
		Warning		= 4,
		Information	= 5
	}
}

