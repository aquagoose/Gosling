namespace Gander;

public struct Instruction
{
    public OpCode OpCode;
    public string Value;

    public Instruction(OpCode opCode, string value = null)
    {
        OpCode = opCode;
        Value = value;
    }
}