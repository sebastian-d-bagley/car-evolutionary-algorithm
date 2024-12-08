using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Car_Genetic_Algorithm.Utlities
{
    internal class Circle
    {
        public float radius;
        public Vector2 center;

        public Circle(float radius, Vector2 center)
        {
            this.center = center;
            this.radius = radius;
        }
        public string ToString()
        {
            return "Center: (" + center.X + ", " + center.Y + ") | Radius: " + radius;
        }
    }
}
