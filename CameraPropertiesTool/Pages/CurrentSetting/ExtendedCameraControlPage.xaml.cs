using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CameraPropertiesTool.Pages.CurrentSetting
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExtendedCameraControlPage : Page
    {
        string cameraId = null;
        private MediaCapture m_mediaCapture = null;
        private MediaPlayer m_mediaPlayer = null;

        public ExtendedCameraControlPage()
        {
            this.InitializeComponent();
            InitAsync();

        }

        public async void InitAsync()
        {
            //1. list all camera  -- TBD

            //2. load default camera 
            await LoadDefaultCameraAsync();

            //3. List all Properties
            await ListAllPropertiesAsync();

            //4. show each property ui
            await ShowAllSwitchsAsync();


        }

        public async Task LoadDefaultCameraAsync()
        {

            cameraId = await GetCameraIDAsync();
            if (string.IsNullOrEmpty(cameraId))
            {
                var ignore = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    SelectedCameraTB.Text = "Camera Not Found";
                });
                return;
            }
            await CameraInitializeAsync();
        }

        private async Task CameraInitializeAsync()
        {
            try
            {
                m_mediaCapture = new MediaCapture();
                m_mediaPlayer = new MediaPlayer();

                // We initialize the MediaCapture instance with the virtual camera in sharing mode
                // to preview its stream without blocking other app from using it
                var initSettings = new MediaCaptureInitializationSettings()
                {
                    SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                    VideoDeviceId = cameraId,
                    StreamingCaptureMode = StreamingCaptureMode.Video
                };

                await m_mediaCapture.InitializeAsync(initSettings);

                // Retrieve the source associated with the video preview stream.
                // On 1-pin camera, this may be the VideoRecord MediaStreamType as opposed to VideoPreview on multi-pin camera
                var frameSource = m_mediaCapture.FrameSources.FirstOrDefault(source => source.Value.Info.MediaStreamType == MediaStreamType.VideoPreview
                                                                                  && source.Value.Info.SourceKind == MediaFrameSourceKind.Color).Value;
                if (frameSource == null)
                {
                    frameSource = m_mediaCapture.FrameSources.FirstOrDefault(source => source.Value.Info.MediaStreamType == MediaStreamType.VideoRecord
                                                                                      && source.Value.Info.SourceKind == MediaFrameSourceKind.Color).Value;
                }

                // if no preview stream is available, bail
                if (frameSource == null)
                {
                    throw new Exception("no preview stream is available");
                }

                // Setup MediaPlayer with the preview source
                m_mediaPlayer.RealTimePlayback = true;
                m_mediaPlayer.AutoPlay = true;
                m_mediaPlayer.Source = MediaSource.CreateFromMediaFrameSource(frameSource);
                UIMediaPlayerElement.SetMediaPlayer(m_mediaPlayer);
            }
            catch (Exception ex)
            {
                TextOutput(ex.Message);
            }

        }

        private void TextOutput(string msg)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                UITextOutput.Text = $"error: {msg}";
            });
        }
        private Task ShowAllSwitchsAsync()
        {
            throw new NotImplementedException();
        }

        private async Task ListAllPropertiesAsync()
        {
            if (m_mediaCapture == null || m_mediaCapture.VideoDeviceController == null)
            {
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    UITextOutput.Text = $"error:mediaCapture not available";
                });
                return;
            }

            //bool isBlurControlSupported = false;
            //IExtendedPropertyPayload getPayload = null;

            //getPayload = PropertyInquiry.GetExtendedControl(m_mediaCapture.VideoDeviceController, ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_BACKGROUNDSEGMENTATION);
            //isBlurControlSupported = (getPayload != null);
            //if (isBlurControlSupported && (((ulong)BackgroundSegmentationCapabilityKind.KSCAMERA_EXTENDEDPROP_BACKGROUNDSEGMENTATION_BLUR & ~getPayload.Capability) == 0))
            //{
            //    m_backgroundBlurController = m_controlManager.CreateController(DefaultControlHelper.DefaultControllerType.ExtendedCameraControl, (uint)ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_BACKGROUNDSEGMENTATION);
            //}

        }





        public async Task<string> GetCameraIDAsync() // --TBD: support switch camera
        {
            DeviceInformationCollection deviceInfoCollection = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Find the sources and de-duplicate from source groups not containing VideoCapture devices
            var allGroups = await MediaFrameSourceGroup.FindAllAsync();
            var colorVideoGroupList = allGroups.Where(group =>
                                                      group.SourceInfos.Any(sourceInfo => sourceInfo.SourceKind == MediaFrameSourceKind.Color
                                                                                                       && (sourceInfo.MediaStreamType == MediaStreamType.VideoPreview
                                                                                                           || sourceInfo.MediaStreamType == MediaStreamType.VideoRecord)));
            var validColorVideoGroupList = colorVideoGroupList.Where(group =>
                                                                     group.SourceInfos.All(sourceInfo =>
                                                                                           deviceInfoCollection.Any((deviceInfo) =>
                                                                                                                    sourceInfo.DeviceInformation == null || deviceInfo.Id == sourceInfo.DeviceInformation.Id)));
            var CameraList = validColorVideoGroupList.ToList();

            if (CameraList.Count >= 1)
            {
                return CameraList[0].SourceInfos[0].DeviceInformation.Id;

            }
            return "";
        }


        public void ListAllAvailableProperties()
        {



        }
    }
}
