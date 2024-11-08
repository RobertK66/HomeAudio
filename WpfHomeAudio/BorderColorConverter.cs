using System;
using System.Collections.Generic;
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
            if ( firstValue && secondValue) {
                scb = new SolidColorBrush(Color.FromRgb(0, 200, 0));
            } 
            return scb;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException("Going back to what you had isn't supported.");
        }
    }
}
