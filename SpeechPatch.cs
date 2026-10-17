using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

// Runs inside setup; no downloaded game assemblies or secondary executables.
internal static class SpeechPatch
{
    internal static byte[] Build(byte[] original, byte[] runtime, string managed)
    {
        using (var resolver = new DefaultAssemblyResolver())
        {
            resolver.AddSearchDirectory(managed);
            var options = new ReaderParameters { AssemblyResolver = resolver, InMemory = true };
            using (var input = new MemoryStream(original))
            using (var hookInput = new MemoryStream(runtime))
            using (var module = ModuleDefinition.ReadModule(input, options))
            using (var hooks = ModuleDefinition.ReadModule(hookInput, options))
            {
                var type = module.GetType("PocketTtsSingleton");
                if (type == null || module.AssemblyReferences.Any(r => r.Name == "FamousCallers"))
                    throw new InvalidDataException("Unrecognized speech assembly. Use the verified backup before retrying.");
                var hookType = hooks.GetType("FamousCallers.RickBurps");
                var speak = type.Methods.Single(m => m.Name == "EnqueueSpeak");
                var frame = type.Methods.Single(m => m.Name == "HandleFrameGenerated");
                if (speak.Parameters.Count != 6 || speak.Parameters[0].ParameterType.FullName != "System.String" ||
                    speak.Parameters[1].ParameterType.FullName != "System.String" || speak.Parameters[2].ParameterType.FullName != "System.UInt32" ||
                    speak.Parameters[5].ParameterType.FullName != "System.Boolean" || frame.Parameters.Count != 1 ||
                    frame.Parameters[0].ParameterType.FullName != "PocketTTS.PocketTtsFrame")
                    throw new InvalidDataException("The game's speech interface has changed. This version was left untouched.");

                var oldSpeak = FindLegacy(speak, "Prepare");
                var oldFrame = FindLegacy(frame, "Process");
                if ((oldSpeak == null) != (oldFrame == null))
                    throw new InvalidDataException("An incomplete older speech modification was found.");
                if (oldSpeak != null)
                {
                    // Migrate the old companion hooks rather than adding duplicate effects.
                    oldSpeak.Operand = module.ImportReference(hookType.Methods.Single(m => m.Name == "Prepare"));
                    oldFrame.Operand = module.ImportReference(hookType.Methods.Single(m => m.Name == "Process"));
                }
                else
                {
                    speak.Body.SimplifyMacros();
                    var il = speak.Body.GetILProcessor();
                    var first = speak.Body.Instructions[0];
                    Before(il, first,
                        Instruction.Create(OpCodes.Ldarga, speak.Parameters[0]),
                        Instruction.Create(OpCodes.Ldarg, speak.Parameters[1]),
                        Instruction.Create(OpCodes.Ldarg, speak.Parameters[2]),
                        Instruction.Create(OpCodes.Ldarg, speak.Parameters[5]),
                        Instruction.Create(OpCodes.Call, module.ImportReference(hookType.Methods.Single(m => m.Name == "Prepare"))));
                    frame.Body.SimplifyMacros();
                    var voice = frame.Body.Variables.Single(v => v.VariableType.FullName == "System.String");
                    var store = frame.Body.Instructions.Single(i => i.OpCode == OpCodes.Stloc && i.Operand == voice);
                    var request = type.Fields.Single(f => f.Name == "currentRequest");
                    Before(frame.Body.GetILProcessor(), store.Next,
                        Instruction.Create(OpCodes.Ldarg, frame.Parameters[0]), Instruction.Create(OpCodes.Ldloc, voice),
                        Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldfld, request),
                        Instruction.Create(OpCodes.Call, module.ImportReference(hookType.Methods.Single(m => m.Name == "Process"))));
                }
                using (var output = new MemoryStream()) { module.Write(output); return output.ToArray(); }
            }
        }
    }

    static Instruction FindLegacy(MethodDefinition method, string name)
    {
        return method.Body.Instructions.SingleOrDefault(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference &&
            ((MethodReference)i.Operand).DeclaringType.FullName == "ScamOllama.RickBurps" && ((MethodReference)i.Operand).Name == name);
    }
    static void Before(ILProcessor il, Instruction anchor, params Instruction[] instructions)
    { foreach (var instruction in instructions) il.InsertBefore(anchor, instruction); }
}
