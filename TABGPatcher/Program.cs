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
        private static CrapLogger logger = new CrapLogger("TABGPatcher");

        private static string DearFileAnalyzers()
        {
            return "ily<3";
        }

        static void Main(string[] args)
        {
            Console.WriteLine(Properties.Resources.ASCII);

            TABGInstall tabg;
            try
            {
                tabg = Get();
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

        private static TABGInstall Get()
        {
            logger.Log("Trying to automatically detect TABG directory...");
            var tabg = GetFromDisk();
            //if (tabg != null)
            //    return tabg;
            logger.Log("Trying to get TABG directory from user...");
            return GetFromInput();
        }

        private static TABGInstall GetFromDisk()
        {
            try
            {
                logger.Log("Init SteamManager...");
                var steam = new SteamManager();
                var tabgApp = steam.SteamLibraries.SelectMany(x => x.Apps).FirstOrDefault(x => x.AppId == 823130 || x.Name == "Totally Accurate Battlegrounds");
                if (tabgApp == null)
                    throw new Exception("Could not find TABG installation");
                logger.Log("Init TABGInstall...");
                return new TABGInstall(tabgApp.Path);
            }catch(Exception ex)
            {
                logger.Error("Failed to patch TABG: {0}", ex.Message);
                Console.WriteLine("Source: {0}", ex.Source);
                Console.WriteLine(ex.StackTrace);
            }
            return null;
        }

        private static TABGInstall GetFromInput()
        {
            Console.WriteLine("Please enter the path of the game's installation directory.");
            TABGInstall tabg = null;
            var path = "";
            do
            {
                Console.Write("Directory: ");
                path = Console.ReadLine();
                try { tabg = new TABGInstall(path); }
                catch (Exception ex)
                {
                    logger.Error("Failed to initialize TABG from this folder: {0}", ex.Message);
                    Console.WriteLine("Source: {0}", ex.Source);
                    Console.WriteLine(ex.StackTrace);
                }
            } while (tabg == null);
            return tabg;
        }
    }
}
