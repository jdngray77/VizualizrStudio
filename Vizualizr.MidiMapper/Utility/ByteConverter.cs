using System.Globalization;

namespace Vizualizr.MidiMapper.Utility
{
    public class ByteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (byte.TryParse(value?.ToString(), out var result))
                return result;

            return 0;
        }
    }
}
