using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Balsa
{
    public class PartLimitPatch : PatchInterface
    {
        public string GetName()
        {
            return "PartLimit";
        }

        private MethodDefinition GetMethodDefinition(AssemblyDefinition assembly)
        {
            TypeDefinition modloaderType = assembly.MainModule.Types.First(type => type.FullName == "IO.ModLoader").NestedTypes[0];
            return modloaderType.Methods.First(method => method.FullName.Contains("GetAllPartCfgs"));
        }

        public bool Applied(AssemblyDefinition assembly)
        {
            MethodDefinition allPartCfgs = GetMethodDefinition(assembly);
            foreach (Instruction i in allPartCfgs.Body.Instructions)
            {
                if (i.OpCode == OpCodes.Ldc_I4_S && (sbyte)i.Operand == 25)
                {   
                    return false;
                }
            }
            return true;
        }

        public bool Patch(AssemblyDefinition assembly)
        {
            MethodDefinition allPartCfgs = GetMethodDefinition(assembly);
            ILProcessor processor = allPartCfgs.Body.GetILProcessor();
            List<Instruction> removeInstructions = new List<Instruction>();
            Instruction lastInstruction = null;
            foreach (Instruction i in allPartCfgs.Body.Instructions)
            {
                //Remove everything up to the first object store.
                if (i.OpCode == OpCodes.Call)
                {
                    GenericInstanceMethod gim = i.Operand as GenericInstanceMethod;
                    if (gim.Name == "Take")
                    {
                        removeInstructions.Add(lastInstruction);
                        removeInstructions.Add(i);
                    }
                }
                lastInstruction = i;
            }
            foreach (Instruction i in removeInstructions)
            {
                processor.Remove(i);
            }
            return true;
        }
    }
}
