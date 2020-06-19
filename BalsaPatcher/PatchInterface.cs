using System;
using Mono.Cecil;

namespace Balsa
{
    public interface PatchInterface
    {
        bool Applied(AssemblyDefinition assembly);
        bool Patch(AssemblyDefinition assembly);
        string GetName();
    }
}
