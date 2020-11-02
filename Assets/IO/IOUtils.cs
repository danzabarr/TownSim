﻿using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace TownSim.IO
{
    public static class IOUtils
    {
        public static string SaveDirectory = "saves";
        public static string SaveExtension = "save";

        public static List<string> List(string path, string searchPattern)
        {
            List<string> files = new List<string>();
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                    Debug.Log("Created directory: " + path);
                }
                catch (System.Exception)
                {
                    Debug.LogError("Failed to create directory: " + path);
                    return files;
                }
            }

            foreach (string file in Directory.GetFiles(path, searchPattern))
                files.Add(file);

            return files;
        }

        public static bool Load<T>(string path, out T data)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(path, FileMode.Open);
                data = (T)bf.Deserialize(file);
                Debug.Log("Loaded: " + data + " from: " + path);
                file.Close();
                return true;
            }
            catch (System.Exception)
            {
                data = default;
                Debug.LogError("Failed to load: " + data + " from: " + path);
                return false;
            }
        }

        public static bool Save(object data, string path)
        {
            string directory = System.IO.Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                    Debug.Log("Created directory: " + directory);
                }
                catch (System.Exception)
                {
                    Debug.LogError("Failed to create directory: " + path);
                    return false;
                }
            }

            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(path);
                bf.Serialize(file, data);
                Debug.Log("Saved: " + data + " to: " + path);
                file.Close();
                return true;
            }
            catch (System.Exception)
            {
                Debug.LogError("Failed to save: " + data + " to: " + path);
                return false;
            }
        }

        public static bool ValidateFileName(string directoryPath, string name, string extension, out string message)
        {
            string path = Path.Combine(directoryPath, name + "." + extension);

            if (string.IsNullOrWhiteSpace(name))
            {
                message = "File name is empty";
                return false;
            }
            if (File.Exists(path))
            {
                message = "File with that name already exists";
                return false;
            }
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                message = "File name contains invalid characters.";
                return false;
            }

            message = null;
            return true;
        }
    }
}
