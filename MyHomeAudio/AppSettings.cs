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


        public AppSettings() {
            _AppId = (string)ApplicationData.Current.LocalSettings.Values[AppSettingKeys.AppId];
            if (string.IsNullOrEmpty(_AppId)) {
                _AppId = AppSetting.DefaultAppId;
            }

            _ReposPath = (string)ApplicationData.Current.LocalSettings.Values[AppSettingKeys.ReposPath];
            if (string.IsNullOrEmpty(_ReposPath)) {
                _ReposPath = ApplicationData.Current.LocalFolder.Path;
            }

            AutoConnectName = (string)ApplicationData.Current.LocalSettings.Values[AppSettingKeys.AutoConnect];


        }
    }
}
