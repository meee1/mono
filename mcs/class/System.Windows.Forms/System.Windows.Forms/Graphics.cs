using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using SvgNet.SvgGdi;

namespace System
{
    public static class Extensions
    {
        //clip_region.IsInfinite (graphics)) 
        public static bool IsInfinite(this Region reg, Graphics g)
        {
            return false;
        }

        public static IntPtr GetHrgn(this Region reg, Graphics g)
        {
            return IntPtr.Zero;
        }

        public static Rectangle GetBounds(this Region reg, Graphics g)
        {
            return Rectangle.Empty;
        }
    }

    public class Graphics: SvgGraphics,  IDeviceContext, IDisposable
    {
        private IntPtr _hdc;
        private IntPtr _hwnd;

        private object _g;

        //HWND is a "handle to a window"
        // A handle to the Device Context (HDC)
        
        public static implicit operator System.Drawing.Graphics(Graphics g)
        {
            return Drawing.Graphics.FromHwnd(g._hwnd);
        }

        public static implicit operator Graphics(System.Drawing.Graphics g)
        {
            return new Graphics() { _hdc = g.GetHdc(), _g = g } ;
        }
        
        private Graphics() //: base(System.Drawing.Graphics.FromImage(new Bitmap(4000,4000)))
        {

        }

        //public Graphics(Drawing.Graphics g) : base(g)
        //{
            
        //}
        private static int cnt = 0;
        public void Dispose()
        {
            cnt++;
            var ans = base.WriteSVGString();
            System.IO.File.WriteAllText(_hwnd + ".svg", ans);
        }

        public static Graphics FromImage(Image bitmap)
        {
            var g = Drawing.Graphics.FromImage(bitmap);
            return new Graphics() {_hdc = g.GetHdc(), _g = g};
        }

        public static Graphics FromHwnd(IntPtr windowHandle)
        {
            return new Graphics() {_hwnd = windowHandle};
        }

        public static Graphics FromHdc(IntPtr hdc)
        {
            return new Graphics() {_hdc = hdc};
        }

        public static void FromHdcInternal(IntPtr zero)
        {
            // X11 only
            //_hdc = zero;
        }

        public IntPtr GetHdc()
        {
            if (_hdc == IntPtr.Zero)
            {
                _hdc = Drawing.Graphics.FromHwnd(_hwnd).GetHdc();
            }
            return _hdc;
        }

        public void ReleaseHdc()
        {
            _hdc = IntPtr.Zero;
            //throw new NotImplementedException();
        }

        public void ReleaseHdc(IntPtr destHdc)
        {
            //_hdc = IntPtr.Zero;
            //throw new NotImplementedException();
        }

        public void DrawImageUnscaledAndClipped(Image image, Rectangle bounds)
        {
            DrawImage(image, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        internal static Graphics FromHdc(IntPtr hdc, IntPtr handle)
        {
            return new Graphics() {_hdc = hdc, _hwnd = handle};
        }

        public void CopyFromScreen(int i, int i1, int i2, int i3, Size size)
        {
            throw new NotImplementedException();
        }
    }
}
