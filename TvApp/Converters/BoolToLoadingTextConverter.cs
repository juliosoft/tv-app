using System.Globalization;
using System.Windows.Data;

namespace TvApp.Converters;

public class BoolToLoadingTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => (value is true) ? "Carregando canais..." : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
