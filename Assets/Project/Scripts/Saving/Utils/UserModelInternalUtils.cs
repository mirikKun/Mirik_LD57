using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Scripts.Saving.Utils
{
    public static class UserModelInternalUtils
    {
        private static readonly Dictionary<System.Type, List<string>> m_Cache =
            new Dictionary<System.Type, List<string>>();

        public static string UserModelSaveDataKey<T>()
        {
            return UserModelInternalUtils.UsedModelSaveDataKeyWithName(
                UserModelInternalUtils.ActualLocation(typeof(T)));
        }

        public static List<string> UserModelLoadDataKeys<T>()
        {
            return UserModelInternalUtils.FindLoadLocations<T>();
        }

        private static string UsedModelSaveDataKeyWithName(string name) => "UserModel-" + name;

        private static string ActualLocation(System.Type type) => type.Name;

        private static List<string> FindLoadLocations<T>()
        {
            System.Type type = typeof(T);
            List<string> loadLocations1;
            if (UserModelInternalUtils.m_Cache.TryGetValue(type, out loadLocations1))
                return loadLocations1;
            List<string> loadLocations2 = new List<string>();
            loadLocations2.Insert(0, UserModelSaveDataKey<T>());
            m_Cache[type] = loadLocations2;
            return loadLocations2;
        }

 

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload() => UserModelInternalUtils.m_Cache.Clear();

        internal static void ResetCacheForTests() => UserModelInternalUtils.m_Cache.Clear();
    }
}