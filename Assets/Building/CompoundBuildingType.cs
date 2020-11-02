using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.Building
{
    [CreateAssetMenu(fileName = "CompoundBuildingType", menuName ="Building/Compound")]
    public class CompoundBuildingType : BuildingType
    {
        public Block[] blockPrefabs;
    }
}
