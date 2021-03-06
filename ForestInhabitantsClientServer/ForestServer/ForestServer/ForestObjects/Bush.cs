﻿using System;
using Newtonsoft.Json;

namespace ForestServer.ForestObjects
{
    [Serializable]
    public class Bush : ForestObject
    {
        [JsonConstructor]
        public Bush()
        { }
        public Bush(Coordinates place) : base(place) { }
        public Bush(params int[] coordinates) : base(coordinates) { }
        public override bool CanMove { get { return false; } }

        public override bool CanEnter(ref Inhabitant inhabitant, ref ForestObject[][] map, Coordinates place)
        {
            return false;
        }

        public override ForestObject CoordinateObject(Coordinates coordinates)
        {
            return new Bush(coordinates);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Bush;
        }

        public override int GetHashCode()
        {
            return 1;
        }
    }   
}
