using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.Building
{
    public enum Condition
    {
        Any,
        Same,
        Different,
        Filled,
        Empty,
    }

    public class BuildingConditional : MonoBehaviour
    {
        public Block parent;

        public Condition n;
        public Condition e;
        public Condition s;
        public Condition w;
        public Condition ne;
        public Condition nw;
        public Condition se;
        public Condition sw;

        public void CheckConditions()
        {
            if (parent == null)
                parent = GetComponentInParent<Block>();

            if (parent == null)
                return;

            if (!parent.Neighbour(new Vector3Int(0, 0, 1), n))
                gameObject.SetActive(false);

            else if (!parent.Neighbour(new Vector3Int(0, 0, -1), s))
                gameObject.SetActive(false);

            else if (!parent.Neighbour(new Vector3Int(1, 0, 0), e))
                gameObject.SetActive(false);

            else if (!parent.Neighbour(new Vector3Int(-1, 0, 0), w))
                gameObject.SetActive(false);

            else if (!parent.Neighbour(new Vector3Int(1, 0, 1), ne))
                gameObject.SetActive(false);

            else if (!parent.Neighbour(new Vector3Int(-1, 0, 1), nw))
                gameObject.SetActive(false);

            else if (!parent.Neighbour(new Vector3Int(1, 0, -1), se))
                gameObject.SetActive(false);

            else if (!parent.Neighbour(new Vector3Int(-1, 0, -1), sw))
                gameObject.SetActive(false);

            else
                gameObject.SetActive(true);
        }
    }
}