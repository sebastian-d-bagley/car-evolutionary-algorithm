using Car_Genetic_Algorithm.Utlities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Car_Genetic_Algorithm
{
    internal class Course
    {
        public Vector2 output;

        public List<Vector2> leftWall = new List<Vector2>();
        public List<Vector2> rightWall = new List<Vector2>();

        public int angleChangeInterval = 10;
        public int num = 0;

        public float roadAngle = 0;
        public float interval = 100;
        public float angleChange = 100;

        public float yCenter = 0;
        public float yVariance = 50;

        public float xCenter = 500;
        public float xBase = 80;
        public float xVariance = 25;

        public int length;

        public Course(int length)
        {
            this.length = length;
            Random rng = new Random();
            for (int i=0; i < length; i++)
            {
                leftWall.Add(new Vector2(xCenter + xBase + ((float)rng.NextDouble() - 0.5f) * xVariance, interval * i + ((float)rng.NextDouble()-0.5f) * yVariance));
                rightWall.Add(new Vector2(xCenter - xBase + ((float)rng.NextDouble() - 0.5f) * xVariance, interval * i + ((float)rng.NextDouble() - 0.5f) * yVariance));
                yCenter += interval * MathF.Cos(roadAngle);
                xCenter += interval * MathF.Sin(roadAngle);
                roadAngle += ((float)rng.NextDouble() - 0.5f) * angleChange;
                num++;
                if (angleChangeInterval == num)
                    num = 0;
            }
        }
        public bool CheckCollision(Car car)
        {
            Vector2[] hitbox = car.GetHitbox();
            for (int i = 0; i < leftWall.Count - 1; i++)
            {
                if (MathF.Abs(leftWall[i].Y - (hitbox[0] + new Vector2(500, 500)).Y) < interval * 2)
                {
                    bool leftWallHitboxOne = Intersection.VectorIntersection(leftWall[i], leftWall[i + 1], hitbox[0] + new Vector2(500, 500), hitbox[3] + new Vector2(500, 500));
                    bool leftWallHitboxTwo = Intersection.VectorIntersection(leftWall[i], leftWall[i + 1], hitbox[1] + new Vector2(500, 500), hitbox[2] + new Vector2(500, 500));
                    bool rightWallHitboxOne = Intersection.VectorIntersection(rightWall[i], rightWall[i + 1], hitbox[0] + new Vector2(500, 500), hitbox[3] + new Vector2(500, 500));
                    bool rightWallHitboxTwo = Intersection.VectorIntersection(rightWall[i], rightWall[i + 1], hitbox[1] + new Vector2(500, 500), hitbox[2] + new Vector2(500, 500));
                    if (leftWallHitboxOne || leftWallHitboxTwo || rightWallHitboxOne || rightWallHitboxTwo)
                        return true;
                }
            }
            return false;
        }
    }
}
