using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TABGPatcher.Steam;
using TABGPatcher.TABG;

namespace TABGPatcher
{
    class Program
    {
        private static string DearFileAnalyzers()
        {
            return "ily<3";
        }
        static void Main(string[] args)
        {
            Console.WriteLine(Properties.Resources.ASCII);

            var logger = new CrapLogger("TABGPatcher");
            try
            {
                logger.Log("Init SteamManager...");
                var steam = new SteamManager();
                var tabgApp = steam.SteamLibraries.SelectMany(x => x.Apps).FirstOrDefault(x => x.AppId == 823130 || x.Name == "Totally Accurate Battlegrounds");
                if (tabgApp == null)
                    throw new Exception("Could not find TABG installation");
                logger.Log("Init TABGInstall...");
                var tabg = new TABGInstall(tabgApp.Path);
                logger.Log("Patch!");
                tabg.Patch();
                logger.Log("Done");
            }
            catch (Exception ex)
            {
                logger.Error("Failed to patch TABG: {0}", ex.Message);
                Console.WriteLine("Source: {0}", ex.Source);
                Console.WriteLine(ex.StackTrace);                
            }
            logger.Info("Press enter to exit");
            Console.ReadLine();
        }
    }
}
