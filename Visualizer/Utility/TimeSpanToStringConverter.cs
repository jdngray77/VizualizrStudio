using System.Globalization;

namespace Visualizer.Utility
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan time)
            {
                if (time.Hours > 0)
                    return time.ToString(@"hh\:mm\:ss\.fff");
                else
                    return time.ToString(@"mm\:ss\.fff");
            }
            return "00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
