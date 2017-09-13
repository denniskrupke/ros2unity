using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// https://unity3d.com/learn/tutorials/modules/intermediate/scripting/extension-methods
/// It is common to create a class to contain all of your
/// extension methods. This class must be static.
/// </summary>
public static class ExtensionMethods
{
    
    /// <summary>
    /// Even though they are used like normal methods, extension
    /// methods must be declared static. Notice that the first
    /// parameter has the 'this' keyword followed by a Transform
    /// variable. This variable denotes which class the extension
    /// method becomes a part of.
    /// </summary>
    /// <param name="trans"></param>
    public static void ResetTransformation(this Transform trans)
    {
        trans.position = Vector3.zero;
        trans.localRotation = Quaternion.identity;
        trans.localScale = new Vector3(1, 1, 1);
    }

    /// <summary>
    /// creates a string with the x, y, z coordinates and spaces in between for matlab output streams
    /// uses F4 float formatting (0.0000)
    /// </summary>
    /// <param name="vector"></param>
    /// <returns>a string with the x, y, z coordinates and spaces in between</returns>
    public static string Vec3ToStringSpace(this Vector3 v)
    {
        return v.x.ToString("F4") + " " + v.y.ToString("F4") + " " + v.z.ToString("F4");
    }
    /// <summary>
    /// creates a string with the x, y, z coordinates and semicolons in between for SPSS output streams
    /// uses F4 float formatting (0.0000)
    /// </summary>
    /// <param name="vector"></param>
    /// <returns>a string with the x, y, z coordinates and semicolons in between</returns>
    public static string Vec3ToStringSemicolon(this Vector3 v)
    {
        return v.x.ToString("F4") + ";" + v.y.ToString("F4") + ";" + v.z.ToString("F4");
    }
    public static Vector3 FromFloat(this Vector3 v,float f)
    {
        v.x = f;
        v.y = f;
        v.z = f;
        return v;
    }

    public static void Shuffle<T>(IList<T> list)
    {
        for (var i = 0; i < list.Count; i++)
            Swap(list, i, UnityEngine.Random.Range(i, list.Count));
    }

    public static void Swap<T>(IList<T> list, int i, int j)
    {
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }

    
}