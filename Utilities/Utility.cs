using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;

namespace Car_Genetic_Algorithm.Utlities
{
    internal class Utility
    {
        public static float Radians(float d)
        {
            return MathF.PI / 180 * d;
        }
        public static float Degrees(float r)
        {
            return r * (180 / MathF.PI);
        }
        public static float TrueDistance(float angle, float target)
        {
            angle = Degrees(angle);
            target = Degrees(target);

            angle = angle % 360;
            target = target % 360;

            if (angle < 0)
                angle = 360 + angle;
            if (target < 0)
                target = 360 + target;

            float between = MathF.Abs(target - angle);

            float max = MathF.Max(angle, target);
            float min = MathF.Min(angle, target);

            float around = MathF.Abs(360 - max) + MathF.Abs(min);

            return Radians(MathF.Min(between, around));
        }
        public static int WhichWay(float angle, float target)
        {
            angle = Degrees(angle);
            target = Degrees(target);

            angle = angle % 360;
            target = target % 360;

            if (angle < 0)
                angle = 360 + angle;
            if (target < 0)
                target = 360 + target;

            float between = MathF.Abs(target - angle);

            float max = MathF.Max(angle, target);
            float min = MathF.Min(angle, target);

            float around = MathF.Abs(360 - max) + MathF.Abs(min);

            if (around > between)
            {
                if (angle - target > 0)
                    return 1;
                else
                    return -1;
            }
            else
            {
                if (angle - target < 0)
                    return 1;
                else
                    return -1;
            }
        }
        public static bool InRange(Microsoft.Xna.Framework.Vector2 p, int width, int height)
        {
            return p.X < 0 || p.Y < 0 || p.X > width || p.Y > height;
        }
        public static bool PolygonInRange(List<Microsoft.Xna.Framework.Vector2> polygon, int width, int height)
        {
            foreach (Microsoft.Xna.Framework.Vector2 vertex in polygon)
                if (!InRange(vertex, width, height)) return false;
            return true;
        }
    }
}
