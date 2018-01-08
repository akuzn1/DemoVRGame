using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic3DLib
{
    public static class Projections
    {
        public static Point2D GetPerspectiveProjection(Point3D point, double r)
        {
            return new Point2D() { X = point.X / (1 - point.Z / r), Y = point.Y / (1 - point.Z / r) };
        }
    }
}
