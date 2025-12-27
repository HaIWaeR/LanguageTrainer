using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace LanguageTrainer
{
    public class SavingLanguageGroup
    {
        private readonly string filePath = "groups.json";

        public List<string> Groups  {get; private set;}

        public SavingLanguageGroup()
        {
            LoadGroups();
        }

        private void LoadGroups()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Groups = JsonConvert.DeserializeObject<List<string>>(json) ?? [];
            }
        }

        public bool AddGroup(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName) || Groups.Contains(groupName))
                return false;

            Groups.Add(groupName);
            SaveGroups();
            return true;
        }

        public void DeleteGroup(string groupName)
        {
            if (Groups.Remove(groupName))
            {
                SaveGroups();
            }
        }

        public bool RenameGroup(string oldName, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName) ||
                string.IsNullOrWhiteSpace(oldName) ||
                oldName == newName)
                return false;

            if (Groups.Contains(newName))
                return false;

            int index = Groups.IndexOf(oldName);
            if (index >= 0)
            {
                Groups[index] = newName;
                SaveGroups();
                return true;
            }

            return false;
        }

        private void SaveGroups()
        {
            string json = JsonConvert.SerializeObject(Groups, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}