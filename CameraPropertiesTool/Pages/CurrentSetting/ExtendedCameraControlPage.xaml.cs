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
            Init();

        }

        public void Init()
        { 
            //1. list all camera
            //2. load default camera
            //3. List all Properties
            //4. show each property ui
        
        }
        public async Task<string> GetCameraIDAsync()
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
