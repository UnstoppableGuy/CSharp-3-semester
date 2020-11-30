using System;

namespace FWS
{
    class ConfigManager : IParsable
    {
        public IParsable parser;
        public ConfigManager(string path, Type type)
        {
            if (path.EndsWith(".xml"))
            {
                parser = new Xml(path, type);
            }
            else if (path.EndsWith(".json"))
            {
                parser = new Json(path);
            }
            else
            {
                throw new ArgumentNullException("An unknown extension has appeared");
            }
        }
        public T GetOptions<T>() => parser.GetOptions<T>();
    }
