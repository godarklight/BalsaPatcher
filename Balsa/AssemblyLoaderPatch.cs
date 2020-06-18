using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
/*
namespace Balsa
{
    public class AssemblyLoaderPatch : PatchInterface
    {
        public string GetName()
        {
            return "AssemblyLoader1";
        }

        private MethodDefinition GetMethodDefinition(AssemblyDefinition assembly)
        {
            TypeDefinition modloaderType = assembly.MainModule.Types.First(type => type.FullName == "IO.ModLoader");
            return modloaderType.Methods.First(method => method.Name == "FindPluginCfgs");
        }

        public bool Applied(AssemblyDefinition assembly)
        {
            MethodDefinition findPluginCfgs = GetMethodDefinition(assembly);
            return findPluginCfgs.Body.Instructions.Count > 2;
        }

        public bool Patch(AssemblyDefinition assembly)
        {
            TypeDefinition modloaderType = assembly.MainModule.Types.First(type => type.FullName == "IO.ModLoader");
            MethodDefinition findPluginCfgs = GetMethodDefinition(assembly);
            FieldReference modListReference = modloaderType.Fields.First(field => field.Name == "modList");
            TypeDefinition pluginInfoType = assembly.MainModule.Types.First(type => type.FullName == "IO.PluginInfo");
            ILProcessor processor = findPluginCfgs.Body.GetILProcessor();
            List<Instruction> removeInstructions = new List<Instruction>();
            foreach (Instruction i in findPluginCfgs.Body.Instructions)
            {
                removeInstructions.Add(i);
            }
            foreach (Instruction i in removeInstructions)
            {
                processor.Remove(i);
            }
            List<Instruction> newInstructions = new List<Instruction>();
            newInstructions.Add(processor.Create(OpCodes.Ldarg_0));
            newInstructions.Add(processor.Create(OpCodes.Ldfld, modListReference));
            newInstructions.Add(processor.Create(OpCodes.Newobj, pluginInfoType));
            //Man I have NO IDEA about this one.
            Instruction firstInstruction = processor.Body.Instructions[0];
            foreach (Instruction i in newInstructions)
            {
                    processor.Append(firstInstruction);
            }
            return true;
        }
    }
}
*/
/*
 *  IL_0000: ldarg.0
    IL_0001: ldfld class IO.ModCFG[] IO.ModLoader::modList
    IL_0006: newobj instance void class [mscorlib]System.Collections.Generic.List`1<class IO.PluginInfo>::.ctor()
    IL_000b: ldsfld class [mscorlib]System.Func`3<class [mscorlib]System.Collections.Generic.List`1<class IO.PluginInfo>, class IO.ModCFG, class [mscorlib]System.Collections.Generic.List`1<class IO.PluginInfo>> IO.ModLoader/'<>c'::'<>9__10_0'
    IL_0010: dup
    IL_0011: brtrue.s IL_002a

    IL_0013: pop
    IL_0014: ldsfld class IO.ModLoader/'<>c' IO.ModLoader/'<>c'::'<>9'
    IL_0019: ldftn instance class [mscorlib]System.Collections.Generic.List`1<class IO.PluginInfo> IO.ModLoader/'<>c'::'<GetAllPlugins>b__10_0'(class [mscorlib]System.Collections.Generic.List`1<class IO.PluginInfo>, class IO.ModCFG)
    IL_001f: newobj instance void class [mscorlib]System.Func`3<class [mscorlib]System.Collections.Generic.List`1<class IO.PluginInfo>, class IO.ModCFG, class [mscorlib]System.Collections.Generic.List`1<class IO.PluginInfo>>::.ctor(object, native int)
    IL_0024: dup
    IL_0025: stsfld class [mscorlib]System.Func`3<class [mscorlib]System.Collections.Generic.List`1<class IO.PluginInfo>, class IO.ModCFG, class [mscorlib]System.Collections.Generic.List`1<class IO.PluginInfo>> IO.ModLoader/'<>c'::'<>9__10_0'

    IL_002a: call !!1 [System.Core]System.Linq.Enumerable::Aggregate<class IO.ModCFG, class [mscorlib]System.Collections.Generic.List`1<class IO.PluginInfo>>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>, !!1, class [mscorlib]System.Func`3<!!1, !!0, !!1>)
    IL_002f: ret

*/