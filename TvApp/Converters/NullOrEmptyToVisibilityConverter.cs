using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TvApp.Converters;

/// <summary>
/// Visible quando o valor NAO e nulo/vazio; Collapsed quando e nulo/vazio (string vazia tambem conta como "vazio").
/// Passe ConverterParameter="Invert" para inverter a logica (util para placeholders que aparecem quando o valor E nulo).
/// </summary>
public class NullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var isEmpty = value switch
        {
            null => true,
            string s => string.IsNullOrWhiteSpace(s),
            _ => false
        };

        var invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
        if (invert) isEmpty = !isEmpty;

        return isEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
