using System.Globalization;

namespace Vizualizr.Utility
{
    public class BpmDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string bpm && bpm != "0")
            {
                return $"{bpm:F1} BPM";
            }
            return "? BPM";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
