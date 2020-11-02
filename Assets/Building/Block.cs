using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.Building
{
    public class Block : MonoBehaviour
    {
        public Vector3Int pos;
        public int rotation;
        public CompoundBuilding parent;
        public Socket[] Sockets { get; private set; }
        public Type type;
        public List<Collider> colliders { get; private set; }

        public enum Type
        {
            Block,
            SlopedRoof,
            Crenellation,
        }

        public int Rotation
        {
            get => rotation;
            set
            {
                rotation = (value % 4 + 4) % 4;
            }
        }

        private void Awake()
        {
            Sockets = GetComponentsInChildren<Socket>();
            colliders = new List<Collider>();
            foreach (Collider c in GetComponentsInChildren<Collider>())
                if (!c.isTrigger)
                    colliders.Add(c);
        }

        public bool Neighbour(Vector3Int offset, out Block neighbour)
        {
            if (parent == null)
            {
                neighbour = null;
                return false;
            }

            for (int i = 0; i < rotation; i++)
                offset = new Vector3Int(offset.z, offset.y, -offset.x);

            return parent.parts.TryGetValue(offset + pos, out neighbour);
        }

        public bool Neighbour(Vector3Int offset, Condition condition)
        {
            if (condition == Condition.Any)
                return true;

            for (int i = 0; i < rotation; i++)
                offset = new Vector3Int(offset.z, offset.y, -offset.x);

            bool exists = parent.parts.TryGetValue(offset + pos, out Block neighbour);

            if (condition == Condition.Same)
                return exists && type == neighbour.type;

            if (condition == Condition.Different)
                return !exists || type != neighbour.type;

            if (condition == Condition.Filled)
                return exists;

            if (condition == Condition.Empty)
                return !exists;

            return false;
        }

        private void Start()
        {
            TempMaterial.CacheAll(gameObject);
        }

        public void PlacingMode(bool enabled)
        {
            if (enabled)
            {
                foreach(Outline o in gameObject.GetComponentsInChildren<Outline>())
                    o.enabled = true;

                //TempMaterial.SetAll(gameObject, Building.BuildingValid);
                gameObject.SetLayerRecursively(LayerMask.GetMask("Buildings"), LayerMask.NameToLayer("Building Preview"));
                gameObject.SetActiveRecursively(LayerMask.GetMask("Path Mask"), false);
                //foreach (Collider c in colliders)
                //    c.isTrigger = true;
            }
            else
            {
                TempMaterial.RevertAll(gameObject, false);
                gameObject.SetLayerRecursively(LayerMask.GetMask("Building Preview"), LayerMask.NameToLayer("Buildings"));
                gameObject.SetActiveRecursively(LayerMask.GetMask("Path Mask"), true);
                //foreach (Collider c in colliders)
                //    c.isTrigger = false;
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            Debug.Log(other);
        }

        public void SetConstructionPhase(int phase)
        {

        }

        //public void OnDrawGizmos()
        //{
        //    Gizmos.color = Color.green;
        //    for (int i = 0; i < 9; i++)
        //    {
        //        if (i == 4)
        //            continue;
        //
        //        if (Neighbour(new Vector3Int(i % 3 - 1, 0, i / 3 - 1), Condition.Same))
        //        {
        //            Gizmos.DrawSphere(parent.transform.TransformPoint(transform.localPosition + new Vector3((i % 3 - 1) * 1.5f, 7, (i / 3 - 1) * 1.5f)), .1f);
        //        }
        //    }
        //}
    }
}
