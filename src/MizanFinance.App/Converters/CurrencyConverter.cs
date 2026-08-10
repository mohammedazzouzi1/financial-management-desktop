using System.Globalization;
using System.Windows.Data;

namespace MizanFinance.App.Converters;

public class CurrencyConverter : IValueConverter
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("fr-FR");

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var amount = value switch
        {
            decimal d => d,
            double db => (decimal)db,
            _ => 0m
        };
        return string.Format(Culture, "{0:N2} MAD", amount);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
