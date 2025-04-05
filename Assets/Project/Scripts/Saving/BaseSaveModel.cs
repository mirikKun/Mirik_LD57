using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Scripts.Saving.Utils;
using UnityEngine;

namespace Project.Scripts.Saving
{
    public abstract class BaseSaveModel<T> : IUserModel where T : new()
    {
        [JsonIgnore] private bool m_BlockSave;
        private JObject m_PreviousJson;

        public event Action<string> OnChanged;

        public event Action OnSaved;

        [System.Runtime.Serialization.OnDeserializing]
        internal void OnDeserializing(StreamingContext context) => this.m_BlockSave = true;

        [System.Runtime.Serialization.OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            this.m_PreviousJson = JObject.FromObject((object)this);
            this.m_BlockSave = false;
        }

        protected void Save()
        {
            if (this.m_BlockSave)
                return;
            JObject newJson = JObject.FromObject((object)this);
            string name = typeof(T).Name;
            if (this.OnChanged != null)
            {
                JObject diff = JsonUtils.FindDiff((JToken)newJson, (JToken)this.m_PreviousJson);
                if (diff.Count != 0)
                {
                    string str = diff.ToString(Formatting.None);
                    Debug.Log(name + " diff: " + str);
                    this.OnChanged.SafeInvoke<string>(str);
                }
            }

            Action onSaved = this.OnSaved;
            if (onSaved != null)
                onSaved.SafeInvoke();
            string str1 = newJson.ToString(Formatting.None);
            PlayerPrefs.SetString(UserModelInternalUtils.UserModelSaveDataKey<T>(), str1);
            Debug.Log(name + " saved.\nData: " + str1);
            PlayerPrefs.Save();
            this.m_PreviousJson = newJson;
        }

        public static T LoadModel()
        {
            List<string> stringList = UserModelInternalUtils.UserModelLoadDataKeys<T>();
            string newLocation = UserModelInternalUtils.UserModelSaveDataKey<T>();
            foreach (string str in stringList)
            {
                T obj = BaseSaveModel<T>.LoadModelFromLocation(str);
                if ((object)obj != null)
                {
                    if (newLocation != str)
                        BaseSaveModel<T>.MigrateSaveLocation(str, newLocation);
                    return obj;
                }
            }

            return new T();
        }

        private static T LoadModelFromLocation(string location)
        {
            if (!PlayerPrefs.HasKey(location))
                return default(T);
            string str = PlayerPrefs.GetString(location);
            Debug.Log("Load model " + typeof(T).Name + ": " + str);
            try
            {
                return JsonConvert.DeserializeObject<T>(str);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to deserialize a {(object)typeof(T).Name}. Error: '{(object)ex}'");
                return new T();
            }
        }

        private static void MigrateSaveLocation(string oldLocation, string newLocation)
        {
            string str = PlayerPrefs.GetString(oldLocation);
            if (str == null)
                return;
            PlayerPrefs.SetString(newLocation, str);
            PlayerPrefs.DeleteKey(oldLocation);
            PlayerPrefs.Save();
            Debug.Log("Migrated save model from " + oldLocation + " to " + newLocation);
        }

        public override string ToString() => JsonConvert.SerializeObject((object)this);
    }
}