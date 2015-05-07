using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientVisualisator.ForestObjects;
using Newtonsoft.Json;

namespace ClientVisualisator
{
    [Serializable]
    public class Forest
    {
        public ForestObject[][] Map;
        public Inhabitant[] Inhabitants = new Inhabitant[64];
        public event Action<Forest> ForestChange;                  //   Событие изменения леса
        public event Func<Inhabitant, bool> InhabitantCreated;      //        События
        public event Func<Inhabitant, bool> InhabitantDestroyed;   //         лесных
        public event Action InhabitantMove;        //        жителей
        public Forest() { }

        public bool CreateInhabitant(ref Inhabitant inhabitant, Coordinates place, Coordinates purpose)
        {
            if (OutOfBorders(place) || OutOfBorders(purpose) || purpose.Equals(place))
                return false;
            var successChange = Map[place.Y][place.X].CanEnter(ref inhabitant, ref Map, place);
            if (successChange && Map[purpose.Y][purpose.X].CanMove)
                inhabitant.Purpose = purpose;
            return successChange && OnInhabitantCreated(inhabitant) && DestroyInhabitantsWithZeroLife();
        }

        public bool Move(ref Inhabitant inhabitant, Coordinates command)
        {
            var newPosition = command.Add(inhabitant.Place);
            if (OutOfBorders(newPosition))
                return false;
            var prevObject = inhabitant.PrevObject;
            var successChange = Map[newPosition.Y][newPosition.X].CanEnter(ref inhabitant, ref Map, newPosition) && inhabitant.Leave(ref Map, prevObject);
            if (successChange && inhabitant.Life > 0)
                OnInhabitantMove();
            return successChange && DestroyInhabitantsWithZeroLife();
        }

        public bool DestroyInhabitant(ref Inhabitant inhabitant)
        {
            Inhabitants.SetValue(null, Array.IndexOf(Inhabitants, inhabitant));
            Map[inhabitant.Place.Y][inhabitant.Place.X] = inhabitant.PrevObject;
            OnInhabitantDestroyed(inhabitant);
            return true;
        }

        private bool OutOfBorders(Coordinates position)
        {
            if (position == null)
                return true;
            return position.X < 0 || position.Y >= Map.Length || position.Y < 0 || position.X >= Map[position.Y].Length;
        }

        private void OnForestChange()
        {
            if (ForestChange != null)
                ForestChange(this);
        }
        private bool OnInhabitantCreated(Inhabitant inhabitant)
        {
            if (InhabitantCreated != null)
                InhabitantCreated(inhabitant);
            OnForestChange();
            Inhabitants[Array.IndexOf(Inhabitants, null)] = inhabitant;
            return true;
        }
        private void OnInhabitantDestroyed(Inhabitant inhabitant)
        {
            if (InhabitantDestroyed != null)
                InhabitantDestroyed(inhabitant);
        }

        private void OnInhabitantMove()
        {
            if (InhabitantMove != null)
                InhabitantMove();
            OnForestChange();
        }

        private bool DestroyInhabitantsWithZeroLife()
        {
            while (Inhabitants.Any(inhabitant => inhabitant != null && inhabitant.Life <= 0))
            {
                var destroyedInhabitant = Inhabitants.First(inhabitant => inhabitant != null && inhabitant.Life <= 0);
                DestroyInhabitant(ref destroyedInhabitant);
                OnForestChange();
            }
            return true;
        }
    }

    public class MoveCommand
    {
        public static Coordinates Up = new Coordinates(0, -1);
        public static Coordinates Down = new Coordinates(0, 1);
        public static Coordinates Left = new Coordinates(-1, 0);
        public static Coordinates Right = new Coordinates(1, 0);
    }
}
