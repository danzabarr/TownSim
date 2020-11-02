using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.Building
{
    public enum Type
    {
        Basic,
        Compound,
        Path
    }

    [CreateAssetMenu(fileName = "BuildingType", menuName = "Building/Type")]
    public class BuildingType : ScriptableObject
    {
        public new string name;
        public int cost;
    }
}
