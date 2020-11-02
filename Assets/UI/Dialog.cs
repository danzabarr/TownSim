using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TownSim.UI
{
    public class Dialog : MenuPage
    {
        public event EventHandler<DialogEventArgs> valueReturned;

        public enum DisposeAction
        {
            None,
            Destroy,
            Disable
        }

        public DisposeAction disposeAction = DisposeAction.Destroy;

        public class DialogEventArgs : EventArgs
        {
            public int returnState;
            public int intValue;
            public float floatValue;
            public string stringValue;
            public object objectValue;
        }

        [SerializeField] private Button cancelOption, primaryOption, secondaryOption;
        [SerializeField] private Text titleText, contentText;

        public string Title
        {
            get => titleText.text;
            set => titleText.text = value;
        }

        public string Content
        {
            get => contentText.text;
            set => contentText.text = value;
        }

        public Button CancelOption => cancelOption;
        public Button PrimaryOption => primaryOption;
        public Button SecondaryOption => secondaryOption;

        public virtual int GetIntValue() => 0;
        public virtual float GetFloatValue() => 1;
        public virtual string GetStringValue() => null;
        public virtual object GetObjectValue() => null;

        public virtual bool ValidateCancelOption() => true;
        public virtual bool ValidatePrimaryOption() => true;
        public virtual bool ValidateSecondaryOption() => true;

        public void ValidateInput()
        {
            cancelOption.enabled = ValidateCancelOption();
            primaryOption.enabled = ValidatePrimaryOption();
            secondaryOption.enabled = ValidateSecondaryOption();
        }

        public string CancelOptionLabel
        {
            get => cancelOption.GetComponentInChildren<Text>().text;
            set => cancelOption.GetComponentInChildren<Text>().text = value;
        }

        public string PrimaryOptionLabel
        {
            get => primaryOption.GetComponentInChildren<Text>().text;
            set => primaryOption.GetComponentInChildren<Text>().text = value;
        }

        public string SecondaryOptionLabel
        {
            get => secondaryOption.GetComponentInChildren<Text>().text;
            set => secondaryOption.GetComponentInChildren<Text>().text = value;
        }

        public void CancelButtonPressed()
        {
            Close();
        }

        public void PrimaryButtonPressed()
        {
            valueReturned?.Invoke(this, new DialogEventArgs()
            {
                returnState = 1,
                intValue = GetIntValue(),
                floatValue = GetFloatValue(),
                stringValue = GetStringValue(),
                objectValue = GetObjectValue()
            });
        }

        public void SecondaryButtonPressed()
        {
            valueReturned?.Invoke(this, new DialogEventArgs()
            {
                returnState = 2,
                intValue = GetIntValue(),
                floatValue = GetFloatValue(),
                stringValue = GetStringValue(),
                objectValue = GetObjectValue()
            });
        }

        public void ClearListeners()
        {
            valueReturned = null;
        }

        public void Information(string title, string content)
        {
            Title = title;
            Content = content;
            gameObject.SetActive(true);
            CancelOption.gameObject.SetActive(false);
            PrimaryOption.gameObject.SetActive(true);
            SecondaryOption.gameObject.SetActive(false);
            PrimaryOptionLabel = "Close";
            valueReturned += (object sender, DialogEventArgs e) => Close();
        }

        public void Confirm(string title, string content, EventHandler<DialogEventArgs> de)
        {
            Title = title;
            Content = content;
            gameObject.SetActive(true);
            CancelOption.gameObject.SetActive(true);
            PrimaryOption.gameObject.SetActive(true);
            SecondaryOption.gameObject.SetActive(false);
            PrimaryOptionLabel = "Confirm";
            CancelOptionLabel = "Cancel";
            valueReturned += de;
        }

        public override void Open()
        {
            gameObject.SetActive(true);
        }

        public override void Close()
        {
            ClearListeners();

            switch (disposeAction)
            {
                case DisposeAction.None:
                    break;

                case DisposeAction.Destroy:
                    Destroy(gameObject);
                    break;

                case DisposeAction.Disable:
                    gameObject.SetActive(false);
                    break;
            }
        }
    }
}
