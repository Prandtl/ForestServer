using System;
using ClientPlayer.ForestObjects;
using Newtonsoft.Json;

namespace ClientPlayer
{
    public class ForestObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(ForestObject));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object forestObject;
            try
            {
                forestObject = serializer.Deserialize<Trap>(reader);
            }
            catch (Exception)
            {
                forestObject = serializer.Deserialize<Footpath>(reader);
            }
            return forestObject;
        }

       
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
