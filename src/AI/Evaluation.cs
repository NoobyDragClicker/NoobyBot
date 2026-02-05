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
        {0, 0, 0, 0, 0, 0, 0, 0, 55, 70, 61, 71, 62, 53, 36, 14, 52, 71, 98, 92, 98, 137, 125, 76, 39, 62, 64, 64, 88, 77, 79, 60, 23, 55, 52, 70, 68, 57, 62, 43, 24, 48, 49, 47, 65, 49, 77, 52, 25, 51, 44, 36, 54, 66, 85, 43, 0, 0, 0, 0, 0, 0, 0, 0},
        {141, 78, 130, 150, 138, 135, 66, 151, 179, 211, 229, 230, 218, 274, 178, 195, 176, 234, 266, 273, 296, 280, 259, 210, 201, 221, 245, 273, 258, 276, 232, 232, 192, 197, 226, 231, 240, 229, 225, 204, 172, 198, 215, 211, 223, 220, 221, 190, 156, 163, 184, 203, 205, 200, 184, 187, 99, 172, 142, 162, 175, 186, 176, 116},
        {203, 136, 126, 139, 129, 149, 100, 158, 196, 218, 206, 190, 222, 214, 213, 197, 203, 223, 229, 245, 227, 263, 232, 231, 195, 207, 227, 241, 242, 237, 208, 201, 181, 200, 209, 227, 225, 209, 205, 192, 197, 205, 204, 212, 212, 204, 209, 211, 198, 202, 214, 189, 200, 212, 219, 200, 162, 197, 183, 165, 170, 178, 177, 174},
        {255, 216, 233, 232, 249, 197, 179, 243, 279, 283, 292, 306, 297, 314, 281, 303, 257, 269, 285, 287, 307, 304, 289, 297, 241, 250, 261, 269, 281, 277, 266, 279, 224, 228, 233, 250, 256, 243, 253, 255, 224, 227, 236, 234, 243, 245, 279, 259, 222, 231, 245, 240, 247, 254, 271, 242, 247, 243, 258, 262, 268, 258, 270, 248},
        {412, 385, 405, 424, 419, 395, 408, 434, 477, 456, 469, 460, 477, 501, 486, 515, 479, 471, 484, 491, 493, 530, 530, 527, 464, 462, 472, 474, 478, 485, 477, 493, 452, 465, 453, 469, 468, 462, 476, 479, 461, 458, 453, 451, 456, 461, 480, 483, 453, 456, 462, 462, 462, 474, 488, 493, 440, 434, 441, 456, 448, 434, 457, 433},
        {-15, -17, -13, -14, -10, -1, 6, -1, -14, -1, -16, -3, -6, -2, 15, 9, -15, 13, -16, -24, -16, 6, 8, -3, -11, -15, -24, -58, -54, -33, -25, -29, -17, -20, -50, -75, -81, -57, -49, -51, -1, 3, -28, -46, -36, -39, 0, -10, 70, 40, 24, -3, -8, 12, 61, 68, 65, 102, 78, -22, 44, -5, 89, 78}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 120, 101, 111, 95, 95, 89, 103, 114, 109, 114, 97, 91, 84, 86, 115, 108, 105, 100, 90, 77, 78, 81, 92, 91, 93, 98, 87, 84, 83, 85, 86, 74, 94, 94, 87, 89, 92, 88, 86, 75, 103, 102, 96, 90, 109, 95, 89, 83, 0, 0, 0, 0, 0, 0, 0, 0},
        {136, 175, 206, 198, 211, 175, 156, 118, 190, 196, 213, 217, 204, 192, 194, 167, 206, 213, 222, 225, 209, 217, 196, 187, 193, 220, 239, 236, 237, 231, 221, 195, 209, 223, 237, 237, 238, 229, 211, 190, 192, 206, 223, 235, 228, 212, 201, 186, 179, 194, 204, 208, 208, 200, 180, 186, 138, 167, 190, 191, 180, 171, 173, 148},
        {211, 232, 233, 234, 234, 221, 224, 202, 207, 223, 231, 234, 223, 225, 222, 201, 229, 231, 239, 229, 233, 235, 227, 222, 227, 246, 244, 254, 245, 241, 242, 223, 224, 244, 248, 248, 249, 246, 235, 213, 224, 232, 245, 247, 248, 243, 224, 216, 221, 223, 218, 229, 233, 217, 225, 193, 206, 215, 201, 215, 206, 218, 192, 171},
        {417, 429, 438, 436, 427, 428, 427, 412, 415, 422, 429, 424, 422, 410, 413, 400, 417, 419, 419, 416, 406, 401, 404, 393, 417, 413, 422, 417, 400, 399, 397, 392, 407, 410, 416, 411, 404, 398, 385, 382, 400, 397, 400, 406, 396, 385, 355, 363, 395, 400, 397, 400, 387, 380, 363, 373, 401, 401, 406, 406, 399, 395, 386, 394},
        {650, 661, 672, 661, 667, 639, 605, 625, 588, 620, 633, 638, 635, 613, 561, 561, 572, 590, 621, 631, 631, 625, 568, 570, 569, 602, 622, 643, 658, 639, 626, 601, 598, 591, 630, 642, 643, 630, 610, 581, 525, 613, 625, 637, 624, 628, 582, 521, 529, 568, 606, 603, 610, 553, 488, 414, 523, 547, 564, 618, 568, 531, 422, 402},
        {-53, -36, -25, -14, -19, -2, 12, -28, -28, 6, 6, 5, 17, 21, 28, 8, -16, 13, 22, 31, 37, 39, 36, 3, -20, 10, 29, 41, 44, 42, 30, 3, -25, -1, 24, 40, 42, 32, 19, 1, -29, -5, 9, 23, 24, 16, 3, -7, -28, -16, -9, -3, 5, 2, -12, -30, -54, -53, -34, -24, -40, -18, -35, -60}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 55, 70, 61, 71, 62, 53, 36, 14, 28, 29, 22, 18, 18, 1, -17, -30, 13, 7, 18, 18, -1, 4, -22, -18, -1, -10, -21, -11, -20, -6, -18, -16, -7, -20, -23, -19, -25, -13, -20, -1, -11, -10, -15, -18, -8, -12, -2, -8, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 120, 101, 111, 95, 95, 89, 103, 114, 107, 100, 79, 62, 63, 72, 75, 94, 58, 54, 35, 31, 32, 37, 54, 56, 39, 32, 22, 17, 20, 24, 37, 44, 5, 16, 13, 5, 7, 7, 27, 10, 7, 8, 7, -2, -14, 2, 11, 8, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {30, 24, 17, 3, -15, -40, -34, -2, 0};
    public static int doubledPawnPenalty = -41;
    public static int bishopPairBonusMG = 54;
    public static int bishopPairBonusEG = 51;

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
