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
using Microsoft.UI;           // Needed for WindowId.
using Microsoft.UI.Windowing; // Needed for AppWindow.
using WinRT.Interop;          // Needed for XAML/HWND interop.
using Windows.UI;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyHomeAudio {
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
   

    public sealed partial class MainWindow : Window {

        private AppWindow m_AppWindow;
        public MainWindow() {
            this.InitializeComponent();
            AppWindow.Title = "My Audio - Cast Application";
            AppWindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
            AppWindow.TitleBar.BackgroundColor = Colors.Bisque;
            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Bisque;
        }

        private void myButton_Click(object sender, RoutedEventArgs e) {
            myButton.Content = "Clicked";
        }
    }
}