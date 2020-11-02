using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TownSim.Items
{
    public class ItemManager : MonoBehaviour
    {
        [SerializeField] private UI.Item uiItemPrefab;
        public UI.Item UIItemPrefab => uiItemPrefab;

        private static ItemManager instance;
        public static ItemManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<ItemManager>();
                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private static UI.Item heldUI;
        private static ItemType heldType;
        private static int heldQuantity;
        private static Dictionary<string, ItemType> ItemTypes;

        public static bool IsHolding { get; private set; }

        public static void SetHeldItem(ItemType type, int quantity)
        {
            heldType = type;
            heldQuantity = quantity;

            if (heldUI == null)
            {
                heldUI = Instantiate(Instance.uiItemPrefab, Instance.transform);
                heldUI.HeldItemMode(true);
            }

            if (type == null || quantity <= 0)
                heldUI.gameObject.SetActive(false);

            else
            {
                heldUI.Display(type, quantity);
                heldUI.gameObject.SetActive(true);
            }
        }

        private void Update()
        {
            if (heldUI != null)
            {
                heldUI.transform.position = Input.mousePosition;
                bool overUI = EventSystem.current.IsPointerOverGameObject();
                if (Input.GetMouseButtonDown(0) && !overUI)
                {
                    if (Player.Instance.Unit.EmitItem(heldType, heldQuantity, out _))
                        SetHeldItem(null, 0);
                }
            }
            if (heldType != null)
                IsHolding = true;
            else if (!Input.GetMouseButton(0))
                IsHolding = false;


            if (ScreenCast.MouseItem.Cast(out Item item, true))
            {
                
            }
        }

        public static void LeftClick(Inventory inventory, int index)
        {
            inventory.LeftClick(index, ref heldType, ref heldQuantity);

            SetHeldItem(heldType, heldQuantity);
        }

        public static void RightClick(Inventory inventory, int index)
        {
            inventory.RightClick(index, ref heldType, ref heldQuantity);

            SetHeldItem(heldType, heldQuantity);
        }

        public static ItemType Type(string name)
        {
            if (ItemTypes == null)
            {
                ItemTypes = new Dictionary<string, ItemType>();
                ItemType[] types = Resources.LoadAll<ItemType>("Item Types");

                foreach (ItemType type in types)
                {
                    if (!ItemTypes.ContainsKey(type.name))
                        ItemTypes.Add(type.name, type);
                }
            }

            return ItemTypes[name];
        }

        public static Item Item(string name, int quantity)
        {
            ItemType type = Type(name);
            Item item = Instantiate(type.prefab);
            item.quantity = quantity;
            return item;
        }
    }
}
