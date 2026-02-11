using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

public static class BitboardHelper
{
    public const ulong FILE_1 = 0x0101010101010101;
    public const ulong RANK_8 = 0b11111111;
    public const ulong whiteKingsideCastleMask = 1ul << 62 | 1ul << 61;
    public const ulong blackKingsideCastleMask = 1ul << 5 | 1ul << 6;
    public const ulong whiteQueensideAttackCastleMask = 1ul << 59 | 1ul << 58;
    public const ulong whiteQueensidePieceCastleMask = whiteQueensideAttackCastleMask | 1ul << 57;
    public const ulong blackQueensideAttackCastleMask = 1ul << 2 | 1ul << 3;
    public const ulong blackQueensidePieceCastleMask = blackQueensideAttackCastleMask | 1ul << 1;
    public static readonly ulong[] files;
    public static readonly ulong[] knightAttacks;

    //For magic bitboards, don't include edgess
    public static readonly ulong[] rookMasks;
    public static readonly ulong[] bishopMasks;

    public static readonly ulong[][] rookAttacks;
    public static readonly ulong[][] bishopAttacks;


    public static readonly ulong[] kingAttacks;
    public static readonly ulong[] wPawnAttacks;
    public static readonly ulong[] wPawnMoves;
    public static readonly ulong[] wPawnDouble;
    public static readonly ulong[] wPawnDoubleMask;
    public static readonly ulong[,] pawnPassedMask;

    public static readonly ulong[] bPawnAttacks;
    public static readonly ulong[] bPawnMoves;
    public static readonly ulong[] bPawnDouble;
    public static readonly ulong[] bPawnDoubleMask;
    public static readonly ulong[] isolatedPawnMask;
    public static readonly ulong[,] kingRing;

