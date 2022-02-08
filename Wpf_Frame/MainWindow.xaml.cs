using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wpf_Frame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper parentWindowInterOp = new WindowInteropHelper(this);
            parentWindowInterOp.EnsureHandle();
            IntPtr handle = parentWindowInterOp.Handle;

            HwndSource? panelHwnd = PresentationSource.FromVisual(myframe) as HwndSource;
            IntPtr hWnd = panelHwnd.Handle;
            if (hWnd == IntPtr.Zero)
                System.Console.WriteLine("null");

            bool same = panelHwnd.Handle == handle;
            Debug.Assert(same);

        }
    }
}
