using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TABGPatcher.VDF;

namespace TABGPatcher.Steam
{
    public class SteamManager
    {
        private const string REG_PATH = @"HKEY_CURRENT_USER\Software\Valve\Steam";
        private const string REG_KEY = "SteamPath";

        public string InstallPath { get; private set; }
        public SteamLibrary[] SteamLibraries { get; private set; }
        public static CrapLogger Logger { get; private set; } = new CrapLogger("Steam");

        public SteamManager()
        {
            InstallPath = (string)Registry.GetValue(REG_PATH, REG_KEY, null);
            if (string.IsNullOrEmpty(InstallPath))
                throw new Exception("Could not find Steam installation");
            Logger.Log("Found steam installation");

            var dir = Path.Combine(InstallPath, "steamapps");
            if (!Directory.Exists(dir))
                throw new Exception($"\"{dir}\" does not exist");

            var dirs = new List<SteamLibrary>();
            dirs.Add(new SteamLibrary(0, InstallPath));

            var vdf = Path.Combine(dir, "libraryfolders.vdf");
            if (File.Exists(vdf))
            {
                var libs = new VDFFile(vdf);
                if (libs.RootElements.Count > 0 && libs.RootElements.Any(x => x.Name == "LibraryFolders"))
                {
                    foreach (var e in libs["LibraryFolders"].Children)
                    {
                        int id = 0;
                        if (int.TryParse(e.Name, out id))
                            dirs.Add(new SteamLibrary(id, e.Value.Replace(@"\\", @"\")));
                    }
                }
            }
            SteamLibraries = dirs.ToArray();
        }
    }
}
