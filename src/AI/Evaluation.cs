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
        {0, 0, 0, 0, 0, 0, 0, 0, 70, 82, 69, 82, 70, 64, 38, 15, 63, 77, 106, 103, 113, 154, 145, 93, 43, 69, 71, 71, 96, 86, 90, 69, 29, 60, 56, 75, 75, 64, 72, 51, 29, 54, 53, 54, 70, 57, 89, 59, 29, 56, 48, 40, 58, 74, 97, 50, 0, 0, 0, 0, 0, 0, 0, 0},
        {139, 158, 204, 241, 252, 211, 166, 184, 229, 253, 271, 290, 271, 334, 245, 274, 235, 276, 306, 311, 345, 338, 307, 270, 237, 252, 279, 307, 290, 311, 264, 273, 220, 237, 257, 261, 271, 261, 260, 235, 201, 228, 244, 244, 258, 250, 251, 221, 184, 197, 216, 232, 234, 234, 216, 218, 140, 199, 175, 197, 205, 218, 204, 163},
        {240, 215, 209, 176, 198, 205, 205, 204, 243, 264, 252, 248, 273, 265, 265, 246, 246, 272, 274, 294, 277, 311, 285, 273, 237, 247, 272, 284, 283, 279, 247, 239, 221, 240, 247, 269, 267, 248, 245, 231, 234, 244, 243, 249, 250, 243, 246, 249, 237, 240, 253, 229, 238, 251, 258, 240, 206, 237, 219, 207, 212, 214, 232, 219},
        {339, 321, 333, 332, 358, 349, 332, 374, 332, 333, 353, 372, 359, 389, 369, 396, 314, 337, 343, 347, 370, 377, 404, 381, 298, 313, 317, 326, 336, 339, 341, 345, 278, 286, 290, 305, 309, 296, 318, 311, 274, 281, 291, 290, 299, 298, 335, 314, 270, 283, 298, 296, 301, 307, 327, 294, 295, 294, 305, 311, 318, 308, 321, 297},
        {474, 493, 526, 555, 557, 584, 576, 522, 514, 487, 503, 503, 517, 543, 514, 552, 510, 509, 515, 527, 530, 577, 569, 560, 491, 488, 503, 500, 507, 515, 506, 516, 480, 488, 480, 493, 495, 487, 502, 502, 480, 485, 480, 479, 484, 490, 505, 502, 474, 482, 492, 493, 491, 501, 511, 520, 464, 458, 467, 486, 475, 460, 493, 484},
        {-80, -95, -67, -118, -74, -4, 40, 103, -103, -41, -98, -22, -48, -32, 45, 27, -106, 1, -84, -96, -70, 13, 8, -25, -64, -78, -88, -162, -140, -99, -86, -107, -62, -66, -113, -144, -144, -107, -99, -116, -28, -18, -65, -82, -77, -81, -35, -48, 57, 15, -2, -35, -40, -21, 29, 40, 47, 77, 49, -54, 11, -34, 58, 55}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 148, 124, 141, 119, 118, 117, 133, 148, 122, 122, 110, 109, 100, 97, 117, 119, 114, 103, 103, 90, 92, 92, 92, 95, 100, 95, 98, 99, 95, 94, 84, 83, 99, 90, 100, 103, 106, 102, 81, 82, 105, 97, 107, 103, 120, 106, 82, 86, 0, 0, 0, 0, 0, 0, 0, 0},
        {223, 272, 294, 277, 291, 267, 271, 200, 273, 286, 301, 301, 290, 277, 282, 253, 289, 300, 313, 316, 300, 302, 289, 275, 292, 315, 331, 332, 332, 325, 317, 288, 302, 312, 332, 331, 334, 324, 309, 285, 283, 300, 313, 327, 323, 306, 294, 283, 278, 293, 299, 299, 299, 295, 281, 282, 255, 250, 283, 284, 279, 269, 263, 270},
        {306, 318, 318, 329, 323, 313, 316, 301, 295, 313, 318, 319, 309, 314, 313, 295, 320, 317, 329, 317, 322, 324, 318, 314, 319, 336, 332, 344, 337, 332, 333, 317, 316, 334, 342, 340, 339, 337, 326, 306, 316, 324, 334, 336, 339, 334, 317, 307, 317, 309, 309, 319, 325, 310, 315, 291, 295, 311, 289, 307, 301, 309, 297, 272},
        {554, 561, 569, 566, 555, 556, 560, 543, 555, 563, 566, 555, 556, 545, 549, 534, 556, 554, 554, 548, 541, 537, 533, 529, 557, 553, 562, 554, 541, 541, 538, 532, 552, 554, 556, 550, 546, 547, 534, 533, 545, 543, 540, 545, 539, 534, 510, 514, 538, 541, 539, 539, 530, 529, 515, 522, 542, 544, 549, 546, 539, 541, 531, 530},
        {1014, 1017, 1030, 1023, 1023, 981, 960, 996, 996, 1034, 1055, 1065, 1076, 1046, 1028, 1011, 995, 1012, 1050, 1061, 1069, 1050, 1008, 1014, 1007, 1038, 1047, 1073, 1086, 1071, 1067, 1040, 1023, 1033, 1054, 1071, 1068, 1064, 1040, 1028, 988, 1026, 1045, 1047, 1044, 1040, 1008, 991, 994, 998, 1004, 1008, 1014, 979, 942, 910, 983, 996, 999, 1008, 995, 977, 938, 937},
        {-72, -34, -22, 6, -10, -4, -2, -89, -16, 13, 24, 12, 29, 34, 28, 4, -5, 16, 37, 49, 51, 45, 39, 8, -16, 19, 41, 59, 60, 54, 41, 14, -23, 6, 36, 54, 55, 41, 25, 7, -31, -5, 15, 30, 30, 23, 5, -8, -36, -17, -7, 1, 8, 3, -13, -33, -70, -59, -40, -27, -47, -23, -46, -73}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 70, 82, 69, 82, 70, 64, 38, 15, 30, 32, 20, 13, 11, -3, -39, -42, 17, 5, 17, 20, 0, 8, -31, -19, -2, -13, -23, -12, -22, -7, -25, -15, -7, -23, -27, -21, -26, -15, -26, 2, -14, -13, -19, -21, -11, -11, -2, -11, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 148, 124, 141, 119, 118, 117, 133, 148, 142, 142, 108, 79, 81, 95, 114, 135, 87, 83, 51, 45, 43, 50, 81, 84, 62, 54, 35, 24, 28, 36, 61, 61, 24, 34, 20, 11, 12, 13, 42, 23, 25, 26, 14, 7, -7, 7, 21, 22, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {33, 12, -8, -25, -45, -72, -88, -86, 49};
    public static int doubledPawnPenalty = -29;
    public static int bishopPairBonusMG = 42;
    public static int bishopPairBonusEG = 56;
    public static int rookOpenFileMG = 22;
    public static int rookOpenFileEG = 20;

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
