using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TownSim.Items
{
    [System.Serializable]
    public class Inventory
    {
        [SerializeField] private ItemType[] itemTypes;
        [SerializeField] private int[] itemQuantities;

        public Inventory(int length)
        {
            itemTypes = new ItemType[length];
            itemQuantities = new int[length];
        }

        public int Length => itemTypes.Length;
        public ItemType Item(int index) => index < 0 || index >= Length ? null : itemTypes[index];
        public int Quantity(int index) => index < 0 || index >= Length ? 0 : itemQuantities[index];

        public event EventHandler<InventoryChangedEventArgs> changed;

        public class InventoryChangedEventArgs : EventArgs
        {
            public int index;
            public ItemType oldType;
            public int oldQuantity;
        }

        protected virtual void OnChanged(InventoryChangedEventArgs e)
        {
            changed?.Invoke(this, e);
        }

        /// <summary>
        /// Returns the number of stacks added
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public int Add(int index, ItemType type, int quantity)
        {
            if (index < 0 || index >= Length)
                return 0;

            if (quantity <= 0 || type == null)
                return 0;

            if (itemTypes[index] != null && (itemTypes[index] != type || !type.splittable))
                return 0;

            int added = Mathf.Min(type.maxStack - itemQuantities[index], quantity);

            if (added <= 0)
                return 0;

            ItemType oldType = itemTypes[index];
            int oldQuantity = itemQuantities[index];

            itemTypes[index] = type;
            itemQuantities[index] += added;

            changed?.Invoke(this, new InventoryChangedEventArgs()
            {
                index = index,
                oldType = oldType,
                oldQuantity = oldQuantity
            });

            return added;
        }

        public bool Remove(int index, out ItemType type, out int quantity)
        {
            type = null;
            quantity = 0;
            if (index < 0 || index >= Length)
                return false;

            type = itemTypes[index];
            quantity = itemQuantities[index];

            itemTypes[index] = null;
            itemQuantities[index] = 0;

            if (type == null)
                return false;

            if (quantity <= 0)
                return false;

            changed?.Invoke(this, new InventoryChangedEventArgs()
            {
                index = index,
                oldType = type,
                oldQuantity = quantity
            });

            return true;
        }

        public void LeftClick(int index, ref ItemType heldType, ref int heldQuantity)
        {
            if (index < 0 || index >= Length)
                return;

            if (heldType == null && itemTypes[index] == null)
                return;

            if (heldQuantity <= 0)
                heldType = null;

            if (heldType != null && heldType == itemTypes[index] && heldQuantity >= heldType.maxStack && itemQuantities[index] >= heldType.maxStack)
                return;

            int oldQuantity = itemQuantities[index];
            ItemType oldType = itemTypes[index];

            if (heldType != itemTypes[index])
            {
                ItemType tempType = heldType;
                int tempQuantity = heldQuantity;
                heldType = itemTypes[index];
                heldQuantity = itemQuantities[index];
                itemTypes[index] = tempType;
                itemQuantities[index] = tempQuantity;
            }
            else if (itemQuantities[index] < heldType.maxStack)
            {
                int space = heldType.maxStack - itemQuantities[index];
                int toAdd = Mathf.Min(space, heldQuantity);
                itemQuantities[index] += toAdd;
                heldQuantity -= toAdd;
                if (heldQuantity <= 0)
                    heldType = null;
            }
            else
            {
                itemQuantities[index] = heldQuantity;
                heldQuantity = heldType.maxStack;
            }


            if (oldType != itemTypes[index] || oldQuantity != itemQuantities[index])
            {
                changed?.Invoke(this, new InventoryChangedEventArgs()
                {
                    index = index,
                    oldType = oldType,
                    oldQuantity = oldQuantity
                });
            }
        }

        public void RightClick(int index, ref ItemType heldType, ref int heldQuantity)
        {
            if (index < 0 || index >= Length)
                return;

            if (heldType == null && itemTypes[index] == null)
                return;

            if (heldQuantity <= 0)
                heldType = null;

            ItemType oldType = itemTypes[index];
            int oldQuantity = itemQuantities[index];

            if (heldType == null)
            {
                heldType = itemTypes[index];
                if (heldType.splittable && itemQuantities[index] > 1)
                {
                    int toAdd = itemQuantities[index] - itemQuantities[index] / 2;
                    heldQuantity = toAdd;
                    itemQuantities[index] -= toAdd;
                }
                else
                {
                    heldQuantity = itemQuantities[index];
                    itemTypes[index] = null;
                    itemQuantities[index] = 0;
                }
            }
            else if (itemTypes[index] == null)
            {
                itemTypes[index] = heldType;
                if (heldType.splittable && heldQuantity > 1)
                {
                    heldQuantity--;
                    itemQuantities[index] = 1;
                }
                else
                {
                    itemQuantities[index] = heldQuantity;
                    heldQuantity = 0;
                    heldType = null;
                }
            }
            else if (itemTypes[index] == heldType && heldType.splittable && itemQuantities[index] < heldType.maxStack)
            {
                itemQuantities[index]++;
                heldQuantity--;
                if (heldQuantity <= 0)
                    heldType = null;
            }
            else
            {
                ItemType tempType = heldType;
                int tempQuantity = heldQuantity;
                heldType = itemTypes[index];
                heldQuantity = itemQuantities[index];
                itemTypes[index] = tempType;
                itemQuantities[index] = tempQuantity;
            }

            if (oldType != itemTypes[index] || oldQuantity != itemQuantities[index])
            {
                changed?.Invoke(this, new InventoryChangedEventArgs()
                {
                    index = index,
                    oldType = oldType,
                    oldQuantity = oldQuantity
                });
            }
        }

        public int FirstIndex(ItemType type)
        {
            for (int i = 0; i < Length; i++)
                if (itemTypes[i] == type)
                    return i;
            return -1;
        }

        public int SpaceFor(ItemType type)
        {
            if (type == null)
                return 0;

            int added = 0;

            for (int i = 0; i < Length; i++)
            {
                if (itemTypes[i] == null)
                    added += type.maxStack;

                else if (type.splittable && itemTypes[i] == type)
                    added += type.maxStack - itemQuantities[i];
            }

            return added;
        }

        public int Add(ItemType type, int quantity)
        {
            if (type == null || quantity <= 0)
                return 0;

            int added = 0;

            if (type.splittable)
            {
                for (int i = 0; i < Length; i++)
                {
                    if (itemTypes[i] == type)
                    {
                        int oldQuantity = itemQuantities[i];
                        int space = type.maxStack - itemQuantities[i];
                        int toAdd = Mathf.Min(quantity, space);
                        quantity -= toAdd;
                        added += toAdd;
                        itemQuantities[i] += toAdd;

                        if (toAdd != 0)
                            changed?.Invoke(this, new InventoryChangedEventArgs()
                            {
                                index = i,
                                oldType = type,
                                oldQuantity = oldQuantity
                            });

                        if (quantity <= 0)
                            return added;
                    }
                }
            }

            for (int i = 0; i < Length; i++)
            {
                if (itemTypes[i] == null)
                {
                    int toAdd = Mathf.Min(quantity, type.maxStack);
                    quantity -= toAdd;
                    itemQuantities[i] += toAdd;
                    itemTypes[i] = type;
                    added += toAdd;

                    if (toAdd != 0)
                        changed?.Invoke(this, new InventoryChangedEventArgs()
                        {
                            index = i,
                            oldType = null,
                            oldQuantity = 0
                        });

                    if (quantity <= 0)
                        return added;
                }
            }

            return added;
        }

        public void Clear()
        {
            for (int i = 0; i < Length; i++)
            {
                ItemType type = itemTypes[i];
                int quantity = itemQuantities[i];

                itemTypes[i] = null;
                itemQuantities[i] = 0;

                if (type != null || quantity != 0)
                {
                    changed?.Invoke(this, new InventoryChangedEventArgs()
                    {
                        index = i,
                        oldType = type,
                        oldQuantity = quantity
                    });
                }
            }
        }

        public void List(out ItemType[] types, out int[] quantities)
        {
            types = new ItemType[Length];
            quantities = new int[Length];

            for (int i = 0; i < Length; i++)
            {
                types[i] = itemTypes[i];
                quantities[i] = itemQuantities[i];
            }
        }

        public int Quantity(ItemType type)
        {
            if (type == null)
                return 0;
            int quantity = 0;
            for (int i = 0; i < Length; i++)
                if (itemTypes[i] == type)
                    quantity += itemQuantities[i];
            return quantity;
        }

        public int Stacks(ItemType type)
        {
            if (type == null)
                return 0;
            int stacks = 0;
            for (int i = 0; i < Length; i++)
                if (itemTypes[i] == type)
                    stacks++;
            return stacks;
        }

        public int FullStacks(ItemType type)
        {
            if (type == null)
                return 0;
            int stacks = 0;
            for (int i = 0; i < Length; i++)
                if (itemTypes[i] == type && itemQuantities[i] >= type.maxStack)
                    stacks++;
            return stacks;
        }

        public int EmptySlots()
        {
            int slots = 0;
            for (int i = 0; i < Length; i++)
                if (itemTypes[i] == null)
                    slots++;
            return slots;
        }
    }
}
