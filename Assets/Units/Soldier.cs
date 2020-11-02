using System.Collections.Generic;
using TownSim.Items;
using TownSim.Navigation;
using TownSim.Units;
using UnityEngine;

namespace TownSim.AI
{
    public class Soldier : MonoBehaviour
    {
        public float aggroRadius;
        public LayerMask collisionMask;
        public float pathCooldown;
        private float pathTimer;

        private Human unit;
        private Agent agent;
        private Unit target;
        public int trajectoryCheckIncrements;

        private Vector3 trajectoryOrigin;
        private Vector3 trajectoryVelocity;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0, 0, .05f);
            Gizmos.DrawSphere(transform.position, aggroRadius);
            if (target)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(target.Nameplate(), Vector3.one * .1f);
            }

            bool DrawArc(Vector3 p0, Vector3 p1)
            {
                Gizmos.DrawLine(p0, p1);
                return false;
            }

            Ballistics.Arc(trajectoryOrigin, trajectoryVelocity, 0, Time.fixedDeltaTime, trajectoryCheckIncrements, DrawArc);

        }

        private void Awake()
        {
            unit = GetComponent<Human>();
            agent = GetComponent<Agent>();
            pathTimer = Random.value * pathCooldown;
        }

        // Update is called once per frame
        void Update()
        {
            if (!Map.started)
                return;

            if (unit.CurrentAction != Action.Idle)
                return;
            switch (unit.Mode)
            {
                case ItemMode.Empty:
                    break;

                case ItemMode.Sword:
                    {
                        //if (Input.GetKeyDown(KeyCode.X) && ScreenCast.MouseTerrain.Cast(out RaycastHit hit))
                        //    agent.SetDestination(hit.point);
                        //
                        //return;

                        if (unit.MeleeAttack(1, 1, 1, .5f, false))
                            return;

                        if (pathTimer > 0)
                        {
                            pathTimer -= Time.deltaTime;
                            return;
                        }

                        if (agent.HasPendingRequest)
                            return;

                        List<Vector3> targets = new List<Vector3>();
                        List<Unit> units = new List<Unit>();

                        foreach (Unit t in Unit.Units())
                        {
                            if (t == this)
                                continue;

                            if (t.faction == unit.faction)
                                continue;

                            if (t.CurrentHealth() <= 0)
                                continue;

                            float sqDist = (t.Center() - unit.Center()).sqrMagnitude;

                            if (sqDist > aggroRadius * aggroRadius)
                                continue;

                            targets.Add(t.transform.position);
                            units.Add(t);

                        }

                        float size = Map.Size;
                        int res = Map.NodeRes;


                        if (targets.Count == 0)
                            return;

                        pathTimer = pathCooldown;

                        if (targets.Count == 1)
                        {
                            agent.SetDestination(targets[0]);
                            return;
                        }

                        Vector3 myPos = transform.position;

                        int targetIndex = -1;

                        agent.LookFor((Node node) =>
                            {
                                for (int i = 0; i < targets.Count; i++)
                                {
                                    Vector3 t = targets[i];
                                    Vector2Int nearestVert = HexUtils.NearestVert(t, size, res);
                                    if (node.Vertex == nearestVert)
                                    {
                                        targetIndex = i;
                                        return true;
                                    }
                                }

                                return false;
                            }
                        );
                    }
                    break;
                case ItemMode.Bow:
                    {
                        target = null;
                        List<Unit> sorted = new List<Unit>();
                        foreach(Unit t in Unit.Units())
                        {
                            if (t == this)
                                continue;

                            if (t.faction == unit.faction)
                                continue;

                            if (t.CurrentHealth() <= 0)
                                continue;

                            float sqDist = (t.Center() - unit.Center()).sqrMagnitude;

                            if (sqDist > aggroRadius * aggroRadius)
                                continue;

                            int index = 0;
                            foreach(Unit s in sorted)
                            {
                                float d = (s.Center() - unit.Center()).sqrMagnitude;
                                if (sqDist < d)
                                    break;
                                index++;
                            }
                            sorted.Insert(index, t);
                        }

                        if (sorted.Count <= 0)
                            return;

                        foreach(Unit t in sorted)
                        {
                            if (InLineOfSight(t) && TrajectoryClear(t))
                            {
                                target = t;
                                break;
                            }
                        }

                        if (target)
                        {
                            unit.RangedAttack(target);
                        }

                    }
                    break;
            }
        }

        public bool InLineOfSight(Unit target)
        {
            Vector3 rangedOrigin = transform.position + Vector3.up * 1f;//unit.Center();
            Vector3 rayDirection = target.Center() - rangedOrigin;

            if (Physics.Raycast(rangedOrigin, rayDirection, out RaycastHit hit, aggroRadius, collisionMask, QueryTriggerInteraction.Ignore))
            {
                Unit hitUnit = hit.collider.GetComponentInParent<Unit>();
                if (hitUnit == null || hitUnit != target)
                    return false;
            }

            return true;
        }

        public bool TrajectoryClear(Unit target)
        {
            if (target == null)
                return false;

            trajectoryOrigin = transform.position + Vector3.up * 1f; //unit.Center();

            if (!Ballistics.SolveArcPitch(trajectoryOrigin, target.Center(), 45, out trajectoryVelocity))
                return false;

            float mag = trajectoryVelocity.magnitude;
            if (mag > unit.rangedAttackMaximumVelocity)
                return false;

            if (mag < unit.rangedAttackMinimumVelocity && Ballistics.SolveArcVector(trajectoryOrigin, unit.rangedAttackMinimumVelocity, target.Center(), target.Velocity(), -Physics.gravity.y, out Vector3 s0, out Vector3 s1) > 0)
                trajectoryVelocity = s0;

            bool arcObstructed = false;

            Collider[] colliders = new Collider[0];

            Ballistics.Arc(trajectoryOrigin, trajectoryVelocity, 0, Time.fixedDeltaTime, trajectoryCheckIncrements, (Vector3 p0, Vector3 p1) =>
            {
                if (Physics.Raycast(p0, p1 - p0, out RaycastHit hit, (p1 - p0).magnitude, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    Unit u = hit.collider.GetComponentInParent<Unit>();
                
                    if (u == unit)
                        return false;
                
                    if (u != target)
                        arcObstructed = true;
                
                    return true;
                }
                return false;
            });

            return !arcObstructed;
        }

    }
}
