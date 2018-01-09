using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic3DLib
{
    public static class Figures
    {
        public static Object3D GetCube(Point3D centralPosition, double scale)
        {
            Point3D p1 = new Point3D() { X = centralPosition.X + -1 * scale, Y = centralPosition.Y + -1 * scale, Z = centralPosition.Z + -1 * scale };
            Point3D p2 = new Point3D() { X = centralPosition.X +  1 * scale, Y = centralPosition.Y + -1 * scale, Z = centralPosition.Z + -1 * scale };
            Point3D p3 = new Point3D() { X = centralPosition.X +  1 * scale, Y = centralPosition.Y +  1 * scale, Z = centralPosition.Z + -1 * scale };
            Point3D p4 = new Point3D() { X = centralPosition.X + -1 * scale, Y = centralPosition.Y +  1 * scale, Z = centralPosition.Z + -1 * scale };
            Point3D p5 = new Point3D() { X = centralPosition.X + -1 * scale, Y = centralPosition.Y + -1 * scale, Z = centralPosition.Z +  1 * scale };
            Point3D p6 = new Point3D() { X = centralPosition.X +  1 * scale, Y = centralPosition.Y + -1 * scale, Z = centralPosition.Z +  1 * scale };
            Point3D p7 = new Point3D() { X = centralPosition.X +  1 * scale, Y = centralPosition.Y +  1 * scale, Z = centralPosition.Z +  1 * scale };
            Point3D p8 = new Point3D() { X = centralPosition.X + -1 * scale, Y = centralPosition.Y +  1 * scale, Z = centralPosition.Z +  1 * scale };

            Plane3D pl1 = new Plane3D() { Points = new List<Point3D>(new Point3D[] { p1, p2, p3, p4 }) };
            Plane3D pl2 = new Plane3D() { Points = new List<Point3D>(new Point3D[] { p1, p2, p6, p5 }) };
            Plane3D pl3 = new Plane3D() { Points = new List<Point3D>(new Point3D[] { p2, p3, p7, p6 }) };
            Plane3D pl4 = new Plane3D() { Points = new List<Point3D>(new Point3D[] { p3, p4, p8, p7 }) };
            Plane3D pl5 = new Plane3D() { Points = new List<Point3D>(new Point3D[] { p4, p1, p5, p8 }) };
            Plane3D pl6 = new Plane3D() { Points = new List<Point3D>(new Point3D[] { p5, p6, p7, p8 }) };

            Object3D obj = new Object3D() {
                Points = new List<Point3D>(new Point3D[] { p1, p2, p3, p4, p5, p6, p7, p8 }),
                Planes = new List<Plane3D>(new Plane3D[] { pl1, pl2, pl3, pl4, pl5, pl6 }),
                Center = new Point3D() { X = centralPosition.X, Y = centralPosition.Y, Z = centralPosition.Z }
            };

            return obj;
        }
    }
}
