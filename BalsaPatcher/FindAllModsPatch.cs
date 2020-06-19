using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Balsa
{
    public class FindAllModsPatch : PatchInterface
    {
        public string GetName()
        {
            return "FindAllMods";
        }

        public bool Applied(AssemblyDefinition assembly)
        {
            TypeDefinition modloaderType = assembly.MainModule.Types.First(type => type.FullName == "IO.ModLoader");
            MethodDefinition findAllMods = modloaderType.Methods.First(method => method.Name == "FindAllMods");
            TypeDefinition directoryType = assembly.MainModule.ImportReference(typeof(System.IO.Directory)).Resolve();
            MethodDefinition getFilesMethod = directoryType.Methods.First(AppliedParameterMatch);
            Instruction callInstruction = findAllMods.Body.Instructions.First(i => i.OpCode == OpCodes.Call);
            MemberReference mr = (MemberReference)callInstruction.Operand;
            if (mr.Name == "GetFiles")
            {
                return true;
            }
            return false;
        }
        public bool Patch(AssemblyDefinition assembly)
        {
            TypeDefinition modloaderType = assembly.MainModule.Types.First(type => type.FullName == "IO.ModLoader");
            FieldDefinition addonPath = modloaderType.Fields.First(field => field.Name == "AddonPath");
            MethodDefinition findAllMods = modloaderType.Methods.First(method => method.Name == "FindAllMods");
            MethodReference getFilesMethod = assembly.MainModule.ImportReference(typeof(System.IO.Directory).GetMethods().First(RefereceParameterMatch));
            ILProcessor processor = findAllMods.Body.GetILProcessor();
            List<Instruction> removeInstructions = new List<Instruction>();
            foreach (Instruction i in findAllMods.Body.Instructions)
            {
                //Remove everything up to the first object store.
                if (i.OpCode == OpCodes.Stloc_0)
                {
                    break;
                }
                removeInstructions.Add(i);
            }
            foreach (Instruction i in removeInstructions)
            {
                processor.Remove(i);
            }
            List<Instruction> newInstructions = new List<Instruction>();
            newInstructions.Add(processor.Create(OpCodes.Ldarg_0));
            newInstructions.Add(processor.Create(OpCodes.Ldfld, addonPath));
            newInstructions.Add(processor.Create(OpCodes.Ldstr, "*.modcfg"));
            newInstructions.Add(processor.Create(OpCodes.Ldc_I4_1));
            newInstructions.Add(processor.Create(OpCodes.Call, getFilesMethod));
            Instruction firstInstruction = processor.Body.Instructions[0];
            foreach (Instruction i in newInstructions)
            {
                    processor.InsertBefore(firstInstruction, i);
            }
            return true;
        }


        bool AppliedParameterMatch(MethodDefinition methodDefinition)
        {
            if (methodDefinition.Name != "GetFiles")
            {
                return false;
            }
            if (methodDefinition.Parameters.Count != 3)
            {
                return false;
            }
            if (methodDefinition.Parameters[0].ParameterType.Name != "String")
            {
                return false;
            }
            if (methodDefinition.Parameters[1].ParameterType.Name != "String")
            {
                return false;
            }
            if (methodDefinition.Parameters[2].ParameterType.Name != "SearchOption")
            {
                return false;
            }
            return true;
        }

        bool RefereceParameterMatch(MethodInfo methodDefinition)
        {
            if (methodDefinition.Name != "GetFiles")
            {
                return false;
            }
            ParameterInfo[] param = methodDefinition.GetParameters();
            if (param.Length != 3)
            {
                return false;
            }
            if (param[0].ParameterType.Name != "String")
            {
                return false;
            }
            if (param[1].ParameterType.Name != "String")
            {
                return false;
            }
            if (param[2].ParameterType.Name != "SearchOption")
            {
                return false;
            }
            return true;
        }
    }
}
