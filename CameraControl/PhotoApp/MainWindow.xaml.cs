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
        private EvfPictureBox _evfPictureBox;
        private List<IObserver> _observerList = new List<IObserver>();

        // Declare controller.
        CameraController controller;

        // Declare camera model.

        CameraModel model = null;

        // Declare action source.
        ActionSource _actionSource = new ActionSource();

        // Declare action listener.
        List<ActionListener> _actionListeners = new List<ActionListener>();

        private GCHandle _controllerHandle;
        private bool _isSDKLoaded = false;

        private EDSDKLib.EDSDK.EdsPropertyEventHandler _handlePropertyEvent;
        private EDSDKLib.EDSDK.EdsObjectEventHandler _handleObjectEvent;
        private EDSDKLib.EDSDK.EdsStateEventHandler _handleStateEvent;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            uint err = EDSDKLib.EDSDK.EdsInitializeSDK();
            _isSDKLoaded = (err == EDSDKLib.EDSDK.EDS_ERR_OK);

            IntPtr cameraList = IntPtr.Zero;
            if (err == EDSDKLib.EDSDK.EDS_ERR_OK)
                err = EDSDKLib.EDSDK.EdsGetCameraList(out cameraList);

            if (err == EDSDKLib.EDSDK.EDS_ERR_OK)
            {
                int count = 0;
                err = EDSDKLib.EDSDK.EdsGetChildCount(cameraList, out count);
                if (count == 0)
                    err = EDSDKLib.EDSDK.EDS_ERR_DEVICE_NOT_FOUND;
            }

            IntPtr camera = IntPtr.Zero;
            if (err == EDSDKLib.EDSDK.EDS_ERR_OK)
                err = EDSDKLib.EDSDK.EdsGetChildAtIndex(cameraList, 0, out camera);

            if (cameraList != IntPtr.Zero)
                EDSDKLib.EDSDK.EdsRelease(cameraList);

            if (err != EDSDKLib.EDSDK.EDS_ERR_OK || camera == IntPtr.Zero)
            {
                MessageBox.Show("Cannot detect camera");
                return;
            }

            model = new CameraModel(camera);
            controller = new CameraController(ref model);

            _actionListeners.Add((ActionListener)controller);
            _actionListeners.ForEach(l => _actionSource.AddActionListener(ref l));

            _controllerHandle = GCHandle.Alloc(controller);
            IntPtr ptr = GCHandle.ToIntPtr(_controllerHandle);

            _handlePropertyEvent = new EDSDKLib.EDSDK.EdsPropertyEventHandler(CameraEventListener.HandlePropertyEvent);
            _handleObjectEvent = new EDSDKLib.EDSDK.EdsObjectEventHandler(CameraEventListener.HandleObjectEvent);
            _handleStateEvent = new EDSDKLib.EDSDK.EdsStateEventHandler(CameraEventListener.HandleStateEvent);

            EDSDKLib.EDSDK.EdsSetPropertyEventHandler(camera, EDSDKLib.EDSDK.PropertyEvent_All, _handlePropertyEvent, ptr);
            EDSDKLib.EDSDK.EdsSetObjectEventHandler(camera, EDSDKLib.EDSDK.ObjectEvent_All, _handleObjectEvent, ptr);
            EDSDKLib.EDSDK.EdsSetCameraStateEventHandler(camera, EDSDKLib.EDSDK.StateEvent_All, _handleStateEvent, ptr);

            controller.Run();

            // Sleep for 1 second (Canon sample)
            System.Threading.Thread.Sleep(1000);

            // Create WinForms EVF control
            _evfPictureBox = new EvfPictureBox();
            _evfPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            _evfPictureBox.SetActionSource(ref _actionSource);

            // Host inside WPF
            EvfHost.Child = _evfPictureBox;

            // REGISTER OBSERVER (this is the missing piece in most broken ports)
            _observerList.Add((IObserver)_evfPictureBox);
            _observerList.ForEach(o => controller.GetModel().Add(ref o));

            //controller.GetModel().Add(ref observer);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            uint err = EDSDKLib.EDSDK.EdsSendCommand(controller.GetModel().Camera, EDSDKLib.EDSDK.CameraCommand_TakePicture, 1);
        }

        private void StartEvf_Click(object sender, RoutedEventArgs e)
        {
            _actionSource.FireEvent(ActionEvent.Command.START_EVF, IntPtr.Zero);
        }

        private void StopEvf_Click(object sender, RoutedEventArgs e)
        {
            _actionSource.FireEvent(ActionEvent.Command.END_EVF, IntPtr.Zero);
        }

        private void TakePicture_Click(object sender, RoutedEventArgs e)
        {
            _actionSource.FireEvent(ActionEvent.Command.TAKE_PICTURE, IntPtr.Zero);
        }
    }
}
