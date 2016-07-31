using System;
using System.ComponentModel.DataAnnotations;

namespace ActorExtractor.Validation
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class MinimumAttribute : ValidationAttribute
    {
        readonly double minimumValue;

        public MinimumAttribute(double minimumValue)
        {
            this.minimumValue = minimumValue;
        }

        public double MinimumValue
        {
            get { return minimumValue; }
        }

        public override bool IsValid(object value)
        {
            return Convert.ToDouble(value) >= MinimumValue;
        }
    }
}
