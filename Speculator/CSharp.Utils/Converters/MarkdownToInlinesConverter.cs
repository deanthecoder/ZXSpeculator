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

        // Colorize addresses in ROM.
        var colorizedRuns = new InlineCollection();
        foreach (var inline in inlines)
        {
            // Find a leading ABCD: hex prefix.
            if (inline is not Run { Text.Length: >= 4 } run || !run.Text.Take(4).All(char.IsAsciiHexDigit))
            {
                // Not hex.
                colorizedRuns.Add(inline);
                continue;
            }

            // See if the address maps to the ROM region.
            var hexString = run.Text.Substring(0, 4);
            if (!int.TryParse(hexString, NumberStyles.HexNumber, null, out var hexValue) || hexValue >= 16384)
            {
                // Not in ROM.
                colorizedRuns.Add(inline);
                continue;
            }
            
            // We're in the ROM - colorize the address.
            var coloredRun = new Run(hexString)
            {
                Foreground = Brushes.Cyan
            };
            run.Text = run.Text.Substring(4);

            colorizedRuns.Add(coloredRun);
            colorizedRuns.Add(run);
        }

        return colorizedRuns;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}