using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Window_Hostage.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((WindowInfo.Status) value)
            {
                case WindowInfo.Status.Hidden:
                    return Brushes.Red;

                case WindowInfo.Status.Visible:
                    return Brushes.LimeGreen;

                case WindowInfo.Status.None:
                    return Brushes.Yellow;

                default:
                    throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}