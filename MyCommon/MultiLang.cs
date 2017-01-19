using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCommon
{
    /// <summary>
    /// Manager MultiLang from .cvs file
    /// </summary>
    public class MultiLang
    {
        private Dictionary<string, List<string>> multiDictionary = new Dictionary<string, List<string>>(); //List of phrases by key
        private List<string> langs = new List<string>(); //Readable langs list

        public int langsCount { get { return langs.Count; } }

        /// <summary>
        /// Create Dictionary from string
        /// </summary>
        /// <param name="dico">Dictionary text</param>
        /// <see cref="Langs.cvs"/>
        public void Initialise(string dico)
        {
            multiDictionary.Clear();
            langs.Clear();
            string[] lines = dico.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries); //Load from .cvs ressources.
            langs = lines[0].Split(';').OfType<string>().ToList();
            langs.RemoveAt(0);
            foreach (string line in lines)
            {
                List<string> items = line.Split(';').OfType<string>().ToList();
                string key = items[0];
                items.RemoveAt(0);
                multiDictionary.Add(key, items);
            }
        }

        /// <summary>
        /// Get Language name from ID
        /// </summary>
        public string IDToLang(int lang)
        {
            if (lang < langs.Count)
            {
                return langs[lang];
            }
            else
            {
                return "!!!UNKNOW LANG KEY!!!";
            }
        }

        /// <summary>
        /// Get Language ID form name
        /// </summary>
        public bool TryLangToID(string lang, out int ID)
        {
            for (int i = 0; i < langs.Count; i++)
            {
                if (lang == langs[i])
                {
                    ID = i;
                    return true;
                }
            }
            ID = -1;
            return false;
        }

        public List<string> GetWords(string key)
        {
            if (!multiDictionary.ContainsKey(key))
                return null;

            return multiDictionary[key];
        }

        public string GetWord(string key, int lang)
        {
            string text = "";

            if (multiDictionary.ContainsKey(key))
            {
                if (multiDictionary[key].Count >= lang)
                {
                    text = multiDictionary[key][lang];
                }
                else
                {
                    text = "!!!UNKNOW LANG KEY!!!";
                }
            }
            else
            {
                text = "!!!UNKNOW WORD KEY!!!";
            }

            return text;
        }
    }
}