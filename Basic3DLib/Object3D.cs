using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic3DLib
{
    public class Object3D
    {
        public Point3D Center { get; set; }
        public List<Point3D> Points { get; set; }
        public List<Plane3D> Planes { get; set; }

        public void Move(double dx, double dy, double dz)
        {
            for(int i = 0; i < Points.Count; i++)
            {
                Points[i].X += dx;
                Points[i].Y += dy;
                Points[i].Z += dz;
            }
            Center.X += dx;
            Center.Y += dy;
            Center.Z += dz;
        }

        public void Scale(double scaleX, double scaleY, double scaleZ)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].X *= scaleX;
                Points[i].Y *= scaleY;
                Points[i].Z *= scaleZ;
            }
            Center.X *= scaleX;
            Center.Y *= scaleY;
            Center.Z *= scaleZ;
        }

        public void RotateX(double angle)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].X -= Center.X;
                Points[i].Y -= Center.Y;
                Points[i].Z -= Center.Z;
            }

            Point3D tmp = new Point3D();
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);
            for (int i = 0; i < Points.Count; i++)
            {
                tmp.X = Points[i].X;
                tmp.Y = Points[i].Y * cosA + Points[i].Z * sinA;
                tmp.Z = -Points[i].Y * sinA + Points[i].Z * cosA;

                Points[i].X = tmp.X;
                Points[i].Y = tmp.Y;
                Points[i].Z = tmp.Z;
            }

            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].X += Center.X;
                Points[i].Y += Center.Y;
                Points[i].Z += Center.Z;
            }
        }

        public void RotateY(double angle)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].X -= Center.X;
                Points[i].Y -= Center.Y;
                Points[i].Z -= Center.Z;
            }

            Point3D tmp = new Point3D();
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);
            for (int i = 0; i < Points.Count; i++)
            {
                tmp.X = Points[i].X * cosA + Points[i].Z * sinA;
                tmp.Y = Points[i].Y;
                tmp.Z = -Points[i].X * sinA + Points[i].Z * cosA;

                Points[i].X = tmp.X;
                Points[i].Y = tmp.Y;
                Points[i].Z = tmp.Z;
            }

            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].X += Center.X;
                Points[i].Y += Center.Y;
                Points[i].Z += Center.Z;
            }
        }

        public void RotateZ(double angle)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].X -= Center.X;
                Points[i].Y -= Center.Y;
                Points[i].Z -= Center.Z;
            }

            Point3D tmp = new Point3D();
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);
            for (int i = 0; i < Points.Count; i++)
            {
                tmp.X = Points[i].X * cosA + Points[i].Y * sinA;
                tmp.Y = -Points[i].X * sinA + Points[i].Y * cosA;
                tmp.Z = Points[i].Z;

                Points[i].X = tmp.X;
                Points[i].Y = tmp.Y;
                Points[i].Z = tmp.Z;
            }

            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].X += Center.X;
                Points[i].Y += Center.Y;
                Points[i].Z += Center.Z;
            }
        }
    }
}
