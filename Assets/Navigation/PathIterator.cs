﻿using System.Collections.Generic;
using UnityEngine;

namespace TownSim.Navigation
{
    public class PathIterator
    {
        private readonly Node[] nodes;
        private readonly float[] distances;
        private int i;
        private float d0, d1;
        private float proximityToEnd;
        public float TotalDistance { get; private set; }
        public float CurrentDistance { get; private set; }
        public float T => Mathf.Clamp(CurrentDistance / TotalDistance, 0, 1);
        public bool AtEnd => nodes.Length <= 1 || TotalDistance <= 0 || T >= 1 || (Last.Position - CurrentPosition).sqrMagnitude < proximityToEnd * proximityToEnd;
        public Vector3 CurrentPosition { get; private set; }
        public Node NodeInfront { get; private set; }
        public Node NodeBehind { get; private set; }
        public Node First => nodes[0];
        public Node Last => nodes[nodes.Length - 1];
        public PathIterator(List<PathFinding.Point> path, float proximityToEnd = 0)
        {
            int length = path.Count;

            this.proximityToEnd = proximityToEnd;
            //if (!path[length - 1].Node.Accessible)
            //    length--;

            nodes = new Node[length];
            for (int i = 0; i < length; i++)
                nodes[i] = path[i].Node;

            distances = new float[length - 1];
            for (int i = 0; i < length - 1; i++)
            {
                distances[i] = Node.Distance(path[i].Node, path[i + 1].Node);
                TotalDistance += distances[i];
            }

            CurrentPosition = nodes.Length > 0 ? nodes[0].Position : Vector3.zero;
            NodeBehind = nodes.Length > 0 ? nodes[0] : null;
            NodeInfront = nodes.Length > 0 ? nodes[0] : null;

            if (distances.Length > 0)
                d1 = distances[0];
        }
        public void SetTime(float t) => SetDistance(t * TotalDistance);
        public void SetDistance(float distance)
        {
            if (distance == CurrentDistance)
                return;

            if (nodes.Length == 0)
            {
                i = 0;
                d0 = 0;
                d1 = 0;
                CurrentDistance = 0;
                CurrentPosition = Vector3.zero;
                NodeBehind = null;
                NodeInfront = null;
                return;
            }

            if (nodes.Length == 1 || distance < 0)
            {
                i = 0;
                d0 = 0;
                d1 = 0;
                CurrentDistance = 0;
                CurrentPosition = nodes[0].Position;
                NodeBehind = nodes[0];
                NodeInfront = nodes[0];
                return;
            }

            if (distance >= TotalDistance)
            {
                i = nodes.Length - 1;
                d0 = TotalDistance - distances[distances.Length - 1];
                d1 = TotalDistance;
                CurrentDistance = TotalDistance;
                CurrentPosition = nodes[nodes.Length - 1].Position;
                NodeBehind = nodes[nodes.Length - 2];
                NodeInfront = nodes[nodes.Length - 1];
                return;
            }

            CurrentDistance = distance;
            float sum = 0;
            for (int i = 0; i < nodes.Length - 1; i++)
            {
                float d = distances[i];
                d0 = sum;
                d1 = sum + d;
                if (distance < sum + d)
                {
                    float t = (distance - sum) / d;
                    CurrentPosition = Vector3.Lerp(nodes[i].Position, nodes[i + 1].Position, t);
                    NodeBehind = nodes[i];
                    NodeInfront = nodes[i + 1];
                    this.i = i;
                    return;
                }
                sum += d;
            }

            CurrentPosition = nodes[nodes.Length - 1].Position;
            NodeBehind = nodes[nodes.Length - 2];
            NodeInfront = nodes[nodes.Length - 1];
        }
        public float AdvanceDistance(float amount)
        {
            if (nodes == null)
                return 0;

            if (nodes.Length <= 1)
                return 0;

            float distance = Mathf.Clamp(CurrentDistance + amount, 0, TotalDistance);
            amount = distance - CurrentDistance;
            CurrentDistance = distance;

            if (amount > 0)
            {
                float sum = d0;
                for (; i < distances.Length; i++)
                {
                    float d = distances[i];
                    d0 = sum;
                    d1 = sum + d;
                    if (distance < sum + d)
                    {
                        float t = (distance - sum) / d;
                        CurrentPosition = Vector3.Lerp(nodes[i].Position, nodes[i + 1].Position, t);
                        NodeBehind = nodes[i];
                        NodeInfront = nodes[i + 1];
                        return amount;
                    }
                    sum += d;
                }
                i--;
                CurrentPosition = nodes[nodes.Length - 1].Position;
                NodeBehind = nodes[nodes.Length - 2];
                NodeInfront = nodes[nodes.Length - 1];

                return amount;
            }

            else if (amount < 0)
            {
                float sum = d1;
                for (; i >= 0; i--)
                {
                    float d = distances[i];
                    d0 = sum - d;
                    d1 = sum;
                    if (distance >= sum - d)
                    {
                        float t = (sum - distance) / d;
                        CurrentPosition = Vector3.Lerp(nodes[i].Position, nodes[i + 1].Position, 1 - t);
                        NodeBehind = nodes[i + 1];
                        NodeInfront = nodes[i];
                        return amount;
                    }
                    sum -= d;
                }
                CurrentPosition = nodes[0].Position;
                NodeBehind = nodes[1];
                NodeInfront = nodes[0];

                return amount;
            }
            else
                return 0;
        }
        public Vector3 CalculatePosition(float distance)
        {
            if (nodes.Length == 0)
                return Vector3.zero;

            if (nodes.Length == 1)
                return nodes[0].Position;

            if (distance < 0)
                return nodes[0].Position;

            if (distance >= TotalDistance)
                return nodes[nodes.Length - 1].Position;

            float sum = 0;

            for (int i = 0; i < nodes.Length - 1; i++)
            {
                float d = distances[i];
                if (distance < sum + d)
                {
                    float t = (distance - sum) / d;

                    return Vector3.Lerp(nodes[i].Position, nodes[i + 1].Position, t);
                }
                sum += d;
            }

            return nodes[nodes.Length - 1].Position;
        }

        public bool NearestPoint(Vector3 point, out Vector3 nearestPointOnPath, out float time)
        {
            float closestSqDistance = float.MaxValue;
            float sumDistance = 0;
            time = 0;
            nearestPointOnPath = default;
            bool found = false;

            for (int i = 0; i < nodes.Length - 1; i++)
            {
                Node n0 = nodes[i];
                Node n1 = nodes[i + 1];

                Vector3 lineA = n0.Position;
                Vector3 lineB = n1.Position;

                float dot = Vector3.Dot(point - lineA, lineB - lineA);
                float len_sq = (lineB - lineA).sqrMagnitude;

                float param = -1;
                if (len_sq != 0) //in case of 0 length line
                    param = dot / len_sq;

                Vector3 nearest = Vector3.Lerp(lineA, lineB, param);

                float t = (sumDistance + distances[i] * param) / TotalDistance;

                float sqDistance = Vector3.SqrMagnitude(nearest - point);

                if (sqDistance < closestSqDistance)
                {
                    time = t;
                    nearestPointOnPath = nearest;
                    closestSqDistance = sqDistance;
                    found = true;
                }

                sumDistance += distances[i];
            }

            return found;
        }
    }
}
