using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TownSim.UI
{
    public class LoadMenu : MenuPage
    {
        [SerializeField] private Transform scrollContent;
        [SerializeField] private SaveData prefab;

        [SerializeField] private Dialog dialog;
        [SerializeField] private InputFieldDialog inputFieldDialog;
        private string selectedName;
        private SaveData selected;



        [SerializeField] private Button loadButton, deleteButton, renameButton, duplicateButton;

        private void Awake()
        {
            loadButton.onClick.AddListener(LoadPressed);
            deleteButton.onClick.AddListener(DeletePressed);
            renameButton.onClick.AddListener(RenamePressed);
            duplicateButton.onClick.AddListener(DuplicatePressed);
            RefreshList();
        }


        public void ValidateButtons()
        {
            loadButton.interactable = selected != null;
            deleteButton.interactable = selected != null;
            renameButton.interactable = selected != null;
            duplicateButton.interactable = selected != null;
        }

        public override void Open()
        {
            RefreshList();
            gameObject.SetActive(true);
        }

        public override void Close()
        {
            Select(null);
            gameObject.SetActive(false);
        }

        [ContextMenu("Refresh List")]
        public void RefreshList()
        {
            selected = null;

            foreach (Transform child in scrollContent.transform)
                Destroy(child.gameObject);
            
            foreach(SaveData data in SaveData.InstantiateList(IO.IOUtils.SaveDirectory, "*." + IO.IOUtils.SaveExtension, prefab, scrollContent, Select, Load))
                if (data.NameLabel == selectedName)
                    Select(data);

            if (selected == null)
                selectedName = null;

            ValidateButtons();
        }

        public void Select(SaveData data)
        {
            if (selected != null)
                selected.Selected = false;
            selected = data;
            selectedName = data == null ? null : data.NameLabel;
            if (data != null) data.Selected = true;
            ValidateButtons();
        }

        public void Load(SaveData data)
        {
            Select(data);
            LoadPressed();
        }

        private void LoadPressed()
        {
            if (selected == null)
                return;

            if (!selected.LoadData(out IO.SaveData data) || !GameManager.Load(data))
            {
                dialog.Information("Load Failed", "Oh no. Failed to load the save file at " + selected.Path + ".");
            }
        }

        private void DeletePressed()
        {
            if (selected == null)
                return;

            dialog.Confirm(
                "Delete Save File",
                "Are you sure you want to permanently delete the save file called '" + selected.NameLabel + "' ? ",
                (object sender, Dialog.DialogEventArgs e) =>
                {
                    File.Delete(selected.Path);
                    Destroy(selected.gameObject);
                    dialog.Close();
                }
            );
        }

        private void RenamePressed()
        {
            if (selected == null)
                return;

            inputFieldDialog.Confirm(
                "Rename File",
                selected.NameLabel,
                (object sender, Dialog.DialogEventArgs e) =>
                {
                    string path = Path.Combine(IO.IOUtils.SaveDirectory, e.stringValue + "." + IO.IOUtils.SaveExtension);

                    if (!IO.IOUtils.ValidateFileName(IO.IOUtils.SaveDirectory, e.stringValue, IO.IOUtils.SaveExtension, out string message))
                    {
                        dialog.Information("Invalid File Name", message);
                    }
                    else
                    {
                        File.Move(selected.Path, path);
                        selected.Path = path;
                        inputFieldDialog.Close();
                    }
                }
            );
        }

        private void DuplicatePressed()
        {
            if (selected == null)
                return;

            inputFieldDialog.Confirm(
                "Duplicate File",
                selected.NameLabel,
                (object sender, Dialog.DialogEventArgs e) =>
                {
                    string path = Path.Combine(IO.IOUtils.SaveDirectory, e.stringValue + "." + IO.IOUtils.SaveExtension);

                    if (!IO.IOUtils.ValidateFileName(IO.IOUtils.SaveDirectory, e.stringValue, IO.IOUtils.SaveExtension, out string message))
                    {
                        dialog.Information("Invalid File Name", message);
                    }
                    else
                    {
                        File.Copy(selected.Path, path);
                        RefreshList();
                        inputFieldDialog.Close();
                    }
                }
            );
        }
    }
}
