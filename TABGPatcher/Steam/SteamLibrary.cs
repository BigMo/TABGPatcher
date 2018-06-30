using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABGPatcher.Steam
{
    public class SteamLibrary
    {
        public int ID { get; private set; }
        public string Path { get; private set; }
        public AppState[] Apps { get; private set; }

        public SteamLibrary(int id, string path)
        {
            ID = id;
            Path = path;
            var steamapps = System.IO.Path.Combine(path, "steamapps");
            if (!Directory.Exists(steamapps))
                throw new Exception($"\"{steamapps}\" does not exist");

            Apps = Directory.GetFiles(steamapps, "*.acf").Select(x => new AppState(this, x)).ToArray();
            SteamManager.Logger.Log($"Found steamlibrary {ID} containing {Apps.Length} apps");
        }

        public override string ToString()
        {
            return $"{ID} \"{Path}\" {Apps.Length} apps";
        }
    }
}
