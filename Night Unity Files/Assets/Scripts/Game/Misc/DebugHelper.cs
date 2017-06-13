using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Helper
{
    public static class DebugHelper
    {
		public static void Log<T>(List<T> aList){
			foreach(T t in aList){
				Debug.Log(t);
			}
		}
    }
}
