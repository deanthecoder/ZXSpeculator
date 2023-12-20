using System.Globalization;
using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace CSharp.Utils.Converters;

public class MarkdownToInlinesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var inlines = new InlineCollection();
        if (value is not string text)
            return inlines;
        var isBold = false;
        var lastPos = 0;

        for (var i = 0; i < text.Length; i++)
        {
            switch (text[i])
            {
                case '*':
                    inlines.Add(new Run(text.Substring(lastPos, i - lastPos))
                    {
                        FontWeight = isBold ? FontWeight.Bold : FontWeight.Normal
                    });
                    isBold = !isBold;
                    lastPos = i + 1;
                    break;
                case '\n':
                    inlines.Add(new Run(text.Substring(lastPos, i - lastPos))
                    {
                        FontWeight = isBold ? FontWeight.Bold : FontWeight.Normal
                    });
                    inlines.Add(new LineBreak());
                    lastPos = i + 1;
                    break;
            }
        }

        if (lastPos < text.Length)
        {
            inlines.Add(new Run(text.Substring(lastPos))
            {
                FontWeight = isBold ? FontWeight.Bold : FontWeight.Normal
            });
        }

        return inlines;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}