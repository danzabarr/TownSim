using UnityEngine;
using UnityEngine.Events;

namespace TownSim.UI
{
    public class MenuPage : MonoBehaviour
    {
        private UIContext context;

        public UnityEvent backward;
        public UnityEvent forward;

        public void OnEnable()
        {
            if (context == null)
                context = GetComponentInParent<UIContext>();
            if (context != null)
                context.GetActivePage();
        }

        public void OnDisable()
        {
            if (context == null)
                context = GetComponentInParent<UIContext>();
            if (context != null)
                context.GetActivePage();
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
        }

        public virtual void Update()
        {
            if (context == null)
                context = GetComponentInParent<UIContext>();

            if (context == null)
                return;

            if (this != context.Active)
                return;

            if (Input.GetButtonDown("Submit"))
                forward.Invoke();

            if (Input.GetButtonDown("Cancel"))
                backward.Invoke();
        }

        public void QuitGame()
        {
            Debug.Log("Application.Quit()");
            Application.Quit();
        }
    }
}
