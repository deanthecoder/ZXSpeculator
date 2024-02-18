// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using CSharp.Core.ViewModels;

namespace Speculator.Converters;

/// <summary>
/// Sanitize the ROM file name so it doesn't have all the bracketed
/// info at the end of it.
/// E.g. 'Commando (1985)(Elite Systems)[a2]' -> 'Commando'
/// </summary>
public class RomFileToNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var fileName = (value as MruFiles.SingleItem)?.ToString();
        if (string.IsNullOrEmpty(fileName))
            return null;

        int i;
        while ((i = fileName.IndexOfAny(new []{ '(', '[' })) > 0)
            fileName = fileName.Substring(0, i);

        return fileName.Trim();
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}