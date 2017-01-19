using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace MyCommon
{
    public static class XmlManager
    {
        public enum LoadMode { ReadOnly, ReadOrCreate, ReadCreateOrReplace };
        /// <summary>
        /// Load class from xml file
        /// </summary>
        /// <param name="filepath">Path to xml file</param>
        /// <param name="schema">null disable Correct check (unsafe)</param>
        /// <returns>Loaded class</returns>
        /// <remarks>App.config is too easy</remarks>
        public static T Load<T>(string filepath, LoadMode mode, XmlReader schema, Logger logger = null) where T : new()
        {
            T file = new T();
            if (logger != null) { logger.Write("Loading " + file.GetType().Name, Logger.logType.info); }
            if (File.Exists(filepath))
            {
                bool correct = false;
                if(schema != null)
                {
                    correct = Correct<T>(file, filepath, schema, logger);
                }
                else
                {
                    correct = true;
                }
                if (correct)
                {
                    XmlSerializer xs = new XmlSerializer(typeof(T));
                    using (StreamReader re = new StreamReader(filepath))
                    {
                        file = (T)xs.Deserialize(re);
                    };
                }
                else
                {
                    if (mode == LoadMode.ReadCreateOrReplace)
                    {
                        if (logger != null) { logger.Write("Old " + file.GetType().Name + " in .old", Logger.logType.warm); }
                        File.Delete(filepath + ".old");
                        File.Move(filepath, filepath + ".old");
                        Save<T>(file, filepath, logger);
                    }
                }
            }
            else
            {
                if (logger != null) { logger.Write("Any config file", Logger.logType.error); }
                if (mode != LoadMode.ReadOnly)
                {
                    Save<T>(file, filepath, logger);
                }
            }
            if (logger != null) { logger.Write(file.GetType().Name + " loaded", Logger.logType.debug); }
            return file;
        }

        /// <summary>
        /// Write class in xml file
        /// </summary>
        public static void Save<T>(T file, string path, Logger logger = null)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (StreamWriter st = new StreamWriter(path))
            {
                xs.Serialize(st, file);
            };
            if (logger != null) { logger.Write(file.GetType().Name + " saved", Logger.logType.debug); }
        }

        /// <summary>
        /// Check file format using Schema
        /// </summary>
        public static bool Correct<T>(T file, string filepath, XmlReader schema, Logger logger = null)
        {
            bool isCorrect = false;

            using (Stream fs = new FileStream(filepath, FileMode.Open))
            {
                XmlReader re = new XmlTextReader(fs);
                XmlSerializer xs = new XmlSerializer(typeof(T));
                try
                {
                    isCorrect = xs.CanDeserialize(re);
                }
                catch (XmlException e)
                {
                    isCorrect = false;
                    if (logger != null) { logger.Write("Format check error: " + e.Message, Logger.logType.error); }
                }
            }

            if (isCorrect)
            {
                try
                {
                    XmlDocument d = new XmlDocument();
                    d.Load(filepath);
                    d.Schemas.Add("", schema);

                    d.Validate((o, e) =>
                    {
                        if (logger != null) { logger.Write("Format check error: " + e.Message, Logger.logType.error); }
                        isCorrect = false;
                    });
                }
                catch (XmlException e)
                {
                    isCorrect = false;
                    if (logger != null) { logger.Write("Format check error: " + e.Message, Logger.logType.error); }
                }
            }

            return isCorrect;
        }
    }
}
