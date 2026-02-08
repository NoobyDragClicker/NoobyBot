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
        {0, 0, 0, 0, 0, 0, 0, 0, 68, 80, 69, 82, 70, 61, 30, 4, 62, 75, 104, 103, 110, 154, 134, 89, 43, 69, 70, 70, 94, 86, 83, 62, 29, 60, 56, 75, 72, 63, 65, 45, 29, 54, 53, 54, 68, 56, 83, 54, 29, 55, 46, 40, 53, 73, 80, 44, 0, 0, 0, 0, 0, 0, 0, 0},
        {141, 160, 205, 242, 252, 210, 170, 183, 230, 255, 273, 291, 274, 334, 248, 276, 237, 278, 308, 314, 346, 343, 307, 270, 238, 254, 281, 308, 290, 314, 264, 274, 222, 238, 258, 262, 271, 263, 261, 236, 202, 229, 245, 246, 259, 251, 252, 222, 185, 198, 218, 234, 234, 236, 218, 221, 142, 200, 177, 199, 207, 219, 205, 165},
        {240, 216, 209, 177, 198, 204, 205, 208, 243, 266, 254, 250, 273, 264, 263, 232, 247, 272, 275, 295, 277, 311, 280, 271, 238, 249, 273, 286, 283, 279, 247, 239, 223, 241, 248, 270, 267, 249, 245, 232, 235, 246, 244, 250, 250, 244, 247, 251, 238, 241, 254, 230, 238, 252, 261, 242, 207, 238, 220, 209, 214, 216, 233, 224},
        {339, 322, 334, 334, 358, 350, 335, 374, 334, 335, 355, 374, 361, 392, 373, 398, 315, 339, 345, 350, 372, 379, 403, 380, 300, 315, 319, 328, 337, 341, 340, 345, 280, 288, 292, 307, 311, 298, 317, 312, 276, 283, 293, 292, 300, 300, 335, 315, 272, 285, 301, 299, 303, 309, 324, 295, 297, 296, 307, 313, 319, 310, 319, 302},
        {475, 493, 526, 554, 556, 584, 580, 526, 515, 489, 505, 504, 519, 545, 526, 559, 510, 510, 516, 528, 531, 576, 569, 557, 491, 489, 505, 500, 508, 517, 508, 518, 481, 488, 481, 494, 495, 489, 503, 504, 482, 486, 481, 480, 485, 492, 507, 503, 475, 484, 494, 494, 490, 503, 510, 520, 465, 459, 468, 487, 476, 461, 491, 485},
        {-76, -90, -60, -113, -68, -4, 40, 106, -100, -35, -93, -16, -44, -29, 48, 31, -104, 4, -79, -90, -65, 16, 10, -23, -63, -74, -84, -156, -135, -97, -84, -106, -60, -63, -108, -138, -138, -102, -96, -113, -27, -17, -62, -80, -73, -79, -32, -46, 53, 16, -2, -32, -39, -17, 21, 35, 41, 69, 46, -48, 20, -36, 56, 53}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 149, 125, 141, 119, 118, 118, 134, 150, 123, 122, 110, 109, 100, 96, 119, 120, 114, 103, 103, 89, 92, 91, 92, 95, 101, 96, 97, 98, 96, 94, 84, 83, 99, 90, 99, 103, 106, 101, 80, 82, 105, 98, 107, 103, 121, 105, 86, 87, 0, 0, 0, 0, 0, 0, 0, 0},
        {223, 271, 293, 277, 291, 266, 270, 200, 274, 285, 301, 300, 289, 277, 282, 253, 289, 300, 313, 315, 300, 300, 288, 274, 291, 315, 330, 331, 332, 324, 316, 288, 302, 312, 332, 331, 334, 323, 307, 284, 283, 299, 313, 326, 322, 305, 293, 283, 278, 293, 299, 299, 298, 294, 280, 280, 255, 250, 282, 284, 278, 269, 263, 270},
        {306, 317, 317, 329, 323, 313, 316, 300, 295, 312, 318, 318, 309, 314, 313, 299, 320, 317, 328, 317, 322, 324, 319, 315, 319, 335, 332, 344, 336, 331, 332, 316, 316, 334, 342, 339, 339, 336, 325, 305, 316, 323, 334, 336, 339, 333, 316, 306, 317, 309, 309, 319, 325, 310, 312, 291, 295, 311, 289, 307, 300, 308, 297, 270},
        {554, 561, 569, 566, 555, 556, 559, 544, 555, 563, 565, 554, 555, 545, 548, 534, 556, 554, 554, 547, 540, 537, 533, 529, 557, 553, 561, 553, 540, 541, 539, 533, 552, 553, 555, 550, 546, 546, 534, 533, 545, 543, 540, 545, 539, 534, 511, 514, 538, 541, 538, 538, 530, 528, 515, 522, 542, 544, 549, 546, 540, 540, 532, 530},
        {1014, 1018, 1031, 1024, 1024, 983, 960, 995, 997, 1035, 1056, 1066, 1077, 1046, 1023, 1009, 996, 1013, 1050, 1061, 1069, 1051, 1008, 1018, 1008, 1039, 1048, 1074, 1086, 1071, 1068, 1039, 1024, 1034, 1054, 1072, 1069, 1064, 1042, 1029, 989, 1027, 1046, 1048, 1045, 1041, 1009, 993, 995, 999, 1005, 1010, 1017, 980, 944, 911, 984, 997, 1000, 1009, 996, 979, 941, 937},
        {-73, -35, -23, 6, -11, -4, -3, -90, -17, 12, 24, 11, 28, 34, 27, 4, -5, 16, 37, 48, 50, 44, 39, 8, -16, 18, 41, 59, 59, 54, 41, 14, -23, 6, 36, 53, 54, 41, 25, 7, -31, -5, 15, 30, 30, 24, 5, -8, -35, -17, -6, 1, 8, 2, -10, -31, -69, -58, -40, -28, -49, -22, -46, -74}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 68, 80, 69, 82, 70, 61, 30, 4, 30, 32, 21, 13, 12, -4, -38, -52, 16, 5, 18, 19, 1, 7, -29, -19, -2, -14, -23, -12, -22, -9, -23, -14, -7, -23, -27, -21, -26, -16, -25, 1, -14, -13, -18, -21, -12, -12, 2, -10, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 149, 125, 141, 119, 118, 118, 134, 150, 141, 142, 108, 79, 81, 96, 114, 136, 87, 83, 51, 45, 43, 51, 82, 84, 61, 54, 35, 24, 28, 36, 62, 61, 23, 33, 20, 11, 12, 13, 43, 23, 25, 25, 13, 6, -7, 7, 20, 22, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {33, 12, -8, -24, -43, -69, -85, -83, 49};
    public static int doubledPawnPenalty = -27;
    public static int bishopPairBonusMG = 43;
    public static int bishopPairBonusEG = 55;
    public static int rookOpenFileMG = 22;
    public static int rookOpenFileEG = 20;
    public static int kingOpenFileMG = -41;
    public static int kingOpenFileEG = 4;
    public static int kingPawnShieldMG = 19;
    public static int kingPawnShieldEG = -8;

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
        (int, int) wKingBonus = EvaluateKingSafety(board, whiteKingIndex, Piece.White);
        mgScore += wKingBonus.Item1;
        egScore += wKingBonus.Item2;

        (int, int) bKingBonus = EvaluateKingSafety(board, blackKingIndex, Piece.Black);
        mgScore -= bKingBonus.Item1;
        egScore -= bKingBonus.Item2;

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


    (int, int) EvaluateKingSafety(Board board, int kingIndex, int kingColor)
    {
        int mgBonus = 0;
        int egBonus = 0;

        int currentColorIndex = kingColor == Piece.White ? Board.WhiteIndex : Board.BlackIndex;
        if(((board.sideBitboard[currentColorIndex] ^ 1ul << kingIndex) & BitboardHelper.files[kingIndex % 8]) == 0)
        {
            mgBonus += kingOpenFileMG;
            egBonus += kingOpenFileEG;
        }
        
        int direction = kingColor == Piece.White ? -1 : 1;
        int frontSquare = kingIndex + (direction * 8);

        if(frontSquare >= 0 && frontSquare <= 63)
        {
            if(BitboardHelper.ContainsSquare(board.pieceBitboards[Board.PieceBitboardIndex(currentColorIndex, Piece.Pawn)], frontSquare))
            {
                mgBonus += kingPawnShieldMG;
                egBonus += kingPawnShieldEG;
            }
        }

        return (mgBonus, egBonus);
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
