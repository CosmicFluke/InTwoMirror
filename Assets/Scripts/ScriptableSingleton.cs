using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A Unity Hack! Allows for creation of a singleton despite not being able to change constructor access level.
/// Cannot prevent use of the `new` keyword so use with care.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
{
    static T _instance = null;
    public static T Instance
    {
        get
        {
            if (!_instance)
                _instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
            return _instance;
        }
    }
}
