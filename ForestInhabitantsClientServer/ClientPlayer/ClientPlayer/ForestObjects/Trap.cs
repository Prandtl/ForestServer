namespace ClientPlayer.ForestObjects
{
    public class Trap : ForestObject
    {
        public Trap()
        { }
        public Trap(Coordinates place) : base(place) { }
        public Trap(params int[] coordinates) : base(coordinates) { }
        public override bool CanMove { get { return true; } }

        public override bool CanEnter(ref Inhabitant inhabitant, ref ForestObject[][] map, Coordinates place)
        {
            inhabitant.PrevObject = new Trap(place);
            map[place.Y][place.X] = inhabitant;
            inhabitant.Place = place;
            --inhabitant.Life;
            return true;
        }

        public override ForestObject CoordinateObject(Coordinates coordinates)
        {
            return new Trap(coordinates);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Trap;
        }

        public override int GetHashCode()
        {
            return 1;
        }
    }
}
