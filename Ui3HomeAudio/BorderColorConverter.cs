using AudioCollectionApi.api;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using Windows.UI;

namespace Ui3HomeAudio {


    public class BorderColorConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, string language) {
            IPlayerProxy? record = (parameter as TextBox)?.DataContext as IPlayerProxy;
            if (record != null) {
                return ConvertFromIPlayerProxy(record);
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException("nicht in diese richtung :-)");
        }

        private SolidColorBrush ConvertFromIPlayerProxy(IPlayerProxy value) {
            var c = App.Current.Resources["SystemControlErrorTextForegroundBrush"];
            SolidColorBrush scb = (App.Current.Resources["SystemErrorTextColor"] as SolidColorBrush)??new SolidColorBrush(Colors.Red);
            if (value.IsOn & value.IsConnected) {
                scb = (App.Current.Resources["SystemFillColorSuccess"] as SolidColorBrush) ?? new SolidColorBrush(Colors.Green);
            }
            return scb;
        }


    }
}
