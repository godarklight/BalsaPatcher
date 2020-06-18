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
            //Get UnityObject
            AssemblyDefinition unityCore = AssemblyDefinition.ReadAssembly("UnityEngine.CoreModule.dll");
            TypeReference unityObject = assembly.MainModule.ImportReference(unityCore.MainModule.Types.First(type => type.FullName == "UnityEngine.GameObject"));
            TypeReference typeType = assembly.MainModule.ImportReference(typeof(Type));
            TypeReference typeArrayType = assembly.MainModule.ImportReference(new ArrayType(typeType));
            MethodReference getTypeFromHandle = assembly.MainModule.ImportReference(typeType.Resolve().Methods.First(method => method.Name == "GetTypeFromHandle"));

            //Get method to patch
            MethodDefinition load = GetMethodDefinition(assembly);

            TypeDefinition assemblyLoaderType = assembly.MainModule.Types.First(type => type.FullName == "IO.AssemblyLoader");
            FieldDefinition patchedAssemblyLoader = new FieldDefinition("patchedAssemblyLoader", Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.Public, unityObject);
            //patchedAssemblyLoader.DeclaringType = assemblyLoaderType;
            assemblyLoaderType.Fields.Add(patchedAssemblyLoader);
            MethodReference assemblyLoaderCtor = assembly.MainModule.ImportReference(unityObject.Resolve().Methods.First(method => method.Name == ".ctor" && method.Parameters.Count == 2));
            MethodReference setActive = assembly.MainModule.ImportReference(unityObject.Resolve().Methods.First(method => method.Name == "SetActive" && method.Parameters.Count == 1));

            load.Body.Variables.Clear();
            load.Body.Variables.Add(new VariableDefinition(typeArrayType));

            ILProcessor processor = load.Body.GetILProcessor();

            List<Instruction> newInstructions = new List<Instruction>() {
                processor.Create(OpCodes.Ldstr, "patchedAssemblyLoader"),
                processor.Create(OpCodes.Ldc_I4_1),
                processor.Create(OpCodes.Newarr, typeType),
                processor.Create(OpCodes.Stloc_0),
                processor.Create(OpCodes.Ldloc_0),
                processor.Create(OpCodes.Ldc_I4_0),
                processor.Create(OpCodes.Ldtoken, assemblyLoaderType),
                processor.Create(OpCodes.Call, getTypeFromHandle),
                processor.Create(OpCodes.Stelem_Ref),
                processor.Create(OpCodes.Ldloc_0),
                processor.Create(OpCodes.Newobj, assemblyLoaderCtor),
                processor.Create(OpCodes.Stsfld, patchedAssemblyLoader),
                processor.Create(OpCodes.Ldsfld, patchedAssemblyLoader),
                processor.Create(OpCodes.Ldc_I4_1),
                processor.Create(OpCodes.Callvirt, setActive),
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

/*  IL_0032: ldc.i4.1
    IL_0033: newarr [mscorlib]System.Type
    IL_0038: dup
    IL_0039: ldc.i4.0
    IL_003a: ldtoken IO.AssemblyLoader
    IL_003f: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
    IL_0044: stelem.ref
    IL_0045: newobj instance void [UnityEngine.CoreModule]UnityEngine.GameObject::.ctor(string, class [mscorlib]System.Type[])
    IL_004a: stsfld class [UnityEngine.CoreModule]UnityEngine.GameObject IO.ModLoader::assLoader
    // assLoader.SetActive(true);
    IL_004f: ldsfld class [UnityEngine.CoreModule]UnityEngine.GameObject IO.ModLoader::assLoader
    IL_0054: ldc.i4.1
    IL_0055: callvirt instance void [UnityEngine.CoreModule]UnityEngine.GameObject::SetActive(bool)
    */