using System;

namespace Socrates.ValueTypes
{
    /// <summary>
    /// A string that is constrained to 4 uppercase chars.
    /// </summary>
    public struct Quad : IComparable
    {
        private readonly string quadString;

        #region Constructors
        public Quad(char[] chars) : this(new string(chars))
        {
        }

        public Quad(string value)
        {
            value = value.ToUpperInvariant();
            if (value.Length != 4)
            {
                value = value.PadRight(4);
                value = value.Substring(0, 4);
            }
            for (int i = 0; i < 4; i++)
            {
                if ((value[i] < 'A' || value[i] > 'Z') && (value[i] < '0' || value[i] > '9') && value[i] != ' ')
                {
                    value = value.Remove(i, 1);
                    value = value.Insert(i, " ");
                }
            }
            quadString = value;
        }
        #endregion

        public char[] ToCharArray()
        {
            return quadString.ToCharArray();
        }

        #region Operators
        public static implicit operator string(Quad quad)
        {
            return quad.quadString;
        }

        public static explicit operator Quad(string str)
        {
            return new Quad(str);
        }

        public static explicit operator Quad(char[] chars)
        {
            return new Quad(chars);
        }

        public static bool operator ==(Quad a, Quad b)
        {
            return a.quadString == b.quadString;
        }

        public static bool operator !=(Quad a, Quad b)
        {
            return a.quadString != b.quadString;
        }
        #endregion

        #region Overrides
        public override int GetHashCode()
        {
            if (quadString == null)
                throw new InvalidOperationException("Calling GetHashCode on a NULL quad.");

            unchecked
            {
                int hash = 17;
                hash = hash * 31 + quadString[0].GetHashCode();
                hash = hash * 31 + quadString[1].GetHashCode();
                hash = hash * 31 + quadString[2].GetHashCode();
                hash = hash * 31 + quadString[3].GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return quadString;
        }

        public override bool Equals(object obj)
        {
            if (obj is Quad)
                return this == (Quad)obj;
            return base.Equals(obj);
        }
        #endregion

        #region IComparable Implementation
        public int CompareTo(object obj)
        {
            if (obj is Quad)
                return quadString.CompareTo(((Quad)obj).quadString);
            return quadString.CompareTo(obj);
        }
        #endregion
    }
}