using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUiHomeAudio.model;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUiHomeAudio {
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window {
        public MainPage MainPage { get { return this.myMainPage; } }


        public MainWindow() {
            this.InitializeComponent();

            var appSettings = App.Host.Services.GetRequiredService<AppSettings>();

            if (appSettings.IsLeftMode) {
                MainPage.MainNavPane.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
            } else {
                MainPage.MainNavPane.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
            }

            var t = appSettings.GetEnum<ElementTheme>(appSettings.UiTheme);
            if (this.Content is FrameworkElement rootElement) {
                rootElement.RequestedTheme = t;
            }

        }


    }
}
