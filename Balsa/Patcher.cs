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
            string fileToPatch = "Assembly-CSharp.dll";
            if (!File.Exists(fileToPatch))
            {
                Console.WriteLine("Unable to find Assembly-CSharp.dll, please place this in the balsa/balsa_Data/Managed folder.");
                return;
            }
            Backup(fileToPatch);
            if (!Patch(fileToPatch))
            {
                Console.WriteLine("Patch failed, won't save patched assembly.");
            }
            else
            {
                if (File.Exists($"{fileToPatch}.old"))
                {
                    File.Delete($"{fileToPatch}.old");
                }
                File.Move(fileToPatch, $"{fileToPatch}.old");
                File.Move($"{fileToPatch}.patched", fileToPatch);
            }
        }

        private static void Backup(string fileToPatch)
        {
            if (!File.Exists(fileToPatch + ".original"))
            {
                File.Copy(fileToPatch, fileToPatch + ".original");
            }
        }

        private static bool Patch(string fileToPatch)
        {
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(fileToPatch);

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
                assembly.Write($"{fileToPatch}.patched");
            }
            return allOk;
        }

    }
}
