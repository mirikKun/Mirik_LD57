using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.Scripts.Extensions
{
    public static class ListExtensions
    {
         static Random rng;
        

        public static bool IsNullOrEmpty<T>(this IList<T> list) {
            return list == null || !list.Any();
        }
        
        public static List<T> Clone<T>(this IList<T> list) {
            List<T> newList = new List<T>();
            foreach (T item in list) {
                newList.Add(item);
            }

            return newList;
        }

        public static void Swap<T>(this IList<T> list, int indexA, int indexB) {
            (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
        }

     
        public static List<T> Shuffle<T>(this List<T> list) {
            if (rng == null) rng = new Random();
            int count = list.Count;
            while (count > 1) {
                --count;
                int index = rng.Next(count + 1);
                (list[index], list[count]) = (list[count], list[index]);
            }
            return list;
        }

        /// <summary>
        /// Filters a collection based on a predicate and returns a new list
        /// containing the elements that match the specified condition.
        /// </summary>
        /// <param name="source">The collection to filter.</param>
        /// <param name="predicate">The condition that each element is tested against.</param>
        /// <returns>A new list containing elements that satisfy the predicate.</returns>
        public static IList<T> Filter<T>(this IList<T> source, Predicate<T> predicate) {
            List<T> list = new List<T>();
            foreach (T item in source) {
                if (predicate(item)) {
                    list.Add(item);
                }
            }
            return list;
        }
    }
}