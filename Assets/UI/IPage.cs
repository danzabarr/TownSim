using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.UI
{
    public abstract class IPage : MonoBehaviour
    {
        public static List<IPage> openPages;

        public void OnEnable()
        {
            
        }

        public abstract void Submit();
        public abstract void Cancel();
    }
}