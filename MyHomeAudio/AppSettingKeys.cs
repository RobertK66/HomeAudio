using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHomeAudio {
    internal class AppSettingKeys {
        internal const String IsLeftMode = "NavView_IsLeftMode";
        internal const String UiTheme = "App_Theme";
        internal const String ReposPath = "Repos_Path";
        internal const String AutoConnect = "Auto_Connect";
        internal const String AppId = "App_Id";

    }

    internal class AppSetting {
        internal static string DefaultAppId = "CC1AD845";    //Default Nedia Player
        internal static string DefaultUiTheme = "Default";
        internal static bool DefaultIsLeftMode = true;
    }
}
