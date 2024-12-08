using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Car_Genetic_Algorithm.Graphics
{
    public class Polygon : Shape
    {
        public int Edges { get; private set; }

        public Polygon(IEnumerable<Vector2> vertices, Color color) : base(vertices, color)
        {
            Edges = Vertices.Count;
        }
    }
}
