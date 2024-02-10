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

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CSharp.Core.Validators;

/// <summary>
/// Add to an MVVM string property bound to a TextBox.
/// </summary>
public class HexStringAttribute : ValidationAttribute
{
    override protected ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var stringValue = value as string;
        if (string.IsNullOrEmpty(stringValue))
            return new ValidationResult("Address is required.");

        var regex = new Regex("^[0-9A-Fa-f]{4}$");
        return regex.IsMatch(stringValue) ? ValidationResult.Success : new ValidationResult("Invalid hex address.");
    }
}
