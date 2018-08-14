using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;

namespace TABGPatcher.TABG
{
    public class Equ8Patch : TABGPatch
    {

        public Equ8Patch() : base("EQU8")
        {
        }

        public override bool Patch(ModuleDefMD assemblyCSharp, ModuleDefMD assemblyCSharpFirstpass)
        {
            logger.Log("Getting all types...");
            var types = assemblyCSharp.GetTypes().OrderBy(x => x.FullName).ToArray();
            logger.Log(" -> {0} types", types.Length);

            logger.Log("Getting equ8 types...");
            var equ8Types = FindEquTypes(types);
            if (equ8Types.Length == 0)
            {
                logger.Error("Could not find any types in the equ8-namespace");
                return false;
            }
            logger.Log(" -> Found {0}", string.Join(", ", equ8Types.Select(x => x.FullName).ToArray()));

            logger.Log("Finding references to those types...");
            var usingEqu8 = GetMethodsUsingEqu8(types.Except(equ8Types).ToArray(), equ8Types);
            if (usingEqu8.Length == 0)
            {
                logger.Error("Did not find any methods referencing those types. Did you already run the patch?");
                return false;
            }
            logger.Log(" -> Found {0} methods in {1}", usingEqu8.Length, string.Join(", ", usingEqu8.Select(x => x.DeclaringType).Distinct().Select(x => x.FullName).ToArray()));


            logger.Log("Stripping method bodies...");
            foreach (var m in usingEqu8)
                StripInstructions(m);
            logger.Log(" -> Done");
            return true;
        }

        private static string[] EQU8_METHODS = new string[] {
            "gg_client_close_session",
            "gg_client_deinitialize",
            "gg_client_unregister_d2d_object",
            "gg_client_establish_session",
            "gg_client_initialize"
        };

        private TypeDef[] FindEquTypes(TypeDef[] types)
        {
            var equTypes = types.Where(x => x.Namespace == "equ8");
            if (!equTypes.Any())
                return new TypeDef[0];

            var methods = equTypes
                .SelectMany(x => x.Methods)
                .Where(x => x.HasImplMap && EQU8_METHODS.Contains(x.ImplMap.Name.ToString()));
            if (!methods.Any())
                return new TypeDef[0];

            var t = methods.Select(x => x.DeclaringType).Distinct().AsParallel().ToArray();
            return t;
        }
        private MethodDef[] GetMethodsUsingEqu8(TypeDef[] allTypes, TypeDef[] equ8Types)
        {
            return allTypes
                .Where(x => x.HasMethods)
                .SelectMany(x => x.Methods)
                .Where(x => x.HasBody)
                .Where(x => x.Body.Instructions.Any(i => i.Operand != null && i.Operand is MethodDef && equ8Types.Contains(((MethodDef)i.Operand).DeclaringType))).ToArray();
        }
    }
}
