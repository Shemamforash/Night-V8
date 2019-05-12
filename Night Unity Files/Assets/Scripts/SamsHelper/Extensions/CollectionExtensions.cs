using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Extensions
{
	public static class CollectionExtensions
	{
		public static void AddOnce<T>(this List<T> list, T t)
		{
			if (list.Contains(t)) return;
			list.Add(t);
		}

		public static T RandomElement<T>(this T[] arr) => arr[Random.Range(0, arr.Length)];

		public static T RandomElement<T>(this List<T> arr) => arr[Random.Range(0, arr.Count)];

		public static T NextElement<T>(this List<T> list, int iterator) => list[list.NextIndex(iterator)];

		public static T PrevElement<T>(this List<T> list, int iterator) => list[list.PrevIndex(iterator)];

		public static int NextIndex<T>(this List<T> list, int iterator)
		{
			if (iterator + 1 == list.Count) return 0;
			return iterator + 1;
		}

		public static int PrevIndex<T>(this List<T> list, int iterator)
		{
			if (iterator - 1 == -1) return list.Count - 1;
			return iterator - 1;
		}

		public static T RemoveLast<T>(this List<T> list)
		{
			int endIndex = list.Count - 1;
			T   end      = list[endIndex];
			list.RemoveAt(endIndex);
			return end;
		}

		public static void Swap<T>(this List<T> list, int from, int to)
		{
			T temp = list[to];
			list[to]   = list[from];
			list[from] = temp;
		}

		public static void Print<T>(this List<T> list)
		{
			string listString = "";
			Helper.DoFor(list.Count, i =>
			{
				if (i != 0) listString += ", ";
				listString += list[i];
			});
			Debug.Log(listString);
		}

		public static void Print<T>(this T[] arr)
		{
			new List<T>(arr).Print();
		}

		public static bool Empty<T>(this ICollection<T> list) => list.Count == 0;

		public static bool Empty<T>(this T[] arr) => arr.Length == 0;

		public static void PrintIfEmpty<T>(this T[] arr, string message)
		{
			if (arr.Empty()) Debug.Log(message);
		}

		public static void PrintIfEmpty<T>(this ICollection<T> list, string message)
		{
			if (list.Empty()) Debug.Log(message);
		}

		public static bool Empty<T>(this Stack<T> stack) => stack.Count == 0;

		public static bool NotEmpty<T>(this Queue<T> queue) => queue.Count != 0;

		public static bool NotEmpty<T>(this List<T> list) => list.Count != 0;

		public static bool NotEmpty<T>(this T[] arr) => arr.Length != 0;

		public static bool NotEmpty<T>(this Stack<T> stack) => stack.Count != 0;

		public static T RemoveRandom<T>(this List<T> list)
		{
			int randomIndex = Random.Range(0, list.Count);
			T   element     = list[randomIndex];
			list.RemoveAt(randomIndex);
			return element;
		}

		public static void Shuffle<T>(this List<T> list)
		{
			List<T> randomList = new List<T>();
			while (list.Count > 0)
			{
				int removePosition = Random.Range(0, list.Count);
				T   element        = list[removePosition];
				randomList.Add(element);
				list.RemoveAt(removePosition);
			}

			list.Clear();
			list.AddRange(randomList);
		}

		public static void Shuffle<T>(this T[] arr)
		{
			List<T> list = new List<T>(arr);
			Shuffle(list);
			Helper.DoFor(arr.Length, i => arr[i] = list[i]);
		}

		public static T[] ValuesToArray<T>() where T : Enum => (T[]) Enum.GetValues(typeof(T)).Cast<T>();

		public static List<T> ValuesToList<T>() where T : Enum => ValuesToArray<T>().ToList();

		public static Dictionary<string, T> ToLookupDictionary<T>() where T : Enum
		{
			T[] values = ValuesToArray<T>();
			return values.ToDictionary(value => value.ToString());
		}
	}
}