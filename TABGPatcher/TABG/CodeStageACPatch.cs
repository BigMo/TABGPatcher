using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;

namespace TABGPatcher.TABG
{
    public class CodeStageACPatch : TABGPatch
    {
        private const string CSAC_NAMESPACE = "CodeStage.AntiCheat.Detectors";

        public CodeStageACPatch() : base("CodeStageAC")
        { }

        public override bool Patch(ModuleDefMD assemblyCSharp, ModuleDefMD assemblyCSharpFirstpass)
        {
            logger.Log("Getting all types...");
            var types = assemblyCSharpFirstpass.GetTypes().OrderBy(x => x.FullName).ToArray();
            logger.Log(" -> {0} types", types.Length);

            logger.Log("Getting CodeStage types...");
            var csacTypes = FindCodeStageDetectorTypes(types);
            if (csacTypes.Length == 0)
            {
                logger.Error($"Could not find any types in the {CSAC_NAMESPACE}-namespace");
                return false;
            }
            logger.Log(" -> Found {0}", string.Join(", ", csacTypes.Select(x => x.FullName).ToArray()));

            logger.Log("Getting all AC-methods...");
            var methods = FindVoidMethods(csacTypes);
            if (methods.Length == 0)
            {
                logger.Error($"Could not find any non-empty methods used by the detector-types. Did you already run the patch?");
                return false;
            }
            logger.Log(" -> Found {0} methods in {1}", methods.Length, string.Join(", ", methods.Select(x => x.DeclaringType).Distinct().Select(x => x.FullName).ToArray()));


            logger.Log("Stripping method bodies...");
            foreach (var m in methods)
                StripInstructions(m);
            logger.Log(" -> Done");

            return true;
        }

        private TypeDef[] FindCodeStageDetectorTypes(TypeDef[] types)
        {
            var csacTypes = types.Where(x => x.Namespace == CSAC_NAMESPACE && !x.IsAbstract).AsParallel();
            if (!csacTypes.Any())
                return new TypeDef[0];
            return csacTypes.ToArray();
        }

        private MethodDef[] FindVoidMethods(TypeDef[] types)
        {
            var methods = types
                .Where(x => x.HasMethods)
                .SelectMany(x => x.Methods)
                .Where(x => !x.IsAbstract && !x.IsVirtual && x.ReturnType.TypeName == "Void" && x.HasBody && x.Body.HasInstructions && x.Body.Instructions.Count > 1);
            if (!methods.Any())
                return new MethodDef[0];
            return methods.ToArray();
        }
    }
}
