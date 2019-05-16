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
//	Peter Bartok		pbartok@novell.com
//	Sebastien Pouliot	sebastien@ximian.com
//

// COMPLETE

using System.Drawing; using MissionPlanner.Utilities.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using MissionPlanner.Utilities.Drawing;

namespace System.Windows.Forms {
	public abstract class XplatUIDriver {
		public abstract IntPtr	InitializeDriver();
		public abstract void		ShutdownDriver(IntPtr token);
		public delegate IntPtr	WndProc(IntPtr hwnd, Msg msg, IntPtr wParam, IntPtr lParam);


		#region XplatUI Driver Properties
		public virtual int ActiveWindowTrackingDelay { get { return 0; } }

		public virtual Color ForeColor {
			get {
				return ThemeEngine.Current.DefaultWindowForeColor;
			}
		}

		public virtual  Color BackColor { 
			get {
				return ThemeEngine.Current.DefaultWindowBackColor;
			}
		}

		public virtual Size Border3DSize {
			get {
				return new Size (2, 2);
			}
		}

		public virtual Size BorderSize {
			get {
				return new Size (1, 1);
			}
		}

		public virtual Size CaptionButtonSize {
			get {
				return new Size (18, 18);
			}
		}

		public virtual int CaretBlinkTime { get { return 530; } }
		public virtual int CaretWidth { get { return 10; } }
		
		public virtual Size DoubleClickSize {
			get {
				return new Size (4, 4);
			}
		}

		public virtual int DoubleClickTime {
			get {
				return 500;
			}
		}

		public virtual Size FixedFrameBorderSize {
			get {
				return new Size (3, 3);
			}
		}

		public virtual Font Font {
			get {
				return ThemeEngine.Current.DefaultFont;
			}
		}

		public virtual int FontSmoothingContrast { get { return 1400; } }
		public virtual int FontSmoothingType { get { return 1; } }
		public virtual int HorizontalResizeBorderThickness { get { return 8; } }
		public virtual bool IsActiveWindowTrackingEnabled { get { return false; } }
		public virtual bool IsComboBoxAnimationEnabled { get { return false; } }
		public virtual bool IsDropShadowEnabled { get { return false; } }
		public virtual bool IsFontSmoothingEnabled { get { return true; } }
		public virtual bool IsHotTrackingEnabled { get { return false; } }
		public virtual bool IsIconTitleWrappingEnabled { get { return true; } }
		public virtual bool IsKeyboardPreferred { get { return false; } }
		public virtual bool IsListBoxSmoothScrollingEnabled { get { return true; } }
		public virtual bool IsMenuAnimationEnabled { get { return false; } }
		public virtual bool IsMenuFadeEnabled { get { return true; } }
		public virtual bool IsMinimizeRestoreAnimationEnabled { get { return false; } }
		public virtual bool IsSelectionFadeEnabled { get { return false; } }
		public virtual bool IsSnapToDefaultEnabled { get { return false; } }
		public virtual bool IsTitleBarGradientEnabled { get { return false; } }
		public virtual bool IsToolTipAnimationEnabled { get { return false; } }
		public virtual Size MenuBarButtonSize { get { return new Size (19, 19); } }
		public virtual Size MenuButtonSize {
			get {
				return new Size(18, 18);
			}
		}
		public virtual int MenuShowDelay { get { return 0; } }
		
		public virtual Keys ModifierKeys {
			get {
				return Keys.None;
			}
		}

		public virtual MouseButtons MouseButtons {
			get {
				return MouseButtons.None;
			}
		}

		public virtual Size MouseHoverSize {
			get {
				return new Size (1, 1);
			}
		}

		public virtual int MouseHoverTime {
			get {
				return 500;
			}
		}

		public virtual int MouseSpeed {
			get { return 10; }
		}
		
		public virtual int MouseWheelScrollDelta {
			get {
				return 120;
			}
		}
		
		public virtual Point MousePosition {
			get {
				return Point.Empty;
			}
		}

		public virtual int MenuHeight {
			get {
				return 19;
			}
		}

		public virtual LeftRightAlignment PopupMenuAlignment {
			get { return LeftRightAlignment.Left; }
		}
		
		public virtual PowerStatus PowerStatus {
			get { throw new NotImplementedException ("Has not been implemented yet for this platform."); }
		}

		public virtual int SizingBorderWidth {
			get { return 4; }
		}
		
