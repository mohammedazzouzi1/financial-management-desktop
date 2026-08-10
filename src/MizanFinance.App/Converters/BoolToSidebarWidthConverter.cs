using System.Globalization;
using System.Windows.Data;

namespace MizanFinance.App.Converters;

public class BoolToSidebarWidthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool open && open ? 230d : 64d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
