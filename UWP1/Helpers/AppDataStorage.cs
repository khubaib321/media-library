using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UWP1.Helpers
{
    public class AppDataStorage
    {
        private bool committedForSave = false;
        private List<String> storables = new List<string>();
        private Dictionary<String, object> temporaryData = new Dictionary<string, object>();
        private Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        private bool save()
        {
            foreach(KeyValuePair<String, object> savable in this.temporaryData)
            {
                if (savable.Value.GetType().Name == "String")
                    localSettings.Values[savable.Key] = savable.Value;
                else
                    Debug.WriteLine("Trying to store object of type " + savable.Value.GetType().Name + " in local storage. Skipped.");
            }
            return true;
        }

        private bool load()
        {
            this.temporaryData.Clear();
            
            /**
             * First initialize temporary data with registered storables
             * This makes sure we won't have any non existent property access
             */
            foreach(String savableProperty in this.storables)
                this.temporaryData[savableProperty] = null;
            // now load stored values in local store
            foreach (KeyValuePair<String, object> keyValuePair in this.localSettings.Values)
                this.temporaryData[keyValuePair.Key] = keyValuePair.Value;
            return true;
        }

        public void registerForSave(List<String> propertyNames)
        {
            if (propertyNames.Count == 0)
            {
                Debug.WriteLine("Nothing to register.");
                return;
            }
            foreach(String propertyName in propertyNames)
            {
                if (this.isRegisterable(propertyName))
                {
                    this.storables.Add(propertyName);
                }
            }
            return;
        }

        private bool isRegisterable(String propertyName)
        {
            return !String.IsNullOrEmpty(propertyName) && !this.storables.Contains(propertyName);
        }

        private bool isStorable(String propertyName)
        {
            return !String.IsNullOrEmpty(propertyName) && this.storables.Contains(propertyName);
        }

        public bool prepareForSave(Dictionary<String, object> savables)
        {
            if (savables.Count == 0)
            {
                Debug.WriteLine("Nothing to save.");
                return false;
            }
            foreach(KeyValuePair<String, object> savable in savables)
            {
                object savedValue = null;
                this.temporaryData.TryGetValue(savable.Key, out savedValue);
                if (savedValue == savable.Value ||
                    !this.isStorable(savable.Key))
                {
                    // if previous value has not been changed or it's key is not registered, then skip it
                    continue;
                }
                this.temporaryData[savable.Key] = savable.Value;
                // uncommit temporary save data. changes can be detected here.
                this.committedForSave = false;
            }
            return true;
        }

        public bool commitAppData()
        {
            if (this.committedForSave)
            {
                Debug.WriteLine("Already saved.");
                return true;
            }
            if (this.temporaryData.Count == 0)
            {
                Debug.WriteLine("Nothing to commit.");
                return true;
            }
            this.committedForSave = true;
            return this.save();
        }

        public void uncommitAppData()
        {
            this.committedForSave = false;
        }

        public Dictionary<String, object> fetchAppData()
        {
            if (this.committedForSave)
            {
                Debug.WriteLine("Save in progress... Cannot load app data.");
                return new Dictionary<string, object>();
            }
            if (this.load())
            {
                return this.temporaryData;
            }
            Debug.WriteLine("Failed to fetch app data.");
            return new Dictionary<string, object>();
        }
    }
}
