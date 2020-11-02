using System.Collections;
using System.Collections.Generic;
using TownSim.Navigation;
using UnityEngine;

namespace TownSim.Units
{
    public class Deer : Unit
    {
        public float maxCost;
        public float takeExistingPaths;
        public float wanderRadius;
        private bool RandomPlace(out Vector3 pos)
        {
            pos = transform.position;

            for (int i = 0; i < 100; i++)
            {
                pos = Map.Instance.OnMesh(transform.position.xz() + Random.insideUnitCircle * wanderRadius);
                if (ValidPlace(pos))
                    return true;
            }

            return false;
        }

        private bool ValidPlace(Vector3 pos)
        {
            return true;
        }

        private void Update()
        {
            if (Agent.IsIdle && RandomPlace(out Vector3 pos))
                Agent.SetDestination(pos);
        }
    }
}
