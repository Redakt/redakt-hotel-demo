using System;
using System.ComponentModel.DataAnnotations;

namespace RedaktHotel.BackOfficeExtensions.Annotations
{
    /// <summary>
    /// This is an example of a custom property validator attribute. Any <see cref="ValidationAttribute"/> derived attribute can be used to decorate Redakt view model properties.
    /// </summary>
    public class MinCurrentDateAttribute: ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null || value.GetType() != typeof(DateTime)) return true;  // Only validate DateTime types.

            return (DateTime) value >= DateTime.Today;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} cannot be a date in the past.";
        }
    }
}
