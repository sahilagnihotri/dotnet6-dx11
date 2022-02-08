#define useStaticChildWindow
//#define testChildWindow

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using ClassLibrary2;
using myVRXamlComponent;
using System.Diagnostics;

using System.Runtime.InteropServices;
using static Wpf_DXApp.NativeChildWindow;

namespace Wpf_DXApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    static class NativeChildWindow
    {
        public const int WS_OVERLAPPED = 0x0;
        public const int WS_BORDER = 0x00800000;
        public const int WS_POPUP = unchecked((int)0x80000000L);
        public const int WS_CHILD = 0x40000000;
        public const int WS_MINIMIZE = 0x20000000;
        public const int WS_VISIBLE = 0x10000000;
        public const int WS_DISABLED = 0x8000000;
        public const int SS_NOTIFY = 0x00000100;
        [DllImport("User32.dll", SetLastError = true)]
        public static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
        [DllImport("User32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);
        public static int HIWORD(int n)
        {
            return (n >> 16) & 0xffff;
        }
        public static int LOWORD(int n)
        {
            return n & 0xffff;
        }
        public delegate int SUBCLASSPROC(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, uint dwRefData);
        [System.Runtime.InteropServices.DllImport("Comctl32.dll", SetLastError = true)]
        public static extern bool SetWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass, uint dwRefData);
        [System.Runtime.InteropServices.DllImport("Comctl32.dll", SetLastError = true)]
        public static extern int DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
        public const int WM_SIZE = 0x0005;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
    }

    public partial class MainWindow : Window
    {
        DXDeviceResources dxDevice;
#if testChildWindow
        Window childWindow;
#endif
        IntPtr handle;
        double dpiScale = 1.0; // default value for 96 dpi
                               // State Management
        TimeSpan lastRender = TimeSpan.Zero;
        bool lastVisible = false;
        bool continousRender = false;
        bool randClearColor = false;
        int frameSpeed = 1;

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private double _frameCounter;

        private IntPtr hWndContainer = IntPtr.Zero;
        //        private NativeChildWindow SubClassDelegate;
        private SUBCLASSPROC SubClassDelegate;

        public MainWindow()
        {
            InitializeComponent();
            dxDevice = new DXDeviceResources();
#if testChildWindow
            childWindow = new Window();
#endif

#if !useStaticChildWindow
            this.SizeChanged += MainWindow_SizeChanged;
#endif
            this.Loaded += MainWindow_Loaded;
        }

#if useStaticChildWindow
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource? hWndSource = PresentationSource.FromVisual(this) as HwndSource;
            var nWidth = ((Panel)Application.Current.MainWindow.Content).ActualWidth;
            var nHeight = ((Panel)Application.Current.MainWindow.Content).ActualHeight;

            int surfWidth = (int)(myVRCanvas.ActualWidth < 0 ? 0 : Math.Ceiling(myVRCanvas.ActualWidth * dpiScale));
            int surfHeight = (int)(myVRCanvas.ActualHeight < 0 ? 0 : Math.Ceiling(myVRCanvas.ActualHeight * dpiScale));
            int x = (int)(guiElements.ActualWidth < 0 ? 0 : Math.Ceiling(guiElements.ActualWidth * dpiScale));

            //            hWndContainer = CreateWindowEx(0, "Static", "", WS_VISIBLE | WS_CHILD | WS_BORDER | SS_NOTIFY, 120, 10, surfWidth , surfHeight, hWndSource.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            hWndContainer = CreateWindowEx(0, "Static", "", WS_VISIBLE | WS_CHILD | WS_BORDER | SS_NOTIFY, 120, 10, (int)nWidth - 120 - 10, (int)nHeight - 10 * 2, hWndSource.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);


            if (hWndContainer != IntPtr.Zero)
            {
                handle = hWndContainer;
                SubClassDelegate = new SUBCLASSPROC(WindowSubClass);
                bool bRet = SetWindowSubclass(hWndContainer, SubClassDelegate, 0, 0);
            }
            hWndSource.AddHook(WndProc);
        }

        private int nX = 250, nY = 0;
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SIZE)
            {
                if (hWndContainer != IntPtr.Zero)
                {
                     MoveWindow(hWndContainer, nX, nY, LOWORD((int)lParam) - nX, HIWORD((int)lParam), true);
                }
            }
            return IntPtr.Zero;
        }

        private int WindowSubClass(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, uint dwRefData)
        {
            switch (uMsg)
            {
                case WM_LBUTTONDBLCLK:
                    {
                        Console.Beep(6000, 10);
                        return 0;
                    }
                    break;
            }
            return DefSubclassProc(hWnd, uMsg, wParam, lParam);
        }
