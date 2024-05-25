using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using SkiaSharp;
using IntPtr = System.IntPtr;
using Timer = System.Windows.Forms.Timer;

public class XplatUIMine : XplatUIDriver
{
    //private XplatUIDriver driver = XplatUIWin32.GetInstance();

    public override IntPtr InitializeDriver()
    {
        return IntPtr.Zero;
    }

    public override void ShutdownDriver(IntPtr token)
    {

    }


    public override Point MousePosition
    {
        get { return mouse_position; }
    }

    public override int CaptionHeight => 20;

    public override Size CursorSize => throw new NotImplementedException();

    public override bool DragFullWindows => throw new NotImplementedException();

    public override Size DragSize => new Size(5, 5);

    public override Size FrameBorderSize => new Size(4, 4);

    public override Size IconSize => throw new NotImplementedException();

    public override Size MaxWindowTrackSize => throw new NotImplementedException();

    public override bool MenuAccessKeysUnderlined => false;

    public override Size MinimizedWindowSpacingSize => throw new NotImplementedException();

    public override Size MinimumWindowSize => new Size(100, 20);

    public override Size SmallIconSize => throw new NotImplementedException();

    public override int MouseButtonCount => throw new NotImplementedException();

    public override bool MouseButtonsSwapped => throw new NotImplementedException();

    public override bool MouseWheelPresent => throw new NotImplementedException();

    public override MouseButtons MouseButtons
    {
        get { return mouse_state; }
    }

    public override Rectangle VirtualScreen
    {
        get
        {
            if (Screen.AllScreens == null || Screen.AllScreens.Length == 0) return new Rectangle(0, 0, 960, 960);
            return Screen.PrimaryScreen.WorkingArea;
        }
    }

    public override Rectangle WorkingArea
    {
        get
        {
            if (Screen.AllScreens == null || Screen.AllScreens.Length == 0) return new Rectangle(0, 0, 960, 960);
            return Screen.PrimaryScreen.WorkingArea;
        }
    }


    public override Screen[] AllScreens
    {
        get
        {
            return null;
        }
    }

    public override bool ThemesEnabled => _themesEnabled;

    public override int VerticalScrollBarWidth => 30;

    public override int HorizontalScrollBarHeight => 30;

    public override event EventHandler Idle;

    private static int ref_count;
    public static IntPtr FosterParentLast;

    internal static MouseButtons mouse_state;
    internal static Point mouse_position;
    internal static bool grab_confined;
    internal static IntPtr grab_hwnd;
    internal static Rectangle grab_area;
    internal static WndProc wnd_proc;
    internal static IntPtr prev_mouse_hwnd;
    internal static bool caret_visible;

    internal static bool themes_enabled;
    private Hashtable timer_list;
    private static Queue message_queue;
    private static IntPtr clip_magic = new IntPtr(27051977);
    private static int scroll_width;
    private static int scroll_height;
    private static Hashtable wm_nc_registered;
    private static RECT clipped_cursor_rect;

    private IntPtr InternalWndProc(IntPtr hWnd, Msg msg, IntPtr wParam, IntPtr lParam)
    {
        return NativeWindow.WndProc(hWnd, msg, wParam, lParam);
    }
    public XplatUIMine()
    {
        Instance = this;
        // Handle singleton stuff first
        ref_count = 0;

        mouse_state = MouseButtons.None;
        mouse_position = Point.Empty;

        grab_confined = false;
        grab_area = Rectangle.Empty;

        message_queue = new Queue();

        themes_enabled = false;

        wnd_proc = new WndProc(InternalWndProc);

        FosterParentLast = IntPtr.Zero;

        scroll_height = 0;//Win32GetSystemMetrics(SystemMetrics.SM_CYHSCROLL);
        scroll_width = 0;//Win32GetSystemMetrics(SystemMetrics.SM_CXVSCROLL);

        Caret.Timer = new Timer();
        Keyboard = new keyboardimp();

        timer_list = new Hashtable();
        registered_classes = new Hashtable();

        MessageQueues = Hashtable.Synchronized(new Hashtable(7));
        unattached_timer_list = ArrayList.Synchronized(new ArrayList(3));
        messageHold = Hashtable.Synchronized(new Hashtable(3));

        ModalWindows = new Stack(3);
    }

    static XplatUIMine Instance { get; set; } = null;

    public static XplatUIMine GetInstance()
    {
        if (Instance == null)
            return new XplatUIMine();
        return Instance;
    }

    public override void AudibleAlert(AlertType alert)
    {
        try
        {
            Console.Beep();
        }
        catch
        {
        }
    }

    public override void BeginMoveResize(IntPtr handle)
    {
        throw new NotImplementedException();
    }

    public override void EnableThemes()
    {
        _themesEnabled = true;
    }

    public override void GetDisplaySize(out Size size)
    {
        size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
    }
    private IntPtr GetFosterParent()
    {
        //if (!IsWindow(FosterParentLast))
        {
            //FosterParentLast = Win32CreateWindow(WindowExStyles.WS_EX_TOOLWINDOW, "static", "Foster Parent Window", WindowStyles.WS_OVERLAPPEDWINDOW, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if (FosterParentLast == IntPtr.Zero)
            {
                //Win32MessageBox(IntPtr.Zero, "Could not create foster window, win32 error " + Win32GetLastError().ToString(), "Oops", 0);
            }
        }
        return FosterParentLast;
    }
    /// <summary>
    /// Creates an overlapped, pop-up, or child window. It specifies the window class, window title, window style, and (optionally) the initial position and size of the window. The function also specifies the window's parent or owner, if any, and the window's menu.
    /// </summary>
    /// <param name="cp"></param>
    /// <returns></returns>
    public override IntPtr CreateWindow(CreateParams cp)
    {
        XSetWindowAttributes Attributes;
        Hwnd hwnd;
        Hwnd parent_hwnd = null;
        int X;
        int Y;
        int Width;
        int Height;
        IntPtr ParentHandle;
        IntPtr WholeWindow;
        IntPtr ClientWindow;
        SetWindowValuemask ValueMask;
        int[] atoms;

        hwnd = new Hwnd();

        Attributes = new XSetWindowAttributes();
        X = cp.X;
        Y = cp.Y;
        Width = cp.Width;
        Height = cp.Height;

        if (Width < 1) Width = 1;
        if (Height < 1) Height = 1;

        if (cp.Parent != IntPtr.Zero)
        {
            parent_hwnd = Hwnd.ObjectFromHandle(cp.Parent);
            ParentHandle = parent_hwnd.client_window;
        }
        else
        {
            if (StyleSet(cp.Style, WindowStyles.WS_CHILD))
            {
                // We need to use our foster parent window until this poor child gets it's parent assigned
                ParentHandle = GetFosterParent();
            }
            else
            {
                //ParentHandle = RootWindow;
                ParentHandle = GetFosterParent();
            }
        }

        // Set the default location location for forms.
        if (cp.control is Form && cp.X == int.MinValue && cp.Y == int.MinValue)
        {
            Point next = Hwnd.GetNextStackedFormLocation(cp);
            X = next.X;
            Y = next.Y;
        }
        ValueMask = SetWindowValuemask.BitGravity | SetWindowValuemask.WinGravity;

        Attributes.bit_gravity = Gravity.NorthWestGravity;
        Attributes.win_gravity = Gravity.NorthWestGravity;

        // Save what's under the toolwindow
        if (ExStyleSet(cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW))
        {
            Attributes.save_under = true;
            ValueMask |= SetWindowValuemask.SaveUnder;
        }


        // If we're a popup without caption we override the WM
        if (StyleSet(cp.Style, WindowStyles.WS_POPUP) && !StyleSet(cp.Style, WindowStyles.WS_CAPTION))
        {
            Attributes.override_redirect = true;
            ValueMask |= SetWindowValuemask.OverrideRedirect;
        }

        hwnd.x = X;
        hwnd.y = Y;
        hwnd.width = Width;
        hwnd.height = Height;
        hwnd.parent = Hwnd.ObjectFromHandle(cp.Parent);
        hwnd.initial_style = cp.WindowStyle;
        hwnd.initial_ex_style = cp.WindowExStyle;

        if (StyleSet(cp.Style, WindowStyles.WS_DISABLED))
        {
            hwnd.enabled = false;
        }

        Size XWindowSize = TranslateWindowSizeToXWindowSize(cp);
        Rectangle XClientRect = TranslateClientRectangleToXClientRectangle(hwnd, cp.control);

        WholeWindow = new IntPtr(WindowHandleCount++);// Win32CreateWindow(cp.WindowExStyle, class_name, cp.Caption, cp.WindowStyle, location.X, location.Y, cp.Width, cp.Height, ParentHandle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (XWindowSize.Width == XClientRect.Width && XWindowSize.Height == XClientRect.Height)
            ClientWindow = WholeWindow;
        else
            ClientWindow = new IntPtr(WindowHandleCount++);

        if (Thread.CurrentThread.ManagedThreadId == 1)
        {

        }

        hwnd.Queue = ThreadQueue(Thread.CurrentThread);
        hwnd.WholeWindow = WholeWindow;
        hwnd.ClientWindow = ClientWindow;

        if (ExStyleSet(cp.ExStyle, WindowExStyles.WS_EX_TOPMOST))
            SetTopmost(hwnd.whole_window, true);

        SetWMStyles(hwnd, cp);

        if (StyleSet(cp.Style, WindowStyles.WS_MINIMIZE))
        {
            SetWindowState(hwnd.Handle, FormWindowState.Minimized);
        }
        else if (StyleSet(cp.Style, WindowStyles.WS_MAXIMIZE))
        {
            SetWindowState(hwnd.Handle, FormWindowState.Maximized);
        }

        // Set caption/window title
        Text(hwnd.Handle, cp.Caption);

        SendMessage(hwnd.Handle, Msg.WM_CREATE, (IntPtr)1, IntPtr.Zero /* XXX unused */);
        SendParentNotify(hwnd.Handle, Msg.WM_CREATE, int.MaxValue, int.MaxValue);

        if (StyleSet(cp.Style, WindowStyles.WS_VISIBLE))
        {
            hwnd.visible = true;
            MapWindow(hwnd, WindowType.Both);

            if (!(Control.FromHandle(hwnd.Handle) is Form))
                SendMessage(hwnd.Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);
        }

        if ((Control.FromHandle(hwnd.Handle) is Form))
        {
            var frm = ((Form)Control.FromHandle(hwnd.Handle));
            frm.window_manager = new FormWindowManager(frm);
            RequestNCRecalc(hwnd.Handle);
            AddExpose(hwnd, true, 0, 0, 1000, 1000);
        }

        return hwnd.zombie ? IntPtr.Zero : hwnd.Handle;
    }

    internal static Size TranslateWindowSizeToXWindowSize(CreateParams cp)
    {
        return TranslateWindowSizeToXWindowSize(cp, new Size(cp.Width, cp.Height));
    }

    internal static Size TranslateWindowSizeToXWindowSize(CreateParams cp, Size size)
    {
        /* 
         * If this is a form with no window manager, X is handling all the border and caption painting
         * so remove that from the area (since the area we set of the window here is the part of the window 
         * we're painting in only)
         */
        Form form = cp.control as Form;
        if (form != null && (form.window_manager == null && !cp.IsSet(WindowExStyles.WS_EX_TOOLWINDOW)))
        {
            Hwnd.Borders borders = Hwnd.GetBorders(cp, null);
            Size xrect = size;

            xrect.Width -= borders.left + borders.right;
            xrect.Height -= borders.top + borders.bottom;

            size = xrect;
        }
        if (size.Height == 0)
            size.Height = 1;
        if (size.Width == 0)
            size.Width = 1;
        return size;
    }
    void SetWMStyles(Hwnd hwnd, CreateParams cp)
    {
        MotifWmHints mwmHints;
        MotifFunctions functions;
        MotifDecorations decorations;
        int[] atoms;
        int atom_count;
        Rectangle client_rect;
        Form form;
        IntPtr window_type;
        bool hide_from_taskbar;
        IntPtr transient_for_parent;

        // Windows we manage ourselves don't need WM window styles.
        if (cp.HasWindowManager && !cp.IsSet(WindowExStyles.WS_EX_TOOLWINDOW))
        {
            return;
        }

        atoms = new int[8];
        mwmHints = new MotifWmHints();
        functions = 0;
        decorations = 0;
        transient_for_parent = IntPtr.Zero;

        mwmHints.flags = (IntPtr)(MotifFlags.Functions | MotifFlags.Decorations);
        mwmHints.functions = (IntPtr)0;
        mwmHints.decorations = (IntPtr)0;

        form = cp.control as Form;

        if (ExStyleSet(cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW))
        {
            /* tool windows get no window manager
               decorations.
            */

            /* just because the window doesn't get any decorations doesn't
               mean we should disable the functions.  for instance, without
               MotifFunctions.Maximize, changing the windowstate to Maximized
               is ignored by metacity. */
            functions |= MotifFunctions.Move | MotifFunctions.Resize | MotifFunctions.Minimize | MotifFunctions.Maximize;
        }
        else if (form != null && form.FormBorderStyle == FormBorderStyle.None)
        {
            /* allow borderless window to be maximized */
            functions |= MotifFunctions.All | MotifFunctions.Resize;
        }
        else
        {
            if (StyleSet(cp.Style, WindowStyles.WS_CAPTION))
            {
                functions |= MotifFunctions.Move;
                decorations |= MotifDecorations.Title | MotifDecorations.Menu;
            }

            if (StyleSet(cp.Style, WindowStyles.WS_THICKFRAME))
            {
                functions |= MotifFunctions.Move | MotifFunctions.Resize;
                decorations |= MotifDecorations.Border | MotifDecorations.ResizeH;
            }

            if (StyleSet(cp.Style, WindowStyles.WS_MINIMIZEBOX))
            {
                functions |= MotifFunctions.Minimize;
                decorations |= MotifDecorations.Minimize;
            }

            if (StyleSet(cp.Style, WindowStyles.WS_MAXIMIZEBOX))
            {
                functions |= MotifFunctions.Maximize;
                decorations |= MotifDecorations.Maximize;
            }

            if (StyleSet(cp.Style, WindowStyles.WS_SIZEBOX))
            {
                functions |= MotifFunctions.Resize;
                decorations |= MotifDecorations.ResizeH;
            }

            if (ExStyleSet(cp.ExStyle, WindowExStyles.WS_EX_DLGMODALFRAME))
            {
                decorations |= MotifDecorations.Border;
            }

            if (StyleSet(cp.Style, WindowStyles.WS_BORDER))
            {
                decorations |= MotifDecorations.Border;
            }

            if (StyleSet(cp.Style, WindowStyles.WS_DLGFRAME))
            {
                decorations |= MotifDecorations.Border;
            }

            if (StyleSet(cp.Style, WindowStyles.WS_SYSMENU))
            {
                functions |= MotifFunctions.Close;
            }
            else
            {
                functions &= ~(MotifFunctions.Maximize | MotifFunctions.Minimize | MotifFunctions.Close);
                decorations &= ~(MotifDecorations.Menu | MotifDecorations.Maximize | MotifDecorations.Minimize);
                if (cp.Caption == "")
                {
                    functions &= ~MotifFunctions.Move;
                    decorations &= ~(MotifDecorations.Title | MotifDecorations.ResizeH);
                }
            }
        }

        if ((functions & MotifFunctions.Resize) == 0)
        {
            hwnd.fixed_size = true;
            Rectangle fixed_rectangle = new Rectangle(cp.X, cp.Y, cp.Width, cp.Height);
            SetWindowMinMax(hwnd.Handle, fixed_rectangle, fixed_rectangle.Size, fixed_rectangle.Size);
        }
        else
        {
            hwnd.fixed_size = false;
        }

        mwmHints.functions = (IntPtr)functions;
        mwmHints.decorations = (IntPtr)decorations;

        //DriverDebug("SetWMStyles ({0}, {1}) functions = {2}, decorations = {3}", hwnd, cp, functions, decorations);

        if (cp.IsSet(WindowExStyles.WS_EX_TOOLWINDOW))
        {
            // needed! map toolwindows to _NET_WM_WINDOW_TYPE_UTILITY to make newer metacity versions happy
            // and get those windows in front of their parents
            //window_type = _NET_WM_WINDOW_TYPE_UTILITY;
        }
        else
        {
            // window_type = _NET_WM_WINDOW_TYPE_NORMAL;
        }

        if (!cp.IsSet(WindowExStyles.WS_EX_APPWINDOW))
        {
            hide_from_taskbar = true;
        }
        else if (cp.IsSet(WindowExStyles.WS_EX_TOOLWINDOW) && form != null && form.Parent != null && !form.ShowInTaskbar)
        {
            hide_from_taskbar = true;
        }
        else
        {
            hide_from_taskbar = false;
        }

        if (ExStyleSet(cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW))
        {
            if (form != null && !hwnd.reparented)
            {
                if (form.Owner != null && form.Owner.Handle != IntPtr.Zero)
                {
                    Hwnd owner_hwnd = Hwnd.ObjectFromHandle(form.Owner.Handle);
                    if (owner_hwnd != null)
                        transient_for_parent = owner_hwnd.whole_window;
                }
            }
        }
        if (StyleSet(cp.Style, WindowStyles.WS_POPUP) && (hwnd.parent != null) && (hwnd.parent.whole_window != IntPtr.Zero))
        {
            transient_for_parent = hwnd.parent.whole_window;
        }

        FormWindowState current_state = GetWindowState(hwnd.Handle);
        if (current_state == (FormWindowState)(-1))
            current_state = FormWindowState.Normal;

        client_rect = TranslateClientRectangleToXClientRectangle(hwnd);

        Invalidate(hwnd.Handle, client_rect, false);
        InvalidateNC(hwnd.Handle);
    }

