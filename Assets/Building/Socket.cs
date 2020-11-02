using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TownSim.Building
{
    public class Socket : MonoBehaviour
    {
        public Vector3Int pos;
        public Vector3Int Pos
        {
            get
            {
                Vector3Int pos = this.pos;
                int rotation = Parent == null ? 0 : Parent.rotation;
                for (int i = 0; i < rotation; i++)
                    pos = new Vector3Int(pos.z, pos.y, -pos.x);
                return Parent == null ? pos : Parent.pos + pos;
            }
        }

        public static void Rotate90(ref Vector3Int pos)
        {
            pos = new Vector3Int(pos.z, pos.y, -pos.x);
        }

        public static void Rotate(ref Vector3Int pos, int rotation)
        {
            rotation = (rotation % 4 + 4) % 4;
            for (int i = 0; i < rotation; i++)
                pos = new Vector3Int(pos.z, pos.y, -pos.x);
        }

        public Block Parent { get; private set; }

        private void Awake()
        {
            Parent = GetComponentInParent<Block>();
        }

        public Vector3 WorldPos
        {
            get
            {
                if (Parent == null)
                    return transform.position;

                return Parent.transform.TransformPoint(new Vector3(pos.x * Building.BlockSize, pos.y * Building.BlockHeight, pos.z * Building.BlockSize));


                //Vector3Int pos = Pos;
                //return Parent.parent.transform.TransformPoint(new Vector3(pos.x * Building.BlockSize, pos.y * Building.BlockHeight, pos.z * Building.BlockSize));
            }
        }

       //private void OnDrawGizmos()
       //{
       //    if (!gameObject.activeInHierarchy)
       //        return;
       //    Vector3 worldPos = WorldPos;
       //    Vector3Int pos = Pos;
       //
       //    Gizmos.color = Color.red;
       //    Gizmos.DrawSphere(worldPos, .1f);
       //    Handles.Label(worldPos, $"[{pos.x},{pos.y},{pos.z}]");
       //}
    }
}
