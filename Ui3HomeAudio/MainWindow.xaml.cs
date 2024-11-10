using HomeAudioViewModel;
using Microsoft.Extensions.Logging;
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
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Ui3HomeAudio {
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window {

        private readonly ILogger _logger;

        public MainWindow(ILogger<MainWindow> log) {
            _logger = log;
            this.InitializeComponent();
            //this.mainPage.DataContext = vm;

            //new Thread(() => _ = vm.LoadReposAsync()) { IsBackground = true }.Start();
            ////_ = vm.LoadReposAsync();

            _logger.LogDebug("Main window constructor finished....");
        }

   
    }
}
