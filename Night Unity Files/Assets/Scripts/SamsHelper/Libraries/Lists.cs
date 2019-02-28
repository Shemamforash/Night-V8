using System.Collections.Generic;

namespace SamsHelper.Libraries
{
    public static class Lists
    {
        public static void AddOnce<T>(this List<T> list, T t)
        {
            if (list.Contains(t)) return;
            list.Add(t);
        }   
    }
}