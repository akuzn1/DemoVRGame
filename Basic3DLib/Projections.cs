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
        public static Point2D GetPerspectiveProjection(Point3D point, double r, Point3D viewPoint)
        {
            return new Point2D()
            {
                X = (point.X - viewPoint.X) / (1 - (point.Z - viewPoint.Z) / r),
                Y = (point.Y - viewPoint.Y) / (1 - (point.Z - viewPoint.Z) / r)
            };
        }
    }
}
