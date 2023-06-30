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
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI;
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
        private Dictionary<ExtendedControlKind, IExtendedPropertyPayload> m_extendedControls = new Dictionary<ExtendedControlKind, IExtendedPropertyPayload>();

        public ExtendedCameraControlPage()
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
                    ExtendedControlKind[] properties = (ExtendedControlKind[])Enum.GetValues(typeof(ExtendedControlKind));
                    foreach (var p in properties)
                    { 
                        IExtendedPropertyPayload payload = null;
                        if (m_extendedControls.TryGetValue(p, out payload))
                        {
                            if (payload == null)
                            {
                                continue;
                            }
                            ComboBoxItem comboBoxItem = new ComboBoxItem(); 
                            comboBoxItem.Content = p;
                            comboBoxItem.Tag = (int)p; 
                            DropDownList.Items.Add(comboBoxItem);
                        }

                    }
                    DropDownList.Visibility = Visibility.Visible;

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

        private void DropDownList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_mediaCapture == null || m_mediaCapture.VideoDeviceController == null)
            {
                var ignore = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    UITextOutput.Text = $"error:mediaCapture not available";
                });
                return ;
            }

            var comboBoxItem = DropDownList.SelectedItem as ComboBoxItem;
            ExtendedControlKind kind = (ExtendedControlKind)comboBoxItem.Tag;
            try {
                var getPayload = PropertyInquiry.GetExtendedControl(m_mediaCapture.VideoDeviceController, kind);
                UITextOutput.Text = PayloadtoString(getPayload);

            }
            catch (Exception ex)
            {
                UITextOutput.Text = $"error:" + ex.Message;
            }

        }

        private string PayloadtoString(IExtendedPropertyPayload payload)
        {
            string outputstring = payload.ExtendedControlKind + ":\n";
            var cap = Convert.ToString((long)payload.Capability, 2).PadLeft(16, '0');
            var flag = Convert.ToString((long)payload.Flags, 2).PadLeft(16, '0');
            outputstring += "Capability:  0x" + cap + "\n" + "Flags:    0x" + flag + "\n";
            switch (payload.ExtendedControlKind)
            {
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_PHOTOMODE:
                    
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_PHOTOMODE 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_PHOTOFRAMERATE:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_PHOTOFRAMERATE 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_PHOTOMAXFRAMERATE:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_PHOTOMAXFRAMERATE 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_PHOTOTRIGGERTIME:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_PHOTOTRIGGERTIME 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_WARMSTART:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_WARMSTART 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_MAXVIDFPS_PHOTORES:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_MAXVIDFPS_PHOTORES 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_PHOTOTHUMBNAIL:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_PHOTOTHUMBNAIL 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_SCENEMODE:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_SCENEMODE 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_TORCHMODE:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_TORCHMODE 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_FLASHMODE:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_FLASHMODE 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_OPTIMIZATIONHINT:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_OPTIMIZATIONHINT 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_WHITEBALANCEMODE:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_WHITEBALANCEMODE 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_EXPOSUREMODE:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_EXPOSUREMODE 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_FOCUSMODE:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_FOCUSMODE 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_ISO:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_ISO 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_FIELDOFVIEW:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_FIELDOFVIEW 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_EVCOMPENSATION:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_EVCOMPENSATION 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_CAMERAANGLEOFFSET:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_CAMERAANGLEOFFSET 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_METADATA:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_METADATA 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_FOCUSPRIORITY:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_FOCUSPRIORITY 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_FOCUSSTATE:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_FOCUSSTATE 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_ROI_CONFIGCAPS:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_ROI_CONFIGCAPS 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_ROI_ISPCONTROL:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_ROI_ISPCONTROL 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_PHOTOCONFIRMATION:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_PHOTOCONFIRMATION 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_ZOOM:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_ZOOM 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_MCC:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_MCC 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_ISO_ADVANCED:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_ISO_ADVANCED 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_VIDEOSTABILIZATION:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_VIDEOSTABILIZATION 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_VFR:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_VFR 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_FACEDETECTION:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_FACEDETECTION 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_VIDEOHDR:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_VIDEOHDR 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_HISTOGRAM:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_HISTOGRAM 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_OIS:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_OIS 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_ADVANCEDPHOTO:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_ADVANCEDPHOTO 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_PROFILE:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_PROFILE 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_FACEAUTH_MODE:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_FACEAUTH_MODE 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_SECURE_MODE:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_SECURE_MODE 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_VIDEOTEMPORALDENOISING:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_VIDEOTEMPORALDENOISING 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_IRTORCHMODE:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_IRTORCHMODE 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_RELATIVEPANELOPTIMIZATION:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_RELATIVEPANELOPTIMIZATION 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_EYEGAZECORRECTION:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_EYEGAZECORRECTION 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_BACKGROUNDSEGMENTATION:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_BACKGROUNDSEGMENTATION 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_DIGITALWINDOW_CONFIGCAPS:

                    var caps = payload as DigitalWindowConfigCapsPayload;
                    foreach (var conf in caps.ConfigCaps)
                    {
                        outputstring +=String.Format("ResolutionX:{0}\nResolutionY:{1}\nPorchLeft:{2}\nPorchRight:{3}\nPorchTop:{4}\nPorchBottom:{5}\nNonUpscalingWindowSize:{6}\n\n", 
                            conf.ResolutionX ,conf.ResolutionY,conf.PorchLeft,conf.PorchRight, conf.PorchTop, conf.PorchBottom,conf.NonUpscalingWindowSize);
                        

                    }
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_DIGITALWINDOW_CONFIGCAPS 的逻辑
                    break;
                case ExtendedControlKind.KSPROPERTY_CAMERACONTROL_EXTENDED_DIGITALWINDOW:
                    // 处理 KSPROPERTY_CAMERACONTROL_EXTENDED_DIGITALWINDOW 的逻辑
                    break;
                default:
                    // 默认情况下的逻辑
                    break;


            }
            return outputstring;
        }
    }
}