		public virtual Size SmallCaptionButtonSize {
			get { return new Size (15, 15); }
		}
		
		public virtual bool UIEffectsEnabled {
			get { return false; }
		}
		
		public virtual bool DropTarget {
			get {
				return false;
			}

			set {
			}
		}

		public virtual int HorizontalScrollBarHeight {
			get {
				return 16;
			}
		}

		public virtual bool UserClipWontExposeParent {
			get {
				return true;
			}
		}

		public virtual int VerticalResizeBorderThickness { get { return 8; } }

		public virtual int VerticalScrollBarWidth {
			get {
				return 16;
			}
		}

		public abstract int CaptionHeight { get; }
		public abstract Size CursorSize { get; }
		public abstract bool DragFullWindows { get; }
		public abstract Size DragSize { get; }
		public abstract Size FrameBorderSize { get; }
		public abstract Size IconSize { get; }
		public abstract Size MaxWindowTrackSize { get; }
		public abstract bool MenuAccessKeysUnderlined { get; }
		public virtual Size MinimizedWindowSize {
			get {
				const int BorderWidth = 3;
				return new Size (154 + 2 * BorderWidth, SystemInformation.CaptionHeight + 2 * BorderWidth - 1);
			}
		}
		public abstract Size MinimizedWindowSpacingSize { get; }
		public abstract Size MinimumWindowSize { get; }
		public virtual Size MinimumFixedToolWindowSize { get { return Size.Empty; } }
		public virtual Size MinimumSizeableToolWindowSize { get { return Size.Empty; } }
		public virtual Size MinimumNoBorderWindowSize { get { return Size.Empty; } }
		public virtual Size MinWindowTrackSize {
			get {
				return new Size (112, 27);
			}
		}
		public abstract Size SmallIconSize { get; }
		public abstract int MouseButtonCount { get; }
		public abstract bool MouseButtonsSwapped { get; }
		public abstract bool MouseWheelPresent { get; }
		public abstract Rectangle VirtualScreen { get; }
		public abstract Rectangle WorkingArea { get; }
		public abstract Screen[] AllScreens { get; }
		public abstract bool ThemesEnabled { get; }

		public virtual bool RequiresPositiveClientAreaSize {
			get {
				return true;
			}
		}

		public virtual int ToolWindowCaptionHeight {
			get {
				return 16;
			}
		}

		public virtual Size ToolWindowCaptionButtonSize {
			get {
				return new Size (15, 15);
			}
		}
		#endregion	// XplatUI Driver Properties

		public abstract event EventHandler Idle;

		#region XplatUI Driver Methods
		public abstract void AudibleAlert(AlertType alert);

		public abstract void BeginMoveResize (IntPtr handle); // init a window manager driven resize event

		public abstract void EnableThemes();

		public abstract void GetDisplaySize(out Size size);

		public abstract IntPtr CreateWindow(CreateParams cp);
		public abstract IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height);
		public abstract void DestroyWindow(IntPtr handle);

		public abstract FormWindowState GetWindowState(IntPtr handle);
		public abstract void SetWindowState(IntPtr handle, FormWindowState state);
		public abstract void SetWindowMinMax(IntPtr handle, Rectangle maximized, Size min, Size max);

		public abstract void SetWindowStyle(IntPtr handle, CreateParams cp);

		public abstract double GetWindowTransparency(IntPtr handle);
		public abstract void SetWindowTransparency(IntPtr handle, double transparency, Color key);
		public abstract TransparencySupport SupportsTransparency();

		public virtual void SetAllowDrop (IntPtr handle, bool value)
		{
			Console.Error.WriteLine ("Drag and Drop is currently " +
					"not supported on this platform");
		}

		public virtual DragDropEffects StartDrag(IntPtr handle, object data, DragDropEffects allowedEffects) {
			Console.Error.WriteLine ("Drag and Drop is currently " +
					"not supported on this platform");
			return DragDropEffects.None;
		}

		public abstract void SetBorderStyle(IntPtr handle, FormBorderStyle border_style);
		public abstract void SetMenu(IntPtr handle, Menu menu);

		public abstract bool GetText(IntPtr handle, out string text);
		public abstract bool Text(IntPtr handle, string text);
		public abstract bool SetVisible(IntPtr handle, bool visible, bool activate);
		public abstract bool IsVisible(IntPtr handle);
		public abstract bool IsEnabled(IntPtr handle);
		public virtual bool IsKeyLocked (VirtualKeys key) { return false; }
		public abstract IntPtr SetParent(IntPtr handle, IntPtr parent);
		public abstract IntPtr GetParent(IntPtr handle);

