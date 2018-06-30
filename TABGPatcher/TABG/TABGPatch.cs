using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABGPatcher.TABG
{
    public abstract class TABGPatch
    {
        public string Name { get { return GetType().Name; } }
        protected CrapLogger logger;

        protected TABGPatch()
        {
            logger = new CrapLogger(Name);
        }

        public abstract bool Patch(ModuleDefMD assemblyCSharp, ModuleDefMD assemblyCSharpFirstpass);
    }
}
