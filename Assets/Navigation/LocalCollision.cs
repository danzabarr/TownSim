using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.Navigation
{

    public class LocalCollision : MonoBehaviour
    {

        private static List<LocalCollision> _agents;
        public static List<LocalCollision> agents
        {
            get
            {
                if (_agents == null)
                    _agents = new List<LocalCollision>(FindObjectsOfType<LocalCollision>());

                return _agents;
            }
        }

        public static void ResetMoved()
        {
            foreach (LocalCollision a in _agents)
                a.moved = false;
        }

        public float radius;
        public float speed;
        public float avoidance;
        public float lookahead;
        public Vector3 destination;
        private Vector3 direction;
        private bool moved;

        private Rigidbody rb;
        public Transform destinationTransform;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (!agents.Contains(this))
                agents.Add(this);
        }

        private void OnDestroy()
        {
            agents.Remove(this);
        }

        private void FixedUpdate()
        {

            if (destinationTransform)
                destination = destinationTransform.position;

            direction = (destination - transform.position).normalized;

            float speed = this.speed * Time.deltaTime;

            if (DetectObstacle(direction, speed + lookahead, out LocalCollision agent, out float nearestParam, out Vector2 collision))
            {
                Vector2 avoidanceDirection = (collision - agent.transform.position.xz()).normalized;

                direction += avoidanceDirection.x0y() * avoidance * (1 - Mathf.Clamp(nearestParam, 0.5f, 1));
                direction.Normalize();

            }

            Vector3 move = direction * speed;

            //if (DetectObstacle(direction, speed + lookahead, out agent, out nearestParam, out collision))
            //{
            //    move *= Mathf.Clamp(nearestParam, 0.01f, 1);
            //}
            rb.velocity = Vector3.zero;
            rb.MovePosition(transform.position + move);
            //transform.position += move;
            moved = true;
        }

        private bool DetectObstacle(Vector3 direction, float speed, out LocalCollision agent, out float nearestParam, out Vector2 collision)
        {
            agent = null;

            Vector2 lineA = transform.position.xz();
            Vector2 lineB = (transform.position + direction * speed).xz();
            float len_sq = (lineB - lineA).sqrMagnitude;


            nearestParam = float.PositiveInfinity;
            collision = default;

            foreach (LocalCollision a in agents)
            {
                if (a == this)
                    continue;

                Vector2 point = a.transform.position.xz();

                //if (!a.moved)
                //    point += a.direction.xz() * a.speed * Time.deltaTime;

                float dot = Vector2.Dot(point - lineA, lineB - lineA);

                float param = -1;
                if (len_sq != 0) //in case of 0 length line
                    param = dot / len_sq;

                if (param < 0)
                    continue;

                Vector2 pointOnLine = Vector2.LerpUnclamped(lineA, lineB, param);

                float dist_sq = (pointOnLine - point).sqrMagnitude;

                if (dist_sq > (radius + a.radius) * (radius + a.radius))
                    continue;

                if (agent == null || param < nearestParam)
                {
                    nearestParam = param;
                    agent = a;
                    collision = pointOnLine;
                }
            }

            return agent != null;
        }

        private void OnDrawGizmos()
        {
            if (destinationTransform)
                destination = destinationTransform.position;

            //Gizmos.color = new Color(1, 1, 1, .5f);
            //Gizmos.DrawSphere(transform.position, radius);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + (destination - transform.position).normalized * 10);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + direction);
        }
    }
}
