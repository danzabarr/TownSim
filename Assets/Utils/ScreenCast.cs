using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCast : MonoBehaviour
{
    public static Instance MouseTerrain { get; private set; } = new Instance();
    public static Instance CenterTerrain { get; private set; } = new Instance();
    public static Instance MouseScene { get; private set; } = new Instance();
    public static Instance MouseTriggers { get; private set; } = new Instance();
    public static Instance MouseUnit { get; private set; } = new Instance();
    public static Instance MouseItem { get; private set; } = new Instance();

    private void Awake()
    {
        MouseTerrain = new Instance(ScreenPosition.Mouse, default, 1000, LayerMask.GetMask("Terrain"), QueryTriggerInteraction.Ignore);
        CenterTerrain = new Instance(ScreenPosition.Center, default, 1000, LayerMask.GetMask("Terrain"), QueryTriggerInteraction.Ignore);
        MouseScene = new Instance(ScreenPosition.Mouse, default, 1000, LayerMask.GetMask("Terrain", "Units", "Buildings", "Trees", "Resources", "Items"), QueryTriggerInteraction.Ignore);
        MouseUnit = new Instance(ScreenPosition.Mouse, default, 1000, LayerMask.GetMask("Terrain", "Buildings", "Units", "Unit Selection"), QueryTriggerInteraction.Ignore);
        MouseItem = new Instance(ScreenPosition.Mouse, default, 1000, LayerMask.GetMask("Terrain", "Buildings", "Items", "Item Selection"), QueryTriggerInteraction.Ignore);
        MouseTriggers = new Instance(ScreenPosition.Mouse, default, 1000, LayerMask.GetMask("Terrain", "Buildings", "Units", "Items"), QueryTriggerInteraction.Collide);
    }

    private void LateUpdate()
    {
        MouseTerrain.Reset();
        CenterTerrain.Reset();
        MouseScene.Reset();
        MouseTriggers.Reset();
        MouseUnit.Reset();
        MouseItem.Reset();
    }

    public enum ScreenPosition
    {
        Mouse,
        Center,
        Fixed,
        Relative
    }

    public class Instance
    {
        private readonly ScreenPosition positionType;
        private readonly Vector2 positionVector;
        private readonly float maxDistance;
        private readonly int layerMask;
        private readonly QueryTriggerInteraction queryTriggerInteraction;

        private bool cast;
        private bool returned;
        private RaycastHit hit;
        public Instance() { }
        public Instance(ScreenPosition positionType, Vector2 positionVector, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            this.positionType = positionType;
            this.positionVector = positionVector;
            this.maxDistance = maxDistance;
            this.layerMask = layerMask;
            this.queryTriggerInteraction = queryTriggerInteraction;
        }
        public bool Cast(out RaycastHit hit)
        {
            if (!cast)
                Update();

            hit = this.hit;
            return returned;
        }
        public bool Cast<T>(out T component, bool inParent = false) 
        {
            if (!cast)
                Update();

            component = default;
            if (returned)
                component = inParent ? hit.collider.GetComponentInParent<T>() : hit.collider.GetComponent<T>();
            return component != null;
        }

        public bool Cast<T>(out RaycastHit hit, out T component, bool inParent = false) 
        {
            if (!cast)
                Update();

            component = default;
            hit = this.hit;
            if (returned)
                component = inParent ? hit.collider.GetComponentInParent<T>() : hit.collider.GetComponent<T>();
            return component != null;
        }

        public void Update()
        {
            Vector3 position = positionVector;
            switch (positionType)
            {
                case ScreenPosition.Mouse:
                    position = Input.mousePosition;
                    break;
                case ScreenPosition.Center:
                    position = new Vector3(Screen.width / 2f, Screen.height / 2f);
                    break;
                case ScreenPosition.Relative:
                    position = new Vector3(Screen.width * positionVector.x, Screen.height * positionVector.y);
                    break;
            }
            returned = Physics.Raycast(Camera.main.ScreenPointToRay(position), out hit, maxDistance, layerMask, queryTriggerInteraction);
            cast = true;
        }

        public void Reset()
        {
            cast = false;
        }
    }
}
