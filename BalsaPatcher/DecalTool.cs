using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Balsa
{
    public class DecalTool : PatchInterface
    {
        public string GetName()
        {
            return "DecalTool";
        }

        private TypeDefinition GetTypeDefinition(AssemblyDefinition assembly)
        {
            return assembly.MainModule.Types.First(type => type.FullName == "Construction.DecalTool").Resolve();
        }

        public bool Applied(AssemblyDefinition assembly)
        {
            TypeDefinition decalTool = GetTypeDefinition(assembly);
            CustomAttribute decalAttribute = decalTool.CustomAttributes[0];
            return (int)decalAttribute.Fields.First(field => field.Name == "visible").Argument.Value == 0;
        }

        public bool Patch(AssemblyDefinition assembly)
        {
            TypeDefinition decalTool = GetTypeDefinition(assembly);
            CustomAttribute decalAttribute = decalTool.CustomAttributes[0];
            TypeDefinition whenType = decalAttribute.AttributeType.Resolve().NestedTypes[0];
            decalAttribute.Fields.Remove(decalAttribute.Fields.First(canaTest => canaTest.Name == "visible"));
            CustomAttributeNamedArgument cana = new CustomAttributeNamedArgument("visible", new CustomAttributeArgument(whenType, 0));
            decalAttribute.Fields.Add(cana);
            return true;
        }
    }
}