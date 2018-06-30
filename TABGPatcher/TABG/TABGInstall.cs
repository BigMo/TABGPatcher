using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TABGPatcher.TABG
{
    public class TABGInstall
    {
        public string InstallPath { get; private set; }
        public FileInfo[] ManagedAssemblies { get; private set; }
        public FileInfo[] PluginAssemblies { get; private set; }
        public FileInfo AssemblyCSharp { get; private set; }
        public FileInfo AssemblyCSharpFirstpass { get; private set; }

        private CrapLogger logger = new CrapLogger("TABG");

        private FileInfo[] allAssemblies;
        private TABGPatch[] patches = new TABGPatch[]
        {
            new Equ8Patch()
        };

        private static string GetSubfolder(string parent, string sub)
        {
            var path = Path.Combine(parent, sub);
            if (!Directory.Exists(path))
                throw new Exception($"Could not find {sub} subfolder");
            return path;
        }

        public TABGInstall(string installPath)
        {
            InstallPath = installPath;
            if (string.IsNullOrEmpty(InstallPath))
                throw new Exception("Could not find TABG installation");
            logger.Log("Found TABG installation, ");

            var data = GetSubfolder(InstallPath, "TotallyAccurateBattlegrounds_Data");
            var managed = GetSubfolder(data, "Managed");
            var plugins = GetSubfolder(data, "Plugins");
            logger.Log("Found subdirectories {0}, {1} and {2}", new DirectoryInfo(data).Name, new DirectoryInfo(managed).Name, new DirectoryInfo(plugins).Name);

            ManagedAssemblies = Directory.GetFiles(managed, "*.dll").Select(x => new FileInfo(x)).ToArray();
            PluginAssemblies = Directory.GetFiles(plugins, "*.dll").Select(x => new FileInfo(x)).ToArray();
            allAssemblies = ManagedAssemblies.Concat(PluginAssemblies).ToArray();
            logger.Log("Found {0} managed assemblies and {1} plugins ({2} total), ", ManagedAssemblies.Length, PluginAssemblies.Length, allAssemblies.Length);

            AssemblyCSharp = ManagedAssemblies.FirstOrDefault(x => x.Name == "Assembly-CSharp.dll");
            AssemblyCSharpFirstpass = ManagedAssemblies.FirstOrDefault(x => x.Name == "Assembly-CSharp-firstpass.dll");
            if (AssemblyCSharp == null)
                throw new Exception("Could not find Assembl-CSharp.dll");
            if (AssemblyCSharpFirstpass == null)
                throw new Exception("Could not find Assembl-CSharp-firstpass.dll");
            logger.Log("Found {0} and {1}", AssemblyCSharp.Name, AssemblyCSharpFirstpass.Name);
        }

        private class Resolver : IAssemblyResolver
        {
            public TABGInstall Install { get; private set; }
            public ModuleContext Context { get; set; }

            private List<AssemblyDef> assemblies;

            public Resolver(TABGInstall install)
            {
                Install = install;
                assemblies = new List<AssemblyDef>();
            }

            public bool AddToCache(AssemblyDef asm)
            {
                if (!assemblies.Contains(asm))
                {
                    assemblies.Add(asm);
                    return true;
                }
                return false;
            }

            public void Clear()
            {
                assemblies.Clear();
            }

            public bool Remove(AssemblyDef asm)
            {
                if (assemblies.Contains(asm))
                {
                    assemblies.Remove(asm);
                    return true;
                }
                return false;
            }

            public AssemblyDef Resolve(IAssembly assembly, ModuleDef sourceModule)
            {
                var existing = assemblies.FirstOrDefault(x => x.FullName == assembly.FullName);
                if (existing != null)
                    return existing;

                var file = Install.ManagedAssemblies.FirstOrDefault(x => x.Name.Substring(0, x.Name.Length - x.Extension.Length) == assembly.Name.ToString());
                if (file == null)
                    return null;

                var asm = AssemblyDef.Load(File.ReadAllBytes(file.FullName), Context);
                assemblies.Add(asm);

                return asm;
            }
        }

        public void Patch()
        {
            var resolver = new Resolver(this);
            var context = new ModuleContext(resolver);
            resolver.Context = context;
            bool modified = false;

            logger.Log($"Loading {AssemblyCSharp.Name}...");
            var assemblyCSharp = ModuleDefMD.Load(File.ReadAllBytes(AssemblyCSharp.FullName), context);
            logger.Log(" -> Loaded: {0}", assemblyCSharp.FullName);
            resolver.AddToCache(assemblyCSharp.Assembly);

            logger.Log($"Loading {AssemblyCSharpFirstpass.Name}...");
            var assemblyCSharpFirstpass= ModuleDefMD.Load(File.ReadAllBytes(AssemblyCSharpFirstpass.FullName), context);
            logger.Log(" -> Loaded: {0}", assemblyCSharpFirstpass.FullName);
            resolver.AddToCache(assemblyCSharpFirstpass.Assembly);

            foreach (var p in patches)
            {
                logger.Log("Executing patch {0}...", p.Name);
                if (p.Patch(assemblyCSharp, assemblyCSharpFirstpass))
                    modified = true;
                else
                    logger.Log("{0} was not applied", p.Name);
            }

            if (modified)
            {
                var suffix = $".{DateTime.Now.Ticks}.bak";
                SaveModule(AssemblyCSharp, assemblyCSharp, suffix);
                SaveModule(AssemblyCSharpFirstpass, assemblyCSharpFirstpass, suffix);
            }
            else
            {
                logger.Error("The assembly was not modified");
            }
        }

        private void SaveModule(FileInfo file, ModuleDef module, string suffix)
        {
            logger.Log($"Backing up original file to {file.Name}{suffix}...");
            file.CopyTo(Path.Combine(file.Directory.FullName, file.FullName + suffix));
            logger.Log($"Saving file to {file.Name}...");
            module.Write(file.FullName);
            logger.Log(" -> Done");
        }
        
    }
}
