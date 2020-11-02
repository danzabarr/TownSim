using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TownSim.UI
{
    public class SaveData : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Text nameLabel, creationTimeLabel, lastWriteTimeLabel;
        [SerializeField] private Button button;
        [SerializeField] private GameObject selectedMark;
        private string path;
        
        public string Path
        {
            get => path;
            set
            {
                path = value;

                bool fileExists = path != null && System.IO.File.Exists(path);

                if (fileExists)
                {
                    nameLabel.text = System.IO.Path.GetFileNameWithoutExtension(path);
                    creationTimeLabel.text = System.IO.File.GetCreationTime(path).ToString("yyyy-MM-dd\\THH:mm:ss\\Z");
                    lastWriteTimeLabel.text = System.IO.File.GetLastWriteTime(path).ToString("yyyy-MM-dd\\THH:mm:ss\\Z");
                }
                else
                {
                    nameLabel.text = "";
                    creationTimeLabel.text = "";
                    lastWriteTimeLabel.text = "";
                }
            }
        }

        public string NameLabel => nameLabel.text;
        public string CreationTimeLabel => creationTimeLabel.text;
        public string LastWriteTimeLabel => lastWriteTimeLabel.text;

        public bool LoadData(out IO.SaveData data)
        {
            return IO.SaveData.Load(path, out data);
        }

        public bool Selected
        {
            get => selectedMark.activeSelf;
            set => selectedMark.SetActive(value);
        }

        public delegate void Select(SaveData data);
        public delegate void Load(SaveData data);

        private Load load;

        public static List<SaveData> InstantiateList(string directoryPath, string searchPattern, SaveData prefab, Transform parent, Select select, Load load)
        {
            List<SaveData> list = new List<SaveData>();
            foreach (string path in IO.IOUtils.List(directoryPath, searchPattern))
            {
                SaveData saveData = Instantiate(prefab, parent);
                saveData.Path = path;
                list.Add(saveData);
                saveData.button.onClick.AddListener(() => select(saveData));
                saveData.load = load;
            }

            return list;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount > 1)
                load(this);
        }
    }
}
