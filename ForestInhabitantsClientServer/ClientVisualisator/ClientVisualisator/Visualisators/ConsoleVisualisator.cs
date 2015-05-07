using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientVisualisator.ForestObjects;
using ClientVisualisator.Visualisators;

namespace ClientVisualisator
{
    public class ConsoleVisualisator : IForestVisualisator
    {
        //public Forest Forest { get; private set; }
        private readonly Dictionary<ForestObject, Func<ForestObject, string>> dictionaryOfForestObjects = new Dictionary<ForestObject, Func<ForestObject, string>>();

        public ConsoleVisualisator()
        {
            InitializeDictionary();
        }
        //public ConsoleVisualisator(Forest forest)
        //{
        //    InitializeDictionary();
        //    Forest = forest;
        //    //Forest.InhabitantCreated += AddInhabitantToDictionary;
        //    //Forest.InhabitantDestroyed += RemoveInhabitantFromDictionary;
        //    //Forest.ForestChange += DrawForest;
        //}

        private void InitializeDictionary()
        {
            dictionaryOfForestObjects.Add(new Footpath(), forestObject => String.Format("{0} - тропинка", " "));
            dictionaryOfForestObjects.Add(new Bush(), forestObject => String.Format("{0} - заросли", char.ConvertFromUtf32(9632)[0]));
            dictionaryOfForestObjects.Add(new Trap(), forestObject => String.Format("{0} - капкан", '\u2663'));
            dictionaryOfForestObjects.Add(new Life(), forestObject => String.Format("{0} - жизнь", '\u2665'));
        }

        public void AddInhabitantToDictionary(Inhabitant inhabitant)
        {
            dictionaryOfForestObjects.Add(inhabitant, forestObject => String.Format("{0} - лесной житель {1} ({2} жизни). Цель - {3},{4}", inhabitant.Name[0], inhabitant.Name, inhabitant.Life, inhabitant.Purpose.X, inhabitant.Purpose.Y));
        }

        //private bool RemoveInhabitantFromDictionary(Inhabitant inhabitant)
        //{
        //    if (inhabitant.Life <= 0)
        //        dictionaryOfForestObjects[inhabitant] =
        //       forestObject => String.Format("{0} - лесной житель {1} УМЕР", inhabitant.Name[0], inhabitant.Name);
        //    else
        //        dictionaryOfForestObjects[inhabitant] =
        //       forestObject => String.Format("{0} - лесной житель {1} ДОСТИГ ЦЕЛИ", inhabitant.Name[0], inhabitant.Name);
        //    DrawForest(Forest);
        //    Thread.Sleep(1500);
        //    dictionaryOfForestObjects.Remove(inhabitant);
        //    return true;
        //}

        public void DrawForest(Forest forest)
        {
            Console.Clear();
            foreach (var rowForestObjects in forest.Map)
            {
                foreach (var forestObject in rowForestObjects)
                    Console.Write(dictionaryOfForestObjects[forestObject].Invoke(forestObject)[0]);
                Console.WriteLine();
            }
            DrawLegend();
        }

        private void DrawLegend()
        {
            Console.WriteLine();
            foreach (var forestObject in dictionaryOfForestObjects.Keys)
                Console.WriteLine(dictionaryOfForestObjects[forestObject].Invoke(forestObject));
            Console.Beep();
        }
    }
}
