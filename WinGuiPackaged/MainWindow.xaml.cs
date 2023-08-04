using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinGuiPackaged {
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window {
        public MainViewModel AnyViewModel { get; set; }

        public MainWindow() {
            this.InitializeComponent();
            //AnyViewModel = new MainViewModel();
        }

     


        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
                if (args.IsSettingsInvoked) {
                    ContentFrame.Navigate(typeof(SettingsPage), AnyViewModel);
                } else {
                    //TextBlock ItemContent = args.InvokedItem as TextBlock;
                    //if (ItemContent != null) {
                        switch (args.InvokedItem) {
                            case "Radios":
                                ContentFrame.Navigate(typeof(RadioPage), AnyViewModel);
                                break;

                        case "CDs - 1":
                                ContentFrame.Navigate(typeof(CdPage), AnyViewModel);
                                break;

                        case "CDs - 2":
                                ContentFrame.Navigate(typeof(CdPage), AnyViewModel);
                                break;

                }
                //}
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            _ = AnyViewModel.CheckAndConnectChromecast();
        }

    }

    
}
