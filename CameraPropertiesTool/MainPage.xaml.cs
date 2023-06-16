using CameraPropertiesTool.Pages.CurrentSetting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CameraPropertiesTool
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            NavView.SelectedItem = NavView.MenuItems[0];
            NavView.ItemInvoked += NavView_ItemInvoked;
        }
        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                // 处理设置菜单项的点击事件
                // TODO: 添加处理设置的逻辑
            }
            else
            {
                var item = args.InvokedItemContainer as NavigationViewItem;
                if (item != null)
                {
                    switch (item.Content)
                    {
                        //case "Page1":
                        //    MainFrame.Navigate(typeof(Page1));
                        //    break;
                        //case "Page2":
                        //    MainFrame.Navigate(typeof(Page2));
                        //    break;
                        case "ExtendedCameraControl":
                            MainFrame.Navigate(typeof(ExtendedCameraControlPage));
                            break;
                            // 添加更多页面的导航逻辑
                    }
                }
            }
        }
    }
}
