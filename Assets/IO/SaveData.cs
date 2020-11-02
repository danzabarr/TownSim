using System;
using System.IO;

namespace TownSim.IO
{
    [Serializable]
    public sealed class SaveData
    {
        public string Path { get; private set; }
        public string FileName => System.IO.Path.GetFileNameWithoutExtension(Path);
        public DateTime LastWriteTime => File.GetLastWriteTime(Path);
        public DateTime CreationTime => File.GetCreationTime(Path);

        public bool Save()
        {
            return IOUtils.Save(this, Path);
        }

        public bool SaveAs(string path, bool overwrite)
        {
            if (!overwrite)
            {
                if (File.Exists(path))
                    return false;
            }

            if (!IOUtils.Save(this, path))
                return false;

            Path = path;
            return true;
        }

        public static bool Load(string path, out SaveData data)
        {
            if (!IOUtils.Load(path, out data))
                return false;

            data.Path = path;
            return true;
        }


        public string generationSettings;
        public int seed;
    }
}
