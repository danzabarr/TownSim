using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.Building
{
    public class BuildingSystem : MonoBehaviour
    {
        public static BuildingSystem Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<BuildingSystem>();
                return instance;
            }
        }

        private static BuildingSystem instance;
        public static bool IsHolding => Instance.held != null;

        public Block held;
        private Block prefab;
        public CompoundBuilding current;
        public float rotationSpeed = 50;

        private float heldRotationDegrees;
        private int heldRotationTurns;

        public Block[] prefabs;
        public bool attached;

        public void SetBuildingPart(Block prefab)
        {
            this.prefab = prefab;
            if (held != null)
            {
                Destroy(held.gameObject);
            }

            held = Instantiate(prefab);
            held.PlacingMode(true);
        }

        private Building selected;

        public enum Mode
        {
            SelectBuilding,
            Build,
            Destroy
        }

        public Mode mode;

        private void Update()
        {
            if (mode == Mode.SelectBuilding)
            {
                if (held != null)
                {
                    Destroy(held.gameObject);
                    held = null;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    Building building = null;

                    if (ScreenCast.MouseScene.Cast(out RaycastHit hit))
                    {
                        building = hit.collider.GetComponentInParent<Building>();

                        if (building != null)
                            OutlineEffect.EnableOutline(building.gameObject, true);
                    }

                    if (selected != null && selected != building)
                        OutlineEffect.EnableOutline(selected.gameObject, false);

                    selected = building;
                }
                return;
            }

            if (mode == Mode.Build)
            {
                for (int i = 0; i < 9; i++)
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                        SetBuildingPart(prefabs[i]);

                if (Input.GetKeyDown(KeyCode.Return))
                    current = null;

                if (held != null)
                {
                    if (ScreenCast.MouseTriggers.Cast(out Socket socket))
                    {
                        if (!attached)
                        {
                            float angleDifference = held.transform.rotation.eulerAngles.y - socket.Parent.parent.transform.rotation.eulerAngles.y;

                            heldRotationTurns = (Mathf.RoundToInt(angleDifference / 90) % 4 + 4) % 4;
                        }

                        if (Input.GetButtonDown("Rotate Building"))
                        {
                            heldRotationTurns += (int)Input.GetAxisRaw("Rotate Building");
                        }
                        held.Rotation = heldRotationTurns;

                        current = socket.Parent.parent;
                        Vector3Int socketPos = socket.Pos;
                        held.transform.position = current.transform.TransformPoint(new Vector3(socketPos.x * Building.BlockSize, socketPos.y * Building.BlockHeight, socketPos.z * Building.BlockSize));
                        held.transform.rotation = current.transform.rotation * Quaternion.AngleAxis(held.Rotation * 90, Vector3.up);

                        if (Input.GetMouseButtonDown(0))
                            current.AddPart(socketPos, held.Rotation, prefab, out Block placed);
                        attached = true;
                    }

                    else if (ScreenCast.MouseTerrain.Cast(out RaycastHit hit))
                    {
                        held.transform.position = hit.point;
                        held.transform.rotation *= Quaternion.AngleAxis(Time.deltaTime * rotationSpeed * Input.GetAxis("Rotate Building"), Vector3.up);
                        //held.Rotation = 0;
                        if (Input.GetMouseButtonDown(0))
                        {
                            current = new GameObject("Building").AddComponent<CompoundBuilding>();
                            current.transform.position = held.transform.position;
                            current.transform.rotation = held.transform.rotation;
                            current.AddPart(Vector3Int.zero, 0, prefab, out Block placed);
                        }
                        attached = false;
                    }
                    else
                    {
                        attached = false;
                    }
                }
            }

            return;
        }

        private void OnDrawGizmos()
        {
            if (held)
                Gizmos.DrawSphere(held.transform.position, .05f);
        }
    }
}