namespace Socrates.ValueTypes
{
    public struct Matrix3x4
    {
        public float[] this[int column]
        {
            get
            {
                switch (column)
                {
                    case 0:
                        return new float[] { M00, M10, M20 };
                    case 1:
                        return new float[] { M01, M11, M21 };
                    case 2:
                        return new float[] { M02, M12, M22 };
                    case 3:
                        return new float[] { M03, M13, M23 };
                    default:
                        return null;
                }
            }
        }
        public float M00 { get; set; }
        public float M10 { get; set; }
        public float M20 { get; set; }

        public float M01 { get; set; }
        public float M11 { get; set; }
        public float M21 { get; set; }

        public float M02 { get; set; }
        public float M12 { get; set; }
        public float M22 { get; set; }

        public float M03 { get; set; }
        public float M13 { get; set; }
        public float M23 { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Matrix3x4)
                return GetHashCode() == obj.GetHashCode();
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + M00.GetHashCode();
                hash = hash * 31 + M10.GetHashCode();
                hash = hash * 31 + M20.GetHashCode();

                hash = hash * 31 + M01.GetHashCode();
                hash = hash * 31 + M11.GetHashCode();
                hash = hash * 31 + M21.GetHashCode();

                hash = hash * 31 + M02.GetHashCode();
                hash = hash * 31 + M12.GetHashCode();
                hash = hash * 31 + M22.GetHashCode();

                hash = hash * 31 + M03.GetHashCode();
                hash = hash * 31 + M13.GetHashCode();
                hash = hash * 31 + M23.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Matrix3x4 m1, Matrix3x4 m2)
        {
            return m1.Equals(m2);
        }

        public static bool operator !=(Matrix3x4 m1, Matrix3x4 m2)
        {
            return !(m1 == m2);
        }
    }
}