using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

public static class BitboardHelper
{
    public static readonly Bitboard FILE_1 = 0x0101010101010101;
    public static readonly Bitboard RANK_8 = 0b11111111;
    public static readonly Bitboard whiteKingsideCastleMask = 1ul << 62 | 1ul << 61;
    public static readonly Bitboard blackKingsideCastleMask = 1ul << 5 | 1ul << 6;
    public static readonly Bitboard whiteQueensideAttackCastleMask = 1ul << 59 | 1ul << 58;
    public static readonly Bitboard whiteQueensidePieceCastleMask = whiteQueensideAttackCastleMask | 1ul << 57;
    public static readonly Bitboard blackQueensideAttackCastleMask = 1ul << 2 | 1ul << 3;
    public static readonly Bitboard blackQueensidePieceCastleMask = blackQueensideAttackCastleMask | 1ul << 1;
    public static readonly Bitboard[] files;
    public static readonly Bitboard[] knightAttacks;

    //For magic bitboards, don't include edgess
    public static readonly Bitboard[] rookMasks;
    public static readonly Bitboard[] bishopMasks;

    public static readonly Bitboard[][] rookAttacks;
    public static readonly Bitboard[][] bishopAttacks;


    public static readonly Bitboard[] kingAttacks;
    public static readonly Bitboard[] wPawnAttacks;
    public static readonly Bitboard[] wPawnMoves;
    public static readonly Bitboard[] wPawnDouble;
    public static readonly Bitboard[] wPawnDoubleMask;
    public static readonly Bitboard[,] pawnPassedMask;

    public static readonly Bitboard[] bPawnAttacks;
    public static readonly Bitboard[] bPawnMoves;
    public static readonly Bitboard[] bPawnDouble;
    public static readonly Bitboard[] bPawnDoubleMask;
    public static readonly Bitboard[] isolatedPawnMask;
    public static readonly Bitboard[,] kingRing;

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

    //Creates a list of all the possible blocker combinations
    public static Bitboard[] GenerateAllBlockerBitboards(Bitboard pieceMask)
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
        Bitboard[] blockerBitboards = new Bitboard[numBlockerBitboards];

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
    public static Bitboard LegalMoveBitboardFromBlocker(int startSquare, Bitboard blockerBitboard, bool ortho)
    {
        int x = Coord.IndexToFile(startSquare) - 1;
        int y = 8 - Coord.IndexToRank(startSquare);
        Bitboard bitboard = 0;

        (int x, int y)[] directions = ortho ? rookDirections : bishopDirections;

        foreach ((int xOffset, int yOffset) dir in directions)
        {
            for (int dst = 1; dst < 8; dst++)
            {
                int squareIndex;

                if (ValidSquareIndex(x + (dir.xOffset * dst), y + (dir.yOffset * dst), out squareIndex))
                {
                    bitboard.SetSquare(squareIndex);
                    //If we've hit a blocker, break after adding them to the bitboard
                    if (blockerBitboard.ContainsSquare(squareIndex))
                    {
                        break;
                    }
                }
                else { break; }
            }
        }

        return bitboard;
    }

    public static Bitboard GetRookAttacks(int square, Bitboard pieceBitboards)
    {
        ulong key = ((pieceBitboards & rookMasks[square]) * Magics.rookMagics[square]) >> Magics.rookShifts[square];
        return rookAttacks[square][key];
    }
    public static Bitboard GetBishopAttacks(int square, Bitboard pieceBitboards)
    {
        ulong key = ((pieceBitboards & bishopMasks[square]) * Magics.bishopMagics[square]) >> Magics.bishopShifts[square];
        return bishopAttacks[square][key];
    }

    public static Bitboard GetAllPawnAttacks(Bitboard pawns, int pawnColor)
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
        knightAttacks = new Bitboard[64];
        rookMasks = new Bitboard[64];
        rookAttacks = new Bitboard[64][];
        bishopMasks = new Bitboard[64];
        bishopAttacks = new Bitboard[64][];
        kingAttacks = new Bitboard[64];
        wPawnAttacks = new Bitboard[64];
        bPawnAttacks = new Bitboard[64];
        wPawnMoves = new Bitboard[64];
        bPawnMoves = new Bitboard[64];
        wPawnDouble = new Bitboard[64];
        bPawnDouble = new Bitboard[64];
        wPawnDoubleMask = new Bitboard[64];
        bPawnDoubleMask = new Bitboard[64];
        pawnPassedMask = new Bitboard[2, 64];
        kingRing = new Bitboard[2, 64];
        isolatedPawnMask = new Bitboard[64];
        files = new Bitboard[8];
        
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
                        kingAttacks[startIndex].SetSquare(attackIndex);
                    }
                }
                kingRing[Board.WhiteIndex, startIndex] = kingAttacks[startIndex] | (kingAttacks[startIndex] >> 8);
                kingRing[Board.BlackIndex, startIndex] = kingAttacks[startIndex] | (kingAttacks[startIndex] << 8);

                //Knights
                for (int knightIndex = 0; knightIndex < knightJumps.Length; knightIndex++)
                {
                    if (ValidSquareIndex(x + knightJumps[knightIndex].x, y + knightJumps[knightIndex].y, out attackIndex))
                    {
                        knightAttacks[startIndex].SetSquare(attackIndex);
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
                            bishopMasks[startIndex].SetSquare(attackIndex);
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
                Bitboard singleWhiteFile = FILE_1 >> (64 - startIndex);
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
        Bitboard[] CreateMagicTable(int square, bool rook, ulong magic, int shift)
        {
            int numBits = 64 - shift;
            int lookupsize = 1 << numBits;
            Bitboard[] table = new Bitboard[lookupsize];

            Bitboard movementMask = rook ? rookMasks[square] : bishopMasks[square];
            Bitboard[] blockerBitboards = GenerateAllBlockerBitboards(movementMask);

            foreach (Bitboard blockerBitboard in blockerBitboards)
            {
                Bitboard index = (blockerBitboard * magic) >> shift;
                Bitboard moves = LegalMoveBitboardFromBlocker(square, blockerBitboard, rook);
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