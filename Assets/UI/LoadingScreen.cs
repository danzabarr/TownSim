using UnityEngine;
using UnityEngine.UI;

namespace TownSim.UI
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private Text loadingMessage;
        [SerializeField] private Bar loadingBar;
        public string LoadingMessage
        {
            get => loadingMessage.text;
            set => loadingMessage.text = value;
        }

        public float LoadingProgress
        {
            get => loadingBar.Fill;
            set => loadingBar.Fill = value;
        }
    }
}
