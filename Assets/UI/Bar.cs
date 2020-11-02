using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TownSim.UI
{
    public class Bar : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private Gradient color;

        public float Fill
        {
            get => fill.fillAmount;
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                fill.fillAmount = value;
                fill.color = color.Evaluate(value);
            }
        }
    }
}