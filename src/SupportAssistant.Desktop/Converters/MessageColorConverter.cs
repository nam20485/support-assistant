using Avalonia.Data.Converters;
using Avalonia.Media;
using SupportAssistant.Core.Models;
using System.Globalization;

namespace SupportAssistant.Desktop.Converters;

public class MessageColorConverter : IValueConverter
{
    public static readonly MessageColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MessageSender sender)
        {
            return sender switch
            {
                MessageSender.User => Colors.LightBlue,
                MessageSender.Assistant => Colors.LightGreen,
                MessageSender.System => Colors.LightYellow,
                _ => Colors.White
            };
        }

        return Colors.White;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}