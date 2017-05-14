using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lattice.Models.Common
{
    struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    struct PointD
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    struct Point3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double GetLength()
        {
            var xy = Math.Sqrt(this.X * this.X + this.Y * this.Y);
            return Math.Sqrt(xy * xy + this.Z * this.Z);
        }
        public static Point3D operator+(Point3D a, Point3D b)
        {
            return new Point3D
            {
                X = a.X + b.X,
                Y = a.Y + b.Y,
                Z = a.Z + b.Z,
            };
        }
        public static Point3D operator/(Point3D a, double b)
        {
            return new Point3D
            {
                X = a.X / b,
                Y = a.Y / b,
                Z = a.Z / b,
            };
        }
    }

    struct Size
    {
        public int W { get; set; }
        public int H { get; set; }
    }

    struct SizeD
    {
        public double W { get; set; }
        public double H { get; set; }
    }

    struct Size3D
    {
        public double W { get; set; }
        public double H { get; set; }
        public double Z { get; set; }
    }

    struct TwoValuesD
    {
        public double First { get; set; }
        public double Second { get; set; }
    }
}
