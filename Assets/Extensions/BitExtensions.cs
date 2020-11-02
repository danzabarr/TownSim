using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BitExtensions 
{
    public static uint SetBit(this ref uint num, int position)
    {
        // Set a bit at position to 1.
        return num |= (uint)(1 << position);
    }

    public static uint UnsetBit(this ref uint num, int position)
    {
        // Set a bit at position to 0.
        return num &= ~(uint)(1 << position);
    }

    public static bool IsBitSet(this uint num, int position)
    {
        // Return whether bit at position is set to 1.
        return (num & (1 << position)) != 0;
    }

    public static string ToBitString(this uint num)
    {
        return Convert.ToString(num, 2).PadLeft(32, '0');
    }

    public static uint ClearLower16Bits(this ref uint num)
    {
        return num &= 0xFFFF0000;
    }

    public static uint ClearUpper16Bits(this ref uint num)
    {
        return num &= 0x0000FFFF;
    }

    public static uint SetLower16Bits(this ref uint num, uint value)
    {
        num &= 0xFFFF0000;
        num |= value;
        return num;
    }

    public static uint SetUpper16Bits(this ref uint num, uint value)
    {
        num &= 0x0000FFFF;
        num |= value << 16;
        return num;
    }

    public static uint GetLower16Bits(this uint num)
    {
        return num & 0x0000FFFF;
    }

    public static uint GetUpper16Bits(this uint num)
    {
        return num >> 16;
    }
}
