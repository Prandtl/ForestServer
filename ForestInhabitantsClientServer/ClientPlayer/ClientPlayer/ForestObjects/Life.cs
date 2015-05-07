namespace ClientPlayer.ForestObjects
{
    public class Life : ForestObject
    {
        public Life()
        { }
        public Life(Coordinates place) : base(place) { }
        public Life(params int[] coordinates) : base(coordinates) { }
        public override bool CanMove { get { return true; } }

        public override bool CanEnter(ref Inhabitant inhabitant, ref ForestObject[][] map, Coordinates place)
        {
            inhabitant.PrevObject = new Footpath(place);
            map[place.Y][place.X] = inhabitant;
            inhabitant.Place = place;
            ++inhabitant.Life;
            return true;
        }

        public override ForestObject CoordinateObject(Coordinates coordinates)
        {
            return new Life(coordinates);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Life;
        }

        public override int GetHashCode()
        {
            return 1;
        }
    }
}
