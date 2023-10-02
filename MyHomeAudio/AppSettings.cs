using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyHomeAudio {
    public class AppSettings {
        public string AppId { get; set; }
        public string? AutoConnectName { get; set; }

        public AppSettings() {

            AppId = (string)ApplicationData.Current.LocalSettings.Values[AppSettingKeys.AppId];
            if (string.IsNullOrEmpty(AppId)) {
                AppId = AppSetting.DefaultAppId;
            }

            AutoConnectName = (string)ApplicationData.Current.LocalSettings.Values[AppSettingKeys.AutoConnect];


        }
    }
}
