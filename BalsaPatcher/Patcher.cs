using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;

namespace Balsa
{
    public class Patcher
    {
        public static void Main()
        {
            if (!File.Exists("Assembly-CSharp.dll"))
            {
                Console.WriteLine("Unable to find Assembly-CSharp.dll, please place this in the balsa/balsa_Data/Managed folder.");
                return;
            }
            Backup();
            if (Patch())
            {
                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Patch failed, restoring original.");
                Restore();
            }
        }

        private static void Backup()
        {
            if (!File.Exists("Assembly-CSharp.dll.original"))
            {
                File.Copy("Assembly-CSharp.dll", "Assembly-CSharp.dll.original");
            }
        }

        private static void Restore()
        {
            if (File.Exists("Assembly-CSharp.dll"))
            {
                File.Delete("Assembly-CSharp.dll");
            }
            File.Copy("Assembly-CSharp.dll.original", "Assembly-CSharp.dll");
        }

        private static bool Patch()
        {
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly("Assembly-CSharp.dll.original");

            bool allOk = true;
            List<Type> patches = new List<Type>();
            foreach (Type t in Assembly.GetExecutingAssembly().GetExportedTypes())
            {
                if (typeof(PatchInterface).IsAssignableFrom(t) && t.IsClass)
                {
                    patches.Add(t);
                }
            }
            Console.WriteLine("Found " + patches.Count + " available patches.");
            List<PatchInterface> patchesToApply = new List<PatchInterface>();
            foreach (Type t in patches)
            {
                PatchInterface pi = (PatchInterface)Activator.CreateInstance(t);
                if (!pi.Applied(assembly))
                {
                    patchesToApply.Add(pi);
                }
                else
                {
                    Console.WriteLine($"Patch: {pi.GetName()} already applied");
                }
            }
            foreach (PatchInterface pi in patchesToApply)
            {
                bool patchResult = true;
#if !DEBUG
                try
                {
#endif
                    patchResult = pi.Patch(assembly);
#if !DEBUG
                }
                catch
                {
                    patchResult = false;
                }
#endif
                if (!patchResult)
                {
                    Console.WriteLine($"Patch: {pi.GetName()} failed to apply");
                    allOk = false;
                }
                else
                {
                    Console.WriteLine($"Patch: {pi.GetName()} applied successfully");
                }
            }
            if (allOk)
            {
                assembly.Write("Assembly-CSharp.dll");
            }
            return allOk;
        }

    }
}
