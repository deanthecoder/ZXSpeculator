using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CSharp.Utils.Validators;

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