using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Car_Genetic_Algorithm
{
    internal class Ray
    {
        public Vector2 ray;
        public float length;
        public bool intersect;
        public float maxDistance;

        Vector2 out1 = new Vector2();
        Vector2 out2 = new Vector2();
        Vector2 out3 = new Vector2();


        public Ray(Vector2 ray, float maxDistance)
        {
            this.ray = ray;
            this.maxDistance = maxDistance;
        }

        public void Raycast(Car car, Course course)
        {
            intersect = false;
            length = maxDistance;

            for (int i = 0; i < course.leftWall.Count - 1; i++)
                if (MathF.Abs(course.leftWall[i].Y - car.Position.Y) < course.interval * 10)
                {
                    if (RayIntersect.RayIntersectsSegment(ray, course.leftWall[i] - car.Position - new Vector2(500, 500), course.leftWall[i + 1] - car.Position - new Vector2(500, 500), out float distanceLeft))
                    {
                        intersect = true;
                        length = distanceLeft;
                        break;
                    }
                    if (RayIntersect.RayIntersectsSegment(ray, course.rightWall[i] - car.Position - new Vector2(500, 500), course.rightWall[i + 1] - car.Position - new Vector2(500, 500), out float distanceRight))
                    {
                        intersect = true;
                        length = distanceRight;
                        break;
                    }
                }
        }
    }
}
