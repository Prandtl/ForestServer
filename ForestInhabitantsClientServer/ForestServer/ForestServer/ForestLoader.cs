using System;
using System.Collections.Generic;
using System.IO;
using ForestServer.ForestObjects;

namespace ForestServer
{
    public class ForestLoader
    {
        private readonly StreamReader reader;
        public ForestLoader(StreamReader reader)
        {
            if (reader == null)
                throw new NullReferenceException();
            this.reader = reader;
        }
        private Dictionary<char, ForestObject> convertionDictionary = new Dictionary<char, ForestObject>();

        public Forest Load()
        {
            FillConvertionDictionary();
            var forest = new Forest();           
            var map = reader.ReadToEnd().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            reader.Close();
            forest.Map = new ForestObject[map.Length][];
            ;
            for (var i = 0; i < map.Length; i++)
            {
                forest.Map[i] = new ForestObject[map[i].Length];
                for (var j = 0; j < map[i].Length; j++)
                {
                    if (!convertionDictionary.ContainsKey(map[i][j]))
                        throw new KeyNotFoundException();
                    forest.Map[i][j] = convertionDictionary[map[i][j]].CoordinateObject(new Coordinates(j, i));
                }
            }
            return forest;
        }

        private void FillConvertionDictionary()
        {
            convertionDictionary.Add('0', new Footpath());
            convertionDictionary.Add('1', new Bush());
            convertionDictionary.Add('K', new Trap());
            convertionDictionary.Add('L', new Life());
        }
    }
}
