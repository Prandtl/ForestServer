using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientVisualisator
{
    public class Coordinates
    {
        public int X;
        public int Y;

        public Coordinates()
        { }
        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Coordinates Add(Coordinates otherCoordinates)
        {
            return new Coordinates(X + otherCoordinates.X, Y + otherCoordinates.Y);
        }
        public Coordinates Substract(Coordinates otherCoordinates)
        {
            return new Coordinates(X - otherCoordinates.X, Y - otherCoordinates.Y);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Coordinates))
                return false;
            var otherCoordinates = obj as Coordinates;
            return (otherCoordinates.X == X) && (otherCoordinates.Y == Y);
        }
    }
}
