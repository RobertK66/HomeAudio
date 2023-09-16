using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
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

namespace MyHomeAudio {
    public sealed partial class CcPlayer : UserControl {
        public CcPlayer() {
            this.InitializeComponent();
        }

        public event EventHandler VolumeUp;
        public event EventHandler VolumeDown;

        public string PlayerName {
            get { return (string)GetValue(PlayerNameProperty); }
            set { SetValue(PlayerNameProperty, value); }
        }
        public static readonly DependencyProperty PlayerNameProperty =
            DependencyProperty.Register(
                "PlayerName",
                typeof(string),
                typeof(CcPlayer),
                new PropertyMetadata(null));

        public string Status {
            get { return (string)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(
                "Status",
                typeof(string),
                typeof(CcPlayer),
                new PropertyMetadata(null));

        public int Volume {
            get { return (int)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }
        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register(
                "Volume",
                typeof(int),
                typeof(CcPlayer),
                new PropertyMetadata(null));

        public string AppId {
            get { return (string)GetValue(AppIdProperty); }
            set { SetValue(AppIdProperty, value); }
        }
        public static readonly DependencyProperty AppIdProperty =
            DependencyProperty.Register(
                "AppId",
                typeof(string),
                typeof(CcPlayer),
                new PropertyMetadata(null));

        public string MediaStatus {
            get { return (string)GetValue(MediaStatusProperty); }
            set { SetValue(MediaStatusProperty, value); }
        }
        public static readonly DependencyProperty MediaStatusProperty =
            DependencyProperty.Register(
                "MediaStatus",
                typeof(string),
                typeof(CcPlayer),
                new PropertyMetadata(null));

        private void VolDown_Click(object sender, RoutedEventArgs e) {
            VolumeDown?.Invoke(this, EventArgs.Empty);
        }

        private void VolUp_Click(object sender, RoutedEventArgs e) {
            VolumeUp?.Invoke(this, EventArgs.Empty);
        }
    }
}
