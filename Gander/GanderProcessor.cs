using System;
using System.Collections.Generic;

namespace Gander;

public class GanderProcessor
{
    private List<Instruction> _instructions;
    private Stack<GanderVariable> _stack;
    private Dictionary<string, GanderVariable> _variables;
    private List<int> _labels;
    private List<int> _functions;
    private int _currentPos;
    private bool _isInFunc;

    public GanderProcessor()
    {
        _instructions = new List<Instruction>();
        _stack = new Stack<GanderVariable>();
        _variables = new Dictionary<string, GanderVariable>();
        _labels = new List<int>();
        _functions = new List<int>();
        _currentPos = -1;
    }

    public void PreProcess(string code)
    {
        HashSet<string> labels = new HashSet<string>();
        HashSet<string> functions = new HashSet<string>();
        string[] splitCode = code.Split('\n');
        for (int i = 0; i < splitCode.Length; i++)
        {
            string line = splitCode[i].Trim();
            if (line == "" || line.StartsWith("#"))
                continue;
            string[] splitLine = line.Split(' ');
            switch (splitLine[0])
            {
                case "add":
                    _instructions.Add(new Instruction(OpCode.Add));
                    break;
                case "br":
                    _instructions.Add(new Instruction(OpCode.Br));
                    break;
                case "bne":
                    _instructions.Add(new Instruction(OpCode.Bne, splitLine[1]));
                    break;
                case "call":
                    _instructions.Add(new Instruction(OpCode.Call, ));
                default:
                    throw new Exception("Error while preprocessing, error at line " + i + 1 +
                                        ": Unrecognized opcode \"" + splitLine[0]);
            }
        }
    }

