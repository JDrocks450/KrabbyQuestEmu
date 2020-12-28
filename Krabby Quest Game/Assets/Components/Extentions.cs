using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Components.Extention
{
    public static class Extentions
    {
        public static T GetComponentInChildren<T>(this Transform obj, bool excludeParent = true)
        {
            var transforms = new HashSet<Transform>(obj.GetComponentsInChildren<Transform>());
            if (excludeParent)
                transforms.Remove(obj);
            foreach(var t in transforms)
            {
                var comp = t.GetComponentInChildren<T>();
                if (comp != null) return comp;
            }
            return default(T);
        }
        public static Transform Find(this Transform t, string name, bool recursiveSearch = true)
        {
            var obj = t.Find(name);
            if (obj != null) return obj;
            foreach (Transform transform in t)
            {
                obj = transform.Find(name, true);
                if (obj != null) return obj;
            }
            return null;
        }
    }
}
