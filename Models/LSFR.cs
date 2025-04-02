using System;

namespace lab2.Models;

public class LSFR
{
    public uint State { get; private set; }
    public LSFR(uint seed)
    {
        State = seed;
    }
    private uint GetNextBit()
    {
        uint feedback = ((State >> 31) ^ (State >> 27) ^ (State >> 26) ^ State) & 1;
        return feedback;
    }
    // return lost Bit 
    public uint ShiftLeftBit()
    {
        uint lostBit = State >> 31;
        State = (State << 1) | GetNextBit();
        return lostBit;
    }
    public byte ShiftLeftByte()
    {
        uint lostByte = 0;
        for (int i = 0; i < 8; i++)
            lostByte = (lostByte << 1) | ShiftLeftBit();
        return (byte)lostByte;  
    }
    public void PrintState()
    {
        Console.Write(Convert.ToString(State, 2).PadLeft(32, '0'));
    }
}