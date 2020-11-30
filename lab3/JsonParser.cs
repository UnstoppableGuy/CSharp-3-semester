using System.IO;
using System.Text.Json;

namespace FWS
{
    class Json : IParsable
    {
        private readonly string currentpath;
        public Json(string path)
        {
            currentpath = path;
        }
        public T GetOptions<T>()
        {
            string json;
            using(StreamReader reader = new StreamReader(currentpath))
            {
                json = reader.ReadToEnd();
            }
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}

