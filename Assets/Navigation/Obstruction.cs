using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.Navigation
{
    public class Obstruction : MonoBehaviour
    {
        private bool hasStarted;
        private bool hasObstructed;
        private void Start()
        {
            if (!hasObstructed)
            {
                Obstruct();
                hasObstructed = true;
            }
            hasStarted = true;
        }

        private void OnEnable()
        {
            if (hasStarted && !hasObstructed)
            {
                Obstruct();
                hasObstructed = true;
            }
        }

        private void OnDisable()
        {
            if (hasObstructed)
            {
                Revert();
                hasObstructed = false;
            }
        }

        public Vector2Int Vert => HexUtils.HexRound(HexUtils.CartToVert(transform.position.x, transform.position.z, Map.Size, Map.NodeRes));

        public void Obstruct()
        {
            if (Map.nodes.TryGetValue(Vert, out Node node))
                node.Obstructions++;
        }

        public void Revert()
        {
            if (Map.nodes.TryGetValue(Vert, out Node node))
                node.Obstructions--;
        }

    }
}
