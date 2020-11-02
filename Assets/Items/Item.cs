using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TownSim.Items
{
    [RequireComponent(typeof(Rigidbody))]
    public class Item : MonoBehaviour
    {
        public bool pickedUp;
        public ItemType type;
        public int quantity;

        private Collider[] colliders;
        private Rigidbody rb;
        private Outline outline;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>();
            outline = GetComponentInChildren<Outline>();
            ShowOutline = false;
        }

        public void DisableColliders()
        {
            foreach (Collider c in colliders)
                c.enabled = false;
        }

        public void DisableRigidbody()
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.isKinematic = true;
        }

        public bool ShowOutline
        {
            get => outline.enabled;
            set => outline.enabled = value;
        }
    }
}
