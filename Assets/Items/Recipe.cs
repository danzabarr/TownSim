using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.Items
{
    [CreateAssetMenu(fileName = "Recipe", menuName = "Items/Recipe")]
    public class Recipe : ScriptableObject
    {
        public float duration;

        [Icon(64, 64)] public ItemType i_00;
        [Icon(64, 64)] public ItemType i_10;
        [Icon(64, 64)] public ItemType i_20;
        [Icon(64, 64)] public ItemType i_01;
        [Icon(64, 64)] public ItemType i_11;
        [Icon(64, 64)] public ItemType i_21;
        [Icon(64, 64)] public ItemType i_02;
        [Icon(64, 64)] public ItemType i_12;
        [Icon(64, 64)] public ItemType i_22;

        public int q_00;
        public int q_10;
        public int q_20;
        public int q_01;
        public int q_11;
        public int q_21;
        public int q_02;
        public int q_12;
        public int q_22;
    }
}