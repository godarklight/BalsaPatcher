using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Balsa
{
    public class AssemblyLoaderPatch1 : PatchInterface
    {
        public string GetName()
        {
            return "AssemblyLoader1";
        }

        private MethodDefinition GetMethodDefinition(AssemblyDefinition assembly)
        {
            TypeDefinition modloaderType = assembly.MainModule.Types.First(type => type.FullName == "IO.ModLoader");
            return modloaderType.Methods.First(method => method.Name == "GetAllPlugins");
        }

        public bool Applied(AssemblyDefinition assembly)
        {
            MethodDefinition findPluginCfgs = GetMethodDefinition(assembly);
            foreach (Instruction i in findPluginCfgs.Body.Instructions)
            {
                if (i.OpCode == OpCodes.Callvirt)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Patch(AssemblyDefinition assembly)
        {
            //Get method to patch
            MethodDefinition getAllPlugins = GetMethodDefinition(assembly);

            //Get types
            TypeReference intType = assembly.MainModule.TypeSystem.Int32;
            TypeReference listType = assembly.MainModule.ImportReference(typeof(List<>));
            TypeDefinition modcfgType = assembly.MainModule.Types.First(type => type.FullName == "IO.ModCFG");
            ArrayType modcfgTypeArray = new ArrayType(modcfgType);
            TypeReference modcfgTypeArrayRef = assembly.MainModule.ImportReference(modcfgTypeArray);
            TypeDefinition modloaderType = assembly.MainModule.Types.First(type => type.FullName == "IO.ModLoader");
            TypeDefinition pluginInfoType = assembly.MainModule.Types.First(type => type.FullName == "IO.PluginInfo");
            ArrayType pluginInfoArray = new ArrayType(pluginInfoType);
            TypeReference pluginInfoArrayType = assembly.MainModule.ImportReference(pluginInfoArray);

            //Get List<PluginInfo>
            TypeReference lpiTypeGeneric = getAllPlugins.ReturnType;
            MethodReference lpiTypeGenericCtorRef = assembly.MainModule.ImportReference(lpiTypeGeneric.Resolve().Methods.First(method => method.Name == ".ctor" && method.Parameters.Count == 0));
            lpiTypeGenericCtorRef.DeclaringType = lpiTypeGeneric;
            MethodReference addrange = assembly.MainModule.ImportReference(lpiTypeGeneric.Resolve().Methods.First(method => method.Name == "AddRange"));
            addrange.DeclaringType = lpiTypeGeneric;

            //Get modList
            FieldReference modListReference = modloaderType.Fields.First(field => field.Name == "modList");

            //Get pluginInfos
            FieldReference pluginInfosReference = modcfgType.Fields.First(field => field.Name == "pluginInfos");


            //Allow us to write into locals
            getAllPlugins.Body.Variables.Clear();
            //modList 0
            getAllPlugins.Body.Variables.Add(new VariableDefinition(modcfgTypeArrayRef));
            //length 1
            getAllPlugins.Body.Variables.Add(new VariableDefinition(intType));
            //count 2
            getAllPlugins.Body.Variables.Add(new VariableDefinition(intType));
            //retVal 3
            getAllPlugins.Body.Variables.Add(new VariableDefinition(lpiTypeGeneric));
            //modCfg 4
            getAllPlugins.Body.Variables.Add(new VariableDefinition(modcfgType));
            //pluginInfo[] 5
            getAllPlugins.Body.Variables.Add(new VariableDefinition(pluginInfoArrayType));

            //Remove the method instructions
            ILProcessor processor = getAllPlugins.Body.GetILProcessor();
            List<Instruction> removeInstructions = new List<Instruction>();
            foreach (Instruction i in getAllPlugins.Body.Instructions)
            {
                removeInstructions.Add(i);
            }
            foreach (Instruction i in removeInstructions)
            {
                processor.Remove(i);
            }

            //Add new instructions
            //Need a jump point to get out.
            Instruction loopstart = processor.Create(OpCodes.Ldloc_1);
            Instruction exit = processor.Create(OpCodes.Ldloc_3);
            Instruction skip = processor.Create(OpCodes.Ldc_I4_1);

            List<Instruction> newInstructions = new List<Instruction>() {
                //modList = this.modList
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldfld, modListReference),
                processor.Create(OpCodes.Stloc_0),

                //length = modList.Length
                processor.Create(OpCodes.Ldloc_0),
                processor.Create(OpCodes.Ldlen),
                processor.Create(OpCodes.Stloc_1),

                //count == 0
                processor.Create(OpCodes.Ldc_I4_0),
                processor.Create(OpCodes.Stloc_2),

                //retVal = new List<PluginInfos>
                processor.Create(OpCodes.Newobj, lpiTypeGenericCtorRef),
                processor.Create(OpCodes.Stloc_3),

                //Start of loop if count == length goto exit
                loopstart,
                processor.Create(OpCodes.Ldloc_2),
                processor.Create(OpCodes.Ceq),
                processor.Create(OpCodes.Brtrue_S, exit),

                //modCfg = modList[count]
                processor.Create(OpCodes.Ldloc_0),
                processor.Create(OpCodes.Ldloc_2),
                processor.Create(OpCodes.Ldelem_Ref),
                processor.Create(OpCodes.Stloc_S, (byte)4),

                //if modCfg == null goto skip
                processor.Create(OpCodes.Ldloc_S, (byte)4),
                processor.Create(OpCodes.Ldnull),
                processor.Create(OpCodes.Cgt_Un),
                processor.Create(OpCodes.Brfalse_S, skip),

                //pluginInfos = modCfg.pluginInfos
                processor.Create(OpCodes.Ldloc_S, (byte)4),
                processor.Create(OpCodes.Ldfld, pluginInfosReference),
                processor.Create(OpCodes.Stloc_S, (byte)5),

                //if pluginInfos == null goto skip
                processor.Create(OpCodes.Ldloc_S, (byte)5),
                processor.Create(OpCodes.Ldnull),
                processor.Create(OpCodes.Cgt_Un),
                processor.Create(OpCodes.Brfalse_S, skip),

                //retVal.AddRange(modCfg)
                processor.Create(OpCodes.Ldloc_3),
                processor.Create(OpCodes.Ldloc_S, (byte)5),
                processor.Create(OpCodes.Callvirt, addrange),

                skip,
                processor.Create(OpCodes.Ldloc_2),
                processor.Create(OpCodes.Add),
                processor.Create(OpCodes.Stloc_2),
                processor.Create(OpCodes.Br_S, loopstart),
                exit,
                processor.Create(OpCodes.Ret)
            };

            //Build the method
            foreach (Instruction i in newInstructions)
            {
                processor.Append(i);
            }

            return true;
        }
    }
}
