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

namespace Wpf_DXApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

        public MainWindow()
        {
            InitializeComponent();
            dxDevice = new DXDeviceResources();
#if testChildWindow
            childWindow = new Window();
#endif
            this.SizeChanged += MainWindow_SizeChanged;
            this.Loaded += MainWindow_Loaded;
        }

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
            handle = parentWindowInterOp.Handle;
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