		public abstract void UpdateWindow(IntPtr handle);
		public abstract PaintEventArgs PaintEventStart (ref Message msg, IntPtr handle, bool client);
		public abstract void PaintEventEnd (ref Message msg, IntPtr handle, bool client);

		public abstract void SetWindowPos(IntPtr handle, int x, int y, int width, int height);
		public abstract void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height);
		public abstract void Activate(IntPtr handle);
		public abstract void EnableWindow(IntPtr handle, bool Enable);
		public abstract void SetModal(IntPtr handle, bool Modal);
		public abstract void Invalidate(IntPtr handle, Rectangle rc, bool clear);
		public abstract void InvalidateNC(IntPtr handle);
		public abstract IntPtr DefWndProc(ref Message msg);
		public abstract void HandleException(Exception e);
		public abstract void DoEvents();
		public abstract bool PeekMessage(Object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags);
		public abstract void PostQuitMessage(int exitCode);
		public abstract bool GetMessage(object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax);
		public abstract bool TranslateMessage(ref MSG msg);
		public abstract IntPtr DispatchMessage(ref MSG msg);

		public abstract bool SetZOrder(IntPtr hWnd, IntPtr AfterhWnd, bool Top, bool Bottom);
		public abstract bool SetTopmost(IntPtr hWnd, bool Enabled);
		public abstract bool SetOwner(IntPtr hWnd, IntPtr hWndOwner);

		public abstract bool CalculateWindowRect(ref Rectangle ClientRect, CreateParams cp, Menu menu, out Rectangle WindowRect);

		public abstract Region GetClipRegion(IntPtr hwnd);
		public abstract void SetClipRegion(IntPtr hwnd, Region region);

		public abstract void SetCursor(IntPtr hwnd, IntPtr cursor);
		public abstract void ShowCursor(bool show);
		public abstract void OverrideCursor(IntPtr cursor);
		public abstract IntPtr DefineCursor(Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot);
		public abstract IntPtr DefineStdCursor(StdCursor id);
		public abstract Bitmap DefineStdCursorBitmap(StdCursor id);
		public abstract void DestroyCursor(IntPtr cursor);
		public abstract void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y);

		public abstract void GetCursorPos(IntPtr hwnd, out int x, out int y);
		public abstract void SetCursorPos(IntPtr hwnd, int x, int y);

		public abstract void ScreenToClient(IntPtr hwnd, ref int x, ref int y);
		public abstract void ClientToScreen(IntPtr hwnd, ref int x, ref int y);

		public abstract void GrabWindow(IntPtr hwnd, IntPtr ConfineToHwnd);
		public abstract void GrabInfo(out IntPtr hwnd, out bool GrabConfined, out Rectangle GrabArea);
		public abstract void UngrabWindow(IntPtr hwnd);

		public abstract void SendAsyncMethod (AsyncMethodData method);
		public abstract void SetTimer (Timer timer);
		public abstract void KillTimer (Timer timer);

		public abstract void CreateCaret(IntPtr hwnd, int width, int height);
		public abstract void DestroyCaret(IntPtr hwnd);
		public abstract void SetCaretPos(IntPtr hwnd, int x, int y);
		public abstract void CaretVisible(IntPtr hwnd, bool visible);

		public abstract IntPtr GetFocus();
		public abstract void SetFocus(IntPtr hwnd);
		public abstract IntPtr GetActive();
		public abstract IntPtr GetPreviousWindow(IntPtr hwnd);

		public abstract void ScrollWindow(IntPtr hwnd, Rectangle rectangle, int XAmount, int YAmount, bool with_children);
		public abstract void ScrollWindow(IntPtr hwnd, int XAmount, int YAmount, bool with_children);

		public abstract bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent);

		public abstract bool SystrayAdd(IntPtr hwnd, string tip, Icon icon, out ToolTip tt);
		public abstract bool SystrayChange(IntPtr hwnd, string tip, Icon icon, ref ToolTip tt);
		public abstract void SystrayRemove(IntPtr hwnd, ref ToolTip tt);
		public abstract void SystrayBalloon(IntPtr hwnd, int timeout, string title, string text, ToolTipIcon icon);

		public abstract Point GetMenuOrigin(IntPtr hwnd);
		public abstract void MenuToScreen(IntPtr hwnd, ref int x, ref int y);
		public abstract void ScreenToMenu(IntPtr hwnd, ref int x, ref int y);

		public abstract void SetIcon(IntPtr handle, Icon icon);

		public abstract void ClipboardClose(IntPtr handle);
		public abstract IntPtr ClipboardOpen (bool primary_selection);
		public abstract int ClipboardGetID(IntPtr handle, string format);
		public abstract void ClipboardStore(IntPtr handle, object obj, int id, XplatUI.ObjectToClipboard converter, bool copy);
		public abstract int[] ClipboardAvailableFormats(IntPtr handle);
		public abstract object ClipboardRetrieve(IntPtr handle, int id, XplatUI.ClipboardToObject converter);

		public abstract void DrawReversibleLine(Point start, Point end, Color backColor);
		public abstract void DrawReversibleRectangle(IntPtr handle, Rectangle rect, int line_width);
		public abstract void FillReversibleRectangle (Rectangle rectangle, Color backColor);
		public abstract void DrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style);

		public abstract SizeF GetAutoScaleSize(Font font);

		public abstract IntPtr SendMessage(IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam);
		public abstract bool PostMessage(IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam);
		public abstract int SendInput(IntPtr hwnd, System.Collections.Queue keys);

		public abstract object StartLoop(Thread thread);
		public abstract void EndLoop(Thread thread);

		public abstract void RequestNCRecalc(IntPtr hwnd);
		public abstract void ResetMouseHover(IntPtr hwnd);
		public abstract void RequestAdditionalWM_NCMessages(IntPtr hwnd, bool hover, bool leave);

		public abstract void RaiseIdle (EventArgs e);
		
		// System information
		public abstract int KeyboardSpeed { get; } 
		public abstract int KeyboardDelay { get; } 


		// Double buffering
		public virtual void CreateOffscreenDrawable (IntPtr handle,
							       int width, int height,
							       out object offscreen_drawable)
		{
			Bitmap bmp = new Bitmap (width, height, PixelFormat.Format32bppArgb);

			offscreen_drawable = bmp;
		}

		public virtual void DestroyOffscreenDrawable (object offscreen_drawable)
		{
			Bitmap bmp = (Bitmap)offscreen_drawable;

			bmp.Dispose ();
		}
		
		public virtual Graphics GetOffscreenGraphics (object offscreen_drawable)
		{
			Bitmap bmp = (Bitmap)offscreen_drawable;
			return Graphics.FromImage (bmp);
		}

		public virtual void BlitFromOffscreen (IntPtr dest_handle,
							 Graphics dest_dc,
							 object offscreen_drawable,
							 Graphics offscreen_dc,
							 Rectangle r)
		{
			dest_dc.DrawImage ((Bitmap)offscreen_drawable, r, r, GraphicsUnit.Pixel);
		}
		
		public virtual void SetForegroundWindow (IntPtr handle)
		{
		}

