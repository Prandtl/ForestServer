using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientVisualisator.ForestObjects
{
    [Serializable]
    public abstract class ForestObject
    {
        public Coordinates Place;
        public abstract bool CanMove { get; }

        protected ForestObject(Coordinates place)
        {
            Place = place;
        }

        protected ForestObject(params int[] coordinates)
        {
            Place = new Coordinates(coordinates[0], coordinates[1]);
        }
        public ForestObject() { }

        public abstract bool CanEnter(ref Inhabitant inhabitant, ref ForestObject[][] map, Coordinates place);

        public abstract ForestObject CoordinateObject(Coordinates coordinates);
    }
}
