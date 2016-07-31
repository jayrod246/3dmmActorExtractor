using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ActorExtractor.Validation
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class ShortNameAttribute : ValidationAttribute
    {
        public ShortNameAttribute()
        {
        }

        public override bool IsValid(object value)
        {
            return (value as string)?.EndsWith(".") != true && (value as string)?.Any(c => (c < 'A' || c > 'Z') && (c < 'a' || c > 'z') && (c < '0' || c > '9') && c != '.' && c != '-' && c != '_') != true;
        }
    }
}
