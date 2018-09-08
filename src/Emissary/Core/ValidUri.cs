using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Emissary.Core
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ValidUri : ValidationAttribute
    {
        public ValidUri() : base("The value for '{0}' is not a valid URI.")
        {

        }

        public override bool IsValid(object value)
        {
            var valid = value is string str
                        && Uri.IsWellFormedUriString(str, UriKind.Absolute);

            return valid;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name);
        }
    }
}