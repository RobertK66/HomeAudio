using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
using WinGuiPackaged.logger;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinGuiPackaged {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page {
        private MainViewModel myModel = null;


        public SettingsPage() {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            myModel = e.Parameter as MainViewModel;
//            loggerVm = myModel?.LogWindowViewModel;
  //          InitializeComponent();
            //Bindings.Initialize();
            base.OnNavigatedTo(e);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            _ = myModel.CheckAndConnectChromecast();
        }
    }
}
