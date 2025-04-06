using System;
using UnityEngine;

namespace Project.Scripts.Utils.SerialisedTypes
{
    [Serializable]
    public class SerializableType : ISerializationCallbackReceiver
    {
        [SerializeField] private string _assemblyQualifiedName = string.Empty;

        public Type Type { get; private set; }

        public void OnBeforeSerialize()
        {
            _assemblyQualifiedName = Type?.AssemblyQualifiedName ?? _assemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {
            if (!TryGetType(_assemblyQualifiedName, out var type))
            {
                Debug.LogError($"Type {_assemblyQualifiedName} not found");
                return;
            }

            Type = type;
        }

        public static bool TryGetType(string typeString, out Type type)
        {
            type = Type.GetType(typeString);
            return type != null || !string.IsNullOrEmpty(typeString);
        }

        // Implicit conversion from SerializableType to Type
        public static implicit operator Type(SerializableType sType) => sType.Type;

        // Implicit conversion from Type to SerializableType
        public static implicit operator SerializableType(Type type) => new() { Type = type };
    }
}