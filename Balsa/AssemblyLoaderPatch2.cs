using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Balsa
{
    public class AssemblyLoaderPatch2 : PatchInterface
    {
        public string GetName()
        {
            return "AssemblyLoader2";
        }

        private MethodDefinition GetMethodDefinition(AssemblyDefinition assembly)
        {
            TypeDefinition modloaderType = assembly.MainModule.Types.First(type => type.FullName == "IO.ModLoader");
            return modloaderType.Methods.First(method => method.Name == "Load");
        }

        public bool Applied(AssemblyDefinition assembly)
        {
            MethodDefinition findPluginCfgs = GetMethodDefinition(assembly);
            foreach (Instruction i in findPluginCfgs.Body.Instructions)
            {
                if (i.OpCode == OpCodes.Newobj)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Patch(AssemblyDefinition assembly)
        {
            //Get method to patch
            MethodDefinition load = GetMethodDefinition(assembly);

            TypeDefinition assemblyLoaderType = assembly.MainModule.Types.First(type => type.FullName == "IO.AssemblyLoader");
            FieldDefinition patchedAssemblyLoader = new FieldDefinition("patchedAssemblyLoader", Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.Public, assemblyLoaderType);
            //patchedAssemblyLoader.DeclaringType = assemblyLoaderType;
            assemblyLoaderType.Fields.Add(patchedAssemblyLoader);
            MethodReference assemblyLoaderCtor = assemblyLoaderType.Methods.First(method => method.Name == ".ctor");

            ILProcessor processor = load.Body.GetILProcessor();

            List<Instruction> newInstructions = new List<Instruction>() {
                processor.Create(OpCodes.Newobj, assemblyLoaderCtor),
                processor.Create(OpCodes.Stfld, patchedAssemblyLoader),
            };

            //Build the method
            Instruction lastInstruction = processor.Body.Instructions.Last();
            foreach (Instruction i in newInstructions)
            {
                processor.InsertBefore(lastInstruction, i);
            }
            return true;
        }
    }
}
