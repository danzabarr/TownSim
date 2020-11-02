using TownSim.Units;
using UnityEngine;

namespace TownSim.Navigation
{
    [DisallowMultipleComponent]
    public class Agent : MonoBehaviour
    {
        [Header("Pathing")]
        [System.NonSerialized]
        private Node current, destinationNode;
        public LayerMask destinationCheckMask;
        public float destinationSpread;
        public float proximityToEnd;
        private PathFinding.Request pathRequest;

        [Header("Movement")]
        public float movementSpeed;
        public float rotationSpeed;
        public float maxSpeed, maxForce, acceleration;

        private Rigidbody rb;
        private Animator animator;
        private Unit unit;
        public bool useRigidbodyMovement;

        public Vector3 lookAheadOrigin;
        public float lookAheadDistance;
        public float lookAheadRadius;
        private bool isWaiting;
        private RaycastHit[] lookAheadResults = new RaycastHit[4];
        public bool Paused;

        public bool HasPath => Iterator != null;
        public bool HasRequest => pathRequest != null;
        public bool HasPendingRequest => pathRequest != null && !pathRequest.Completed && !pathRequest.Cancelled;
        public bool IsIdle => !HasPath && unit.CurrentAction == Action.Idle;

        public PathIterator Iterator { get; private set; }
        public PathFinding.Result Result { get; private set; }
        public Vector3 Destination { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            unit = GetComponent<Unit>();
        }

        private void FixedUpdate()
        {
            if (HasPath && unit.CurrentAction == Action.Idle)
            {
                float distance = Vector3.Distance(transform.position, Destination);
                animator.SetBool("running", false);
                if (Iterator.AtEnd)
                {
                    ClearPath();
                }

                else
                {

                    float move = movementSpeed * Time.deltaTime;
                    Vector3 targetPoint = transform.position;

                    if (Iterator.NearestPoint(transform.position, out Vector3 n, out float t))
                    {
                        Iterator.SetTime(t);
                        Iterator.AdvanceDistance(move);
                        targetPoint = Iterator.CurrentPosition;
                        //float dist = Vector3.Magnitude(n - transform.position);
                        //
                        //if (dist > move)
                        //{
                        //    targetPoint += (n - transform.position) / dist * move;
                        //}
                        //
                        //else
                        //{
                        //    targetPoint = n;
                        //    move -= dist;
                        //
                        //    pathIterator.AdvanceDistance(move);
                        //
                        //    targetPoint = pathIterator.CurrentPosition;
                        //
                        //}

                        Vector3 origin = transform.TransformPoint(lookAheadOrigin);
                        Vector3 direction = (targetPoint - transform.position).normalized;

                        isWaiting = false;



                        int r = Physics.SphereCastNonAlloc(origin, lookAheadRadius, direction, lookAheadResults, lookAheadDistance, LayerMask.GetMask("Unit Base"), QueryTriggerInteraction.Ignore);

                        for (int i = 0; i < r; i++)
                        {
                            RaycastHit hit = lookAheadResults[i];

                            Agent unit = hit.collider.GetComponent<Agent>();

                            if (unit == null)
                                continue;

                            if (unit == this)
                                continue;

                            //if (unit.Faction() != Faction())
                            //    continue;

                            if (unit.isWaiting)
                                continue;

                            isWaiting = true;
                            break;
                        }

                        if (!isWaiting)
                        {
                            rb.MovePosition(targetPoint);
                            float rotation = 90 - Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
                            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(rotation, Vector3.up), rotationSpeed * Time.deltaTime);
                            animator.SetBool("running", true);
                        }
                            //MoveToPoint(targetPoint);
                    }
                }
                //
                //
                //
                //if (pathIterator.NearestPoint(transform.position, out Vector3 n, out float t))
                //    if (Vector3.SqrMagnitude(n - pathIterator.CurrentPosition) > 2)
                //        pathIterator.SetTime(t);
                //
                //pathIterator.AdvanceDistance(MovementSpeed * Time.deltaTime);
                //
                //float distance = Vector3.Distance(transform.position, DestinationPoint);
                //bool atTarget = distance < .2f;
                //
                //if (atTarget)
                //{
                //    Cancel();
                //}
                //
                //else
                //{
                //    Vector3 forward = (pathIterator.CurrentPosition - transform.position);
                //    Vector3 localForward = transform.InverseTransformDirection(forward);
                //    runX = localForward.x;
                //    runY = localForward.z;
                //
                //    if (Mathf.Abs(forward.xz().magnitude) > MovementSpeed * Time.deltaTime * .2f)
                //    {
                //        float rotation = 90 - Mathf.Atan2(forward.z, forward.x) * Mathf.Rad2Deg;
                //        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(rotation, Vector3.up), rotationSpeed);
                //
                //        if (useRigidbodyMovement && rb)
                //        {
                //            //move = forward.normalized * Mathf.Min(movementSpeed, distance) * Time.deltaTime;
                //
                //            //rb.MovePosition(pathIterator.CurrentPosition);
                //            //rb.AddForce(move, ForceMode.Acceleration);
                //            MoveToPoint(pathIterator.CurrentPosition);
                //            running = true;
                //        }
                //        else
                //        {
                //            transform.position = pathIterator.CurrentPosition;
                //        }
                //    }
                //}

                animator.SetFloat("runSpeed", movementSpeed);

            }
        }
        private void OnDrawGizmosSelected()
        {
            if (HasPath)
            {
                Gizmos.color = Color.white;
                PathFinding.DrawPath(pathRequest.Path, true);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(Iterator.CurrentPosition, .5f);
            }
        }

