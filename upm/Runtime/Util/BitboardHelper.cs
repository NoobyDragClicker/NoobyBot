using System;

public static class BitboardHelper
{
    public static string convertToPrintableBitboard(ulong bitboard)
    {
        string strBinary = Convert.ToString((long)bitboard, 2).PadLeft(64, '0');
        string boardRepresentation = "";
        for (int x = 0; x < 64; x++)
        {
            boardRepresentation += strBinary[63 - x];
            if (x % 8 == 7) { boardRepresentation += "\n"; }
        }
        return boardRepresentation;

    }
    public static int PopLSB(ref ulong b)
    {
        int i = TrailingZeroCount(b);
        b &= b - 1;
        return i;
    }

    public static void SetSquare(ref ulong bitboard, int squareIndex)
    {
        bitboard |= 1ul << squareIndex;
    }

    public static void ClearSquare(ref ulong bitboard, int squareIndex)
    {
        bitboard &= ~(1ul << squareIndex);
    }

    public static void ToggleSquare(ref ulong bitboard, int squareIndex)
    {
        bitboard ^= 1ul << squareIndex;
    }
    

    private static readonly int[] MultiplyDeBruijnBitPosition64 = new int[64]
    {
        0, 1, 2, 53, 3, 7, 54, 27,
        4, 38, 41, 8, 34, 55, 48, 28,
        62, 5, 39, 46, 44, 42, 22, 9,
        24, 35, 59, 56, 49, 18, 29, 11,
        63, 52, 6, 26, 37, 40, 33, 47,
        61, 45, 43, 21, 23, 58, 17, 10,
        51, 25, 36, 32, 60, 20, 57, 16,
        50, 31, 19, 15, 30, 14, 13, 12
    };

    public static int TrailingZeroCount(ulong v)
    {
        if (v == 0) return 64;
        unchecked
        {
            ulong isolated = v & (ulong)-(long)v;
            return MultiplyDeBruijnBitPosition64[(isolated * 0x022FDD63CC95386DUL) >> 58];
        }
    }
}