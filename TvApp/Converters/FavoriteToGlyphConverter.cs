using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TvApp.Converters;

/// <summary>Cor do icone de estrela: dourado quando favorito, cinza quando nao.</summary>
public class FavoriteToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => (value is true)
            ? new SolidColorBrush(Color.FromRgb(0xF5, 0xC5, 0x1D))
            : new SolidColorBrush(Color.FromRgb(0x5A, 0x62, 0x72));

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
