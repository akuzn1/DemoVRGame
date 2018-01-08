using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic3DLib
{
    public class Plane3D
    {
        public List<Point3D> Points { get; set; }
        public Point3D GetCentralPoint()
        {
            if (Points == null || Points.Count == 0)
                return null;
            Point3D res = new Point3D();
            for (int i = 0; i < Points.Count; i++)
            {
                res.X += Points[i].X;
                res.Y += Points[i].Y;
                res.Z += Points[i].Z;
            }
            res.X /= Points.Count;
            res.Y /= Points.Count;
            res.Z /= Points.Count;

            return res;
        }
    }
}
