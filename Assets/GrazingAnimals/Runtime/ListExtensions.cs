using System.Collections.Generic;

namespace GrazingAnimals
{
    internal static class ListExtensions
    {
        public static void FastRemoveAt<T>(this List<T> list, int index)
        {
            list[index] = list[^1];
            list.RemoveAt(list.Count - 1);
        }
    }
}