#endif

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper parentWindowInterOp = new WindowInteropHelper(this);
            parentWindowInterOp.EnsureHandle();

#if testChildWindow
            childWindow.Owner = this;
            childWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
//            childWindow.WindowStyle = WindowStyle.ToolWindow;
            childWindow.Show();

            WindowInteropHelper childWindowInterOp = new WindowInteropHelper(childWindow);
            childWindowInterOp.EnsureHandle();
            childWindowInterOp.Owner = parentWindowInterOp.Handle;
//            bool res = childWindow.Activate();
            handle = childWindowInterOp.Handle;
#else
#if !useStaticChildWindow
              handle = parentWindowInterOp.Handle;
#endif
#endif
            HwndSource? canvasHwnd = PresentationSource.FromVisual(myVRCanvas as Canvas) as HwndSource;
            if (canvasHwnd != null)
            {
                IntPtr hWnd = canvasHwnd.Handle;
                bool same = canvasHwnd.Handle == parentWindowInterOp.Handle;
                Debug.Assert(same);
            }

            int surfWidth = (int)(myVRCanvas.ActualWidth < 0 ? 0 : Math.Ceiling(myVRCanvas.ActualWidth * dpiScale));
            int surfHeight = (int)(myVRCanvas.ActualHeight < 0 ? 0 : Math.Ceiling(myVRCanvas.ActualHeight * dpiScale));

            unsafe
            {
                int x = (int)(guiElements.ActualWidth < 0 ? 0 : Math.Ceiling(guiElements.ActualWidth * dpiScale));
                dxDevice.InitDeviceResources(handle.ToPointer(), x, 0, surfWidth, surfHeight);
            }

            bool isVisible = (surfWidth != 0 && surfHeight != 0);
            if (lastVisible != isVisible)
            {
                lastVisible = isVisible;
                if (lastVisible)
                {
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                }
                else
                {
                    CompositionTarget.Rendering -= CompositionTarget_Rendering;
                }
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(handle != IntPtr.Zero && dxDevice != null)
            {
                int x = (int)(guiElements.ActualWidth < 0 ? 0 : Math.Ceiling(guiElements.ActualWidth * dpiScale));
                int surfWidth = (int)(myVRCanvas.ActualWidth < 0 ? 0 : Math.Ceiling(myVRCanvas.ActualWidth * dpiScale));
                int surfHeight = (int)(myVRCanvas.ActualHeight < 0 ? 0 : Math.Ceiling(myVRCanvas.ActualHeight * dpiScale));
                unsafe
                {
                    dxDevice.ResizeRenderTarget(handle.ToPointer(), x, 0, surfWidth, surfHeight);
                }
                if (!continousRender)
                    render();
            }
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
/*
            RenderingEventArgs args = (RenderingEventArgs)e;
            if (this.lastRender != args.RenderingTime)
            {
                dxDevice.Present();
                guiElements.Focus();          
                guiElements.InvalidateVisual();
                this.lastRender = args.RenderingTime;
            }
*/

            if (_frameCounter++ == 0)
            {
                // Starting timing.
                _stopwatch.Start();
            }

            if(continousRender)
            {
                // Determine frame rate in fps (frames per second).
                var frameRate = (long)(_frameCounter * frameSpeed / (_stopwatch.Elapsed.TotalSeconds));
                if (frameRate > 0)
                {
                    render();
                    // Update elapsed time, number of frames, and frame rate.
                    _frameCounter = 0;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //          string ButtonUrl = ((Button)sender).CommandParameter.ToString();

#if testChildWindow
            childWindow.Focus();
#endif
            render();
        }
        private void render()
        {
            if (handle != IntPtr.Zero && dxDevice != null)
            {
                dxDevice.Present();
                guiElements.Focus();
                guiElements.InvalidateVisual();
                //            guiElements.InvalidateArrange();
            }
        }

        private void enableContinousRender(object sender, RoutedEventArgs e)
        {
            continousRender = true;
        }
        private void disableContinousRender(object sender, RoutedEventArgs e)
        {
            continousRender = false;
        }
        private void toggleRandClearColor(object sender, RoutedEventArgs e)
        {
            randClearColor = !randClearColor;
            if (handle != IntPtr.Zero && dxDevice != null)
            {
                dxDevice.randClearColor(randClearColor);
                if (!continousRender)
                    render();
            }
        }
    }
}