#endregion	// XplatUI Driver Methods
	}

	static class XplatUIDriverSupport {
		#region XplatUI Driver Support Methods
		internal static void ExecutionCallback (AsyncMethodData data)
		{
			AsyncMethodResult result = data.Result;
			
			object ret;
			try {
				ret = data.Method.DynamicInvoke (data.Args);
			} catch (Exception ex) {
				if (result != null) {
					result.CompleteWithException (ex);
					return;
				}
				
				throw;
			}
		
			if (result != null) {
				result.Complete (ret);
			}
		}

		static void ExecutionCallbackInContext (object state)
		{
			AsyncMethodData data = (AsyncMethodData) state;

			if (data.SyncContext == null) {
				ExecutionCallback (data);
				return;
			}

			var oldContext = SynchronizationContext.Current;
			SynchronizationContext.SetSynchronizationContext (data.SyncContext);

			try {
				ExecutionCallback (data);
			} finally {
				SynchronizationContext.SetSynchronizationContext (oldContext);
			}
		}

		internal static void ExecuteClientMessage (GCHandle gchandle)
		{
			AsyncMethodData data = (AsyncMethodData) gchandle.Target;
			try {
				if (data.Context == null) {
					ExecutionCallback (data);
				} else {
					data.SyncContext = SynchronizationContext.Current;
					ExecutionContext.Run (data.Context, new ContextCallback (ExecutionCallbackInContext), data);
				}
			}
			finally {
				gchandle.Free ();
			}
		}
		
		#endregion	// XplatUI Driver Support Methods
	}
}
