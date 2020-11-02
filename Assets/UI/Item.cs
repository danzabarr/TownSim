using TMPro;
using TownSim.Items;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TownSim.UI
{
    public class Item : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Image overlay;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image icon;
        [SerializeField] private Image bar;
        [SerializeField] private Image barBackground;
        [SerializeField] private Gradient barColor;
        [SerializeField] private UnityEvent rightClick;

        public void HeldItemMode(bool enable)
        {
            overlay.gameObject.SetActive(!enable);
            background.enabled = !enable;
            RectTransform rt = background.GetComponent<RectTransform>();
            rt.pivot = enable ? Vector2.one / 2 : Vector2.zero;
        }

        public void AddButtonEvent(UnityAction l, UnityAction r)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(l);
            rightClick.RemoveAllListeners();
            rightClick.AddListener(r);
        }

        public void Display(int quantity, Sprite icon)
        {
            Display(quantity + "", icon);
        }

        public void Display(string text, Sprite icon)
        {
            bar.gameObject.SetActive(false);
            barBackground.gameObject.SetActive(false);
            this.text.gameObject.SetActive(true);
            this.icon.gameObject.SetActive(true);

            this.text.text = text;
            this.icon.sprite = icon;
        }

        public void Display(float fillAmount, Sprite icon)
        {
            text.gameObject.SetActive(false);
            bar.gameObject.SetActive(true);
            barBackground.gameObject.SetActive(true);
            this.icon.gameObject.SetActive(true);

            fillAmount = Mathf.Clamp(fillAmount, 0, 1);
            this.icon.sprite = icon;
            bar.fillAmount = fillAmount;
            bar.color = barColor.Evaluate(fillAmount);
        }

        public void DisplayNone()
        {
            text.gameObject.SetActive(false);
            bar.gameObject.SetActive(false);
            barBackground.gameObject.SetActive(false);
            icon.gameObject.SetActive(false);
        }

        public void Display(ItemType type, int quantity)
        {
            if (type == null || quantity <= 0)
                DisplayNone();

            else if (type.splittable)
                Display(quantity, type.icon);

            else
                Display((float)quantity / type.maxStack, type.icon);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
                rightClick.Invoke();
        }
    }
}
