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
        {0, 0, 0, 0, 0, 0, 0, 0, 61, 72, 61, 73, 64, 55, 35, 16, 51, 69, 96, 92, 98, 138, 128, 76, 38, 62, 67, 66, 90, 81, 85, 58, 22, 56, 51, 69, 65, 57, 62, 44, 24, 51, 50, 50, 66, 51, 81, 52, 26, 54, 45, 41, 54, 68, 91, 46, 0, 0, 0, 0, 0, 0, 0, 0},
        {133, 87, 137, 160, 153, 139, 77, 147, 180, 215, 231, 237, 225, 281, 188, 210, 186, 240, 271, 280, 303, 287, 267, 222, 210, 226, 251, 277, 262, 282, 239, 239, 196, 204, 232, 233, 245, 237, 232, 214, 180, 207, 222, 218, 233, 222, 227, 201, 160, 170, 190, 209, 209, 208, 192, 193, 107, 176, 147, 169, 177, 190, 181, 129},
        {201, 147, 139, 140, 140, 155, 114, 161, 203, 228, 214, 198, 228, 222, 224, 204, 211, 231, 240, 259, 237, 273, 240, 240, 205, 213, 239, 252, 246, 248, 220, 211, 187, 209, 217, 234, 234, 220, 213, 201, 205, 217, 213, 217, 218, 211, 214, 218, 206, 209, 224, 200, 209, 222, 231, 210, 170, 204, 187, 173, 179, 184, 189, 185},
        {272, 235, 254, 251, 272, 222, 202, 268, 289, 293, 305, 317, 312, 329, 298, 320, 272, 282, 297, 300, 322, 317, 312, 314, 255, 263, 271, 281, 295, 290, 281, 292, 234, 240, 247, 262, 265, 254, 271, 266, 237, 240, 244, 246, 256, 259, 291, 272, 233, 241, 257, 254, 259, 263, 284, 252, 257, 256, 266, 273, 279, 266, 278, 258},
        {429, 413, 432, 448, 453, 435, 447, 462, 486, 471, 481, 477, 492, 514, 498, 528, 490, 484, 495, 504, 506, 547, 542, 543, 476, 480, 484, 487, 492, 500, 489, 501, 464, 479, 469, 480, 479, 473, 488, 489, 468, 467, 465, 461, 467, 475, 492, 494, 463, 472, 475, 477, 475, 489, 502, 503, 455, 447, 452, 474, 458, 446, 471, 453},
        {-16, -19, -16, -20, -14, 1, 8, 1, -20, -2, -20, -5, -7, -0, 20, 15, -22, 16, -23, -29, -20, 9, 13, -1, -12, -20, -28, -69, -58, -36, -29, -33, -18, -19, -50, -74, -82, -56, -49, -54, 2, 7, -27, -47, -30, -35, 3, -5, 79, 41, 27, 2, -4, 16, 62, 70, 70, 108, 80, -19, 44, -1, 92, 84}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 125, 105, 116, 98, 96, 91, 107, 117, 104, 109, 98, 88, 85, 81, 105, 106, 98, 91, 86, 77, 76, 77, 78, 85, 83, 82, 85, 83, 78, 80, 69, 71, 84, 78, 80, 87, 92, 84, 68, 69, 93, 89, 95, 93, 105, 90, 73, 79, 0, 0, 0, 0, 0, 0, 0, 0},
        {149, 186, 214, 209, 221, 186, 175, 128, 197, 205, 223, 225, 213, 202, 205, 177, 210, 219, 230, 234, 220, 226, 209, 196, 204, 228, 252, 244, 248, 242, 232, 202, 217, 230, 248, 248, 248, 237, 220, 200, 200, 215, 232, 244, 235, 224, 211, 196, 189, 207, 214, 214, 217, 209, 193, 195, 150, 175, 200, 202, 192, 186, 182, 162},
        {222, 237, 238, 248, 238, 228, 234, 211, 215, 235, 241, 242, 233, 234, 230, 210, 237, 244, 247, 237, 243, 245, 234, 230, 237, 252, 252, 263, 257, 249, 250, 231, 232, 255, 260, 257, 258, 258, 245, 221, 231, 239, 253, 252, 259, 251, 231, 226, 233, 231, 226, 238, 242, 227, 234, 202, 212, 224, 210, 221, 216, 228, 201, 180},
        {432, 442, 450, 449, 438, 440, 439, 423, 432, 438, 442, 439, 435, 425, 423, 414, 433, 436, 435, 432, 420, 416, 415, 405, 432, 431, 437, 433, 417, 414, 413, 405, 424, 425, 430, 427, 419, 417, 403, 398, 414, 412, 416, 420, 409, 400, 378, 380, 409, 413, 413, 413, 410, 396, 382, 389, 414, 415, 424, 425, 413, 414, 402, 411},
        {683, 688, 701, 696, 701, 666, 632, 655, 634, 665, 675, 682, 685, 658, 611, 614, 617, 632, 665, 675, 678, 674, 618, 620, 617, 647, 667, 691, 703, 686, 670, 647, 641, 643, 674, 689, 688, 673, 655, 633, 579, 658, 667, 677, 666, 672, 624, 578, 578, 613, 646, 645, 655, 599, 539, 469, 578, 597, 609, 657, 609, 582, 482, 457},
        {-53, -35, -22, -15, -17, -1, 11, -35, -26, 5, 8, 6, 17, 24, 28, 12, -16, 13, 25, 35, 41, 42, 37, 4, -20, 13, 28, 44, 45, 44, 30, 2, -28, 2, 26, 42, 42, 35, 19, 1, -31, -9, 9, 24, 20, 22, 4, -6, -32, -20, -8, -2, 4, 2, -13, -32, -58, -53, -40, -25, -42, -21, -35, -63}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 61, 72, 61, 73, 64, 55, 35, 16, 29, 31, 18, 20, 14, 1, -19, -30, 14, 8, 15, 22, -1, 7, -24, -11, 1, -10, -21, -15, -25, -11, -19, -16, -5, -18, -22, -24, -28, -19, -24, 5, -12, -9, -12, -17, -10, -14, -1, -12, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 125, 105, 116, 98, 96, 91, 107, 117, 122, 117, 84, 66, 71, 73, 85, 105, 73, 72, 41, 35, 39, 42, 67, 68, 55, 52, 28, 21, 27, 31, 51, 52, 20, 32, 17, 10, 10, 14, 42, 16, 18, 22, 15, 1, -11, 5, 22, 20, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {41, 22, 8, -6, -19, -43, -42, -7, 0};
    public static int doubledPawnPenalty = -27;
    public static int bishopPairBonusMG = 48;
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
