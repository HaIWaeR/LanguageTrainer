using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LanguageTrainer
{
    public class WordDictionary
    {
        public string Name { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public List<WordPair> Words { get; set; } = new List<WordPair>();
    }

    public class WordPair
    {
        public string NativeWord { get; set; } = string.Empty;
        public string ForeignWord { get; set; } = string.Empty;
        public bool IsLearned { get; set; } = false;
    }

    public class DictionaryManager
    {
        private readonly string filePath = "dictionaries.json";
        private List<WordDictionary> dictionaries = new List<WordDictionary>();

        public DictionaryManager()
        {
            LoadDictionaries();
        }

        public void DeleteAllDictionariesForGroup(string groupName)
        {
            dictionaries.RemoveAll(d => d.GroupName == groupName);
            SaveDictionaries();
        }

        private void LoadDictionaries()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                dictionaries = JsonConvert.DeserializeObject<List<WordDictionary>>(json) ?? new List<WordDictionary>();
            }
        }

        public bool AddDictionary(string dictionaryName, string groupName)
        {
            if (string.IsNullOrWhiteSpace(dictionaryName) ||
                dictionaries.Any(d => d.Name == dictionaryName && d.GroupName == groupName))
                return false;

            dictionaries.Add(new WordDictionary
            {
                Name = dictionaryName,
                GroupName = groupName
            });

            SaveDictionaries();
            return true;
        }

        public void UpdateDictionary(WordDictionary dictionary)
        {
            if (dictionary == null) return;

            dictionaries.RemoveAll(d =>
                d.Name == dictionary.Name && d.GroupName == dictionary.GroupName);

            dictionaries.Add(dictionary);
            SaveDictionaries();
        }

        public List<WordDictionary> GetDictionariesForGroup(string groupName)
        {
            return dictionaries.Where(d => d.GroupName == groupName).ToList();
        }

        public void DeleteDictionary(string dictionaryName, string groupName)
        {
            WordDictionary dictionary = dictionaries.FirstOrDefault(d =>
                d.Name == dictionaryName && d.GroupName == groupName);

            if (dictionary != null)
            {
                dictionaries.Remove(dictionary);
                SaveDictionaries();
            }
        }

        public bool RenameDictionary(string oldName, string newName, string groupName)
        {
            if (string.IsNullOrWhiteSpace(newName) ||
                dictionaries.Any(d => d.Name == newName && d.GroupName == groupName))
                return false;

            WordDictionary dictionary = dictionaries.FirstOrDefault(d =>
                d.Name == oldName && d.GroupName == groupName);

            if (dictionary != null)
            {
                dictionary.Name = newName;
                SaveDictionaries();
                return true;
            }

            return false;
        }

        public WordDictionary GetDictionary(string dictionaryName, string groupName)
        {
            return dictionaries.FirstOrDefault(d =>
                d.Name == dictionaryName && d.GroupName == groupName);
        }

        private void SaveDictionaries()
        {
            string json = JsonConvert.SerializeObject(dictionaries, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}