using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TownSim.UI
{
    public class InputFieldDialog : Dialog
    {
        [SerializeField] private InputField inputField;
        public override string GetStringValue() => inputField.text;

        public string Input
        {
            get => inputField.text;
            set => inputField.text = value;
        }
    }
}