    public static Rectangle TranslateClientRectangleToXClientRectangle(Hwnd hwnd)
    {
        return TranslateClientRectangleToXClientRectangle(hwnd, Control.FromHandle(hwnd.Handle));
    }

    internal static Rectangle TranslateClientRectangleToXClientRectangle(Hwnd hwnd, Control ctrl)
    {
        /* 
         * If this is a form with no window manager, X is handling all the border and caption painting
         * so remove that from the area (since the area we set of the window here is the part of the window 
         * we're painting in only)
         */
        Rectangle rect = hwnd.ClientRect;
        Form form = ctrl as Form;
        CreateParams cp = null;

        if (form != null)
            cp = form.GetCreateParams();

        if (form != null && (form.window_manager == null && !cp.IsSet(WindowExStyles.WS_EX_TOOLWINDOW)))
        {
            Hwnd.Borders borders = Hwnd.GetBorders(cp, null);
            Rectangle xrect = rect;

            xrect.Y -= borders.top;
            xrect.X -= borders.left;
            xrect.Width += borders.left + borders.right;
            xrect.Height += borders.top + borders.bottom;

            rect = xrect;
        }

        if (rect.Width < 1 || rect.Height < 1)
        {
            rect.Width = 1;
            rect.Height = 1;
            rect.X = -5;
            rect.Y = -5;
        }

        return rect;
    }
    void WaitForHwndMessage(Hwnd hwnd, Msg message)
    {
        WaitForHwndMessage(hwnd, message, false);

    }
    // messages WaitForHwndMwssage is waiting on
    static Hashtable messageHold;
    void WaitForHwndMessage(Hwnd hwnd, Msg message, bool process)
    {
        MSG msg = new MSG();
        XEventQueue queue;

        queue = ThreadQueue(Thread.CurrentThread);

        queue.DispatchIdle = false;

        bool done = false;
        string key = hwnd.Handle + ":" + message;
        if (!messageHold.ContainsKey(key))
            messageHold.Add(key, 1);
        else
            messageHold[key] = ((int)messageHold[key]) + 1;


        do
        {

            DebugHelper.WriteLine("Waiting for message " + message + " on hwnd " + String.Format("0x{0:x}", hwnd.Handle.ToInt32()));
            DebugHelper.Indent();

            if (PeekMessage(queue, ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE))
            {
                if ((Msg)msg.message == Msg.WM_QUIT)
                {
                    PostQuitMessage(0);
                    done = true;
                }
                else
                {

                    DebugHelper.WriteLine("PeekMessage got " + msg);

                    if (msg.hwnd == hwnd.Handle)
                    {
                        if ((Msg)msg.message == message)
                        {
                            if (process)
                            {
                                TranslateMessage(ref msg);
                                DispatchMessage(ref msg);
                            }
                            break;
                        }
                        else if ((Msg)msg.message == Msg.WM_DESTROY)
                            done = true;
                    }

                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
            }

            done = !messageHold.ContainsKey(key) || ((int)messageHold[key] < 1) || done;
        } while (!done);

        messageHold.Remove(key);

        DebugHelper.Unindent();
        DebugHelper.WriteLine("Finished waiting for " + key);

        queue.DispatchIdle = true;

    }

    void MapWindow(Hwnd hwnd, WindowType windows)
    {
        if (!hwnd.mapped)
        {
            Form f = Control.FromHandle(hwnd.Handle) as Form;
            if (f != null)
            {
                if (f.WindowState == FormWindowState.Normal)
                {
                    f.waiting_showwindow = true;
                    SendMessage(hwnd.Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);
                }
            }

            // it's possible that our Hwnd is no
            // longer valid after making that
            // SendMessage call, so check here.
            if (hwnd.zombie)
                return;

            if (hwnd.topmost)
            {
                // Most window managers will respect the _NET_WM_STATE property.
                // If not, use XMapRaised to map the window at the top level as
                // a last ditch effort.
                if ((windows & WindowType.Whole) != 0)
                {
                    //XMapRaised(DisplayHandle, hwnd.whole_window);
                }
                if ((windows & WindowType.Client) != 0)
                {
                    //XMapRaised(DisplayHandle, hwnd.client_window);
                }
                AddExpose(hwnd, true, hwnd.X, hwnd.Y, hwnd.Width, hwnd.Height);
            }
            else
            {
                if ((windows & WindowType.Whole) != 0)
                {
                    //XMapWindow(DisplayHandle, hwnd.whole_window);
                    //var ctrl = Control.FromHandle(hwnd.Handle);
                    //ctrl.Invalidate(true);
                    hwnd.DrawNeeded = true;

                    foreach (Hwnd item in hwnd.children)
                    {
                        Hwnd.ObjectFromHandle(item.Handle).DrawNeeded = true;
                    }
                }
                if ((windows & WindowType.Client) != 0)
                {
                    //XMapWindow(DisplayHandle, hwnd.client_window);
                    hwnd.DrawNeeded = true;

                    foreach (Hwnd item in hwnd.children)
                    {
                        Hwnd.ObjectFromHandle(item.Handle).DrawNeeded = true;
                    }
                }
            }

            hwnd.mapped = true;

            if (f != null)
            {
                if (f.waiting_showwindow)
                {
                    WaitForHwndMessage(hwnd, Msg.WM_SHOWWINDOW);
                    CreateParams cp = f.GetCreateParams();
                    if (!ExStyleSet(cp.ExStyle, WindowExStyles.WS_EX_MDICHILD) &&
                        !StyleSet(cp.Style, WindowStyles.WS_CHILD))
                    {
                        WaitForHwndMessage(hwnd, Msg.WM_ACTIVATE, true);
                    }
                }
            }
        }
    }

    void UnmapWindow(Hwnd hwnd, WindowType windows)
    {
        if (hwnd.mapped)
        {
            Form f = null;
            if (Control.FromHandle(hwnd.Handle) is Form)
            {
                f = Control.FromHandle(hwnd.Handle) as Form;
                if (f.WindowState == FormWindowState.Normal)
                {
                    f.waiting_showwindow = true;
                    SendMessage(hwnd.Handle, Msg.WM_SHOWWINDOW, IntPtr.Zero, IntPtr.Zero);
                }
            }

            // it's possible that our Hwnd is no
            // longer valid after making that
            // SendMessage call, so check here.
            // FIXME: it is likely wrong, as it has already sent WM_SHOWWINDOW
            if (hwnd.zombie)
                return;

            if ((windows & WindowType.Client) != 0)
            {
                //XUnmapWindow(DisplayHandle, hwnd.client_window);
            }
            if ((windows & WindowType.Whole) != 0)
            {
                //XUnmapWindow(DisplayHandle, hwnd.whole_window);
            }

            hwnd.mapped = false;

            if (f != null)
            {
                if (f.waiting_showwindow)
                {
                    WaitForHwndMessage(hwnd, Msg.WM_SHOWWINDOW);
                    CreateParams cp = f.GetCreateParams();
                    if (!ExStyleSet(cp.ExStyle, WindowExStyles.WS_EX_MDICHILD) &&
                        !StyleSet(cp.Style, WindowStyles.WS_CHILD))
                    {
                        WaitForHwndMessage(hwnd, Msg.WM_ACTIVATE, true);
                    }
                }
            }
        }
    }

    void SendParentNotify(IntPtr child, Msg cause, int x, int y)
    {
        Hwnd hwnd;

        if (child == IntPtr.Zero)
        {
            return;
        }

        hwnd = Hwnd.GetObjectFromWindow(child);

        if (hwnd == null)
        {
            return;
        }

        if (hwnd.Handle == IntPtr.Zero)
        {
            return;
        }

        if (ExStyleSet((int)hwnd.initial_ex_style, WindowExStyles.WS_EX_NOPARENTNOTIFY))
        {
            return;
        }

        if (hwnd.Parent == null)
        {
            return;
        }

        if (hwnd.Parent.Handle == IntPtr.Zero)
        {
            return;
        }

        if (cause == Msg.WM_CREATE || cause == Msg.WM_DESTROY)
        {
            SendMessage(hwnd.Parent.Handle, Msg.WM_PARENTNOTIFY, Control.MakeParam((int)cause, 0), child);
        }
        else
        {
            SendMessage(hwnd.Parent.Handle, Msg.WM_PARENTNOTIFY, Control.MakeParam((int)cause, 0), Control.MakeParam(x, y));
        }

        SendParentNotify(hwnd.Parent.Handle, cause, x, y);
    }
    static Hashtable MessageQueues;
    static ArrayList unattached_timer_list; // holds timers that are enabled but not attached to a window.
    XEventQueue ThreadQueue(Thread thread)
    {
        XEventQueue queue;

        queue = (XEventQueue)MessageQueues[thread];
        if (queue == null)
        {
            queue = new XEventQueue(thread);
            MessageQueues[thread] = queue;
        }

        return queue;
    }
    bool StyleSet(int s, WindowStyles ws)
    {
        return (s & (int)ws) == (int)ws;
    }

    bool ExStyleSet(int ex, WindowExStyles exws)
    {
        return (ex & (int)exws) == (int)exws;
    }

    private static int WindowHandleCount = 1;

    private Hashtable registered_classes;
    private IntPtr AsyncAtom = new IntPtr(6544352);
    private IntPtr PostAtom = new IntPtr(6544353);

    private string RegisterWindowClass(int classStyle)
    {
        string class_name;

        lock (registered_classes)
        {
            class_name = (string)registered_classes[classStyle];

            if (class_name != null)
                return class_name;

            class_name = string.Format("Mono.WinForms.{0}.{1}", System.Threading.Thread.GetDomainID().ToString(), classStyle);

            WNDCLASS wndClass;

            wndClass.style = classStyle;
            wndClass.lpfnWndProc = wnd_proc;
            wndClass.cbClsExtra = 0;
            wndClass.cbWndExtra = 0;
            wndClass.hbrBackground = (IntPtr)(GetSysColorIndex.COLOR_WINDOW + 1);
            //wndClass.hCursor = Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);
            wndClass.hIcon = IntPtr.Zero;
            wndClass.hInstance = IntPtr.Zero;
            wndClass.lpszClassName = class_name;
            wndClass.lpszMenuName = "";

            //bool result = Win32RegisterClass(ref wndClass);

            //if (result == false) Win32MessageBox(IntPtr.Zero, "Could not register the window class, win32 error " + Win32GetLastError().ToString(), "Oops", 0);

            registered_classes[classStyle] = class_name;
        }

        return class_name;
    }

    public override IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height)
    {
        CreateParams create_params = new CreateParams();

        create_params.Caption = "";
        create_params.X = X;
        create_params.Y = Y;
        create_params.Width = Width;
        create_params.Height = Height;

        create_params.ClassName = XplatUI.GetDefaultClassName(GetType());
        create_params.ClassStyle = 0;
        create_params.ExStyle = 0;
        create_params.Parent = IntPtr.Zero;
        create_params.Param = 0;

        return CreateWindow(create_params);
    }

