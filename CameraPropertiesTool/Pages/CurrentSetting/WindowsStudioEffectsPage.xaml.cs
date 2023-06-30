using CameraKsPropertyHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture.Frames;
using Windows.Media.Capture;
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
    public sealed partial class WindowsStudioEffectsPage : Page
    {
        string cameraId = null;
        private MediaCapture m_mediaCapture = null;
        private MediaPlayer m_mediaPlayer = null;
        private Dictionary<ExtendedControlKind, IExtendedPropertyPayload> m_extendedControls = new Dictionary<ExtendedControlKind, IExtendedPropertyPayload>();

        public WindowsStudioEffectsPage()
        {
            this.InitializeComponent();
            InitAsync();
        }
     
       
        public async void InitAsync()
        {
            //1. list all camera  -- TBD

            //2. load default camera 
            if (!await LoadDefaultCameraAsync()) return;

            //3. List all Properties
            if (!await ListAllPropertiesAsync()) return;

            //4. show each property ui
            if (!await ShowAllSwitchsAsync()) return;


        }

        public async Task<bool> LoadDefaultCameraAsync()
        {

            cameraId = await GetCameraIDAsync();
            if (string.IsNullOrEmpty(cameraId))
            {
                var ignore = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    SelectedCameraTB.Text = "Camera Not Found";
                });
                return false;
            }
            await CameraInitializeAsync();

            return true;
        }

        private async Task<bool> CameraInitializeAsync()
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
                return false;
            }
            return true;

        }

        private void TextOutput(string msg)
        {
            var ignore = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                UITextOutput.Text = $"error: {msg}";
            });
        }
        private async Task<bool> ShowAllSwitchsAsync()
        {
            if (m_extendedControls.Count == 0)
            {
                return false;
            }

            // Updating UI elements
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {

                {
                    DefaultBlurToggle.Toggled -= DefaultBlurToggle_Toggled;
                    DefaultBlurToggle.Visibility = Visibility.Visible;
                    IExtendedPropertyPayload payload = null;
                    if (m_extendedControls.TryGetValue(ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_BACKGROUNDSEGMENTATION, out payload) && payload != null)
                    {
                        DefaultBlurToggle.IsOn = (payload.Flags != 0);
                    }
                    else
                    {
                        DefaultBlurToggle.IsEnabled = false;
                    }

                    DefaultBlurToggle.Toggled += DefaultBlurToggle_Toggled;
                }

                {
                    DefaultECToggle.Toggled -= DefaultECToggle_Toggled;
                    DefaultECToggle.Visibility = Visibility.Visible;
                    IExtendedPropertyPayload payload = null;
                    if (m_extendedControls.TryGetValue(ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_EYEGAZECORRECTION, out payload) && payload != null)
                    {
                        DefaultECToggle.IsOn = (payload.Flags != 0);
                    }
                    else
                    {
                        DefaultECToggle.IsEnabled = false;
                    }

                    DefaultECToggle.Toggled += DefaultBlurToggle_Toggled;
                }
                {
                    DefaultAFToggle.Toggled -= DefaultECToggle_Toggled;
                    DefaultAFToggle.Visibility = Visibility.Visible;
                    IExtendedPropertyPayload payload = null;
                    if (m_extendedControls.TryGetValue(ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_DIGITALWINDOW, out payload) && payload != null)
                    {
                        DefaultAFToggle.IsOn = (payload.Flags != 0);
                    }
                    else
                    {
                        DefaultAFToggle.IsEnabled = false;
                    }

                    DefaultAFToggle.Toggled += DefaultBlurToggle_Toggled;
                }


            });

            return true;


        }

        private async Task<bool> ListAllPropertiesAsync()
        {
            if (m_mediaCapture == null || m_mediaCapture.VideoDeviceController == null)
            {
                var ignore = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    UITextOutput.Text = $"error:mediaCapture not available";
                });
                return false;
            }

            m_extendedControls.Clear();

            ExtendedControlKind[] properties = (ExtendedControlKind[])Enum.GetValues(typeof(ExtendedControlKind));

            var output = "Available properties:\n";
            foreach (ExtendedControlKind p in properties)
            {
                bool isControlSupported = false;
                IExtendedPropertyPayload getPayload = null;
                try
                {
                    getPayload = PropertyInquiry.GetExtendedControl(m_mediaCapture.VideoDeviceController, p);
                }
                catch (ArgumentException)
                {
                    continue;
                }
                isControlSupported = (getPayload != null);
                m_extendedControls.Add(p, getPayload);
                if (isControlSupported == true)
                {
                    output += p.ToString() + "\n";
                }
            }

            {
                var ignore = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    UITextOutput.Text = output;
                });
            }





            return true;
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





        private void DefaultAFToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                int flags = (int)((DefaultAFToggle.IsOn == true) ? AutoFramingCapabilityKind.KSCAMERA_EXTENDEDPROP_DIGITALWINDOW_AUTOFACEFRAMING : AutoFramingCapabilityKind.KSCAMERA_EXTENDEDPROP_DIGITALWINDOW_MANUAL);

                PropertyInquiry.SetExtendedControlFlags(m_mediaCapture.VideoDeviceController, ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_DIGITALWINDOW, (uint)flags);
            }
            catch (Exception ex)
            {
                UITextOutput.Text = $"error: {ex.Message}";
            }

        }


        private void DefaultBlurToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                //int flags = (int)((DefaultBlurToggle.IsOn == true) ? BackgroundSegmentationCapabilityKind.KSCAMERA_EXTENDEDPROP_BACKGROUNDSEGMENTATION_BLUR : BackgroundSegmentationCapabilityKind.KSCAMERA_EXTENDEDPROP_BACKGROUNDSEGMENTATION_BLUR | BackgroundSegmentationCapabilityKind.KSCAMERA_EXTENDEDPROP_BACKGROUNDSEGMENTATION_SHALLOWFOCUS);
                int flags = (int)((DefaultBlurToggle.IsOn == true) ? BackgroundSegmentationCapabilityKind.KSCAMERA_EXTENDEDPROP_BACKGROUNDSEGMENTATION_BLUR : BackgroundSegmentationCapabilityKind.KSCAMERA_EXTENDEDPROP_BACKGROUNDSEGMENTATION_OFF);
                PropertyInquiry.SetExtendedControlFlags(m_mediaCapture.VideoDeviceController, ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_BACKGROUNDSEGMENTATION, (uint)flags);
            }
            catch (Exception ex)
            {
                UITextOutput.Text = $"error: {ex.Message}";
            }
        }

        private void DefaultECToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                int flags = (int)((DefaultECToggle.IsOn == true) ? EyeGazeCorrectionCapabilityKind.KSCAMERA_EXTENDEDPROP_EYEGAZECORRECTION_ON : EyeGazeCorrectionCapabilityKind.KSCAMERA_EXTENDEDPROP_EYEGAZECORRECTION_OFF);

                PropertyInquiry.SetExtendedControlFlags(m_mediaCapture.VideoDeviceController, ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_EYEGAZECORRECTION, (uint)flags);
            }
            catch (Exception ex)
            {
                UITextOutput.Text = $"error: {ex.Message}";
            }
        }

        private void DefaultEVCompSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

        }
    }
}
