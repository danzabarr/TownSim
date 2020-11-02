using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.Items
{
    [CreateAssetMenu(fileName = "ItemType", menuName = "Items/Type")]

    public class ItemType : ScriptableObject
    {
        [TextArea]
        public string description;
        public Item prefab;
        public int maxStack;
        public bool splittable;
        public Sprite icon;
        public ItemMode mode;
        public string boneName;
        public string meshName;
    }

    public enum ItemMode
    {
        Empty,
        Sword,
        Bow,
        Pickaxe,
        Axe
    }
}
