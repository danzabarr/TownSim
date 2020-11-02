using TownSim.Items;
using UnityEngine;

namespace TownSim.UI
{
    public class Inventory : MonoBehaviour
    {
        private Items.Inventory contents;

        public Items.Inventory Contents
        {
            get => contents;
            set
            {
                
                if (contents != value)
                {
                    if (contents != null)
                        contents.changed -= Changed;

                    contents = value;

                    if (contents != null)
                        contents.changed += Changed;

                    Refresh();
                }
            }
        }

        public void Changed(object sender, Items.Inventory.InventoryChangedEventArgs e)
        {
            Refresh(e.index);
        }

        private void ValidateLength()
        {
            if (contents.Length != transform.childCount)
            {
                while (contents.Length > transform.childCount)
                {
                    Item item = Instantiate(ItemManager.Instance.UIItemPrefab, transform);
                    int index = transform.childCount - 1;
                    item.AddButtonEvent(() => { LeftClick(index); }, () => { RightClick(index); });
                }

                while (transform.childCount > contents.Length)
                    Destroy(transform.GetChild(transform.childCount - 1));
            }
        }

        public void Refresh()
        {
            ValidateLength();

            for (int i = 0; i < contents.Length; i++)
            {
                Item item = transform.GetChild(i).GetComponent<Item>();

                ItemType type = contents.Item(i);
                int quantity = contents.Quantity(i);

                item.Display(type, quantity);
            }
        }

        public void Refresh(int index)
        {
            if (index < 0 || index >= contents.Length)
                return;

            if (contents.Length != transform.childCount)
            {
                Refresh();
                return;
            }

            Item item = transform.GetChild(index).GetComponent<Item>();

            ItemType type = contents.Item(index);
            int quantity = contents.Quantity(index);

            item.Display(type, quantity);
        }

        public void LeftClick(int index)
        {
            ItemManager.LeftClick(contents, index);
            Refresh(index);
        }

        public void RightClick(int index)
        {
            ItemManager.RightClick(contents, index);
            Refresh(index);
        }
    }
}
