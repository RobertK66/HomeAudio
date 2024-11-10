using AudioCollectionApi.api;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfHomeAudio {
    public class BorderColorConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            bool firstValue = (bool)values[0];
            bool secondValue = (bool)values[1];

            SolidColorBrush scb = new SolidColorBrush(Color.FromRgb(200, 0, 0));
            if (firstValue && secondValue) {
                scb = new SolidColorBrush(Color.FromRgb(0, 200, 0));
            }
            return scb;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException("Going back to what you had isn't supported.");
        }
    }
    //public class BorderColorConverter : IValueConverter {


    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    //        IPlayerProxy? record = (value as IPlayerProxy);
    //        if (record != null) {
    //            return ConvertFromIPlayerProxy(record);
    //        }
    //        return new SolidColorBrush(Colors.Black);
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string language) {
    //        throw new NotImplementedException("nicht in diese richtung :-)");
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    //        throw new NotImplementedException();
    //    }

    //    private SolidColorBrush ConvertFromIPlayerProxy(IPlayerProxy value) {
    //        var c = App.Current.Resources["SystemControlErrorTextForegroundBrush"];
    //        SolidColorBrush scb = (App.Current.Resources["SystemErrorTextColor"] as SolidColorBrush) ?? new SolidColorBrush(Colors.Red);
    //        if (value.IsOn & value.IsConnected) {
    //            scb = (App.Current.Resources["SystemFillColorSuccess"] as SolidColorBrush) ?? new SolidColorBrush(Colors.Green);
    //        }
    //        return scb;
    //    }






}
