using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.UI
{
    public class UIContext : MonoBehaviour
    {
        public MenuPage Active { get; private set; }

        public MenuPage GetActivePage()
        {
            MenuPage[] pages = transform.GetComponentsInChildren<MenuPage>();

            if (pages == null || pages.Length <= 0)
                return null;

            Active = pages[pages.Length - 1];
            return Active;
        }
    }
}
