using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.Building
{
    public class CompoundBuilding : Building
    {
        public Dictionary<Vector3Int, Block> parts = new Dictionary<Vector3Int, Block>();

        public static bool ValidPlacement(Vector3 position, Quaternion rotation, Block prefab)
        {
            return true;
        }

        public bool AddPart(Vector3Int pos, int rotation, Block prefab, out Block part)
        {
            part = null;

            if (parts.ContainsKey(pos))
                return false;

            Vector3 posF = new Vector3(pos.x * BlockSize, pos.y * BlockHeight, pos.z * BlockSize);

            if (!ValidPlacement(transform.TransformPoint(posF), transform.rotation * Quaternion.AngleAxis(rotation * 90, Vector3.up), prefab))
                return false;

            part = Instantiate(prefab, transform);
            parts.Add(pos, part);
            part.transform.localPosition = posF;
            part.transform.localRotation = Quaternion.AngleAxis(rotation * 90, Vector3.up);
            part.parent = this;
            part.pos = pos;
            part.Rotation = rotation;

            foreach (Block p in parts.Values)
                foreach (Socket socket in p.Sockets)
                {
                    Vector3Int sPos = socket.Pos;

                    if (sPos == pos)
                        socket.gameObject.SetActive(false);

                    else if (sPos.y > 0 && !parts.ContainsKey(sPos + Vector3Int.down))
                        socket.gameObject.SetActive(false);
                }

            foreach (Socket socket in part.Sockets)
            {
                if (parts.ContainsKey(socket.Pos))
                    socket.gameObject.SetActive(false);
            }

            foreach (StandardBlock c in GetComponentsInChildren<StandardBlock>())
                c.CheckConditions();

            return true;
        }

        public bool RemovePart(Vector3Int pos, out Block part)
        {
            if (parts.TryGetValue(pos, out part) && parts.Remove(pos))
            {
                foreach (Block p in parts.Values)
                    foreach (Socket socket in p.Sockets)
                    {
                        Vector3Int sPos = socket.Pos;
                        if (sPos == pos && (sPos.y <= 0 || parts.ContainsKey(sPos + Vector3Int.down)))
                            socket.gameObject.SetActive(true);
                    }

                return true;
            }
            return false;
        }
    }
}
