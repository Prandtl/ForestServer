using System.IO;
using System.Xml.Serialization;
using ForestServer.ForestObjects;

namespace ForestServer
{
    public static class XmlLoader
    {
        private static readonly XmlSerializer settingsFormatter = new XmlSerializer(typeof(Settings), new[] { typeof(Forest), typeof(int), typeof(Bush), typeof(Trap), typeof(Inhabitant), typeof(Life), typeof(Footpath) });
        private static readonly XmlSerializer forestFormatter = new XmlSerializer(typeof(Forest), new[] { typeof(Bush), typeof(Trap), typeof(Inhabitant), typeof(Life), typeof(Footpath) });
        public static void SaveData(string path, object obj)
        {
            if (obj is Forest)
            {
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    forestFormatter.Serialize(fs, (obj as Forest));
                }
            }
            else if (obj is Settings)
            {
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    settingsFormatter.Serialize(fs, (obj as Settings));
                }
            }
        }
        public static Settings DeserializeForest(string path)
        {
            Settings settings;
            using (var fs = new FileStream(path, FileMode.Open))
                settings = (Settings)settingsFormatter.Deserialize(fs);
            return settings;
        }
    }
}
