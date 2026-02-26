using CameraControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PhotoApp
{
    public class CameraControllerDriver
    { 
        // Declare evf picture box.
        public EvfPictureBox _evfPictureBox;

        // Declare observer list.
        public List<IObserver> _observerList = new List<IObserver>();

        // Declare controller.
        CameraController controller;

        // Declare camera model.
        CameraModel model = null;

        // Declare action source.
        ActionSource _actionSource = new ActionSource();

        // Declare action listener.
        List<ActionListener> _actionListeners = new List<ActionListener>();

        // Declare controller handle.
        public GCHandle _controllerHandle;

        // Declare SDK loaded field.
        public bool _isSDKLoaded = false;

        private EDSDKLib.EDSDK.EdsPropertyEventHandler _handlePropertyEvent;
        private EDSDKLib.EDSDK.EdsObjectEventHandler _handleObjectEvent;
        private EDSDKLib.EDSDK.EdsStateEventHandler _handleStateEvent;

        public EvfPictureBox EvfControl => _evfPictureBox;

        public bool IsInitialized => _isSDKLoaded;

        public CameraControllerDriver()
        {
            // Ensure fields are in a known state
            _observerList = new List<IObserver>();
            _actionListeners = new List<ActionListener>();
            _actionSource = new ActionSource();
        }

        public void initialize()
        {
            if (_isSDKLoaded)
            {
                return;
            }

            // Initialize the SDk.
            uint err = EDSDKLib.EDSDK.EdsInitializeSDK();
            _isSDKLoaded = (err == EDSDKLib.EDSDK.EDS_ERR_OK);

            // Get camera list.
            IntPtr cameraList = IntPtr.Zero;
            if (err == EDSDKLib.EDSDK.EDS_ERR_OK)
            {
                err = EDSDKLib.EDSDK.EdsGetCameraList(out cameraList);
            }

            // Get the camera list count.
            if (err == EDSDKLib.EDSDK.EDS_ERR_OK)
            {
                int count = 0;
                err = EDSDKLib.EDSDK.EdsGetChildCount(cameraList, out count);

                // Edge case - Camera not found.
                if (count == 0)
                {
                    err = EDSDKLib.EDSDK.EDS_ERR_DEVICE_NOT_FOUND;
                }
            }

            IntPtr camera = IntPtr.Zero;
            if (err == EDSDKLib.EDSDK.EDS_ERR_OK)
            {
                // Get the first camera in the list.
                err = EDSDKLib.EDSDK.EdsGetChildAtIndex(cameraList, 0, out camera);
            }

            // if the camera is not zero ptr. Relese camera list.
            if (cameraList != IntPtr.Zero)
            {
                EDSDKLib.EDSDK.EdsRelease(cameraList);
            }

            if (err != EDSDKLib.EDSDK.EDS_ERR_OK || camera == IntPtr.Zero)
            {
                throw new Exception("Cannot detect camera");
            }

            // Set the camera model.
            model = new CameraModel(camera);

            // Set the controller.
            controller = new CameraController(ref model);

            // Add the controller to action listeners.
            _actionListeners.Add((ActionListener)controller);
            _actionListeners.ForEach(l => _actionSource.AddActionListener(ref l));

            // Add the controller to handle.
            _controllerHandle = GCHandle.Alloc(controller);
            IntPtr ptr = GCHandle.ToIntPtr(_controllerHandle);

            // Set Property, Object, and State handlers.
            _handlePropertyEvent = new EDSDKLib.EDSDK.EdsPropertyEventHandler(CameraEventListener.HandlePropertyEvent);
            _handleObjectEvent = new EDSDKLib.EDSDK.EdsObjectEventHandler(CameraEventListener.HandleObjectEvent);
            _handleStateEvent = new EDSDKLib.EDSDK.EdsStateEventHandler(CameraEventListener.HandleStateEvent);

            // Set property.
            EDSDKLib.EDSDK.EdsSetPropertyEventHandler(camera, EDSDKLib.EDSDK.PropertyEvent_All, _handlePropertyEvent, ptr);
            EDSDKLib.EDSDK.EdsSetObjectEventHandler(camera, EDSDKLib.EDSDK.ObjectEvent_All, _handleObjectEvent, ptr);
            EDSDKLib.EDSDK.EdsSetCameraStateEventHandler(camera, EDSDKLib.EDSDK.StateEvent_All, _handleStateEvent, ptr);

            // Run the controller.
            controller.Run();

            // Sleep for 1 second (Canon sample)
            System.Threading.Thread.Sleep(1000);

            // Create WinForms EVF control
            _evfPictureBox = new EvfPictureBox();
            _evfPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            _evfPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

            _evfPictureBox.SetActionSource(ref _actionSource);

            // REGISTER OBSERVER (this is the missing piece in most broken ports)
            _observerList.Add((IObserver)_evfPictureBox);
            _observerList.ForEach(o => controller.GetModel().Add(ref o));
        }

        /// <summary>
        /// Take picture command.
        /// </summary>
        public void TakePicture()
        {
            // Send take picture.
            _actionSource.FireEvent(ActionEvent.Command.TAKE_PICTURE, IntPtr.Zero);
        }

        /// <summary>
        /// Start electronic view finder.
        /// </summary>
        public void StartEvf()
        {
            // Send start evf.
            _actionSource.FireEvent(ActionEvent.Command.START_EVF, IntPtr.Zero);
        }

        /// <summary>
        /// Stop electronic view finder.
        /// </summary>
        public void StopEvf()
        {
            // Send stop evf.
            _actionSource.FireEvent(ActionEvent.Command.END_EVF, IntPtr.Zero);
        }
    }
}
