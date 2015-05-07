namespace ClientPlayer.ForestObjects
{
    public class Footpath : ForestObject
    {
        public Footpath()
        { }
        public Footpath(Coordinates place) : base(place) { }
        public Footpath(params int[] coordinates) : base(coordinates) { }
        public override bool CanMove { get { return true; } }

        public override bool CanEnter(ref Inhabitant inhabitant, ref ForestObject[][] map, Coordinates place)
        {
            inhabitant.PrevObject = new Footpath(place);
            map[place.Y][place.X] = inhabitant;
            inhabitant.Place = place;
            return true;
        }

        public override ForestObject CoordinateObject(Coordinates coordinates)
        {
            return new Footpath(coordinates);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Footpath;
        }

        public override int GetHashCode()
        {
            return 1;
        }
    }
}
