using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientVisualisator.ForestObjects
{
    [Serializable]
    public class Inhabitant : ForestObject
    {
        public string Name;
        public int Life;
        public ForestObject PrevObject;
        public Coordinates Purpose;
        public bool EnterIntoLife;
        public Inhabitant()
        { }
        public Inhabitant(string name, int life)
        {
            Name = name;
            Life = life;
        }

        public override bool CanMove { get { return false; } }

        public override bool CanEnter(ref Inhabitant inhabitant, ref ForestObject[][] map, Coordinates place)
        {
            return false;
        }

        public bool Leave(ref ForestObject[][] map, ForestObject prevObject)
        {
            map[prevObject.Place.Y][prevObject.Place.X] = prevObject;
            return true;
        }

        public override ForestObject CoordinateObject(Coordinates coordinates)
        {
            return new Inhabitant(Name, Life) { Place = coordinates };
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Inhabitant && ((Inhabitant)obj).Name == Name;
        }

        public override int GetHashCode()
        {
            return 1;
        }

        public Inhabitant GetClone()
        {
            return (Inhabitant)MemberwiseClone();
        }
    }
}