    public override void DestroyWindow(IntPtr handle)
    {

        Hwnd hwnd;
        hwnd = Hwnd.ObjectFromHandle(handle);

        // The window should never ever be a zombie here, since we should
        // wait until it's completely dead before returning from 
        // "destroying" calls, but just in case....
        if (hwnd == null || hwnd.zombie)
        {
            DriverDebug("window {0:X} already destroyed", handle.ToInt32());
            return;
        }

        DriverDebug("Destroying window {0}", XplatUI.Window(hwnd.client_window));

        SendParentNotify(hwnd.Handle, Msg.WM_DESTROY, int.MaxValue, int.MaxValue);

        CleanupCachedWindows(hwnd);

        ArrayList windows = new ArrayList();

        AccumulateDestroyedHandles(Control.ControlNativeWindow.ControlFromHandle(hwnd.Handle), windows);


        foreach (Hwnd h in windows)
        {
            SendMessage(h.Handle, Msg.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
            h.zombie = true;
            h.Dispose();
        }

        //lock (XlibLock)
        {
            if (hwnd.whole_window != IntPtr.Zero)
            {
                DriverDebug("XDestroyWindow (whole_window = {0:X})", hwnd.whole_window.ToInt32());
                //Keyboard.DestroyICForWindow(hwnd.whole_window);
                //XDestroyWindow(DisplayHandle, hwnd.whole_window);
            }
            else if (hwnd.client_window != IntPtr.Zero)
            {
                DriverDebug("XDestroyWindow (client_window = {0:X})", hwnd.client_window.ToInt32());
                //Keyboard.DestroyICForWindow(hwnd.client_window);
                //XDestroyWindow(DisplayHandle, hwnd.client_window);
            }

            PaintPending = true;
        }
    }

    void AccumulateDestroyedHandles(Control c, ArrayList list)
    {
        DebugHelper.Enter();
        if (c != null)
        {

            Control[] controls = c.Controls.GetAllControls();

            DebugHelper.WriteLine("Checking control:0x{0:x}", c.IsHandleCreated ? c.Handle.ToInt32() : 0);

            if (c.IsHandleCreated && !c.IsDisposed)
            {
                Hwnd hwnd = Hwnd.ObjectFromHandle(c.Handle);

                DriverDebug(" + adding {0} to the list of zombie windows", XplatUI.Window(hwnd.Handle));
                //DriverDebug(" + parent X window is {0:X}", XGetParent(hwnd.whole_window).ToInt32());

                list.Add(hwnd);
                CleanupCachedWindows(hwnd);
            }

            for (int i = 0; i < controls.Length; i++)
            {
                AccumulateDestroyedHandles(controls[i], list);
            }
        }
        DebugHelper.Leave();
    }
    static IntPtr ActiveWindow;		// Handle of the active window

    void CleanupCachedWindows(Hwnd hwnd)
    {
        if (ActiveWindow == hwnd.Handle)
        {
            SendMessage(hwnd.client_window, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
            ActiveWindow = IntPtr.Zero;
        }

        if (FocusWindow == hwnd.Handle)
        {
            SendMessage(hwnd.client_window, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
            FocusWindow = IntPtr.Zero;
        }

        if (Grab.Hwnd == hwnd.Handle)
        {
            Grab.Hwnd = IntPtr.Zero;
            Grab.Confined = false;
        }

        DestroyCaret(hwnd.Handle);
    }
    public override FormWindowState GetWindowState(IntPtr handle)
    {
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);

        if (hwnd.cached_window_state == (FormWindowState)(-1))
            hwnd.cached_window_state = UpdateWindowState(handle);

        return hwnd.cached_window_state;
    }


    FormWindowState UpdateWindowState(IntPtr handle)
    {
        IntPtr actual_atom;
        int actual_format;
        IntPtr nitems;
        IntPtr bytes_after;
        IntPtr prop = IntPtr.Zero;
        IntPtr atom;
        int maximized;
        bool minimized;
        XWindowAttributes attributes;
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);

        maximized = 0;
        minimized = false;


        return FormWindowState.Normal;
    }

    public override void SetWindowState(IntPtr handle, FormWindowState state)
    {

        FormWindowState current_state;
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);

        current_state = GetWindowState(handle);

        if (current_state == state)
        {
            return;
        }

        switch (state)
        {
            case FormWindowState.Normal:
                {
                    //lock (XlibLock)
                    {
                        if (current_state == FormWindowState.Minimized)
                        {
                            MapWindow(hwnd, WindowType.Both);
                        }
                        else if (current_state == FormWindowState.Maximized)
                        {
                            // SendNetWMMessage(hwnd.whole_window, _NET_WM_STATE, (IntPtr)2 /* toggle */, _NET_WM_STATE_MAXIMIZED_HORZ, _NET_WM_STATE_MAXIMIZED_VERT);
                        }
                    }
                    Activate(handle);
                    return;
                }

            case FormWindowState.Minimized:
                {
                    // lock (XlibLock)
                    {
                        if (current_state == FormWindowState.Maximized)
                        {
                            //SendNetWMMessage(hwnd.whole_window, _NET_WM_STATE, (IntPtr)2 /* toggle */, _NET_WM_STATE_MAXIMIZED_HORZ, _NET_WM_STATE_MAXIMIZED_VERT);
                        }
                        //XIconifyWindow(DisplayHandle, hwnd.whole_window, ScreenNo);
                    }
                    return;
                }

            case FormWindowState.Maximized:
                {
                    // lock (XlibLock)
                    {
                        if (current_state == FormWindowState.Minimized)
                        {
                            MapWindow(hwnd, WindowType.Both);
                        }

                        //SendNetWMMessage(hwnd.whole_window, _NET_WM_STATE, (IntPtr)1 /* Add */, _NET_WM_STATE_MAXIMIZED_HORZ, _NET_WM_STATE_MAXIMIZED_VERT);
                    }
                    Activate(handle);
                    return;
                }
        }
    }

    public override void SetWindowMinMax(IntPtr handle, Rectangle maximized, Size min, Size max)
    {
        Control ctrl = Control.FromHandle(handle);
        SetWindowMinMax(handle, maximized, min, max, ctrl != null ? ctrl.GetCreateParams() : null);
    }

    internal void SetWindowMinMax(IntPtr handle, Rectangle maximized, Size min, Size max, CreateParams cp)
    {
        Hwnd hwnd;
        XSizeHints hints;
        IntPtr dummy;

        hwnd = Hwnd.ObjectFromHandle(handle);
        if (hwnd == null)
        {
            return;
        }

        min.Width = Math.Max(min.Width, SystemInformation.MinimumWindowSize.Width);
        min.Height = Math.Max(min.Height, SystemInformation.MinimumWindowSize.Height);

        hints = new XSizeHints();

        //XGetWMNormalHints(DisplayHandle, hwnd.whole_window, ref hints, out dummy);
        if ((min != Size.Empty) && (min.Width > 0) && (min.Height > 0))
        {
            if (cp != null)
                min = TranslateWindowSizeToXWindowSize(cp, min);
            hints.flags = (IntPtr)((int)hints.flags | (int)XSizeHintsFlags.PMinSize);
            hints.min_width = min.Width;
            hints.min_height = min.Height;
        }

        if ((max != Size.Empty) && (max.Width > 0) && (max.Height > 0))
        {
            if (cp != null)
                max = TranslateWindowSizeToXWindowSize(cp, max);
            hints.flags = (IntPtr)((int)hints.flags | (int)XSizeHintsFlags.PMaxSize);
            hints.max_width = max.Width;
            hints.max_height = max.Height;
        }

        if (hints.flags != IntPtr.Zero)
        {
            // The Metacity team has decided that they won't care about this when clicking the maximize icon, 
            // they will maximize the window to fill the screen/parent no matter what.
            // http://bugzilla.ximian.com/show_bug.cgi?id=80021
            //XSetWMNormalHints(DisplayHandle, hwnd.whole_window, ref hints);
        }

        if ((maximized != Rectangle.Empty) && (maximized.Width > 0) && (maximized.Height > 0))
        {
            if (cp != null)
                maximized.Size = TranslateWindowSizeToXWindowSize(cp);
            hints.flags = (IntPtr)XSizeHintsFlags.PPosition;
            hints.x = maximized.X;
            hints.y = maximized.Y;
            hints.width = maximized.Width;
            hints.height = maximized.Height;

            // Metacity does not seem to follow this constraint for maximized (zoomed) windows
            //XSetZoomHints(DisplayHandle, hwnd.whole_window, ref hints);
        }
    }

    public override void SetWindowStyle(IntPtr handle, CreateParams cp)
    {
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);
        SetHwndStyles(hwnd, cp);
        SetWMStyles(hwnd, cp);

