using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using ScreenshotCaptureWithMouse.ScreenCapture;
using System.Runtime.InteropServices;

namespace PictureInPicture
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
      InitializeMonitor();
    }

    private void InitializeMonitor()
    {
      Screen.AllScreens.ToList().ForEach(x => screensUI.Items.Add(x.DeviceName));
      screensUI.SelectedIndex = 0;

      Timer timer = new Timer();
      timer.Tick += new EventHandler(timer_Tick);
      timer.Interval = 1;
      timer.Start();
      DoubleBuffered = true;
    }

    void timer_Tick(object sender, EventArgs e)
    {
      CaptureScreenShot();
    }

    private void CaptureScreenShot()
    {

      if (pictureBox.Image != null)
      {
        pictureBox.Image.Dispose();
      }

      Screen screen = Screen.AllScreens[screensUI.SelectedIndex];
      using (Bitmap bmpScreenshot = new Bitmap(screen.Bounds.Width, screen.Bounds.Height, PixelFormat.Format32bppArgb))
      {
        using (Graphics graphics = Graphics.FromImage(bmpScreenshot))
        {
          int cursorX = 0;
          int cursorY = 0;
          graphics.CopyFromScreen(screen.Bounds.X, screen.Bounds.Y, 0, 0, screen.Bounds.Size, CopyPixelOperation.SourceCopy);
          Bitmap bmpCursor = CaptureCursor(ref cursorX, ref cursorY);
          if (cursorX > screen.Bounds.X && cursorY > screen.Bounds.Y)
          {
            cursorX = cursorX - screen.Bounds.X;
            cursorY = cursorY - screen.Bounds.Y;
            graphics.DrawImage(bmpCursor, new Rectangle(cursorX, cursorY, bmpCursor.Width, bmpCursor.Height));
            graphics.DrawEllipse(new Pen(new SolidBrush(Color.Red), 5), new Rectangle(cursorX, cursorY, bmpCursor.Width, bmpCursor.Height));
          }
          bmpCursor.Dispose();
        }

        Bitmap bit = ResizeBitmap(bmpScreenshot, Width, Height);
        pictureBox.Image = bit;
      }
    }

    private Bitmap CaptureCursor(ref int x, ref int y)
    {
      Bitmap bmp = null;
      IntPtr hicon = IntPtr.Zero;
      Win32Cursor.CURSORINFO ci = new Win32Cursor.CURSORINFO();
      Win32Cursor.ICONINFO icInfo;
      ci.cbSize = Marshal.SizeOf(ci);
      if (Win32Cursor.GetCursorInfo(out ci))
      {
        if (ci.flags == Win32Cursor.CURSOR_SHOWING)
        {
          hicon = Win32Cursor.CopyIcon(ci.hCursor);
          if (Win32Cursor.GetIconInfo(hicon, out icInfo))
          {
            x = ci.ptScreenPos.x - ((int)icInfo.xHotspot);
            y = ci.ptScreenPos.y - ((int)icInfo.yHotspot);
            Icon ic = Icon.FromHandle(hicon);
            bmp = ic.ToBitmap();
            ic.Dispose();
          }
        }
      }

      Win32Cursor.DestroyIcon(hicon);
      return bmp;
    }


    public Bitmap ResizeBitmap( Bitmap b, int nWidth, int nHeight )
    {
      Bitmap result = new Bitmap( nWidth, nHeight );
      Graphics g = Graphics.FromImage((Image)result);
      g.DrawImage(b, 0, 0, nWidth, nHeight);
      g.Dispose();
      return result;
    }
  }
}
