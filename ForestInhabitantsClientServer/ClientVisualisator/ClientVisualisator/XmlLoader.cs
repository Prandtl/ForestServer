using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ClientVisualisator.ForestObjects;

namespace ClientVisualisator
{
    public static class XmlLoader
    {
        private static readonly XmlSerializer forestFormatter = new XmlSerializer(typeof(Forest), new[] { typeof(Bush), typeof(Trap), typeof(Inhabitant), typeof(Life), typeof(Footpath) });

        public static Forest DeserializeForest(string path)
        {
            Forest forest;
            using (var fs = new FileStream(path, FileMode.Open))
                forest = (Forest)forestFormatter.Deserialize(fs);
            return forest;
        }
    }
}
