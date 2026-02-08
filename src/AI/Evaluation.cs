using System;
using System.Drawing;
using System.Linq.Expressions;
using System.Net.Security;

public class Evaluation
{

    int colorTurn;

    int[] isolatedPawnCount;
    SearchLogger logger;

    //Unused in actual eval
    public static int pawnValue = 90;
    public static int knightValue = 336;
    public static int bishopValue = 366;
    public static int rookValue = 538;
    public static int queenValue = 1024;
    
    public static int[,] mg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 68, 80, 69, 82, 70, 61, 30, 4, 63, 76, 104, 103, 110, 154, 137, 89, 43, 69, 70, 71, 93, 86, 83, 64, 29, 60, 56, 75, 72, 63, 64, 45, 28, 54, 53, 54, 67, 57, 80, 54, 29, 55, 48, 40, 55, 73, 88, 45, 0, 0, 0, 0, 0, 0, 0, 0},
        {141, 159, 205, 242, 252, 211, 168, 185, 230, 255, 272, 291, 273, 334, 246, 275, 236, 277, 307, 313, 345, 339, 307, 267, 237, 254, 280, 308, 289, 313, 264, 274, 221, 238, 258, 262, 270, 262, 260, 235, 202, 229, 245, 245, 258, 250, 251, 222, 185, 197, 217, 233, 234, 235, 215, 219, 141, 200, 176, 199, 207, 219, 206, 165},
        {240, 216, 209, 177, 198, 205, 205, 205, 243, 265, 253, 249, 273, 265, 263, 237, 247, 272, 274, 295, 277, 311, 282, 273, 238, 248, 273, 285, 283, 280, 247, 240, 222, 241, 248, 270, 266, 249, 244, 232, 235, 245, 244, 250, 249, 243, 246, 249, 238, 241, 254, 230, 238, 251, 257, 241, 207, 238, 220, 209, 213, 215, 233, 219},
        {340, 323, 335, 334, 359, 351, 334, 376, 334, 335, 355, 374, 361, 391, 371, 398, 315, 339, 345, 349, 372, 378, 404, 381, 300, 315, 319, 328, 337, 341, 340, 345, 280, 288, 292, 307, 310, 298, 317, 312, 276, 283, 293, 292, 300, 300, 335, 315, 272, 284, 300, 299, 302, 309, 324, 294, 297, 296, 307, 313, 319, 310, 320, 300},
        {475, 494, 527, 555, 557, 585, 579, 526, 515, 489, 505, 504, 519, 546, 524, 558, 510, 510, 516, 528, 531, 577, 571, 559, 491, 489, 504, 500, 508, 517, 507, 518, 480, 488, 481, 493, 495, 489, 503, 504, 481, 485, 481, 480, 484, 491, 506, 503, 475, 483, 493, 494, 490, 503, 509, 520, 465, 459, 468, 487, 476, 462, 492, 485},
        {-78, -91, -61, -114, -69, -6, 37, 104, -103, -37, -94, -16, -45, -31, 45, 27, -107, 2, -80, -91, -66, 14, 7, -27, -66, -76, -85, -157, -136, -98, -86, -108, -63, -64, -110, -139, -138, -104, -97, -115, -28, -19, -64, -80, -73, -79, -33, -45, 56, 18, -2, -35, -40, -19, 29, 41, 45, 73, 46, -54, 12, -34, 56, 54}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 148, 125, 141, 119, 118, 117, 134, 150, 123, 122, 110, 109, 100, 96, 118, 119, 115, 103, 102, 89, 92, 91, 92, 95, 101, 96, 97, 98, 96, 94, 84, 83, 99, 90, 99, 103, 106, 101, 81, 82, 105, 98, 107, 103, 121, 105, 83, 87, 0, 0, 0, 0, 0, 0, 0, 0},
        {223, 271, 293, 277, 291, 266, 271, 200, 273, 285, 301, 300, 290, 277, 282, 253, 289, 300, 313, 315, 300, 301, 288, 275, 291, 315, 330, 332, 332, 325, 316, 288, 302, 312, 332, 331, 334, 324, 308, 284, 283, 300, 313, 327, 322, 305, 293, 283, 278, 293, 299, 299, 299, 294, 281, 281, 255, 250, 282, 284, 279, 269, 263, 270},
        {306, 317, 317, 329, 323, 313, 316, 300, 295, 313, 318, 319, 309, 314, 313, 298, 320, 317, 329, 317, 322, 324, 318, 314, 319, 336, 332, 344, 336, 331, 332, 316, 316, 334, 342, 339, 339, 336, 326, 305, 316, 323, 334, 335, 339, 333, 316, 307, 317, 309, 309, 319, 324, 310, 314, 291, 295, 311, 289, 307, 301, 309, 296, 272},
        {554, 561, 569, 565, 555, 555, 560, 543, 555, 563, 565, 554, 555, 545, 549, 534, 556, 554, 554, 547, 540, 537, 533, 529, 557, 553, 561, 553, 540, 541, 539, 533, 552, 553, 555, 550, 546, 547, 534, 533, 545, 543, 540, 545, 539, 534, 511, 514, 538, 541, 539, 539, 530, 528, 516, 523, 543, 544, 549, 546, 540, 540, 532, 531},
        {1014, 1018, 1030, 1023, 1024, 983, 960, 995, 997, 1035, 1056, 1066, 1076, 1046, 1024, 1009, 996, 1013, 1050, 1061, 1070, 1050, 1007, 1017, 1008, 1039, 1048, 1075, 1086, 1071, 1068, 1039, 1024, 1034, 1054, 1073, 1070, 1064, 1041, 1029, 989, 1027, 1046, 1048, 1045, 1041, 1008, 992, 995, 999, 1006, 1010, 1017, 980, 944, 911, 984, 997, 1000, 1009, 996, 978, 940, 938},
        {-72, -34, -23, 6, -11, -3, -2, -89, -16, 13, 24, 11, 29, 35, 28, 5, -4, 17, 37, 48, 50, 45, 40, 9, -15, 19, 41, 59, 60, 55, 41, 15, -22, 6, 36, 53, 54, 41, 25, 8, -31, -5, 15, 30, 29, 24, 5, -9, -36, -17, -7, 2, 8, 3, -13, -34, -70, -59, -40, -26, -46, -23, -47, -74}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 68, 80, 69, 82, 70, 61, 30, 4, 30, 32, 21, 13, 12, -4, -39, -50, 17, 5, 18, 19, 1, 7, -29, -20, -2, -14, -23, -12, -22, -8, -23, -14, -7, -24, -27, -21, -26, -17, -24, 1, -14, -13, -18, -21, -11, -12, -0, -9, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 148, 125, 141, 119, 118, 117, 134, 150, 141, 142, 108, 79, 81, 96, 114, 136, 86, 83, 51, 45, 43, 51, 82, 84, 61, 54, 35, 24, 28, 36, 61, 61, 23, 33, 20, 11, 12, 13, 42, 23, 25, 25, 14, 6, -7, 7, 21, 22, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {32, 12, -8, -24, -43, -69, -85, -84, 49};
    public static int doubledPawnPenalty = -27;
    public static int bishopPairBonusMG = 42;
    public static int bishopPairBonusEG = 55;
    public static int rookOpenFileMG = 22;
    public static int rookOpenFileEG = 20;
    public static int kingOpenFileMG = -49;
    public static int kingOpenFileEG = 7;

    int playerTurnMultiplier;
    public Evaluation(SearchLogger logger)
    {
        this.logger = logger;
        isolatedPawnCount = new int[2];
    }
    public int EvaluatePosition(Board board)
    {
        colorTurn = board.colorTurn;
        playerTurnMultiplier = (colorTurn == Piece.White) ? 1 : -1;

        isolatedPawnCount[0] = 0;
        isolatedPawnCount[1] = 0;
        int boardVal = IncrementalCount(board);
        return boardVal;
    }

    int IncrementalCount(Board board)
    {
        const int totalPhase = 24;
        int mgScore = board.gameStateHistory[board.fullMoveClock].mgPSQTVal;
        int egScore = board.gameStateHistory[board.fullMoveClock].egPSQTVal;


        ulong whitePawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)];
        ulong blackPawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)];

        ulong whiteRooks = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Rook)];
        ulong blackRooks = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Rook)];

        ulong whiteKing = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.King)];
        ulong blackKing = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.King)];


        while (whitePawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref whitePawns);
            (int, int) pawnBonus = EvaluatePawnStrength(board, index, Board.WhiteIndex);
            mgScore += pawnBonus.Item1;
            egScore += pawnBonus.Item2;
        }

        while (blackPawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackPawns);
            (int, int) pawnBonus = EvaluatePawnStrength(board, index, Board.BlackIndex);
            mgScore -= pawnBonus.Item1;
            egScore -= pawnBonus.Item2;
        }

        while (whiteRooks != 0)
        {
            int index = BitboardHelper.PopLSB(ref whiteRooks);
            if((BitboardHelper.files[index % 8] & (board.allPiecesBitboard ^ 1ul << index)) == 0)
            {
                mgScore += rookOpenFileMG;
                egScore += rookOpenFileMG;
            }
        }

        while (blackRooks != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackRooks);
            if((BitboardHelper.files[index % 8] & (board.allPiecesBitboard ^ 1ul << index)) == 0)
            {
                mgScore -= rookOpenFileMG;
                egScore -= rookOpenFileMG;
            }
        }
        int whiteKingIndex = BitboardHelper.PopLSB(ref whiteKing);
        int blackKingIndex = BitboardHelper.PopLSB(ref blackKing);

        if(((board.sideBitboard[Board.WhiteIndex] ^ 1ul << whiteKingIndex) & BitboardHelper.files[whiteKingIndex % 8]) == 0)
        {
            mgScore += kingOpenFileMG;
            egScore += kingOpenFileEG;
        }
        if(((board.sideBitboard[Board.BlackIndex] ^ 1ul << blackKingIndex) & BitboardHelper.files[blackKingIndex % 8]) == 0)
        {
            mgScore -= kingOpenFileMG;
            egScore -= kingOpenFileEG;
        }

        egScore += isolatedPawnPenalty[isolatedPawnCount[Board.WhiteIndex]];
        egScore -= isolatedPawnPenalty[isolatedPawnCount[Board.BlackIndex]];


        if(board.pieceCounts[Board.WhiteIndex, Piece.Bishop] >= 2)
        {
            mgScore += bishopPairBonusMG;
            egScore += bishopPairBonusMG;
        }
        if(board.pieceCounts[Board.BlackIndex, Piece.Bishop] >= 2)
        {
            mgScore -= bishopPairBonusMG;
            egScore -= bishopPairBonusMG;
        }

        int phase = (4 * (board.pieceCounts[Board.WhiteIndex, Piece.Queen] + board.pieceCounts[Board.BlackIndex, Piece.Queen])) + (2 * (board.pieceCounts[Board.WhiteIndex, Piece.Rook] + board.pieceCounts[Board.BlackIndex, Piece.Rook]));
        phase += board.pieceCounts[Board.WhiteIndex, Piece.Knight] + board.pieceCounts[Board.BlackIndex, Piece.Knight] + board.pieceCounts[Board.WhiteIndex, Piece.Bishop] + board.pieceCounts[Board.BlackIndex, Piece.Bishop];

        
        if (phase > 24) { phase = 24; }
        return (mgScore * phase + egScore * (totalPhase - phase)) / totalPhase * playerTurnMultiplier;
    }


    int EvaluateKingSafety(Board board, int kingIndex, int kingColor)
    {
        int penaltyMultiplier;
        int numPenalties = 0;
        if (kingColor == Piece.White)
        {
            //Pawn shield
            //Not back rank
            if (Coord.IndexToRank(kingIndex) != 8)
            {
                //White Pawn in front
                if (board.PieceAt(kingIndex - 8) != Piece.Pawn || board.ColorAt(kingIndex - 8) != Piece.White) { numPenalties += 1; }

                //White Pawn front left
                if (Coord.IndexToFile(kingIndex) != 1)
                {
                    if (board.PieceAt(kingIndex - 9) != Piece.Pawn || board.ColorAt(kingIndex - 9) != Piece.White) { numPenalties += 1; }
                }
                //White Pawn front right
                if (Coord.IndexToFile(kingIndex) != 8)
                {
                    if (board.PieceAt(kingIndex - 7) != Piece.Pawn || board.ColorAt(kingIndex - 7) != Piece.White) { numPenalties += 1; }
                }
            }
            penaltyMultiplier = (!board.HasKingsideRight(Piece.White) && !board.HasQueensideRight(Piece.White)) ? 6 : 1;
        }
        else
        {
            //Pawn shield
            //Not back rank
            if (Coord.IndexToRank(kingIndex) != 1)
            {
                //Black Pawn in front
                if (board.PieceAt(kingIndex + 8) != Piece.Pawn || board.ColorAt(kingIndex + 8) != Piece.White) { numPenalties += 1; }

                //Black Pawn front left
                if (Coord.IndexToFile(kingIndex) != 1)
                {
                    if (board.PieceAt(kingIndex + 7) != Piece.Pawn || board.ColorAt(kingIndex + 7) != Piece.White) { numPenalties += 1; }
                }
                //Black Pawn front right
                if (Coord.IndexToFile(kingIndex) != 8)
                {
                    if (board.PieceAt(kingIndex + 9) != Piece.Pawn || board.ColorAt(kingIndex + 9) != Piece.White) { numPenalties += 1; }
                }
            }
            penaltyMultiplier = (!board.HasKingsideRight(Piece.Black) && !board.HasQueensideRight(Piece.Black)) ? 6 : 1;
        }
        return numPenalties * -5 * penaltyMultiplier;
    }

    (int, int) EvaluatePawnStrength(Board board, int pawnIndex, int currentColorIndex)
    {
        int mgBonus = 0;
        int egBonus = 0;

        int oppositeColorIndex = 1 - currentColorIndex;
        int currentColor = currentColorIndex == Board.WhiteIndex ? Piece.White : Piece.Black;

        bool passer = (board.pieceBitboards[Board.PieceBitboardIndex(oppositeColorIndex, Piece.Pawn)] & BitboardHelper.pawnPassedMask[currentColorIndex, pawnIndex]) == 0;
        int pushSquare = pawnIndex + (currentColorIndex == Board.WhiteIndex ? -8 : 8);
        //Passed pawn
        if (passer) { 
            int psqtIndex = currentColorIndex == Board.WhiteIndex ? pawnIndex : pawnIndex ^ 56;
            mgBonus += passedPawnBonuses[0, psqtIndex]; 
            egBonus += passedPawnBonuses[1, psqtIndex]; 
        }

        //Doubled pawn penalty
        if (board.PieceAt(pushSquare) == Piece.Pawn && board.ColorAt(pushSquare) == currentColor) { egBonus += doubledPawnPenalty; }
        if ((BitboardHelper.isolatedPawnMask[pawnIndex] & board.pieceBitboards[Board.PieceBitboardIndex(currentColorIndex, Piece.Pawn)]) == 0) { isolatedPawnCount[currentColorIndex]++; }

        return (mgBonus, egBonus);
    }

    int EvaluateBishopMobility(Board board, int pieceIndex, int pieceColor)
    {
        int numMoves = 0;
        ulong simpleBishopMoves = BitboardHelper.GetBishopAttacks(pieceIndex, board.allPiecesBitboard) & board.sideBitboard[pieceColor == Piece.White ? Board.WhiteIndex : Board.BlackIndex];
        while (simpleBishopMoves != 0) { numMoves++; BitboardHelper.PopLSB(ref simpleBishopMoves); }
        return (numMoves * 2) - 10;
    }
    int EvaluateRookMobility(Board board, int pieceIndex, int pieceColor)
    {
        int numMoves = 0;
        ulong simpleRookMoves = BitboardHelper.GetRookAttacks(pieceIndex, board.allPiecesBitboard) & board.sideBitboard[pieceColor == Piece.White ? Board.WhiteIndex : Board.BlackIndex];
        while (simpleRookMoves != 0) { numMoves++; BitboardHelper.PopLSB(ref simpleRookMoves); }
        return (numMoves * 2) - 10;
    }

}
