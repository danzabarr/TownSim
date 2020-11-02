using System.Collections;
using System.Collections.Generic;
using System.IO;
using TownSim.IO;
using UnityEngine;
using UnityEngine.UI;

namespace TownSim.UI
{
    public class NewGameMenu : MenuPage
    {
        [SerializeField] private Dialog dialog;
        [SerializeField] private InputField nameField;
        [SerializeField] private InputField seedField;
        [SerializeField] private Dropdown generationSettings;

        private void Start()
        {
            PopulateGenerationSettingsDropdown();
        }

        private void PopulateGenerationSettingsDropdown()
        {
            generationSettings.options = new List<Dropdown.OptionData>();
            foreach (string s in GenerationSettings.SettingsNames)
                generationSettings.options.Add(new Dropdown.OptionData(s));
        }

        public void Create()
        {
            string name = nameField.text;
            string path = IOUtils.SaveDirectory + "/" + name + "." + IOUtils.SaveExtension;

            if (string.IsNullOrWhiteSpace(seedField.text) || !int.TryParse(seedField.text, out int seed))
            {
                seed = Random.Range(int.MinValue, int.MaxValue);
            }

            if (!GenerationSettings.Load(generationSettings.options[generationSettings.value].text, out GenerationSettings settings))
            {
                dialog.Information("Error", "Invalid generation settings.");
            }

            if (!IO.IOUtils.ValidateFileName(IOUtils.SaveDirectory, name, IOUtils.SaveExtension, out string message))
            {
                dialog.Information("Invalid File Name", message);
            }

            else
            {
                IO.SaveData data = new IO.SaveData()
                {
                    seed = seed,
                    generationSettings = settings.name,
                };

                data.SaveAs(path, true);

                GameManager.Load(data);
            }
        }
    }
}
