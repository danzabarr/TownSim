using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Item Filter", menuName ="Items/Filter")]
public class ItemFilter : ScriptableObject
{
    public bool blacklist;
    public string[] names;

}