    public void Process(string code)
    {
        string[] splitCode = code.Split('\n');
        for (int i = 0; i < splitCode.Length; i++)
        {
            string line = splitCode[i].Trim();
            if (line == "" || line.StartsWith("#"))
                continue;
            string[] splitLine = line.Split(' ');
            string tLine = splitLine[0].Trim().ToLower();
            if (_isInFunc && tLine != "fne")
                continue;
            switch (tLine)
            {
                case "lbl":
                    // Already preprocessed, so do nothing.
                    break;
                case "fn":
                    _isInFunc = true;
                    break;
                case "fne":
                    _isInFunc = false;
                    if (_currentPos == -1)
                        break;
                    i = _currentPos;
                    _currentPos = -1;
                    break;

                #region Stack
                
                case "ld.i8":
                    _stack.Push(new GanderVariable(sbyte.Parse(splitLine[1]), GanderType.I8));
                    break;
                case "ld.i16":
                    _stack.Push(new GanderVariable(short.Parse(splitLine[1]), GanderType.I16));
                    break;
                case "ld.i32":
                    _stack.Push(new GanderVariable(int.Parse(splitLine[1]), GanderType.I32));
                    break;
                case "ld.i64":
                    _stack.Push(new GanderVariable(long.Parse(splitLine[1]), GanderType.I64));
                    break;
                case "ld.u8":
                    _stack.Push(new GanderVariable(byte.Parse(splitLine[1]), GanderType.U8));
                    break;
                case "ld.u16":
                    _stack.Push(new GanderVariable(ushort.Parse(splitLine[1]), GanderType.U16));
                    break;
                case "ld.u32":
                    _stack.Push(new GanderVariable(uint.Parse(splitLine[1]), GanderType.U32));
                    break;
                case "ld.u64":
                    _stack.Push(new GanderVariable(ulong.Parse(splitLine[1]), GanderType.U64));
                    break;
                case "ld.f32":
                    _stack.Push(new GanderVariable(float.Parse(splitLine[1]), GanderType.F32));
                    break;
                case "ld.f64":
                    _stack.Push(new GanderVariable(double.Parse(splitLine[1]), GanderType.F64));
                    break;
                case "ld.str":
                    _stack.Push(new GanderVariable(string.Join(' ', splitLine[1..]).Trim('"'), GanderType.String));
                    break;
                
                case "conv.i8":
                    _stack.Push(new GanderVariable(Convert.ToSByte(_stack.Pop().Object), GanderType.I8));
                    break;
                case "conv.i16":
                    _stack.Push(new GanderVariable(Convert.ToInt16(_stack.Pop().Object), GanderType.I16));
                    break;
                case "conv.i32":
                    _stack.Push(new GanderVariable(Convert.ToInt32(_stack.Pop().Object), GanderType.I32));
                    break;
                case "conv.i64":
                    _stack.Push(new GanderVariable(Convert.ToInt64(_stack.Pop().Object), GanderType.I64));
                    break;
                case "conv.u8":
                    _stack.Push(new GanderVariable(Convert.ToByte(_stack.Pop().Object), GanderType.U8));
                    break;
                case "conv.u16":
                    _stack.Push(new GanderVariable(Convert.ToUInt16(_stack.Pop().Object), GanderType.U16));
                    break;
                case "conv.u32":
                    _stack.Push(new GanderVariable(Convert.ToUInt32(_stack.Pop().Object), GanderType.U32));
                    break;
                case "conv.u64":
                    _stack.Push(new GanderVariable(Convert.ToUInt64(_stack.Pop().Object), GanderType.U64));
                    break;
                case "conv.f32":
                    _stack.Push(new GanderVariable(Convert.ToSingle(_stack.Pop().Object), GanderType.F32));
                    break;
                case "conv.f64":
                    _stack.Push(new GanderVariable(Convert.ToDouble(_stack.Pop().Object), GanderType.F64));
                    break;
                case "conv.str":
                    _stack.Push(new GanderVariable(Convert.ToString(_stack.Pop().Object), GanderType.String));
                    break;
                
                #endregion
                
                #region Vars
                
                case "stvar":
                    _variables.AddOrUpdate(splitLine[1], _stack.Pop());
                    break;
                case "stvar.pk":
                    _variables.AddOrUpdate(splitLine[1], _stack.Peek());
                    break;
                
                case "ldvar":
                    _stack.Push(_variables[splitLine[1]]);
                    break;
                
                #endregion
                
                case "add":
                    GanderVariable aItem1 = _stack.Pop();
                    GanderVariable aItem2 = _stack.Pop();
                    GanderType aType = CalculateType(ref aItem1, ref aItem2);
                    switch (aType)
                    {
                        case GanderType.I8:
                            _stack.Push(new GanderVariable(Convert.ToSByte(aItem1.Object) + Convert.ToSByte(aItem2.Object), GanderType.I8));
                            break;
                        case GanderType.I16:
                            _stack.Push(new GanderVariable(Convert.ToInt16(aItem1.Object) + Convert.ToInt16(aItem2.Object), GanderType.I16));
                            break;
                        case GanderType.I32:
                            _stack.Push(new GanderVariable(Convert.ToInt32(aItem1.Object) + Convert.ToInt32(aItem2.Object), GanderType.I32));
                            break;
                        case GanderType.I64:
                            _stack.Push(new GanderVariable(Convert.ToInt64(aItem1.Object) + Convert.ToInt64(aItem2.Object), GanderType.I64));
                            break;
                        case GanderType.U8:
                            _stack.Push(new GanderVariable(Convert.ToByte(aItem1.Object) + Convert.ToByte(aItem2.Object), GanderType.U8));
                            break;
                        case GanderType.U16:
                            _stack.Push(new GanderVariable(Convert.ToUInt16(aItem1.Object) + Convert.ToUInt16(aItem2.Object), GanderType.U16));
                            break;
                        case GanderType.U32:
                            _stack.Push(new GanderVariable(Convert.ToUInt32(aItem1.Object) + Convert.ToUInt32(aItem2.Object), GanderType.U32));
                            break;
                        case GanderType.U64:
                            _stack.Push(new GanderVariable(Convert.ToUInt64(aItem1.Object) + Convert.ToUInt64(aItem2.Object), GanderType.U64));
                            break;
                        case GanderType.F32:
                            _stack.Push(new GanderVariable(Convert.ToSingle(aItem1.Object) + Convert.ToSingle(aItem2.Object), GanderType.F32));
                            break;
                        case GanderType.F64:
                            _stack.Push(new GanderVariable(Convert.ToDouble(aItem1.Object) + Convert.ToDouble(aItem2.Object), GanderType.F64));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case "sub":
                    GanderVariable sItem1 = _stack.Pop();
                    GanderVariable sItem2 = _stack.Pop();
                    GanderType sType = CalculateType(ref sItem1, ref sItem2);
                    switch (sType)
                    {
                        case GanderType.I8:
                            _stack.Push(new GanderVariable(Convert.ToSByte(sItem2.Object) - Convert.ToSByte(sItem2.Object), GanderType.I8));
                            break;
                        case GanderType.I16:
                            _stack.Push(new GanderVariable(Convert.ToInt16(sItem2.Object) - Convert.ToInt16(sItem2.Object), GanderType.I16));
                            break;
                        case GanderType.I32:
                            _stack.Push(new GanderVariable(Convert.ToInt32(sItem2.Object) - Convert.ToInt32(sItem1.Object), GanderType.I32));
                            break;
                        case GanderType.I64:
                            _stack.Push(new GanderVariable(Convert.ToInt64(sItem2.Object) - Convert.ToInt64(sItem1.Object), GanderType.I64));
                            break;
                        case GanderType.U8:
                            _stack.Push(new GanderVariable(Convert.ToByte(sItem2.Object) - Convert.ToByte(sItem1.Object), GanderType.U8));
                            break;
                        case GanderType.U16:
                            _stack.Push(new GanderVariable(Convert.ToUInt16(sItem2.Object) - Convert.ToUInt16(sItem1.Object), GanderType.U16));
                            break;
                        case GanderType.U32:
                            _stack.Push(new GanderVariable(Convert.ToUInt32(sItem2.Object) - Convert.ToUInt32(sItem1.Object), GanderType.U32));
                            break;
                        case GanderType.U64:
                            _stack.Push(new GanderVariable(Convert.ToUInt64(sItem2.Object) - Convert.ToUInt64(sItem1.Object), GanderType.U64));
                            break;
                        case GanderType.F32:
                            _stack.Push(new GanderVariable(Convert.ToSingle(sItem2.Object) - Convert.ToSingle(sItem1.Object), GanderType.F32));
                            break;
                        case GanderType.F64:
                            _stack.Push(new GanderVariable(Convert.ToDouble(sItem2.Object) - Convert.ToDouble(sItem1.Object), GanderType.F64));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case "dbg":
                    GanderVariable tItem = _stack.Peek();
                    Console.WriteLine("Type: " + tItem.Type + ", Contents: " + tItem.Object);
                    break;
                case "dbg.p":
                    GanderVariable tItemP = _stack.Pop();
                    Console.WriteLine("Type: " + tItemP.Type + ", Contents: " + tItemP.Object);
                    break;
                case "call":
                    CallFunction(splitLine[1], ref i);
                    break;
                
                #region Branch
                
                case "br":
                    i = _labels[splitLine[1]];
                    break;
                
                case "br.ne":
                    if (_stack.Pop() != _stack.Pop())
                        i = _labels[splitLine[1]];
                    break;

                #endregion
                
                default:
                    throw new Exception("Unknown opcode \"" + splitLine[0] + "\".");
            }
        }
    }

    private void CallFunction(string funcName, ref int pos)
    {
        switch (funcName)
        {
            case "concat":
                int amount = (int) _stack.Pop().Object;
                string text = "";
                for (int i = 0; i < amount; i++)
                {
                    text = text.Insert(0, _stack.Pop().Object.ToString());
                }
                
                _stack.Push(new GanderVariable(text, GanderType.String));
                break;
            case "stdout":
                string sText = _stack.Pop().Object.ToString();
                Console.Write(sText);
                break;
            case "endl":
                Console.WriteLine();
                break;
            case "stdin":
                _stack.Push(new GanderVariable(Console.ReadLine(), GanderType.String));
                break;
            default:
                _currentPos = pos;
                pos = _functions[funcName];
                break;
        }
    }

    private GanderType CalculateType(ref GanderVariable item1, ref GanderVariable item2)
    {
        if (!item1.IsNumber() || !item2.IsNumber())
            throw new Exception("Stack popped but does not contain number.");
        GanderType aType;
        if (item1.Type < GanderType.F32 && item2.Type < GanderType.F32)
            aType = item1.Type > item2.Type ? item1.Type : item2.Type;
        else if (item1.Type == GanderType.F64 || item2.Type == GanderType.F64)
            aType = GanderType.F64;
        else
            aType = GanderType.F32;
        return aType;
    }
}