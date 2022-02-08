using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

using ClassLibrary2;
using myVRXamlComponent;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        DXDeviceResources dxDevice;
        TimeSpan lastRender = TimeSpan.Zero;
        bool lastVisible = false;

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private double _frameCounter;

        bool continousRender = false;
        bool randomClearColor = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

//            Debug.Assert(splitContainer1.Panel2.Handle == splitContainer1.Panel1.Handle);
            dxDevice.Present();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dxDevice = new DXDeviceResources();
            int surfWidth = splitContainer1.Panel2.Width;
            int surfHeight = splitContainer1.Panel2.Height;

            unsafe
            {
                dxDevice.InitDeviceResources(splitContainer1.Panel2.Handle.ToPointer(), splitContainer1.Panel2.Bounds.X, 0, surfWidth, surfHeight);
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

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (continousRender)
            {
                if (_frameCounter++ == 0)
                {
                    // Starting timing.
                    _stopwatch.Start();
                }

                // Determine frame rate in fps (frames per second).
                int speed = 2;
                var frameRate = (long)(_frameCounter * speed / (_stopwatch.Elapsed.TotalSeconds));
                if (frameRate > 0)
                {
                    // Update elapsed time, number of frames, and frame rate.
                    dxDevice.Present();
                    _frameCounter = 0;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            continousRender = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            continousRender = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            randomClearColor = !randomClearColor;
            if(dxDevice != null)
            {
                dxDevice.randClearColor(randomClearColor);
                if(!continousRender)
                    dxDevice.Present();
            }
        }
    }
}