using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Socrates.ValueTypes
{
    public struct Euler
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Euler(float angleX, float angleY, float angleZ)
        {
            X = angleX;
            Y = angleY;
            Z = angleZ;
        }
    }
}