    public static readonly (int x, int y)[] knightJumps = { (-2, -1), (2, -1), (2, 1), (-2, 1), (-1, -2), (-1, 2), (1, 2), (1, -2) };
    public static readonly (int x, int y)[] kingDirections = { (1, 0), (1, -1), (1, 1), (-1, 0), (-1, -1), (-1, 1), (0, -1), (0, 1) };
    public static readonly (int x, int y)[] bishopDirections = { (1, 1), (1, -1), (-1, 1), (-1, -1) };
    public static readonly (int x, int y)[] rookDirections = { (1, 0), (-1, 0), (0, 1), (0, -1) };


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
        int i = BitOperations.TrailingZeroCount(b);
        b &= b - 1;
        return i;
    }
    public static int GetLSB(ulong b)
    {
        int i = BitOperations.TrailingZeroCount(b);
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

    public static bool ContainsSquare(ulong bitboard, int square)
    {
        return ((bitboard >> square) & 1) != 0;
    }

    private static readonly int[] index64 = new int[64]
    {
        0,  1, 48,  2, 57, 49, 28,  3,
        61, 58, 50, 42, 38, 29, 17,  4,
        62, 55, 59, 36, 53, 51, 43, 22,
        45, 39, 33, 30, 24, 18, 12,  5,
        63, 47, 56, 27, 60, 41, 37, 16,
        54, 35, 52, 21, 44, 32, 23, 11,
        46, 26, 40, 15, 34, 20, 31, 10,
        25, 14, 19,  9, 13,  8,  7,  6
    };

    public static int TrailingZeroCount(ulong bb)
    {
        const ulong debruijn64 = 0x03f79d71b4cb0a89UL;
        return index64[((bb & (~bb + 1)) * debruijn64) >> 58];
    }

    //Creates a list of all the possible blocker combinations
    public static ulong[] GenerateAllBlockerBitboards(ulong pieceMask)
    {
        List<int> squaresInPieceMask = new List<int>();

        for (int index = 0; index < 64; index++)
        {
            //Isolate each bit, one at a time to see if its active
            if (((pieceMask >> index) & 1) == 1)
            {
                squaresInPieceMask.Add(index);
            }
        }

        int numBlockerBitboards = 1 << squaresInPieceMask.Count;
        ulong[] blockerBitboards = new ulong[numBlockerBitboards];

        // Create all bitboards

        //Loop through the total number of bitboards
        for (int patternIndex = 0; patternIndex < numBlockerBitboards; patternIndex++)
        {
            for (int bitIndex = 0; bitIndex < squaresInPieceMask.Count; bitIndex++)
            {
                //Uses the current number of the patternIndex to see whether this bit should be toggled
                int bit = (patternIndex >> bitIndex) & 1;
                blockerBitboards[patternIndex] |= (ulong)bit << squaresInPieceMask[bitIndex];
            }
        }

        return blockerBitboards;

    }

    //Generates a legal moves bitboard from a given blocker, for either rook or bishop
    public static ulong LegalMoveBitboardFromBlocker(int startSquare, ulong blockerBitboard, bool ortho)
    {
        int x = Coord.IndexToFile(startSquare) - 1;
        int y = 8 - Coord.IndexToRank(startSquare);
        ulong bitboard = 0;

        (int x, int y)[] directions = ortho ? rookDirections : bishopDirections;

        foreach ((int xOffset, int yOffset) dir in directions)
        {
            for (int dst = 1; dst < 8; dst++)
            {
                int squareIndex;

                if (ValidSquareIndex(x + (dir.xOffset * dst), y + (dir.yOffset * dst), out squareIndex))
                {
                    SetSquare(ref bitboard, squareIndex);
                    //If we've hit a blocker, break after adding them to the bitboard
                    if (ContainsSquare(blockerBitboard, squareIndex))
                    {
                        break;
                    }
                }
                else { break; }
            }
        }

        return bitboard;
    }

    public static ulong GetRookAttacks(int square, ulong pieceBitboards)
    {
        ulong key = ((pieceBitboards & rookMasks[square]) * Magics.rookMagics[square]) >> Magics.rookShifts[square];
        return rookAttacks[square][key];
    }
    public static ulong GetBishopAttacks(int square, ulong pieceBitboards)
    {
        ulong key = ((pieceBitboards & bishopMasks[square]) * Magics.bishopMagics[square]) >> Magics.bishopShifts[square];
        return bishopAttacks[square][key];
    }

    public static ulong GetAllPawnAttacks(ulong pawns, int pawnColor)
    {
        if (pawnColor == Piece.White)
        {
            pawns = (pawns >> 9 & ~(FILE_1 << 7)) | (pawns >> 7 & ~FILE_1);
        }
        else
        {
            pawns = (pawns << 9 & ~FILE_1) | (pawns << 7 & ~(FILE_1 << 7));
        }
        return pawns;
    }
    //Runtime-computed data
    static BitboardHelper()
    {
        knightAttacks = new ulong[64];
        rookMasks = new ulong[64];
        rookAttacks = new ulong[64][];
        bishopMasks = new ulong[64];
        bishopAttacks = new ulong[64][];
        kingAttacks = new ulong[64];
        wPawnAttacks = new ulong[64];
        bPawnAttacks = new ulong[64];
        wPawnMoves = new ulong[64];
        bPawnMoves = new ulong[64];
        wPawnDouble = new ulong[64];
        bPawnDouble = new ulong[64];
        wPawnDoubleMask = new ulong[64];
        bPawnDoubleMask = new ulong[64];
        pawnPassedMask = new ulong[2, 64];
        kingRing = new ulong[2, 64];
        isolatedPawnMask = new ulong[64];
        files = new ulong[8];
        
        for (int i = 0; i < 8; i++)
        {
            files[i] = FILE_1 << i;
        }

        //Filling in the attack bitboards
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                int startIndex = y * 8 + x;
                int attackIndex;

                //King
                for (int direction = 0; direction < kingDirections.Length; direction++)
                {
                    if (ValidSquareIndex(x + kingDirections[direction].x, y + kingDirections[direction].y, out attackIndex))
                    {
                        kingAttacks[startIndex] |= 1ul << attackIndex;
                    }
                }
                kingRing[Board.WhiteIndex, startIndex] = kingAttacks[startIndex] | (kingAttacks[startIndex] >> 8);
                kingRing[Board.BlackIndex, startIndex] = kingAttacks[startIndex] | (kingAttacks[startIndex] << 8);

                //Knights
                for (int knightIndex = 0; knightIndex < knightJumps.Length; knightIndex++)
                {
                    if (ValidSquareIndex(x + knightJumps[knightIndex].x, y + knightJumps[knightIndex].y, out attackIndex))
                    {
                        knightAttacks[startIndex] |= 1ul << attackIndex;
                    }
                }

                //Full file + rank except the square where they intercept (where the rook is)
                rookMasks[startIndex] = (FILE_1 << x) ^ (RANK_8 << 8 * y);

                //Masking out the edges the rook is not on, saves space for magics table
                //File 1
                if (x != 0) { rookMasks[startIndex] &= ~FILE_1; }
                //File 8
                if (x != 7) { rookMasks[startIndex] &= ~(FILE_1 << 7); }
                //Rank 8
                if (y != 0) { rookMasks[startIndex] &= ~RANK_8; }
                //Rank 1
                if (y != 7) { rookMasks[startIndex] &= ~(RANK_8 << 56); }


                //Bishop
                for (int directionNumber = 0; directionNumber < bishopDirections.Length; directionNumber++)
                {
                    for (int diag = 0; diag < 8; diag++)
                    {
                        if (ValidBishopSquareIndex(x + (bishopDirections[directionNumber].x * (diag + 1)), y + (bishopDirections[directionNumber].y * (diag + 1)), out attackIndex))
                        {
                            bishopMasks[startIndex] |= 1ul << attackIndex;
                        }
                        else
                        {
                            break;
                        }
                    }
                }


                //White Pawn
                if (ValidSquareIndex(x + 1, y - 1, out attackIndex))
                {
                    wPawnAttacks[startIndex] |= 1ul << attackIndex;
                }
                if (ValidSquareIndex(x - 1, y - 1, out attackIndex))
                {
                    wPawnAttacks[startIndex] |= 1ul << attackIndex;
                }
                if (ValidSquareIndex(x, y - 1, out attackIndex))
                {
                    wPawnMoves[startIndex] |= 1ul << attackIndex;
                }
                if (ValidSquareIndex(x, y - 2, out attackIndex) && y == 6)
                {
                    wPawnDouble[startIndex] |= 1ul << attackIndex;
                    wPawnDoubleMask[startIndex] = wPawnMoves[startIndex] | wPawnDouble[startIndex];
                }
                ulong singleWhiteFile = FILE_1 >> (64 - startIndex);
                pawnPassedMask[Board.WhiteIndex, startIndex] = singleWhiteFile;
                if (x != 0) { pawnPassedMask[Board.WhiteIndex, startIndex] |= singleWhiteFile >> 1; }
                ;
                if (x != 7) { pawnPassedMask[Board.WhiteIndex, startIndex] |= singleWhiteFile << 1; }
                ;
                if (startIndex < 8) { pawnPassedMask[Board.WhiteIndex, startIndex] = 0; }

                //Black Pawn
                if (ValidSquareIndex(x + 1, y + 1, out attackIndex))
                {
                    bPawnAttacks[startIndex] |= 1ul << attackIndex;
                }
                if (ValidSquareIndex(x - 1, y + 1, out attackIndex))
                {
                    bPawnAttacks[startIndex] |= 1ul << attackIndex;
                }
                if (ValidSquareIndex(x, y + 1, out attackIndex))
                {
                    bPawnMoves[startIndex] |= 1ul << attackIndex;
                }
                if (ValidSquareIndex(x, y + 2, out attackIndex) && y == 1)
                {
                    bPawnDouble[startIndex] |= 1ul << attackIndex;
                    bPawnDoubleMask[startIndex] = bPawnMoves[startIndex] | bPawnDouble[startIndex];
                }

                pawnPassedMask[Board.BlackIndex, startIndex] = FILE_1 << (startIndex + 8);
                if (x != 0) { pawnPassedMask[Board.BlackIndex, startIndex] |= FILE_1 << (startIndex + 7); }
                if (x != 7) { pawnPassedMask[Board.BlackIndex, startIndex] |= FILE_1 << (startIndex + 9); }
                if (startIndex > 55) { pawnPassedMask[Board.BlackIndex, startIndex] = 0; }


                if (x != 0) { isolatedPawnMask[startIndex] |= FILE_1 << x - 1; }
                if (x != 7) { isolatedPawnMask[startIndex] |= FILE_1 << x + 1; }

            }

        }
        //Don't include edges for bishop mask
        bool ValidBishopSquareIndex(int x, int y, out int index)
        {
            index = y * 8 + x;
            return x >= 1 && x < 7 && y >= 1 && y < 7;
        }

        for (int index = 0; index < 64; index++)
        {
            rookAttacks[index] = CreateMagicTable(index, true, Magics.rookMagics[index], Magics.rookShifts[index]);
            bishopAttacks[index] = CreateMagicTable(index, false, Magics.bishopMagics[index], Magics.bishopShifts[index]);
        }
        ulong[] CreateMagicTable(int square, bool rook, ulong magic, int shift)
        {
            int numBits = 64 - shift;
            int lookupsize = 1 << numBits;
            ulong[] table = new ulong[lookupsize];

            ulong movementMask = rook ? rookMasks[square] : bishopMasks[square];
            ulong[] blockerBitboards = GenerateAllBlockerBitboards(movementMask);

            foreach (ulong blockerBitboard in blockerBitboards)
            {
                ulong index = (blockerBitboard * magic) >> shift;
                ulong moves = LegalMoveBitboardFromBlocker(square, blockerBitboard, rook);
                table[index] = moves;
            }
            return table;
        }
    }

    public static bool ValidSquareIndex(int x, int y, out int index)
    {
        index = y * 8 + x;
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }
    
}