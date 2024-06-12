using HarmonyLib;
using Ionic.Crc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace FrankWilco.RimWorld
{
    internal class ModUtils
    {
        public static Harmony Instance { get; set; }
        public static bool IsInitialized { get; private set; } = false;

        public static void Initialize(
            string packageId = "io.frankwilco.default",
            bool patchAll = false)
        {
            if (IsInitialized)
            {
                return;
            }
#if DEBUG
            Harmony.DEBUG = true;
#endif
            Instance = new Harmony(packageId);
            if (patchAll)
            {
                Instance.PatchAll();
            }
            IsInitialized = true;
        }

        public static void PatchMultiple(string[] targets)
        {
            foreach (var type in targets)
            {
                if (string.IsNullOrWhiteSpace(type))
                {
                    continue;
                }

                Instance.PatchCategory(type);
            }
        }

        public static void PatchMultiple(Type[] targets)
        {
            foreach (var type in targets)
            {
                if (type == null)
                {
                    continue;
                }

                var processor = new PatchClassProcessor(Instance, type);
                processor.Patch();
            }
        }

        public static void Patch(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return;
            }

            Instance.PatchCategory(target);
        }

        public static void Patch(Type target)
        {
            if (target is null)
            {
                return;
            }

            var processor = new PatchClassProcessor(Instance, target);
            processor.Patch();
        }

        public static int GetMethodHash(MethodInfo method)
        {
            int result = 0;
            var il = method.GetMethodBody().GetILAsByteArray();
            using (var stream = new MemoryStream(il))
            {
                var hasher = new CRC32();
                result = hasher.GetCrc32(stream);
            }
            return result;
        }

        public static int GetMethodHash(Type type, string methodName)
        {
            var methodInfo = AccessTools.Method(type, methodName);
            return GetMethodHash(methodInfo);
        }

#if DEBUG
        private const string kILDumpFile = "ildump.txt";
        private const string kILDumpRawFile = "ildump_raw.txt";

        private static bool _cleared = false;
#endif
        public static void Trace(
            IEnumerable<CodeInstruction> instructions,
            string tag)
        {
#if DEBUG
            if (!_cleared)
            {
                File.WriteAllText(kILDumpFile, "");
                File.WriteAllText(kILDumpRawFile, "");
                _cleared = true;
            }

            string startMarker = $"<=== START {tag} ===> \n\n";
            string endMarker = $"\n <=== END {tag} ===> \n\n";
            File.AppendAllText(kILDumpFile, startMarker);
            File.AppendAllText(kILDumpRawFile, startMarker);
            foreach (var instruction in instructions)
            {
                File.AppendAllText(
                    kILDumpFile,
                    instruction.ToString() + "\n");
                File.AppendAllText(
                    kILDumpRawFile,
                    $"{instruction.opcode} {instruction.operand}\n");
            }
            File.AppendAllText(kILDumpFile, endMarker);
            File.AppendAllText(kILDumpRawFile, endMarker);
#else
            throw new NotSupportedException(Messages.InvalidOnReleaseBuilds);
#endif
        }

        public static void Trace(
            List<CodeInstruction> instructions,
            MethodBase method,
            string suffix = "")
        {
            string tag =
                $"{method.DeclaringType.FullName}->{method.Name}{suffix}";
            Trace(instructions, tag);
        }
    }

    public class ILSection
    {
        private readonly List<CodeInstruction> _instructions;

        public ILSection(
            List<CodeInstruction> instructions,
            int startOffset,
            int endOffset,
            OpCode? anchorOpcode,
            Func<object, bool> anchorOperand,
            OpCode? startOpcode,
            Func<object, bool> startOperand,
            OpCode? endOpcode,
            Func<object, bool> endOperand,
            bool checkSectionNow = true)
        {
            _instructions = instructions;
            StartOffset = startOffset;
            EndOffset = endOffset;
            AnchorOpcode = anchorOpcode;
            AnchorOperand = anchorOperand;
            StartOpcode = startOpcode;
            StartOperand = startOperand;
            EndOpcode = endOpcode;
            EndOperand = endOperand;
            if (checkSectionNow)
            {
                CheckAll();
            }
        }

        public ILSection(
            List<CodeInstruction> instructions,
            int startOffset,
            int endOffset,
            OpCode? anchorOpcode,
            Func<object, bool> anchorOperand,
            OpCode? startOpcode,
            OpCode? endOpcode)
            : this(
                  instructions,
                  startOffset,
                  endOffset,
                  anchorOpcode,
                  anchorOperand,
                  startOpcode,
                  null,
                  endOpcode,
                  null)
        {
        }

        public ILSection(List<CodeInstruction> instructions)
            : this(
                  instructions,
                  -1,
                  -1,
                  null,
                  null,
                  null,
                  null,
                  null,
                  null,
                  false)
        {
            IsFound = true;
            IsEntireMethod = true;
            StartIndex = 0;
            EndIndex = _instructions.Count - 1;
        }

        public int StartOffset { get; }
        public int EndOffset { get; }

        public OpCode? AnchorOpcode { get; }
        public Func<object, bool> AnchorOperand { get; }

        public OpCode? StartOpcode { get; }
        public Func<object, bool> StartOperand { get; }

        public OpCode? EndOpcode { get; }
        public Func<object, bool> EndOperand { get; }

        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }

        public CodeInstruction StartInstruction { get; private set; }
        public CodeInstruction EndInstruction { get; private set; }

        public bool IsFound { get; private set; }
        public bool IsInvalid { get; private set; }
        public bool IsRemoved { get; private set; }
        public bool IsEntireMethod { get; }

        private List<List<Label>> _labels;

        public void Check(int index)
        {
            if (IsFound || IsInvalid || IsRemoved)
            {
                return;
            }

            var instruction = _instructions[index];
            if (instruction.opcode != AnchorOpcode)
            {
                return;
            }

            if (!AnchorOperand(instruction.operand))
            {
                return;
            }

            int startIndex = index + StartOffset;
            int endIndex = index + EndOffset;
            if (startIndex < 0 ||
                startIndex > _instructions.Count - 1 ||
                endIndex < 0 ||
                endIndex > _instructions.Count - 1 ||
                startIndex == endIndex)
            {
                IsInvalid = true;
                return;
            }

            var startInstruction = _instructions[startIndex];
            var endInstruction = _instructions[endIndex];
            bool startMatched =
                (startInstruction.opcode == StartOpcode) &&
                (StartOperand == null ||
                 StartOperand(startInstruction.operand));
            bool endMatched =
                (endInstruction.opcode == EndOpcode) &&
                (EndOperand == null ||
                 EndOperand(endInstruction.operand));
            if (startMatched && endMatched)
            {
                StartIndex = startIndex;
                StartInstruction = startInstruction;
                EndIndex = endIndex;
                EndInstruction = endInstruction;
                IsFound = true;
            }
        }

        public void CheckAll()
        {
            if (IsFound || IsInvalid || IsRemoved)
            {
                return;
            }

            for (int i = 0; i < _instructions.Count; i++)
            {
                Check(i);
            }
        }

        public List<List<Label>> GetLabels()
        {
            if (_labels == null)
            {
                if (!IsFound || IsRemoved)
                {
                    return null;
                }

                _labels = new List<List<Label>>();
                for (int i = StartIndex; i <= EndIndex; i++)
                {
                    var instruction = _instructions[i];
                    if (instruction.labels.Count > 0)
                    {
                        _labels.Add(instruction.labels);
                    }
                }
            }
            return _labels;
        }

        public void Remove()
        {
            if (!IsFound || IsRemoved)
            {
                return;
            }
            // If we're covering an entire method, remove everything
            // and return in the first instruction.
            if (IsEntireMethod)
            {
                _instructions[0].opcode = OpCodes.Ret;
                _instructions[0].operand = null;
                StartIndex = 1;
            }
            RemoveRange(_instructions, StartIndex, EndIndex);
            IsRemoved = true;
        }

        public static void RemoveRange(
            List<CodeInstruction> instructions,
            int startIndex,
            int endIndex)
        {
            instructions.RemoveRange(
                startIndex,
                endIndex - startIndex + 1);
        }
    }
}
