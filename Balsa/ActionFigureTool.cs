using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Balsa
{
    public class ActionFigureTools : PatchInterface
    {
        public string GetName()
        {
            return "ActionFigureTools";
        }

        private MethodDefinition GetMethodDefinition(AssemblyDefinition assembly)
        {
            return assembly.MainModule.Types.First(type => type.FullName == "PlayerTools.ActionFigureTool").Resolve().Methods.First(method => method.Name == "GetToolAvailable");
        }

        public bool Applied(AssemblyDefinition assembly)
        {
            MethodDefinition toolAvailable = GetMethodDefinition(assembly);
            return toolAvailable.Body.Instructions[0].OpCode == OpCodes.Ldc_I4_1;
        }

        public bool Patch(AssemblyDefinition assembly)
        {
            MethodDefinition toolAvailable = GetMethodDefinition(assembly);
            ILProcessor processor = toolAvailable.Body.GetILProcessor();
            Instruction oldI = toolAvailable.Body.Instructions[0];
            Instruction newI = processor.Create(OpCodes.Ldc_I4_1);
            processor.Replace(oldI, newI);
            return true;
        }
    }
}