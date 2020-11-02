using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    //say I call CreateInstance from another script and pass in a prefab object with a Test component

    private void Awake()
    {
        Debug.Log("Awake"); //is this line guaranteed to be called first
    }

    public static void CreateInstance(Test prefab)
    {
        Instantiate(prefab);
        Debug.Log("Instantiated"); // before this line?
    }
}
