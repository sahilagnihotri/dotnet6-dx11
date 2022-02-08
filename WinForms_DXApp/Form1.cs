using System.Diagnostics;
using ClassLibrary2;
using myVRXamlComponent;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        DXDeviceResources dxDevice;

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

        }

        private void button1_Click(object sender, EventArgs e)
        {
            dxDevice.Present();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            dxDevice.Present();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            dxDevice.Present();

        }
    }
}