using CameraControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Declare the camera object.
        public CameraControllerDriver cameraDriver { get; } = new CameraControllerDriver();

        public MainWindow()
        {
            InitializeComponent();

            // Declare homepage.
            Home homepage = new Home(cameraDriver);

            // Set homepage as mainframe.
            MainFrame.Navigate(homepage);
        }
    }
}
