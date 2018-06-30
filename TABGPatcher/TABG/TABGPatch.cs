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
        public string Name { get; private set; }
        protected CrapLogger logger;

        protected TABGPatch(string name)
        {
            Name = name;
            logger = new CrapLogger(Name);
        }

        public abstract bool Patch(ModuleDefMD assemblyCSharp, ModuleDefMD assemblyCSharpFirstpass);

        protected void StripInstructions(MethodDef method)
        {
            var body = new dnlib.DotNet.Emit.CilBody();
            body.Instructions.Add(new dnlib.DotNet.Emit.Instruction(dnlib.DotNet.Emit.OpCodes.Ret));
            method.Body = body;
        }
    }
}
