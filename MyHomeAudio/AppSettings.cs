using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyHomeAudio {
    public class AppSettings {

        private string _AppId;
        public string AppId { get { return _AppId; } 
                              set { if (!Object.Equals(_AppId, value)) { _AppId = value; ApplicationData.Current.LocalSettings.Values[AppSettingKeys.AppId] = value; } } }


        private string? _AutoConnectName;
        public string? AutoConnectName {
            get { return _AutoConnectName; }
            set { if (!Object.Equals(_AutoConnectName, value)) { _AutoConnectName = value; ApplicationData.Current.LocalSettings.Values[AppSettingKeys.AutoConnect] = value; } }
        }

        private string _ReposPath;
        public string ReposPath {
            get { return _ReposPath; }
            set { if (!Object.Equals(_ReposPath, value)) { _ReposPath = value; ApplicationData.Current.LocalSettings.Values[AppSettingKeys.ReposPath] = value; } }
        }

        private string _UiTheme;
        public string UiTheme {
            get { return _UiTheme; }
            set { if (!Object.Equals(_UiTheme, value)) { _UiTheme = value; ApplicationData.Current.LocalSettings.Values[AppSettingKeys.UiTheme] = value; } }
        }

        private bool _IsLeftMode;
        public bool IsLeftMode {
            get { return _IsLeftMode; }
            set { if (!Object.Equals(_IsLeftMode, value)) { _IsLeftMode = value; ApplicationData.Current.LocalSettings.Values[AppSettingKeys.IsLeftMode] = value; } }
        }

        public AppSettings() {
            _AutoConnectName = (ApplicationData.Current.LocalSettings.Values[AppSettingKeys.AutoConnect] as string);

            _AppId = (ApplicationData.Current.LocalSettings.Values[AppSettingKeys.AppId] as string) ?? AppSetting.DefaultAppId;
            if (String.IsNullOrWhiteSpace(_AppId)) {    // Overwrite Empty strings with default again
                AppId = AppSetting.DefaultAppId;
            }
            _ReposPath = (ApplicationData.Current.LocalSettings.Values[AppSettingKeys.ReposPath] as string) ?? ApplicationData.Current.LocalFolder.Path;
            _UiTheme = (ApplicationData.Current.LocalSettings.Values[AppSettingKeys.UiTheme] as string) ?? AppSetting.DefaultUiTheme;
            _IsLeftMode = (ApplicationData.Current.LocalSettings.Values[AppSettingKeys.IsLeftMode] as bool?) ?? AppSetting.DefaultIsLeftMode;
        }
    }
}
