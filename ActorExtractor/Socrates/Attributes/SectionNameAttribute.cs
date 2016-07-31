using Socrates.ValueTypes;
using System;

namespace Socrates.Attributes
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class SectionNameAttribute : Attribute
    {
        readonly Quad quad;

        public SectionNameAttribute(string quad)
        {
            this.quad = (Quad)quad;
        }

        public Type OwnerType { get; internal set; }

        public Quad Quad
        {
            get { return quad; }
        }
    }
}