        RequestNCRecalc(hwnd.Handle);
        AddExpose(hwnd, true, 0, 0, hwnd.Width, hwnd.Height);
    }
    void SetHwndStyles(Hwnd hwnd, CreateParams cp)
    {
        DeriveStyles(cp.Style, cp.ExStyle, out hwnd.border_style, out hwnd.border_static, out hwnd.title_style, out hwnd.caption_height, out hwnd.tool_caption_height);
    }

    void DeriveStyles(int Style, int ExStyle, out FormBorderStyle border_style, out bool border_static, out TitleStyle title_style, out int caption_height, out int tool_caption_height)
    {

        caption_height = 0;
        tool_caption_height = 19;
        border_static = false;

        if (StyleSet(Style, WindowStyles.WS_CHILD))
        {
            if (ExStyleSet(ExStyle, WindowExStyles.WS_EX_CLIENTEDGE))
            {
                border_style = FormBorderStyle.Fixed3D;
            }
            else if (ExStyleSet(ExStyle, WindowExStyles.WS_EX_STATICEDGE))
            {
                border_style = FormBorderStyle.Fixed3D;
                border_static = true;
            }
            else if (!StyleSet(Style, WindowStyles.WS_BORDER))
            {
                border_style = FormBorderStyle.None;
            }
            else
            {
                border_style = FormBorderStyle.FixedSingle;
            }
            title_style = TitleStyle.None;

            if (StyleSet(Style, WindowStyles.WS_CAPTION))
            {
                caption_height = 19;
                if (ExStyleSet(ExStyle, WindowExStyles.WS_EX_TOOLWINDOW))
                {
                    title_style = TitleStyle.Tool;
                }
                else
                {
                    title_style = TitleStyle.Normal;
                }
            }

            if (ExStyleSet(ExStyle, WindowExStyles.WS_EX_MDICHILD))
            {
                caption_height = 19;

                if (StyleSet(Style, WindowStyles.WS_OVERLAPPEDWINDOW) ||
                    ExStyleSet(ExStyle, WindowExStyles.WS_EX_TOOLWINDOW))
                {
                    border_style = (FormBorderStyle)0xFFFF;
                }
                else
                {
                    border_style = FormBorderStyle.None;
                }
            }

        }
        else
        {
            title_style = TitleStyle.None;
            if (StyleSet(Style, WindowStyles.WS_CAPTION))
            {
                if (ExStyleSet(ExStyle, WindowExStyles.WS_EX_TOOLWINDOW))
                {
                    title_style = TitleStyle.Tool;
                }
                else
                {
                    title_style = TitleStyle.Normal;
                }
            }

            border_style = FormBorderStyle.None;

            if (StyleSet(Style, WindowStyles.WS_THICKFRAME))
            {
                if (ExStyleSet(ExStyle, WindowExStyles.WS_EX_TOOLWINDOW))
                {
                    border_style = FormBorderStyle.SizableToolWindow;
                }
                else
                {
                    border_style = FormBorderStyle.Sizable;
                }
            }
            else
            {
                if (StyleSet(Style, WindowStyles.WS_CAPTION))
                {
                    if (ExStyleSet(ExStyle, WindowExStyles.WS_EX_CLIENTEDGE))
                    {
                        border_style = FormBorderStyle.Fixed3D;
                    }
                    else if (ExStyleSet(ExStyle, WindowExStyles.WS_EX_STATICEDGE))
                    {
                        border_style = FormBorderStyle.Fixed3D;
                        border_static = true;
                    }
                    else if (ExStyleSet(ExStyle, WindowExStyles.WS_EX_DLGMODALFRAME))
                    {
                        border_style = FormBorderStyle.FixedDialog;
                    }
                    else if (ExStyleSet(ExStyle, WindowExStyles.WS_EX_TOOLWINDOW))
                    {
                        border_style = FormBorderStyle.FixedToolWindow;
                    }
                    else if (StyleSet(Style, WindowStyles.WS_BORDER))
                    {
                        border_style = FormBorderStyle.FixedSingle;
                    }
                }
                else
                {
                    if (StyleSet(Style, WindowStyles.WS_BORDER))
                    {
                        border_style = FormBorderStyle.FixedSingle;
                    }
                }
            }
        }
    }
    public override double GetWindowTransparency(IntPtr handle)
    {
        return 1.0;
    }

    public override void SetWindowTransparency(IntPtr handle, double transparency, Color key)
    {
        throw new NotImplementedException();
    }

    public override TransparencySupport SupportsTransparency()
    {
        return TransparencySupport.None;
    }

    public override void SetBorderStyle(IntPtr handle, FormBorderStyle border_style)
    {
        Form form = Control.FromHandle(handle) as Form;
        if (form != null && form.window_manager == null)
        {
            CreateParams cp = form.GetCreateParams();
            if (border_style == FormBorderStyle.FixedToolWindow ||
                border_style == FormBorderStyle.SizableToolWindow ||
                cp.IsSet(WindowExStyles.WS_EX_TOOLWINDOW))
            {
                form.window_manager = new ToolWindowManager(form);
            }
        }

        RequestNCRecalc(handle);
    }

    public override void SetMenu(IntPtr handle, Menu menu)
    {
        throw new NotImplementedException();
    }

    public override bool GetText(IntPtr handle, out string text)
    {
        throw new NotImplementedException();
    }

    public override bool Text(IntPtr handle, string text)
    {
        //throw new NotImplementedException();
        return true;
    }
    /// <summary>
    /// Sets the specified window's show state.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="visible"></param>
    /// <param name="activate"></param>
    /// <returns></returns>
    public override bool SetVisible(IntPtr handle, bool visible, bool activate)
    {
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);

        if (hwnd == null)
            return false;

        hwnd.visible = visible;

        if (visible)
        {
            MapWindow(hwnd, WindowType.Both);

            if (Control.FromHandle(handle) is Form)
            {
                FormWindowState s;

                s = ((Form)Control.FromHandle(handle)).WindowState;

                switch (s)
                {
                    case FormWindowState.Minimized: SetWindowState(handle, FormWindowState.Minimized); break;
                    case FormWindowState.Maximized: SetWindowState(handle, FormWindowState.Maximized); break;
                }
            }

            SendMessage(handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
        }
        else
        {
            UnmapWindow(hwnd, WindowType.Both);
        }

        PaintPending = true;

        return true;
    }

    public override bool IsVisible(IntPtr handle)
    {
        Hwnd hwnd = Hwnd.ObjectFromHandle(handle);
        return (hwnd != null && hwnd.visible);
    }

    public override bool IsEnabled(IntPtr handle)
    {
        Hwnd hwnd = Hwnd.ObjectFromHandle(handle);
        return (hwnd != null && hwnd.Enabled);
    }

    public override IntPtr SetParent(IntPtr handle, IntPtr parent)
    {
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);
        hwnd.parent = Hwnd.ObjectFromHandle(parent);

        //lock (XlibLock)
        {
            DriverDebug("Parent for window {0} = {1}", XplatUI.Window(hwnd.Handle), XplatUI.Window(hwnd.parent != null ? hwnd.parent.Handle : IntPtr.Zero));
            //XReparentWindow(DisplayHandle, hwnd.whole_window, hwnd.parent == null ? FosterParent : hwnd.parent.client_window, hwnd.x, hwnd.y);
            SendMessage(handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
        }

        return IntPtr.Zero;
    }

    public override IntPtr GetParent(IntPtr handle, bool with_owner)
    {
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);
        if (hwnd != null)
        {
            if (hwnd.parent != null)
            {
                return hwnd.parent.Handle;
            }
            //if (hwnd.owner != null && with_owner)
            {
                //return hwnd.owner.Handle;
            }
        }
        return IntPtr.Zero;
    }

    public override void UpdateWindow(IntPtr handle)
    {
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);

        if (hwnd == null || !hwnd.visible || !hwnd.expose_pending || hwnd.inonpaint) // || !hwnd.Mapped)
        {
            return;
        }

        SendMessage(handle, Msg.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
        hwnd.Queue.Paint.Remove(hwnd);

        //driver.UpdateWindow(handle);
        //throw new NotImplementedException();
    }

    public override PaintEventArgs PaintEventStart(ref Message msg, IntPtr handle, bool client)
    {
        PaintEventArgs paint_event;
        Hwnd hwnd;
        Hwnd paint_hwnd;


        // 
        // handle  (and paint_hwnd) refers to the window that is should be painted.
        // msg.HWnd (and hwnd) refers to the window that got the paint message.
        // 

        hwnd = Hwnd.ObjectFromHandle(msg.HWnd);
        if (msg.HWnd == handle)
        {
            paint_hwnd = hwnd;
        }
        else
        {
            paint_hwnd = Hwnd.ObjectFromHandle(handle);
        }

        Monitor.Enter(paintlock);

        hwnd.inonpaint = true;

        //Console.WriteLine("PaintEventStart " + XplatUI.Window(handle) + " th: " + Thread.CurrentThread.Name);

        Graphics dc;

        if (client)
        {
            Region clip_region = new Region();
            clip_region.MakeEmpty();

            foreach (Rectangle r in hwnd.ClipRectangles)
            {
                /* Expand the region slightly.
                 * See bug 464464.
                 */
                Rectangle r2 = Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom + 1);
                clip_region.Union(r2);
            }

            if (hwnd.UserClip != null)
            {
                clip_region.Intersect(hwnd.UserClip);
            }

            if(hwnd.pic == null)
                hwnd.pic = new SKPictureRecorder();
            var newcanvas = hwnd.pic.BeginRecording(SKRect.Empty);
            if (paint_hwnd.hwndbmp != null)
                if (!hwnd.Invalid.Contains(hwnd.ClientRect) )// && (hwnd.ClientRect.Width != hwnd.Invalid.Width && hwnd.ClientRect.Height != hwnd.Invalid.Height))
                {
                    var raster = paint_hwnd.hwndbmp.Snapshot();
                    newcanvas.DrawPicture(raster, 0, 0);                    
                }

            newcanvas.ClipRegion(clip_region);

            dc = Graphics.FromCanvas(newcanvas);
            dc.Clip = clip_region;
            if (hwnd.WholeWindow != hwnd.ClientWindow)
            {
                var frm = Form.FromHandle(hwnd.Handle);
                if (frm != null)
                {
                    var borders = Hwnd.GetBorders(frm.GetCreateParams(), null);
                    newcanvas.Discard();
                    newcanvas.ClipRect(new SKRect(0, 0, hwnd.width - borders.left - borders.right, hwnd.height - borders.bottom - borders.top), (SKClipOperation) 5);
                }
            }
            paint_event = new PaintEventArgs(dc, hwnd.Invalid) { Tag = hwnd.pic };
            hwnd.expose_pending = false;

            hwnd.ClearInvalidArea();

            return paint_event;
        }
        else
        {
            //dc = Graphics.FromSKImage(paint_hwnd.hwndbmpNC);//Graphics.FromHwnd(paint_hwnd.whole_window);
            var pic = new SKPictureRecorder();
            var newcanvas = pic.BeginRecording(SKRect.Empty);
            dc = Graphics.FromCanvas(newcanvas);

            if (!hwnd.nc_invalid.IsEmpty)
            {
                dc.SetClip(hwnd.nc_invalid);
                paint_event = new PaintEventArgs(dc, hwnd.nc_invalid) { Tag = pic };
            }
            else
            {
                paint_event = new PaintEventArgs(dc, new Rectangle(0, 0, hwnd.width, hwnd.height)) { Tag = pic };
            }
            hwnd.nc_expose_pending = false;

            hwnd.ClearNcInvalidArea();

            return paint_event;
        }
    }
    public override void PaintEventEnd(ref Message msg, IntPtr handle, bool client, PaintEventArgs pevent)
    {
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(msg.HWnd);
        if (msg.HWnd == handle)
        {

        }
        else
        {
            hwnd = Hwnd.ObjectFromHandle(handle);
        }

        if (((SKPictureRecorder)pevent.Tag).RecordingCanvas == null)
        {
            Monitor.Exit(paintlock);
            return;
        }

        if (client)
        {
            var pic = ((SKPictureRecorder)pevent.Tag).EndRecordingAsDrawable();
            //var img = SKImage.FromPicture(pic.Snapshot(), new SKSizeI(hwnd.width, hwnd.height));
            //var bmp = SKBitmap.FromImage(img);
            //pic.Dispose();
            //img.Dispose();

            hwnd.hwndbmp = pic;
            
            //hwnd.hwndbmp = pic;

            //using (var st = File.OpenWrite(hwnd.Handle.ToString() + DateTime.Now.ToString("s") + ".skp"))
            //pic.Serialize(st);

            //using (var st = File.OpenWrite(hwnd.Handle.ToString() /*+ DateTime.Now.ToString("s").Replace(":","-")*/ + ".jpg"))
                //SKImage.FromPicture(pic.Snapshot(), new SKSizeI(hwnd.width, hwnd.height)).Encode().SaveTo(st);
        }
        else
        {
            hwnd.hwndbmpNC = SKImage.FromPicture(((SKPictureRecorder) pevent.Tag).EndRecording(),                new SKSizeI(hwnd.width, hwnd.height));
        }

        //Console.WriteLine("PaintEventEnd " + XplatUI.Window(handle) + " th: " + Thread.CurrentThread.Name + " " + hwnd.hwndbmp);

        //pevent.Graphics.Dispose();

        hwnd.inonpaint = false;

        PaintPending = true;

        // this needs to be drawn
        hwnd.DrawNeeded = true;

        Monitor.Exit(paintlock);

        //driver.PaintEventEnd(ref msg, handle, client, pevent);
        //throw new NotImplementedException();
    }

    public static bool PaintPending { get; set; }

    public static object paintlock = new object();

    /// <summary>
    /// Changes the position and dimensions of the specified window. For a top-level window, the position and dimensions are relative to the upper-left corner of the screen. For a child window, they are relative to the upper-left corner of the parent window's client area.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public override void SetWindowPos(IntPtr handle, int x, int y, int width, int height)
    {
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);

        if (hwnd == null)
        {
            return;
        }

        if (Control.FromHandle(handle) is Form && (Control.FromHandle(handle) as Form).WindowState == FormWindowState.Maximized)
        {
            x = -4;
            y = -4;
            width = Screen.PrimaryScreen.WorkingArea.Width + 8;
            height = Screen.PrimaryScreen.WorkingArea.Height + 8;
        }

        // Win32 automatically changes negative width/height to 0.
        if (width < 0)
            width = 0;
        if (height < 0)
            height = 0;

        // X requires a sanity check for width & height; otherwise it dies
        if (hwnd.zero_sized && width > 0 && height > 0)
        {
            if (hwnd.visible)
            {
                MapWindow(hwnd, WindowType.Whole);
            }
            hwnd.zero_sized = false;
        }

        if ((width < 1) || (height < 1))
        {
            hwnd.zero_sized = true;
            UnmapWindow(hwnd, WindowType.Whole);
        }

        // Save a server roundtrip (and prevent a feedback loop)
        if ((hwnd.x == x) && (hwnd.y == y) &&
            (hwnd.width == width) && (hwnd.height == height))
        {
            return;
        }

        hwnd.x = x;
        hwnd.y = y;
        hwnd.width = width;
        hwnd.height = height;

        if (!hwnd.zero_sized)
        {
            if (hwnd.fixed_size)
            {
                SetWindowMinMax(handle, Rectangle.Empty, new Size(width, height), new Size(width, height));
            }

            //lock (XlibLock)
            {
                Control ctrl = Control.FromHandle(handle);
                Size TranslatedSize = TranslateWindowSizeToXWindowSize(ctrl.GetCreateParams(), new Size(width, height));
                //MoveResizeWindow(DisplayHandle, hwnd.whole_window, x, y, TranslatedSize.Width, TranslatedSize.Height);

                Form form = Control.FromHandle(hwnd.client_window) as Form;
                if (form != null && !hwnd.resizing_or_moving)
                {
                    if (hwnd.x != form.Bounds.X || hwnd.y != form.Bounds.Y)
                    {
                        SendMessage(form.Handle, Msg.WM_SYSCOMMAND, (IntPtr)SystemCommands.SC_MOVE, IntPtr.Zero);
                        hwnd.resizing_or_moving = true;
                    }
                    else if (hwnd.width != form.Bounds.Width || hwnd.height != form.Bounds.Height)
                    {
                        SendMessage(form.Handle, Msg.WM_SYSCOMMAND, (IntPtr)SystemCommands.SC_SIZE, IntPtr.Zero);
                        hwnd.resizing_or_moving = true;
                        // ensure we dont window copy the previous draw, and is exposed bellow
                        lock(paintlock)
                            hwnd.hwndbmp = null;
                    }
                    if (hwnd.resizing_or_moving)
                        if (hwnd.resizing_or_moving)
                            SendMessage(form.Handle, Msg.WM_ENTERSIZEMOVE, IntPtr.Zero, IntPtr.Zero);
                }

                PerformNCCalc(hwnd);
            }
        }

        SendMessage(hwnd.client_window, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);

        return;
    }

    public override void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height,
        out int client_width, out int client_height)
    {

        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);

        if (hwnd != null)
        {
            x = hwnd.x;
            y = hwnd.y;
            width = hwnd.width;
            height = hwnd.height;

            if (width > WorkingArea.Width+8)
            {
                hwnd.width = WorkingArea.Width;
                width = WorkingArea.Width;
            }
            if (height > WorkingArea.Height+8)
            {
                hwnd.height = WorkingArea.Height;
                height = WorkingArea.Height;
            }

            PerformNCCalc(hwnd);

            client_width = hwnd.ClientRect.Width;
            client_height = hwnd.ClientRect.Height;

            return;
        }

        // Should we throw an exception or fail silently?
        // throw new ArgumentException("Called with an invalid window handle", "handle");

        x = 0;
        y = 0;
        width = 0;
        height = 0;
        client_width = 0;
        client_height = 0;

        //driver.GetWindowPos(handle, is_toplevel, out x, out y, out width, out height, out client_width, out client_height);
        //throw new NotImplementedException();
    }


    void PerformNCCalc(Hwnd hwnd)
    {
        NCCALCSIZE_PARAMS ncp;
        IntPtr ptr;
        Rectangle rect;

        rect = new Rectangle(0, 0, hwnd.Width, hwnd.Height);

        ncp = new NCCALCSIZE_PARAMS();
        ptr = Marshal.AllocHGlobal(Marshal.SizeOf(ncp));

        ncp.rgrc1.left = rect.Left;
        ncp.rgrc1.top = rect.Top;
        ncp.rgrc1.right = rect.Right;
        ncp.rgrc1.bottom = rect.Bottom;

        Marshal.StructureToPtr(ncp, ptr, true);
        NativeWindow.WndProc(hwnd.client_window, Msg.WM_NCCALCSIZE, (IntPtr)1, ptr);
        ncp = (NCCALCSIZE_PARAMS)Marshal.PtrToStructure(ptr, typeof(NCCALCSIZE_PARAMS));
        Marshal.FreeHGlobal(ptr);

        var ctl = Control.FromHandle(hwnd.client_window) as Form;
        if (ctl != null && hwnd.parent == null && ctl.WindowState == FormWindowState.Maximized)
        {
            rect = new Rectangle(0, 0, ncp.rgrc1.right,
                ncp.rgrc1.bottom);
            hwnd.ClientRect = rect;
        }
        else
        {
            rect = new Rectangle(ncp.rgrc1.left, ncp.rgrc1.top, ncp.rgrc1.right - ncp.rgrc1.left,
                ncp.rgrc1.bottom - ncp.rgrc1.top);
            hwnd.ClientRect = rect;
        }

        var rect2 = TranslateClientRectangleToXClientRectangle(hwnd);

        if (hwnd.visible)
        {
            //MoveResizeWindow(DisplayHandle, hwnd.client_window, rect.X, rect.Y, rect.Width, rect.Height);
        }

        AddExpose(hwnd, hwnd.WholeWindow == hwnd.ClientWindow, 0, 0, hwnd.Width, hwnd.Height);
    }


    void AddExpose(Hwnd hwnd, bool client, int x, int y, int width, int height)
    {
        // Don't waste time
        if ((hwnd == null) || (x > hwnd.Width) || (y > hwnd.Height) || ((x + width) < 0) || ((y + height) < 0))
        {
            return;
        }

        // Keep the invalid area as small as needed
        if ((x + width) > hwnd.width)
        {
            width = hwnd.width - x;
        }

        if ((y + height) > hwnd.height)
        {
            height = hwnd.height - y;
        }

        if (client)
        {
            hwnd.AddInvalidArea(x, y, width, height);
            if (!hwnd.expose_pending)
            {
                if (!hwnd.nc_expose_pending)
                {
                    hwnd.Queue.Paint.Enqueue(hwnd);
                }
                hwnd.expose_pending = true;
            }
        }
        else
        {
            hwnd.AddNcInvalidArea(x, y, width, height);

            if (!hwnd.nc_expose_pending)
            {
                if (!hwnd.expose_pending)
                {
                    hwnd.Queue.Paint.Enqueue(hwnd);
                }
                hwnd.nc_expose_pending = true;
            }
        }
    }
    /// <summary>
    /// Activates a window. The window must be attached to the calling thread's message queue.
    /// The SetActiveWindow function activates a window, but not if the application is in the background. The window will be brought into the foreground (top of Z-Order) if its application is in the foreground when the system activates the window.
    /// </summary>
    /// <param name="handle"></param>
    public override void Activate(IntPtr handle)
    {
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);

        XEventQueue q = null;
        lock (unattached_timer_list)
        {
            foreach (Timer t in unattached_timer_list)
            {
                if (q == null)
                    q = (XEventQueue)MessageQueues[Thread.CurrentThread];
                t.thread = q.Thread;
                q.timer_list.Add(t);
            }
            unattached_timer_list.Clear();
        }

        return;
    }
    /// <summary>
    /// The EnableWindow function enables or disables mouse and keyboard input to the specified window or control. When input is disabled, the window does not receive input such as mouse clicks and key presses. When input is enabled, the window receives all input.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="Enable"></param>
    public override void EnableWindow(IntPtr handle, bool Enable)
    {
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);
        if (hwnd != null)
        {
            hwnd.Enabled = Enable;
        }

        return;
    }
    // Modality support
    static Stack ModalWindows;		// Stack of our modal windows
    public override void SetModal(IntPtr handle, bool Modal)
    {
        if (Modal)
        {
            ModalWindows.Push(handle);
        }
        else
        {
            if (ModalWindows.Contains(handle))
            {
                ModalWindows.Pop();
            }
            if (ModalWindows.Count > 0)
            {
                Activate((IntPtr)ModalWindows.Peek());
            }
        }

        Hwnd hwnd = Hwnd.ObjectFromHandle(handle);
        Control ctrl = Control.FromHandle(handle);
        SetWMStyles(hwnd, ctrl.GetCreateParams());

        int width = hwnd.width;
        SetWindowPos(hwnd.Handle, hwnd.x, hwnd.y, width + 1, hwnd.height);
        SetWindowPos(hwnd.Handle, hwnd.x, hwnd.y, width, hwnd.height);
    }
    /// <summary>
    /// The InvalidateRect function adds a rectangle to the specified window's update region. The update region represents the portion of the window's client area that must be redrawn.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="rc"></param>
    /// <param name="clear"></param>
    public override void Invalidate(IntPtr handle, Rectangle rc, bool clear)
    {
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);

        if (clear)
        {
            AddExpose(hwnd, true, hwnd.X, hwnd.Y, hwnd.Width, hwnd.Height);
        }
        else
        {
            AddExpose(hwnd, true, rc.X, rc.Y, rc.Width, rc.Height);
        }

        return;
    }

    public override void InvalidateNC(IntPtr handle)
    {
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);

        AddExpose(hwnd, hwnd.WholeWindow == hwnd.ClientWindow, 0, 0, hwnd.Width, hwnd.Height);
    }

    public override IntPtr DefWndProc(ref Message msg)
    {
        //driver.DefWndProc(ref msg);

        switch ((Msg)msg.Msg)
        {

            case Msg.WM_IME_COMPOSITION:
                string s = "";//Keyboard.GetCompositionString();
                foreach (char c in s)
                    SendMessage(msg.HWnd, Msg.WM_IME_CHAR, (IntPtr)c, msg.LParam);
                return IntPtr.Zero;

            case Msg.WM_IME_CHAR:
                // On Windows API it sends two WM_CHAR messages for each byte, but
                // I wonder if it is worthy to emulate it (also no idea how to 
                // reconstruct those bytes into chars).
                SendMessage(msg.HWnd, Msg.WM_CHAR, msg.WParam, msg.LParam);
                return IntPtr.Zero;

            case Msg.WM_PAINT:
                {
                    Hwnd hwnd;

                    hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);
                    if (hwnd != null)
                    {
                        hwnd.expose_pending = false;
                    }

                    return IntPtr.Zero;
                }

            case Msg.WM_NCPAINT:
                {
                    Hwnd hwnd;

                    hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);
                    if (hwnd != null)
                    {
                        hwnd.nc_expose_pending = false;
                    }

                    return IntPtr.Zero;
                }

            case Msg.WM_NCCALCSIZE:
                {
                    Hwnd hwnd;

                    if (msg.WParam == (IntPtr)1)
                    {
                        hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);

                        if (hwnd == null)
                            return IntPtr.Zero;

                        NCCALCSIZE_PARAMS ncp;
                        ncp = (NCCALCSIZE_PARAMS)Marshal.PtrToStructure(msg.LParam, typeof(NCCALCSIZE_PARAMS));

                        // Add all the stuff X is supposed to draw.
                        Control ctrl = Control.FromHandle(hwnd.Handle);

                        if (ctrl != null)
                        {
                            Hwnd.Borders rect = Hwnd.GetBorders(ctrl.GetCreateParams(), null);

                            ncp.rgrc1.top += rect.top;
                            ncp.rgrc1.bottom -= rect.bottom;
                            ncp.rgrc1.left += rect.left;
                            ncp.rgrc1.right -= rect.right;

                            Marshal.StructureToPtr(ncp, msg.LParam, true);
                        }
                    }

                    return IntPtr.Zero;
                }

            case Msg.WM_CONTEXTMENU:
                {
                    Hwnd hwnd;

                    hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);

                    if ((hwnd != null) && (hwnd.parent != null))
                    {
                        SendMessage(hwnd.parent.client_window, Msg.WM_CONTEXTMENU, msg.WParam, msg.LParam);
                    }
                    return IntPtr.Zero;
                }

            case Msg.WM_MOUSEWHEEL:
                {
                    Hwnd hwnd;

                    hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);

                    if ((hwnd != null) && (hwnd.parent != null))
                    {
                        SendMessage(hwnd.parent.client_window, Msg.WM_MOUSEWHEEL, msg.WParam, msg.LParam);
                        if (msg.Result == IntPtr.Zero)
                        {
                            return IntPtr.Zero;
                        }
                    }
                    return IntPtr.Zero;
                }

            case Msg.WM_SETCURSOR:
                {
                    Hwnd hwnd;

                    hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);
                    if (hwnd == null)
                        break; // not sure how this happens, but it does

                    // Pass to parent window first
                    while ((hwnd.parent != null) && (msg.Result == IntPtr.Zero))
                    {
                        hwnd = hwnd.parent;
                        msg.Result = NativeWindow.WndProc(hwnd.Handle, Msg.WM_SETCURSOR, msg.HWnd, msg.LParam);
                    }

                    if (msg.Result == IntPtr.Zero)
                    {
                        IntPtr handle;

                        switch ((HitTest)(msg.LParam.ToInt32() & 0xffff))
                        {
                            case HitTest.HTBOTTOM: handle = Cursors.SizeNS.handle; break;
                            case HitTest.HTBORDER: handle = Cursors.SizeNS.handle; break;
                            case HitTest.HTBOTTOMLEFT: handle = Cursors.SizeNESW.handle; break;
                            case HitTest.HTBOTTOMRIGHT: handle = Cursors.SizeNWSE.handle; break;
                            case HitTest.HTERROR:
                                if ((msg.LParam.ToInt32() >> 16) == (int)Msg.WM_LBUTTONDOWN)
                                {
                                    AudibleAlert(AlertType.Default);
                                }
                                handle = Cursors.Default.handle;
                                break;

                            case HitTest.HTHELP: handle = Cursors.Help.handle; break;
                            case HitTest.HTLEFT: handle = Cursors.SizeWE.handle; break;
                            case HitTest.HTRIGHT: handle = Cursors.SizeWE.handle; break;
                            case HitTest.HTTOP: handle = Cursors.SizeNS.handle; break;
                            case HitTest.HTTOPLEFT: handle = Cursors.SizeNWSE.handle; break;
                            case HitTest.HTTOPRIGHT: handle = Cursors.SizeNESW.handle; break;

#if SameAsDefault
							case HitTest.HTGROWBOX:
							case HitTest.HTSIZE:
							case HitTest.HTZOOM:
							case HitTest.HTVSCROLL:
							case HitTest.HTSYSMENU:
							case HitTest.HTREDUCE:
							case HitTest.HTNOWHERE:
							case HitTest.HTMAXBUTTON:
							case HitTest.HTMINBUTTON:
							case HitTest.HTMENU:
							case HitTest.HSCROLL:
							case HitTest.HTBOTTOM:
							case HitTest.HTCAPTION:
							case HitTest.HTCLIENT:
							case HitTest.HTCLOSE:
#endif
                            default: handle = Cursors.Default.handle; break;
                        }
                        SetCursor(msg.HWnd, handle);
                    }
                    return (IntPtr)1;
                }
        }

        return IntPtr.Zero;
    }

    public override void HandleException(Exception e)
    {
        throw new NotImplementedException();
    }

    public override void DoEvents()
    {

        DebugHelper.Enter();

        MSG msg = new MSG();
        XEventQueue queue;

        queue = ThreadQueue(Thread.CurrentThread);

        queue.DispatchIdle = false;
        in_doevents = true;

        while (PeekMessage(queue, ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE))
        {
            Message m = Message.Create(msg.hwnd, (int)msg.message, msg.wParam, msg.lParam);

            if (Application.FilterMessage(ref m))
                continue;

            TranslateMessage(ref msg);
            DispatchMessage(ref msg);

            string key = msg.hwnd + ":" + msg.message;
            if (messageHold[key] != null)
            {
                messageHold[key] = ((int)messageHold[key]) - 1;
                DebugHelper.WriteLine("Got " + msg + " for " + key);
            }
        }

        in_doevents = false;
        queue.DispatchIdle = true;

        DebugHelper.Leave();
    }

    public override bool PeekMessage(object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags)
    {
        XEventQueue queue = (XEventQueue)queue_id;
        bool pending;

        if ((flags & (uint)PeekMessageFlags.PM_REMOVE) == 0)
        {
            throw new NotImplementedException("PeekMessage PM_NOREMOVE is not implemented yet");    // FIXME - Implement PM_NOREMOVE flag
        }

        pending = false;
        if (queue.Count > 0)
        {
            pending = true;
        }
        else
        {
            // Only call UpdateMessageQueue if real events are pending 
            // otherwise we go to sleep on the socket
            //if (XPending(DisplayHandle) != 0)
            {
                //       UpdateMessageQueue((XEventQueue)queue_id);
                //     pending = true;
            }
            //else
            if (((XEventQueue)queue_id).Paint.Count > 0)
            {
                pending = true;
            }
        }

        CheckTimers(queue.timer_list, Timer.StopWatchNowMilliseconds);

        if (!pending)
        {
            return false;
        }
        return GetMessage(queue_id, ref msg, hWnd, wFilterMin, wFilterMax);
    }


    void CheckTimers(ArrayList timers, long now)
    {
        int count;

        count = timers.Count;

        if (count == 0)
            return;

        for (int i = 0; i < timers.Count; i++)
        {
            Timer timer;

            timer = (Timer)timers[i];

            if (timer.Enabled && timer.Expires <= now && !timer.Busy)
            {
                // Timer ticks:
                //  - Before MainForm.OnLoad if DoEvents () is called.
                //  - After MainForm.OnLoad if not.
                //
                if (in_doevents ||
                    (Application.MWFThread.Current.Context != null &&
                     (Application.MWFThread.Current.Context.MainForm == null ||
                      Application.MWFThread.Current.Context.MainForm.IsLoaded)))
                {
                    timer.Busy = true;
                    timer.Update(now);
                    timer.FireTick();
                    timer.Busy = false;
                }
            }
        }
    }

    public override void PostQuitMessage(int exitCode)
    {
        ApplicationContext ctx = Application.MWFThread.Current.Context;
        Form f = ctx != null ? ctx.MainForm : null;
        if (f != null)
            PostMessage(Application.MWFThread.Current.Context.MainForm.window.Handle, Msg.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
        else
            PostMessage(IntPtr.Zero, Msg.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
    }

    public override bool GetMessage(object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax)
    {

        XEvent xevent;
        bool client;
        Hwnd hwnd;

    ProcessNextMessage:

        if (((XEventQueue)queue_id).Count > 0)
        {
            xevent = (XEvent)((XEventQueue)queue_id).Dequeue();
        }
        else
        {
            var now = Timer.StopWatchNowMilliseconds;
            //UpdateMessageQueue((XEventQueue)queue_id);
            if ((XEventQueue)queue_id != null)
                CheckTimers(((XEventQueue)queue_id).timer_list, now);

            if (((XEventQueue)queue_id).Count > 0)
            {
                xevent = (XEvent)((XEventQueue)queue_id).Dequeue();
            }
            else if (((XEventQueue)queue_id).Paint.Count > 0)
            {
                xevent = ((XEventQueue)queue_id).Paint.Dequeue();
            }
            else
            {
                msg.hwnd = IntPtr.Zero;
                msg.message = Msg.WM_ENTERIDLE;
                var s = DateTime.Now;
                RaiseIdle(new EventArgs());
                var delta = DateTime.Now - s;
                if (delta.TotalMilliseconds < 30)
                    Thread.Sleep(30);
                return true;
            }
        }

        hwnd = Hwnd.GetObjectFromWindow(xevent.AnyEvent.window);

#if DriverDebugDestroy
			if (hwnd != null)
				if (hwnd.zombie)
					Console.WriteLine ( "GetMessage zombie, got Event: " + xevent.ToString () + " for 0x{0:x}", hwnd.Handle.ToInt32());
				else	
					Console.WriteLine ( "GetMessage, got Event: " + xevent.ToString () + " for 0x{0:x}", hwnd.Handle.ToInt32());
#endif
        // Handle messages for windows that are already or are about to be destroyed.

        // we need a special block for this because unless we remove the hwnd from the paint
        // queue it will always stay there (since we don't handle the expose), and we'll
        // effectively loop infinitely trying to repaint a non-existant window.
        if (hwnd != null && hwnd.zombie && xevent.type == XEventName.Expose)
        {
            hwnd.expose_pending = hwnd.nc_expose_pending = false;
            hwnd.Queue.Paint.Remove(hwnd);
            goto ProcessNextMessage;
        }

        if (xevent.ClientMessageEvent.ptr2 == (IntPtr)Msg.WM_QUIT)
            return false;

        // We need to make sure we only allow DestroyNotify events through for zombie
        // hwnds, since much of the event handling code makes requests using the hwnd's
        // client_window, and that'll result in BadWindow errors if there's some lag
        // between the XDestroyWindow call and the DestroyNotify event.
        if (hwnd == null || hwnd.zombie && xevent.AnyEvent.type != XEventName.ClientMessage)
        {
            DriverDebug("GetMessage(): Got message {0} for non-existent or already destroyed window {1:X}", xevent.type,
                xevent.AnyEvent.window.ToInt32());
            goto ProcessNextMessage;
        }


        // If we get here, that means the window is no more but there are Client Messages
        // to be processed, probably a Posted message (for instance, an WM_ACTIVATE message) 
        // We don't want anything else to run but the ClientMessage block, so reset all hwnd
        // properties that might cause other processing to occur.
        if (hwnd.zombie)
        {
            hwnd.resizing_or_moving = false;
        }

        if (hwnd.client_window == xevent.AnyEvent.window)
        {
            client = true;
            //Console.WriteLine("Client message {1}, sending to window {0:X}", msg.hwnd.ToInt32(), xevent.type);
        }
        else
        {
            client = false;
            //Console.WriteLine("Non-Client message, sending to window {0:X}", msg.hwnd.ToInt32());
        }

        msg.hwnd = hwnd.Handle;

        // Windows sends WM_ENTERSIZEMOVE when a form resize/move operation starts and WM_EXITSIZEMOVE 
        // when it is done. The problem in X11 is that there is no concept of start-end of a moving/sizing.
        // Configure events ("this window has resized/moved") are sent for each step of the resize. We send a
        // WM_ENTERSIZEMOVE when we get the first Configure event. The problem is the WM_EXITSIZEMOVE.
        // 
        //  - There is no way for us to know which is the last Configure event. We can't traverse the events 
        //    queue, because the next configure event might not be pending yet.
        //  - We can't get ButtonPress/Release events for the window decorations, because they are not part 
        //    of the window(s) we manage.
        //  - We can't rely on the mouse state to change to "up" before the last Configure event. It doesn't.
        // 
        // We are almost 100% guaranteed to get another event (e.g Expose or other), but we can't know for sure 
        // which, so we have here to check if the mouse buttons state is "up" and send the WM_EXITSIZEMOVE
        //
        if (hwnd.resizing_or_moving)
        {
            int root_x, root_y, win_x, win_y, keys_buttons;
            IntPtr root, child;
            keys_buttons = 0;
            //XQueryPointer(DisplayHandle, hwnd.Handle, out root, out child, out root_x, out root_y, out win_x, out win_y, out keys_buttons);
            if ((keys_buttons & (int)MouseKeyMasks.Button1Mask) == 0 &&
                (keys_buttons & (int)MouseKeyMasks.Button2Mask) == 0 &&
                (keys_buttons & (int)MouseKeyMasks.Button3Mask) == 0)
            {
                hwnd.resizing_or_moving = false;
                SendMessage(hwnd.Handle, Msg.WM_EXITSIZEMOVE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        switch (xevent.type)
        {
            case XEventName.KeyPress:
                {

                    goto ProcessNextMessage;
                }

            case XEventName.ClientMessage:
                {
                    if (xevent.ClientMessageEvent.message_type == AsyncAtom)
                    {
                        XplatUIDriverSupport.ExecuteClientMessage((GCHandle)xevent.ClientMessageEvent.ptr1);
                        goto ProcessNextMessage;
                    }

                    if (xevent.ClientMessageEvent.message_type == (IntPtr)PostAtom)
                    {
                        DebugHelper.Indent();
                        DebugHelper.WriteLine(String.Format(
                            "Posted message:" + (Msg)xevent.ClientMessageEvent.ptr2.ToInt32() + " for 0x{0:x}",
                            xevent.ClientMessageEvent.ptr1.ToInt32()));
                        DebugHelper.Unindent();
                        msg.hwnd = xevent.ClientMessageEvent.ptr1;
                        msg.message = (Msg)xevent.ClientMessageEvent.ptr2.ToInt32();
                        msg.wParam = xevent.ClientMessageEvent.ptr3;
                        msg.lParam = xevent.ClientMessageEvent.ptr4;
                        if (msg.message == (Msg)Msg.WM_QUIT)
                            return false;
                        else
                            return true;
                    }

                    goto ProcessNextMessage;
                }
            case XEventName.Expose:
                {
                    if (!hwnd.Mapped)
                    {
                        if (client)
                        {
                            hwnd.expose_pending = false;
                        }
                        else
                        {
                            hwnd.nc_expose_pending = false;
                        }

                        goto ProcessNextMessage;
                    }

                    if (client)
                    {
                        if (!hwnd.expose_pending)
                        {
                            goto ProcessNextMessage;
                        }
                    }
                    else
                    {
                        if (!hwnd.nc_expose_pending)
                        {
                            goto ProcessNextMessage;
                        }

                        Monitor.Enter(paintlock);
                        try
                        {
                            hwnd.nc_expose_pending = false;
                            if (hwnd.hwndbmpNC != null)
                                switch (hwnd.border_style)
                                {
                                    case FormBorderStyle.Fixed3D:
                                    {
                                        Graphics g;

                                        g = Graphics.FromSKImage(hwnd.hwndbmpNC);
                                        if (hwnd.border_static)
                                            ControlPaint.DrawBorder3D(g, new Rectangle(0, 0, hwnd.Width, hwnd.Height),
                                                Border3DStyle.SunkenOuter);
                                        else
                                            ControlPaint.DrawBorder3D(g, new Rectangle(0, 0, hwnd.Width, hwnd.Height),
                                                Border3DStyle.Sunken);
                                        g.Dispose();
                                        break;
                                    }

                                    case FormBorderStyle.FixedSingle:
                                    {
                                        Graphics g;

                                        g = Graphics.FromSKImage(hwnd.hwndbmpNC);
                                        ControlPaint.DrawBorder(g, new Rectangle(0, 0, hwnd.Width, hwnd.Height),
                                            Color.Black,
                                            ButtonBorderStyle.Solid);
                                        g.Dispose();
                                        break;
                                    }
                                }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        finally
                        {
                            Monitor.Exit(paintlock);
                        }

                        DriverDebug("GetMessage(): Window {0:X} Exposed non-client area {1},{2} {3}x{4}",
                            hwnd.client_window.ToInt32(), xevent.ExposeEvent.x, xevent.ExposeEvent.y,
                            xevent.ExposeEvent.width, xevent.ExposeEvent.height);

                        Rectangle rect = new Rectangle(xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width,
                            xevent.ExposeEvent.height);
                        Region region = new Region(rect);
                        //IntPtr hrgn = region.GetHrgn(null); // Graphics object isn't needed
                        msg.message = Msg.WM_NCPAINT;
                        msg.wParam = (IntPtr)1; // hrgn == IntPtr.Zero ? (IntPtr) 1 : hrgn;
                        msg.refobject = region;
                        break;
                    }

                    DriverDebug("GetMessage(): Window {0:X} Exposed area {1},{2} {3}x{4}",
                        hwnd.client_window.ToInt32(), xevent.ExposeEvent.x, xevent.ExposeEvent.y,
                        xevent.ExposeEvent.width, xevent.ExposeEvent.height);
                    if (Caret.Visible == true)
                    {
                        Caret.Paused = true;
                        HideCaret();
                    }

                    if (Caret.Visible == true)
                    {
                        ShowCaret();
                        Caret.Paused = false;
                    }
                    msg.message = Msg.WM_PAINT;
                    break;
                }

            default:
                {
                    goto ProcessNextMessage;
                }
        }

        return true;
    }

    [Conditional("DriverDebug")]
    static void DriverDebug(string format, params object[] args)
    {
        Console.WriteLine(String.Format(format, args));
    }

    public override bool TranslateMessage(ref MSG msg)
    {
        return false;
    }

    public override IntPtr DispatchMessage(ref MSG msg)
    {
        return NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);
    }

    private Dictionary<IntPtr, ( IntPtr afterhwnd, bool top, bool bottom)> zorder =
        new Dictionary<IntPtr, ( IntPtr afterhwnd, bool top, bool bottom)>();
    public override bool SetZOrder(IntPtr hWnd, IntPtr AfterhWnd, bool Top, bool Bottom)
    {
        var item = (AfterhWnd, Top, Bottom);
        zorder[hWnd] = item;
        return true;
    }

    /// <summary>
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    public (IntPtr afterhwnd, bool top, bool bottom) GetZOrder(IntPtr handle)
    {
        if(zorder.ContainsKey(handle))
            return zorder[handle];
        return (IntPtr.Zero, false, false);
    }


    public override bool SetTopmost(IntPtr handle, bool enabled)
    {
        Hwnd hwnd = Hwnd.ObjectFromHandle(handle);
        hwnd.topmost = enabled;

        if (enabled)
        {
            //lock (XlibLock)
            {
                if (hwnd.Mapped)
                {
                    //SendNetWMMessage(hwnd.WholeWindow, _NET_WM_STATE, (IntPtr)NetWmStateRequest._NET_WM_STATE_ADD, _NET_WM_STATE_ABOVE, IntPtr.Zero);
                }
                else
                {
                    int[] atoms = new int[8];
                    //atoms[0] = _NET_WM_STATE_ABOVE.ToInt32();
                    //XChangeProperty(DisplayHandle, hwnd.whole_window, _NET_WM_STATE, (IntPtr)Atom.XA_ATOM, 32, PropertyMode.Replace, atoms, 1);
                }
            }
        }
        else
        {
            //lock (XlibLock)
            {
                if (hwnd.Mapped)
                {
                    //SendNetWMMessage(hwnd.WholeWindow, _NET_WM_STATE, (IntPtr) NetWmStateRequest._NET_WM_STATE_REMOVE, _NET_WM_STATE_ABOVE, IntPtr.Zero);
                }
                else
                {
                    //XDeleteProperty(DisplayHandle, hwnd.whole_window, _NET_WM_STATE);
                }
            }
        }
        return true;
    }

    public override bool SetOwner(IntPtr hWnd, IntPtr hWndOwner)
    {
        var hw = Hwnd.ObjectFromHandle(hWnd);
        //hw.Parent = Hwnd.ObjectFromHandle(hWndOwner);
        //throw new NotImplementedException();
        return true;
    }

    public override bool CalculateWindowRect(ref Rectangle ClientRect, CreateParams cp, Menu menu, out Rectangle WindowRect)
    {
        WindowRect = Hwnd.GetWindowRectangle(cp, menu, ClientRect);
        return true;
    }

    public override Region GetClipRegion(IntPtr hwnd)
    {
        throw new NotImplementedException();
    }

    public override void SetClipRegion(IntPtr hwnd, Region region)
    {
        throw new NotImplementedException();
    }

    public override void SetCursor(IntPtr hwnd, IntPtr cursor)
    {
        //throw new NotImplementedException();
    }

    public override void ShowCursor(bool show)
    {
        throw new NotImplementedException();
    }

    public override void OverrideCursor(IntPtr cursor)
    {
        //throw new NotImplementedException();
    }

    public override IntPtr DefineCursor(Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot)
    {
        //throw new NotImplementedException();
        return IntPtr.Zero;
    }

    public override IntPtr DefineStdCursor(StdCursor id)
    {
        //throw new NotImplementedException();
        return IntPtr.Zero;
    }

    public override Bitmap DefineStdCursorBitmap(StdCursor id)
    {
        throw new NotImplementedException();
    }

    public override void DestroyCursor(IntPtr cursor)
    {
        throw new NotImplementedException();
    }

    public override void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y)
    {
        width = 20;
        height = 20;
        hotspot_x = 0;
        hotspot_y = 0;
    }

    public override void GetCursorPos(IntPtr handle, out int x, out int y)
    {
        IntPtr use_handle;
        IntPtr root;
        IntPtr child;
        int root_x;
        int root_y;
        int win_x;
        int win_y;
        int keys_buttons;

        if (handle != IntPtr.Zero)
        {
            use_handle = Hwnd.ObjectFromHandle(handle).client_window;
        }
        else
        {
            //use_handle = RootWindow;
        }

        //lock (XlibLock)
        {
            //driver.GetCursorPos(handle, out win_x, out win_y);
            //QueryPointer(DisplayHandle, use_handle, out root, out child, out root_x, out root_y, out win_x, out win_y, out keys_buttons);
        }

        x = mouse_position.X;
        y = mouse_position.Y;

        if (handle != IntPtr.Zero)
        {
            //x = win_x;
            //y = win_y;
        }
        else
        {
           // x = win_x;
           // y = win_y;
        }
    }

    public override void SetCursorPos(IntPtr hwnd, int x, int y)
    {
        throw new NotImplementedException();
    }
    (int X, int Y) GetParent2(Hwnd hw)
    {
        if (hw.Parent == null)
        {
            var frm = Control.FromHandle(hw.ClientWindow) as Form;

            if (frm != null)
            {
                var borders = Hwnd.GetBorders(frm.GetCreateParams(), null);

                if (frm.WindowState == FormWindowState.Maximized)
                    return (hw.X + borders.left, hw.Y + borders.bottom);

                return (hw.X + borders.left, hw.Y + borders.top);
            }

            return (hw.X, hw.Y);
        }
        var parent = GetParent2(hw.Parent);
        return (hw.X  + parent.X, hw.Y + parent.Y);

    }

public override void ScreenToClient(IntPtr handle, ref int x, ref int y)
    {
        Hwnd hwnd = Hwnd.ObjectFromHandle(handle);

        var ctl = Control.FromHandle(hwnd.Handle);

        var (x1, y1) = GetParent2(hwnd);
            x -= x1;
            y -= y1;
    }
    /// <summary>
    /// The ClientToScreen function converts the client-area coordinates of a specified point to screen coordinates.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public override void ClientToScreen(IntPtr handle, ref int x, ref int y)
    {
        Hwnd hwnd = Hwnd.ObjectFromHandle(handle);

        var ctl = Control.FromHandle(hwnd.Handle);

        var (x1, y1) = GetParent2(hwnd);

        x += x1;
        y += y1;
    }

    static GrabStruct Grab;
    public override void GrabWindow(IntPtr handle, IntPtr ConfineToHwnd)
    {
        Hwnd hwnd;
        IntPtr confine_to_window;

        hwnd = Hwnd.ObjectFromHandle(handle);

        if (!hwnd.Mapped || !hwnd.Visible)
            return;


        confine_to_window = IntPtr.Zero;

        if (ConfineToHwnd != IntPtr.Zero)
        {
            XWindowAttributes attributes = new XWindowAttributes();

            hwnd = Hwnd.ObjectFromHandle(ConfineToHwnd);

       
            Grab.Area.X = attributes.x;
            Grab.Area.Y = attributes.y;
            Grab.Area.Width = attributes.width;
            Grab.Area.Height = attributes.height;
            Grab.Confined = true;
            confine_to_window = hwnd.client_window;
        }

        Grab.Hwnd = handle;

    }

    public override void GrabInfo(out IntPtr handle, out bool GrabConfined, out Rectangle GrabArea)
    {
        handle = Grab.Hwnd;
        GrabConfined = Grab.Confined;
        GrabArea = Grab.Area;
    }

    public override void UngrabWindow(IntPtr hwnd)
    {
        WindowUngrabbed(hwnd);
    }

    void WindowUngrabbed(IntPtr hwnd)
    {
        bool was_grabbed = Grab.Hwnd != IntPtr.Zero;

        Grab.Hwnd = IntPtr.Zero;
        Grab.Confined = false;

        if (was_grabbed)
        {
            // lparam should be the handle to the window gaining the mouse capture,
            // but X doesn't seem to give us that information.
            // Also only generate WM_CAPTURECHANGED if the window actually was grabbed.
            // X will send a NotifyUngrab, but since it comes late sometimes we're
            // calling WindowUngrabbed directly from UngrabWindow in order to send
            // this WM right away.
            SendMessage(hwnd, Msg.WM_CAPTURECHANGED, IntPtr.Zero, IntPtr.Zero);
        }
    }

    public override void SendAsyncMethod(AsyncMethodData method)
    {
        Hwnd hwnd;
        XEvent xevent = new XEvent();

        hwnd = Hwnd.ObjectFromHandle(method.Handle);

        xevent.type = XEventName.ClientMessage;
        //xevent.ClientMessageEvent.display = DisplayHandle;
        xevent.ClientMessageEvent.window = method.Handle;
        xevent.ClientMessageEvent.message_type = (IntPtr)AsyncAtom;
        xevent.ClientMessageEvent.format = 32;
        xevent.ClientMessageEvent.ptr1 = (IntPtr)GCHandle.Alloc(method);

        if (hwnd == null)
            return;

        hwnd.Queue.EnqueueLocked(xevent);

        //lock (queuelock)
        {
            //MessageQueue.Enqueue(GCHandle.Alloc(method));
        }

        //driver.SendAsyncMethod(method);
    }

    public override void SetTimer(Timer timer)
    {
        XEventQueue queue = (XEventQueue) MessageQueues [timer.thread];

			if (queue == null) {
				// This isn't really an error, MS doesn't start the timer if
				// it has no assosciated queue at this stage (it will be
				// enabled when a window is activated).
				unattached_timer_list.Add (timer);
				return;
			}
			queue.timer_list.Add (timer);
    }

    public override void KillTimer(Timer timer)
    {
        XEventQueue queue = (XEventQueue)MessageQueues[timer.thread];

        if (queue == null)
        {
            // This isn't really an error, MS doesn't start the timer if
            // it has no assosciated queue. In this case, remove the timer
            // from the list of unattached timers (if it was enabled).
            lock (unattached_timer_list)
            {
                if (unattached_timer_list.Contains(timer))
                    unattached_timer_list.Remove(timer);
            }
            return;
        }
        queue.timer_list.Remove(timer);
    }
    static CaretStruct	Caret;	
    public override void CreateCaret(IntPtr handle, int width, int height)
    {
        XGCValues	gc_values;
        Hwnd		hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);

        if (Caret.Hwnd != IntPtr.Zero) {
            DestroyCaret(Caret.Hwnd);
        }

        Caret.Hwnd = handle;
        Caret.Window = hwnd.client_window;
        Caret.Width = width;
        Caret.Height = height;
        Caret.Visible = false;
        Caret.On = false;
    }

    public override void DestroyCaret(IntPtr handle)
    {
        if (Caret.Hwnd == handle) {
            if (Caret.Visible) {
                HideCaret ();
                Caret.Timer.Stop();
            }
            if (Caret.gc != IntPtr.Zero) {
                //XFreeGC(DisplayHandle, Caret.gc);
                Caret.gc = IntPtr.Zero;
            }
            Caret.Hwnd = IntPtr.Zero;
            Caret.Visible = false;
            Caret.On = false;
        }
    }

    public override void SetCaretPos(IntPtr handle, int x, int y)
    {
        if (Caret.Hwnd == handle) {
            Caret.Timer.Stop();
            HideCaret();

            Caret.X = x;
            Caret.Y = y;

            Keyboard.SetCaretPos (Caret, handle, x, y);

            if (Caret.Visible == true) {
                ShowCaret();
                Caret.Timer.Start();
            }
        }
    }

    public override void CaretVisible(IntPtr handle, bool visible)
    {
        if (Caret.Hwnd == handle) {
            if (visible) {
                if (!Caret.Visible) {
                    Caret.Visible = true;
                    ShowCaret();
                    Caret.Timer.Start();
                }
            } else {
                Caret.Visible = false;
                Caret.Timer.Stop();
                HideCaret();
            }
        }
    }
    void ShowCaret() {
        if ((Caret.gc == IntPtr.Zero) || Caret.On) {
            return;
        }
        Caret.On = true;

       // lock (XlibLock) {
            //XDrawLine(DisplayHandle, Caret.Window, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
       // }
    }

    void HideCaret() {
        if ((Caret.gc == IntPtr.Zero) || !Caret.On) {
            return;
        }
        Caret.On = false;

       // lock (XlibLock) {
           // XDrawLine(DisplayHandle, Caret.Window, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
       // }
    }
    public override IntPtr GetFocus()
    {
        return FocusWindow;
    }
    static IntPtr FocusWindow = IntPtr.Zero;
    private bool in_doevents;
    private Screen[] _allScreens;
    private bool _themesEnabled;

    public override void SetFocus(IntPtr handle)
    {
        Hwnd hwnd;
        IntPtr prev_focus_window;

        hwnd = Hwnd.ObjectFromHandle(handle);

        if (hwnd.client_window == FocusWindow)
        {
            return;
        }

        // Win32 doesn't do anything if disabled
        if (!hwnd.enabled)
            return;

        prev_focus_window = FocusWindow;
        FocusWindow = hwnd.client_window;

        if (prev_focus_window != IntPtr.Zero)
        {
            Keyboard.FocusOut(FocusWindow);
            SendMessage(prev_focus_window, Msg.WM_KILLFOCUS, FocusWindow, IntPtr.Zero);
        }
        Keyboard.FocusIn(FocusWindow);

        if (FocusWindow == IntPtr.Zero)
        {
            Control c = Control.FromHandle(hwnd.client_window);

            if (c == null)
                return;
            Form form = c.FindForm();
            if (form == null)
                return;

            if (ActiveWindow != form.Handle)
            {
                ActiveWindow = form.Handle;
                SendMessage(ActiveWindow, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
            }
        }

        SendMessage(FocusWindow, Msg.WM_SETFOCUS, prev_focus_window, IntPtr.Zero);


    }

    public override IntPtr GetActive()
    {
        var cnt = Application.OpenForms.Count;
        while (cnt > 0)
        {
            if (Application.OpenForms[cnt - 1].IsDisposed)
            {
                cnt--;
                continue;
            }
            return Application.OpenForms[cnt - 1].Handle;
        }
        return IntPtr.Zero;
        
    }

    public override IntPtr GetPreviousWindow(IntPtr hwnd)
    {
        throw new NotImplementedException();
    }

    public override void ScrollWindow(IntPtr handle, Rectangle area, int XAmount, int YAmount, bool with_children)
    {

        Hwnd hwnd;
        IntPtr gc;
        XGCValues gc_values;

        hwnd = Hwnd.ObjectFromHandle(handle);

        Rectangle r = Rectangle.Intersect(hwnd.Invalid, area);
        if (!r.IsEmpty)
        {
            /* We have an invalid area in the window we're scrolling. 
               Adjust our stored invalid rectangle to to match the scrolled amount */

            r.X += XAmount;
            r.Y += YAmount;

            if (r.X < 0)
            {
                r.Width += r.X;
                r.X = 0;
            }

            if (r.Y < 0)
            {
                r.Height += r.Y;
                r.Y = 0;
            }

            if (area.Contains(hwnd.Invalid))
                hwnd.ClearInvalidArea();
            hwnd.AddInvalidArea(r);
        }

        gc_values = new XGCValues();

        if (with_children)
        {
            gc_values.subwindow_mode = GCSubwindowMode.IncludeInferiors;
        }

        //gc = XCreateGC(DisplayHandle, hwnd.client_window, IntPtr.Zero, ref gc_values);

        Rectangle visible_rect = GetTotalVisibleArea(hwnd.client_window);
        visible_rect.Intersect(area);

        Rectangle dest_rect = visible_rect;
        dest_rect.Y += YAmount;
        dest_rect.X += XAmount;
        dest_rect.Intersect(area);

        Point src = new Point(dest_rect.X - XAmount, dest_rect.Y - YAmount);
       // XCopyArea(DisplayHandle, hwnd.client_window, hwnd.client_window, gc, src.X, src.Y,dest_rect.Width, dest_rect.Height, dest_rect.X, dest_rect.Y);


        Rectangle dirty_area = GetDirtyArea(area, dest_rect, XAmount, YAmount);
        AddExpose(hwnd, true, dirty_area.X, dirty_area.Y, dirty_area.Width, dirty_area.Height);

        var ctl = Control.FromHandle(hwnd.ClientWindow);
        ctl.Invalidate();

        //ProcessGraphicsExpose(hwnd);

        //XFreeGC(DisplayHandle, gc);
    }
    bool GraphicsExposePredicate(IntPtr display, ref XEvent xevent, IntPtr arg)
    {
        return (xevent.type == XEventName.GraphicsExpose || xevent.type == XEventName.NoExpose) &&
               arg == xevent.GraphicsExposeEvent.drawable;
    }

    delegate bool EventPredicate(IntPtr display, ref XEvent xevent, IntPtr arg);

    void ProcessGraphicsExpose(Hwnd hwnd)
    {
        XEvent xevent = new XEvent();
        IntPtr handle = Hwnd.HandleFromObject(hwnd);
        EventPredicate predicate = GraphicsExposePredicate;

        DoEvents();

        return;

        for (; ; )
        {
            //XIfEvent(Display, ref xevent, predicate, handle);
            //if (xevent.type != XEventName.GraphicsExpose)
                //break;

            AddExpose(hwnd, xevent.ExposeEvent.window == hwnd.ClientWindow, xevent.GraphicsExposeEvent.x, xevent.GraphicsExposeEvent.y,
                xevent.GraphicsExposeEvent.width, xevent.GraphicsExposeEvent.height);

            if (xevent.GraphicsExposeEvent.count == 0)
                break;
        }
    }
    Rectangle GetDirtyArea(Rectangle total_area, Rectangle valid_area, int XAmount, int YAmount)
    {
        Rectangle dirty_area = total_area;

        if (YAmount > 0)
            dirty_area.Height -= valid_area.Height;
        else if (YAmount < 0)
        {
            dirty_area.Height -= valid_area.Height;
            dirty_area.Y += valid_area.Height;
        }

        if (XAmount > 0)
            dirty_area.Width -= valid_area.Width;
        else if (XAmount < 0)
        {
            dirty_area.Width -= valid_area.Width;
            dirty_area.X += valid_area.Width;
        }

        return dirty_area;
    }
    
    Rectangle GetTotalVisibleArea(IntPtr handle)
    {
        Control c = Control.FromHandle(handle);

        Rectangle visible_area = c.ClientRectangle;
        visible_area.Location = c.PointToScreen(Point.Empty);

        for (Control parent = c.Parent; parent != null; parent = parent.Parent)
        {
            if (!parent.IsHandleCreated || !parent.Visible)
                return visible_area; // Non visible, not need to finish computations

            Rectangle r = parent.ClientRectangle;
            r.Location = parent.PointToScreen(Point.Empty);

            visible_area.Intersect(r);
        }

        visible_area.Location = c.PointToClient(visible_area.Location);
        return visible_area;
    }

    public override void ScrollWindow(IntPtr handle, int XAmount, int YAmount, bool with_children)
    {
        Hwnd hwnd;
        Rectangle rect;

        hwnd = Hwnd.GetObjectFromWindow(handle);

        rect = hwnd.ClientRect;
        rect.X = 0;
        rect.Y = 0;
        ScrollWindow(handle, rect, XAmount, YAmount, with_children);
    }

    public override bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent)
    {
        FontFamily ff = font.FontFamily;
        ascent = ff.GetCellAscent(font.Style);
        descent = ff.GetCellDescent(font.Style);
        return true;
    }

    public override bool SystrayAdd(IntPtr hwnd, string tip, Icon icon, out ToolTip tt)
    {
        throw new NotImplementedException();
    }

    public override bool SystrayChange(IntPtr handle, string tip, Icon icon, ref ToolTip tt)
    {
        Control control;

        control = Control.FromHandle(handle);
        if (control != null && tt != null)
        {
            tt.SetToolTip(control, tip);
            tt.Active = true;
            SendMessage(handle, Msg.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void SystrayRemove(IntPtr handle, ref ToolTip tt)
    {
        SetVisible(handle, false, false);

        // The caller can now re-dock it later...
        if (tt != null)
        {
            tt.Dispose();
            tt = null;
        }
        // Close any balloon window *we* fired.
        ThemeEngine.Current.HideBalloonWindow(handle);
    }

    public override void SystrayBalloon(IntPtr handle, int timeout, string title, string text, ToolTipIcon icon)
    {
        ThemeEngine.Current.ShowBalloonWindow(handle, timeout, title, text, icon);
        SendMessage(handle, Msg.WM_USER, IntPtr.Zero, (IntPtr)Msg.NIN_BALLOONSHOW);
    }

    public override Point GetMenuOrigin(IntPtr handle)
    {
        Hwnd hwnd;

        hwnd = Hwnd.ObjectFromHandle(handle);

        if (hwnd != null)
        {
            return hwnd.MenuOrigin;
        }
        return Point.Empty;
    }

    public override void MenuToScreen(IntPtr hwnd, ref int x, ref int y)
    {
        throw new NotImplementedException();
    }

    public override void ScreenToMenu(IntPtr hwnd, ref int x, ref int y)
    {
        throw new NotImplementedException();
    }

    public override void SetIcon(IntPtr handle, Icon icon)
    {
        //throw new NotImplementedException();
    }

    public override void ClipboardClose(IntPtr handle)
    {
        
    }

    public override IntPtr ClipboardOpen(bool primary_selection)
    {
        return IntPtr.Zero;
    }

    public override int ClipboardGetID(IntPtr handle, string format)
    {
        return 0;
    }

    public override void ClipboardStore(IntPtr handle, object obj, int id, XplatUI.ObjectToClipboard converter, bool copy)
    {
        throw new NotImplementedException();
    }

    public override int[] ClipboardAvailableFormats(IntPtr handle)
    {
        throw new NotImplementedException();
    }

    public override object ClipboardRetrieve(IntPtr handle, int id, XplatUI.ClipboardToObject converter)
    {
        throw new NotImplementedException();
    }

    public override void DrawReversibleLine(Point start, Point end, Color backColor)
    {
        throw new NotImplementedException();
    }

    public override void DrawReversibleRectangle(IntPtr handle, Rectangle rect, int line_width)
    {
        //throw new NotImplementedException();
    }

    public override void FillReversibleRectangle(Rectangle rectangle, Color backColor)
    {
        throw new NotImplementedException();
    }

    public override void DrawReversibleFrame(Rectangle rectangle, Color backColor, FrameStyle style)
    {
        //throw new NotImplementedException();
    }

    public override SizeF GetAutoScaleSize(Font font)
    {
        Graphics g;
        float width;
        string magic_string = "The quick brown fox jumped over the lazy dog.";
        double magic_number = 44.549996948242189;

        g = Graphics.FromHwnd(GetFosterParent());

        width = (float)(g.MeasureString(magic_string, font).Width / magic_number);
        return new SizeF(width, font.Height);
    }

    public static Control FindControlAtPoint(Control container, Point pos)
    {
        Control child;
        // inclide implicit controls
        foreach (Control c in container.Controls.GetAllControls())
        {
            if (c.Visible && c.Bounds.Contains(pos))
            {
                child = FindControlAtPoint(c, new Point(pos.X - c.Left, pos.Y - c.Top));
                if (child == null) {

                    return c;
                }
                else return child;
            }
        }
        return null;
    }

    public static Control FindControlAtCursor(Form form)
    {
        Point pos = Cursor.Position;
        if (form.Bounds.Contains(pos))
            return FindControlAtPoint(form, form.PointToClient(pos));
        return null;
    }

    delegate IntPtr WndProcDelegate(IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam);
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tagWINDOWPOS
    {
       public IntPtr hwndInsertAfter;
       public IntPtr hwnd;
       public int x;
       public int y;
       public int cx;
       public int cy;
       public uint flags;
    }
    public override IntPtr SendMessage(IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam)
    {
        Hwnd h;
        h = Hwnd.ObjectFromHandle(hwnd);

        if (hwnd == IntPtr.Zero)
        {
            // from the real event system

            if (message == Msg.WM_MOUSEMOVE || message == Msg.WM_LBUTTONDOWN || 
                message == Msg.WM_MOUSEACTIVATE || message == Msg.WM_MOUSEFIRST || 
                message == Msg.WM_MOUSELEAVE || message == Msg.WM_LBUTTONUP || 
                message == Msg.WM_RBUTTONDOWN || message == Msg.WM_RBUTTONUP ||
                message == Msg.WM_MBUTTONDOWN || message == Msg.WM_MBUTTONUP ||
                message == Msg.WM_LBUTTONDBLCLK || message == Msg.WM_RBUTTONDBLCLK ||
                message == Msg.WM_MBUTTONDBLCLK)
            {
                if (Grab.Hwnd != IntPtr.Zero)
                {
                    hwnd = Grab.Hwnd;
                    h = Hwnd.ObjectFromHandle(hwnd); 

                    var x = lParam.ToInt32() & 0xffff;
                    var y = lParam.ToInt32() >> 16;

                    mouse_position.X = x;
                    mouse_position.Y = y;                    

                    ScreenToClient(hwnd, ref x, ref y);

                    lParam = (IntPtr)((y) << 16 | (x));

                    if (!h.Mapped || !h.Visible)
                        UngrabWindow(hwnd);
                }
                else
                {
                    //(HoverState.Y << 16 | HoverState.X)
                    var x = lParam.ToInt32() & 0xffff;
                    var y = lParam.ToInt32() >> 16;

                    mouse_position.X = x;
                    mouse_position.Y = y;

                    Control ctl = null;

                    if(h == null)
                    {
                        lock(Hwnd.windows)
                        foreach (Hwnd hw in Hwnd.windows.Values)
                        {
                            if (hw.topmost == true && hw.Mapped && hw.Visible)
                            {
                                var ctlmenu = Control.FromHandle(hw.ClientWindow);
                                if (ctlmenu != null && ctlmenu.Bounds.Contains(x, y) && ctlmenu is ToolStripDropDownMenu )
                                {
                                    ctl = ctlmenu;
                                    break;
                                }
                            }
                        }
                    }

                    if(Application.OpenForms.Count == 0)
                        return IntPtr.Zero;

                    if (ctl == null)
                    {
                        // handle mlutple subwindows
                        var cnt = Application.OpenForms.Count;
                        while (cnt > 0 && ctl == null)
                        {
                            x = lParam.ToInt32() & 0xffff;
                            y = lParam.ToInt32() >> 16;

                            // check if we are in this From via the hwnd/nc
                            if(Application.OpenForms[cnt - 1].IsDisposed)
                            {
                                cnt--;
                                continue;
                            }
                            var hwnd2 = Hwnd.ObjectFromHandle(Application.OpenForms[cnt - 1].Handle);
                            if (x < hwnd2.X || y < hwnd2.Y || x > hwnd2.x + hwnd2.width || y > hwnd2.y + hwnd2.height)
                            {
                                cnt--;
                                continue;
                            }

                            XplatUI.driver.ScreenToClient(Application.OpenForms[cnt - 1].Handle,
                                ref x, ref y);

                            ctl = XplatUIMine.FindControlAtPoint(Application.OpenForms[cnt - 1],
                                new Point(x, y));
                            if (ctl == null)
                            {
                                // if we are in here we are over a form/nc but no control on that form

                                ctl = Application.OpenForms[cnt - 1];

                                if (message == Msg.WM_MOUSEMOVE)
                                    message = Msg.WM_NCMOUSEMOVE;

                                if (message == Msg.WM_LBUTTONDOWN)
                                    message = Msg.WM_NCLBUTTONDOWN;

                                if (message == Msg.WM_LBUTTONUP)
                                    message = Msg.WM_NCLBUTTONUP;

                                if (message == Msg.WM_RBUTTONDOWN)
                                    message = Msg.WM_NCRBUTTONDOWN;

                                if (message == Msg.WM_RBUTTONUP)
                                    message = Msg.WM_NCRBUTTONUP;

                                hwnd = ctl.Handle;
                                if (hwnd != IntPtr.Zero)
                                    PostMessage(hwnd, message, wParam, lParam);
                                return IntPtr.Zero;
                            }
                        }
                    }

                    if (ctl != null)
                    {
                        hwnd = ctl.Handle;
                        h = Hwnd.ObjectFromHandle(hwnd);

                        x = lParam.ToInt32() & 0xffff;
                        y = lParam.ToInt32() >> 16;

                        ScreenToClient(hwnd, ref x, ref y);

                        lParam = (IntPtr) ((y ) << 16 | (x));

                        if(Control.FromHandle(h.Handle) is Form)
                            PostMessage(hwnd, Msg.WM_NCPAINT, IntPtr.Zero, IntPtr.Zero);
                    }
                    else
                    {
                        PostMessage(Application.OpenForms[Application.OpenForms.Count - 1].Handle, Msg.WM_NCHITTEST, IntPtr.Zero, IntPtr.Zero);

                        if(message == Msg.WM_LBUTTONUP)
                        {
                            PostMessage(Application.OpenForms[Application.OpenForms.Count - 1].Handle, Msg.WM_NCLBUTTONUP, wParam, lParam);
                        }
                        if (message == Msg.WM_LBUTTONDOWN)
                        {
                            PostMessage(Application.OpenForms[Application.OpenForms.Count - 1].Handle, Msg.WM_NCLBUTTONDOWN, wParam, lParam);
                        }

                        PostMessage(Application.OpenForms[Application.OpenForms.Count - 1].Handle, Msg.WM_NCPAINT, IntPtr.Zero, IntPtr.Zero);
                    }
                }

                if (message == Msg.WM_LBUTTONDOWN)
                {
                    mouse_state = Control.FromParamToMouseButtons((int)wParam.ToInt32());
                }
                if (message == Msg.WM_LBUTTONUP)
                {
                    mouse_state = Control.FromParamToMouseButtons((int)wParam.ToInt32());
                }

                if (message == Msg.WM_RBUTTONDOWN )
                {
                    mouse_state = Control.FromParamToMouseButtons((int)wParam.ToInt32());
                }     
                if ( message == Msg.WM_RBUTTONUP)
                {
                    mouse_state = Control.FromParamToMouseButtons((int)wParam.ToInt32());
                }

                if (message == Msg.WM_MBUTTONDOWN )
                {
                    mouse_state = Control.FromParamToMouseButtons((int)wParam.ToInt32());
                }  
                if (message == Msg.WM_MBUTTONUP)
                {
                    mouse_state = Control.FromParamToMouseButtons((int)wParam.ToInt32());
                }


                if (hwnd != prev_mouse_hwnd && message == Msg.WM_MOUSEMOVE)
                {
                    TRACKMOUSEEVENT tme;

                    mouse_state = Control.FromParamToMouseButtons((int)wParam.ToInt32());

                    // The current message will be sent out next time around
                    //StoreMessage(ref msg);

                    if (prev_mouse_hwnd != IntPtr.Zero)
                    {
                        PostMessage(prev_mouse_hwnd, Msg.WM_MOUSELEAVE, wParam, lParam);                        
                    }

                    // This is the message we want to send at this point

                    prev_mouse_hwnd = hwnd;

                    tme = new TRACKMOUSEEVENT();
                    tme.size = Marshal.SizeOf(tme);
                    tme.hWnd = hwnd;
                    tme.dwFlags = TMEFlags.TME_LEAVE | TMEFlags.TME_HOVER;

                    PostMessage(hwnd, Msg.WM_MOUSE_ENTER, wParam, lParam);
                }

                if (hwnd != IntPtr.Zero)
                    PostMessage(hwnd, message, wParam, lParam);
                return IntPtr.Zero;
                //lParam = (IntPtr)(mouse_position.Y << 16 | mouse_position.X);
            }
            else if (message == Msg.WM_WINDOWPOSCHANGING || message == Msg.WM_WINDOWPOSCHANGED)
            {
                if (Application.OpenForms.Count > 0)
                {
                    var h2 = Hwnd.ObjectFromHandle(Application.OpenForms[0].Handle);
                    var pos = new tagWINDOWPOS();
                    pos = (tagWINDOWPOS)Marshal.PtrToStructure(lParam, typeof(tagWINDOWPOS));
                    //SWP_NOSIZE 0x0001
                    //SWP_NOMOVE 0x0002
                    //SWP_NOZORDER 0x0004
                    //SWP_NOREDRAW 0x0008
                    //
                    DriverDebug("SendMessage => WM_WINDOWPOSCHANGED  maybe - flags: " + pos.flags);

                    if ((pos.flags & 0x1) == 0)
                    {
                        DriverDebug("SendMessage => WM_WINDOWPOSCHANGED");
                        h = h2;
                        h.x = 0;
                        h.y = 0;
                        SetWindowPos(h.ClientWindow, h.x-4, h.y-4, pos.cx - h.x, pos.cy-h.y-23);
                        return IntPtr.Zero;

                        h2.Width = pos.cx;
                        h2.Height = pos.cy;
                        h2.X = 0;
                        h2.y = 0;
                        hwnd = h2.Handle;
                       

                        PerformNCCalc(h);
                    }

                    //if (hwnd != IntPtr.Zero)
                        //PostMessage(hwnd, message, wParam, lParam);
                }
            
                return IntPtr.Zero;
            } else if (message == Msg.WM_MOUSEWHEEL)
            {

                var ctl = FindControlAtPoint(Application.OpenForms[0], new Point(mouse_position.X, mouse_position.Y));
                
                hwnd = ctl.Handle;
                h = Hwnd.ObjectFromHandle(ctl.Handle);
                if (hwnd != IntPtr.Zero)
                    PostMessage(hwnd, message, wParam, lParam);
                return IntPtr.Zero;
            }
            else if (message == Msg.WM_CONTEXTMENU)
            {
                return IntPtr.Zero;
                var ctl = FindControlAtPoint(Application.OpenForms[0], new Point(mouse_position.X, mouse_position.Y));

                hwnd = ctl.Handle;
                h = Hwnd.ObjectFromHandle(ctl.Handle);
            }
            else if(message == Msg.WM_PAINT)
            {
                return IntPtr.Zero;
            }
            else
            {
                return IntPtr.Zero;
                if (message == Msg.WM_NCHITTEST)
                {
                    
                }

            }
        }

        if (h != null && h.queue != ThreadQueue(Thread.CurrentThread))
        {
            AsyncMethodResult result;
            AsyncMethodData data;

            result = new AsyncMethodResult();
            data = new AsyncMethodData();

            data.Handle = hwnd;
            data.Method = new WndProcDelegate(NativeWindow.WndProc);
            data.Args = new object[] { hwnd, message, wParam, lParam };
            data.Result = result;

            SendAsyncMethod(data);
            DriverDebug("Sending {0} message across.", message);

            return IntPtr.Zero;
        }
        string key = hwnd + ":" + message;
        if (messageHold[key] != null)
            messageHold[key] = ((int)messageHold[key]) - 1;
        return NativeWindow.WndProc(hwnd, message, wParam, lParam);
    }

    public override bool PostMessage(IntPtr handle, Msg message, IntPtr wParam, IntPtr lParam)
    {
        XEvent xevent = new XEvent();
        Hwnd hwnd = Hwnd.ObjectFromHandle(handle);

        xevent.type = XEventName.ClientMessage;
        //xevent.ClientMessageEvent.display = DisplayHandle;

        if (hwnd != null)
        {
            xevent.ClientMessageEvent.window = hwnd.whole_window;
        }
        else
        {
            xevent.ClientMessageEvent.window = IntPtr.Zero;
        }

        xevent.ClientMessageEvent.message_type = (IntPtr)PostAtom;
        xevent.ClientMessageEvent.format = 32;
        xevent.ClientMessageEvent.ptr1 = handle;
        xevent.ClientMessageEvent.ptr2 = (IntPtr)message;
        xevent.ClientMessageEvent.ptr3 = wParam;
        xevent.ClientMessageEvent.ptr4 = lParam;

        if (hwnd != null)
            hwnd.Queue.EnqueueLocked(xevent);
        else
            ThreadQueue(Thread.CurrentThread).EnqueueLocked(xevent);

        return true;
    }

    public override int SendInput(IntPtr hwnd, Queue keys)
    {
        throw new NotImplementedException();
    }

    public override object StartLoop(Thread thread)
    {
        XEventQueue q = ThreadQueue(thread);
        return q;
    }

    public override void EndLoop(Thread thread)
    {

    }

    public override void RequestNCRecalc(IntPtr hwnd)
    {
        Hwnd hwnd2;

        hwnd2 = Hwnd.ObjectFromHandle(hwnd);

        if (hwnd2 == null)
        {
            return;
        }

        PerformNCCalc(hwnd2);
        SendMessage(hwnd, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
        InvalidateNC(hwnd);
    }

    public override void ResetMouseHover(IntPtr hwnd)
    {
        throw new NotImplementedException();
    }

    public override void RequestAdditionalWM_NCMessages(IntPtr hwnd, bool hover, bool leave)
    {
        throw new NotImplementedException();
    }

    public override void RaiseIdle(EventArgs e)
    {
        if (Idle != null)
            Idle(this, e);
    }

    public override int KeyboardSpeed => throw new NotImplementedException();

    public override int KeyboardDelay => throw new NotImplementedException();

    public KeyboardXplat Keyboard { get; set; }
}

public interface KeyboardXplat
{
    public void FocusIn(IntPtr focusWindow);
    public void FocusOut(IntPtr focusWindow);
    void SetCaretPos(CaretStruct caret, IntPtr handle, int x, int y);
}

internal class keyboardimp : KeyboardXplat
{
    public void FocusIn(IntPtr focusWindow)
    {
       
    }

    public void FocusOut(IntPtr focusWindow)
    {
       
    }

    public void SetCaretPos(CaretStruct caret, IntPtr handle, int x, int y)
    {
        
    }
}
