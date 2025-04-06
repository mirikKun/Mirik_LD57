using System;
using Project.Scripts.Utils.SerialisedTypes.Extensions;
using UnityEngine;

namespace Project.Scripts.Utils.SerialisedTypes.Attributes
{
    public class TypeFilterAttribute : PropertyAttribute {
        public Func<Type, bool> Filter { get; }
        
        public TypeFilterAttribute(Type filterType) {
            Filter = type => !type.IsAbstract &&
                             !type.IsInterface &&
                             !type.IsGenericType &&
                             type.InheritsOrImplements(filterType);
        }
    }
}