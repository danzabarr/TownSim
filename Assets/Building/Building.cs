using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.Building
{
    public class Building : MonoBehaviour
    {
        public static int BlockSize = 2;
        public static int BlockHeight = 3;

        private static Material buildingValid;
        private static Material buildingInvalid;

        public static Material BuildingValid
        {
            get
            {
                if (buildingValid == null)
                    buildingValid = Resources.Load<Material>("Materials/BuildValid");

                return buildingValid;
            }
        }
        public static Material BuildingInvalid
        {
            get
            {
                if (buildingInvalid == null)
                    buildingInvalid = Resources.Load<Material>("Materials/BuildInvalid");

                return buildingInvalid;
            }
        }

    }
}
