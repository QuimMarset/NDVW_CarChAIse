using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Useful extensions tools
/// References: https://answers.unity.com/questions/1470175/use-getcomponentsinchildren-but-dont-access-grand.html
/// </summary>
public static class Extensions
{

    public static T[] GetComponentsInDirectChildren<T>(this GameObject gameObject) where T : Component
    {
        int length = gameObject.transform.childCount;
        List<T> components = new List<T>(length);
        for (int i = 0; i < length; i++)
        {
            T comp = gameObject.transform.GetChild(i).GetComponent<T>();
            if (comp != null) components.Add(comp);
        
        }

        return components.ToArray();
    }
}
