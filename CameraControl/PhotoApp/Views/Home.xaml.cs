using CameraControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        private readonly CameraControllerDriver _camera;

        public Home(CameraControllerDriver camera)
        {
            InitializeComponent();
            _camera = camera;
            _camera.initialize();
            EvfHost.Child = _camera._evfPictureBox;

            // Start the Viewfinder
            _camera.StartEvf();

            /* use this logic to identify image height and width.
            Thread.Sleep(5000);
            if (_camera._evfPictureBox.Image != null)
            {
                int w = _camera._evfPictureBox.Image.Width;
                int h = _camera._evfPictureBox.Image.Height;

                Console.WriteLine($"EVF frame: {w} x {h} px");
            }
            */

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void StartEvf_Click(object sender, RoutedEventArgs e)
        {
            _camera.StartEvf();
            //_actionSource.FireEvent(ActionEvent.Command.START_EVF, IntPtr.Zero);
        }

        private void StopEvf_Click(object sender, RoutedEventArgs e)
        {
            _camera.StopEvf();
            //_actionSource.FireEvent(ActionEvent.Command.END_EVF, IntPtr.Zero);
        }

        private void TakePicture_Click(object sender, RoutedEventArgs e)
        {
            _camera.TakePicture();
            //_actionSource.FireEvent(ActionEvent.Command.TAKE_PICTURE, IntPtr.Zero);
        }
    }
}
