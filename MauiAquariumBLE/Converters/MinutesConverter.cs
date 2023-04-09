using System.Globalization;
using static System.Net.Mime.MediaTypeNames;

namespace MauiAquariumBLE.Converters
{
    public class MinutesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int minutes)
            {
                return $"{minutes} min.";
            }
            else if (value is string text)
            {
                if (int.TryParse(text.Replace(" min.", ""), out int minutes2))
                {
                    return minutes2;
                }
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int minutes)
            {
                return $"{minutes} min.";
            } else if (value is string text)
            {
                if (int.TryParse(text.Replace(" min.", ""), out int minutes2))
                {
                    return minutes2;
                }
            }
            return value;
        }
    }

}
