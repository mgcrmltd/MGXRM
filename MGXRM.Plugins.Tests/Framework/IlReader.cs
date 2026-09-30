using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace MGXRM.Plugins.Tests.Framework
{
    internal static class IlReader
    {
        private static readonly OpCode[] SingleByteOpCodes = new OpCode[0x100];
        private static readonly OpCode[] TwoByteOpCodes = new OpCode[0x100];

        static IlReader()
        {
            foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var opCode = (OpCode)field.GetValue(null);
                if (opCode.Size == 1)
                    SingleByteOpCodes[opCode.Value & 0xFF] = opCode;
                else
                    TwoByteOpCodes[opCode.Value & 0xFF] = opCode;
            }
        }

        public static IEnumerable<MethodBase> GetCalledMethods(MethodBase method)
        {
            if (method == null)
                yield break;

            var body = method.GetMethodBody();
            if (body == null)
                yield break;

            var il = body.GetILAsByteArray();
            if (il == null)
                yield break;

            var module = method.Module;
            var typeArguments = method.DeclaringType != null && method.DeclaringType.IsGenericType
                ? method.DeclaringType.GetGenericArguments()
                : null;
            var methodArguments = method.IsGenericMethodDefinition || method.IsGenericMethod
                ? method.GetGenericArguments()
                : null;

            var position = 0;
            while (position < il.Length)
            {
                var code = il[position++];
                OpCode opCode;
                if (code != 0xFE)
                    opCode = SingleByteOpCodes[code];
                else if (position < il.Length)
                    opCode = TwoByteOpCodes[il[position++]];
                else
                    yield break;

                if (opCode.Size == 0)
                    yield break;

                MethodBase called = null;
                if (opCode.OperandType == OperandType.InlineMethod || opCode.OperandType == OperandType.InlineTok)
                {
                    var token = BitConverter.ToInt32(il, position);
                    try
                    {
                        called = module.ResolveMethod(token, typeArguments, methodArguments);
                    }
                    catch (ArgumentException)
                    {
                    }
                }

                if (called != null)
                    yield return called;

                position += OperandSize(opCode, il, position);
            }
        }

        private static int OperandSize(OpCode opCode, byte[] il, int position)
        {
            switch (opCode.OperandType)
            {
                case OperandType.InlineNone:
                    return 0;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    return 1;
                case OperandType.InlineVar:
                    return 2;
                case OperandType.InlineBrTarget:
                case OperandType.InlineField:
                case OperandType.InlineI:
                case OperandType.InlineMethod:
                case OperandType.InlineSig:
                case OperandType.InlineString:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.ShortInlineR:
                    return 4;
                case OperandType.InlineI8:
                case OperandType.InlineR:
                    return 8;
                case OperandType.InlineSwitch:
                    return 4 + (4 * BitConverter.ToInt32(il, position));
                default:
                    throw new NotSupportedException($"Unhandled operand type {opCode.OperandType} for opcode {opCode.Name}.");
            }
        }
    }
}
