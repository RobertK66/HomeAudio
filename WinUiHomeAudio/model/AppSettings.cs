using System;
using System.Reflection;
using Windows.Storage;

namespace WinUiHomeAudio.model {
    public class AppSettings {

        private const string c_IsLeftMode = "NavView_IsLeftMode";
        private const string c_UiTheme = "App_Theme";
        private const string c_ReposPath = "Repos_Path";
        private const string c_AutoConnect = "Auto_Connect";
        private const string c_AppId = "App_Id";

        private string _AppId = (ApplicationData.Current.LocalSettings.Values[c_AppId]  as string) ?? "46C1A819";
        public string AppId {
            get { return _AppId; }
            set { if (!Object.Equals(_AppId, value)) { _AppId = value; ApplicationData.Current.LocalSettings.Values[c_AppId] = value; } }
        }

        private string? _AutoConnectName = (ApplicationData.Current.LocalSettings.Values[c_AutoConnect] as string)??"My JBL";
        public string? AutoConnectName {
            get { return _AutoConnectName; }
            set { if (!Object.Equals(_AutoConnectName, value)) { _AutoConnectName = value; ApplicationData.Current.LocalSettings.Values[c_AutoConnect] = value; } }
        }

        private string _ReposPath = (ApplicationData.Current.LocalSettings.Values[c_ReposPath] as string) ?? ApplicationData.Current.LocalFolder.Path;
        public string ReposPath {
            get { return _ReposPath; }
            set { if (!Object.Equals(_ReposPath, value)) { _ReposPath = value; ApplicationData.Current.LocalSettings.Values[c_ReposPath] = value; } }
        }


        private string _UiTheme = (ApplicationData.Current.LocalSettings.Values[c_UiTheme] as string) ?? "Default";
        public string UiTheme {
            get { return _UiTheme; }
            set { if (!Object.Equals(_UiTheme, value)) { _UiTheme = value; ApplicationData.Current.LocalSettings.Values[c_UiTheme] = value; } }
        }

        private bool _IsLeftMode = (ApplicationData.Current.LocalSettings.Values[c_IsLeftMode] as bool?) ?? true;
        public bool IsLeftMode {
            get { return _IsLeftMode; }
            set { if (!Object.Equals(_IsLeftMode, value)) { _IsLeftMode = value; ApplicationData.Current.LocalSettings.Values[c_IsLeftMode] = value; } }
        }



        public TEnum GetEnum<TEnum>(string text) where TEnum : struct {
            if (!typeof(TEnum).GetTypeInfo().IsEnum) {
                throw new InvalidOperationException("Generic parameter 'TEnum' must be an enum.");
            }
            return (TEnum)Enum.Parse(typeof(TEnum), text);
        }

    }
}