        public void MoveToPoint(Vector3 targetPos)
        {
            Vector3 dist = targetPos - transform.position;
            dist.y = 0; // ignore height differences
                        // calc a target vel proportional to distance (clamped to maxVel)
            Vector3 tgtVel = Vector3.ClampMagnitude(movementSpeed * dist, maxSpeed);
            // calculate the velocity error
            Vector3 error = tgtVel - rb.velocity;
            // calc a force proportional to the error (clamped to maxForce)
            Vector3 force = Vector3.ClampMagnitude(acceleration * error, maxForce);
            rb.AddForce(force, ForceMode.Acceleration);

            if (force.sqrMagnitude > 1)
            {
                float rotation = 90 - Mathf.Atan2(force.z, force.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(rotation, Vector3.up), rotationSpeed * Time.deltaTime);
                animator.SetBool("running", true);
                //CurrentAction = Action.Idle;

                Vector3 localForward = transform.InverseTransformDirection(force);
                animator.SetFloat("runX", localForward.x);
                animator.SetFloat("runY", localForward.z);
            }
        }

        public bool FindDestination(Vector3 target, out Vector3 point)
        {
            return Map.Instance.FindPosition(target, destinationSpread, 100, CheckDestination, out point);
        }

        public bool CheckDestination(Vector3 p)
        {
            return true;
            //float radius = capsuleCollider.radius;
            //Vector3 start = p + Vector3.up * radius;
            //Vector3 end = p + Vector3.up * (capsuleCollider.height - radius);
            //
            //return !Physics.CheckCapsule(start, end, radius, destinationCheckMask, QueryTriggerInteraction.Collide);
        }

        public void SetDestination(Vector3 position, float maxCost = 1000, float takeExistingPaths = 1, bool checkDestination = false)
        {
            //Debug.Log("Setting destination for " + gameObject.name);
            if (checkDestination && FindDestination(position, out Vector3 foundPosition))
            {
                //Debug.Log($"Placing a destination for {name} at [{foundPosition.x},{foundPosition.y},{foundPosition.z}]");
                position = foundPosition;
            }

            pathRequest = new PathFinding.Request()
            {
                start = transform.position,
                startNode = current,
                end = position,
                maxCost = maxCost,
                takeExistingPaths = takeExistingPaths,
                allowInaccessibleEnd = false,
                callback = () =>
                {
                    destinationNode = null;
                    Destination = pathRequest.start;
                    Iterator = null;
                    Result = pathRequest.Result;
                    if (Result == PathFinding.Result.Success)
                    {
                        Iterator = new PathIterator(pathRequest.Path, proximityToEnd);
                        Destination = Iterator.Last.Position;
                    }
                },
            };
            pathRequest.Queue();
        }

        public void SetDestination(Node node, float maxCost = 1000, float takeExistingPaths = 1)
        {
            pathRequest = new PathFinding.Request()
            {
                start = transform.position,
                startNode = current,
                end = Destination,
                endNode = destinationNode,
                maxCost = maxCost,
                takeExistingPaths = takeExistingPaths,
                allowInaccessibleEnd = true,
                callback = () =>
                {
                    destinationNode = null;
                    Destination = pathRequest.start;
                    Iterator = null;
                    Result = pathRequest.Result;
                    if (Result == PathFinding.Result.Success)
                    {
                        Iterator = new PathIterator(pathRequest.Path, proximityToEnd);
                        destinationNode = node;
                        Destination = Iterator.Last.Position;
                    }
                },
            };
            pathRequest.Queue();
        }

        public void LookFor(PathFinding.Match match, Vector3 matchTowards, float maxCost = 1000, float takeExistingPaths = 1, bool allowInaccessibleEnd = false)
        {
            pathRequest = new PathFinding.Request()
            {
                start = transform.position,
                startNode = current,
                end = matchTowards,
                match = match,
                maxCost = maxCost,
                takeExistingPaths = takeExistingPaths,
                matchTowardsEnd = true,
                allowInaccessibleEnd = allowInaccessibleEnd,
                callback = () =>
                {
                    destinationNode = null;
                    Destination = pathRequest.start;
                    Iterator = null;
                    Result = pathRequest.Result;
                    if (Result == PathFinding.Result.Success)
                    {
                        Iterator = new PathIterator(pathRequest.Path, proximityToEnd);
                        Destination = Iterator.Last.Position;
                    }
                }
            };
            pathRequest.Queue();
        }

        public void LookFor(PathFinding.Match match, float maxCost = 200, float takeExistingPaths = 1, bool allowInaccessibleEnd = false)
        {
            pathRequest = new PathFinding.Request()
            {
                start = transform.position,
                startNode = current,
                match = match,
                maxCost = maxCost,
                takeExistingPaths = takeExistingPaths,
                matchTowardsEnd = false,
                allowInaccessibleEnd = allowInaccessibleEnd,
                callback = () =>
                {
                    destinationNode = null;
                    Destination = pathRequest.start;
                    Iterator = null;
                    Result = pathRequest.Result;
                    if (Result == PathFinding.Result.Success)
                    {
                        Iterator = new PathIterator(pathRequest.Path, proximityToEnd);
                        Destination = Iterator.Last.Position;
                    }
                }
            };
            pathRequest.Queue();
        }

        public void ClearPath()
        {
            pathRequest?.Cancel();
            pathRequest = null;
            Result = PathFinding.Result.AtDestination;
            Iterator = null;
            Destination = transform.position;
        }
    }
}
