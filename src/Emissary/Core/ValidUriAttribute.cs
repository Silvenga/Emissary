using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace Emissary.Core
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ValidUriAttribute : ValidationAttribute
    {
        public string[] AllowedSchemas { get; set; } = {
            "http",
            "https",
            "tcp",
            "unix",
            "npipe",
        };

        public ValidUriAttribute() : base("The value for '{0}' is not a valid URI. Ensure that the URI schema is one of {1}.")
        {
        }

        public override bool IsValid(object value)
        {
            var valid = value is string str
                        && Uri.TryCreate(str, UriKind.Absolute, out var uri)
                        && AllowedSchemas.Any(x => x == uri.Scheme);

            return valid;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, string.Join(", ", AllowedSchemas.Select(x => $"'{x}'")));
        }
    }
}