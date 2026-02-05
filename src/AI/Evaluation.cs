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
        {0, 0, 0, 0, 0, 0, 0, 0, 59, 72, 66, 73, 65, 54, 36, 15, 54, 70, 98, 93, 98, 138, 127, 77, 39, 64, 67, 64, 90, 79, 86, 62, 24, 56, 55, 69, 67, 58, 66, 44, 22, 50, 49, 49, 65, 52, 80, 52, 26, 50, 47, 39, 54, 69, 89, 44, 0, 0, 0, 0, 0, 0, 0, 0},
        {134, 85, 140, 159, 151, 144, 76, 151, 186, 217, 234, 234, 226, 278, 187, 206, 184, 239, 274, 282, 304, 286, 267, 219, 207, 226, 250, 281, 263, 283, 240, 245, 194, 201, 232, 234, 246, 235, 228, 210, 177, 206, 219, 218, 228, 225, 222, 193, 157, 166, 193, 208, 211, 208, 189, 193, 107, 176, 150, 169, 182, 190, 184, 127},
        {206, 148, 139, 140, 138, 157, 112, 163, 200, 226, 213, 193, 229, 225, 221, 203, 210, 233, 235, 258, 237, 269, 242, 244, 202, 214, 239, 247, 247, 245, 215, 211, 188, 209, 213, 238, 235, 220, 215, 198, 205, 214, 211, 218, 223, 210, 215, 222, 203, 209, 222, 195, 210, 218, 231, 211, 171, 208, 189, 173, 177, 186, 185, 187},
        {271, 235, 248, 248, 268, 219, 201, 268, 286, 294, 303, 321, 310, 334, 299, 322, 267, 282, 297, 299, 320, 319, 309, 313, 255, 264, 273, 277, 292, 290, 281, 291, 236, 238, 246, 262, 267, 257, 267, 266, 235, 241, 248, 245, 253, 257, 287, 275, 229, 245, 259, 248, 257, 263, 275, 252, 255, 251, 267, 272, 278, 269, 276, 260},
        {434, 413, 431, 450, 445, 431, 442, 464, 488, 471, 483, 475, 492, 516, 500, 521, 496, 484, 498, 507, 512, 543, 549, 542, 479, 478, 488, 488, 493, 497, 496, 508, 463, 479, 466, 481, 484, 476, 493, 495, 470, 474, 469, 463, 471, 479, 492, 493, 467, 471, 475, 480, 478, 491, 502, 508, 451, 446, 454, 474, 464, 450, 474, 456},
        {-17, -20, -14, -19, -13, -0, 8, 1, -16, -4, -18, -4, -7, -1, 20, 12, -19, 16, -20, -29, -18, 8, 10, -1, -15, -17, -26, -67, -61, -38, -29, -34, -12, -20, -52, -77, -81, -51, -49, -53, 3, 7, -27, -42, -33, -37, 6, -12, 76, 45, 27, -1, -0, 16, 61, 68, 71, 102, 80, -21, 45, 2, 89, 85}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 121, 103, 112, 99, 98, 91, 106, 117, 112, 118, 100, 90, 87, 89, 114, 112, 105, 103, 89, 79, 83, 86, 92, 90, 91, 97, 87, 84, 84, 84, 88, 79, 97, 95, 91, 96, 95, 92, 88, 77, 104, 102, 99, 89, 110, 99, 90, 85, 0, 0, 0, 0, 0, 0, 0, 0},
        {146, 184, 215, 207, 219, 184, 171, 125, 198, 204, 222, 223, 210, 200, 204, 174, 213, 220, 233, 235, 220, 226, 205, 194, 201, 232, 249, 246, 246, 239, 228, 205, 218, 231, 249, 245, 248, 238, 221, 203, 197, 216, 234, 241, 234, 224, 215, 197, 190, 206, 214, 217, 219, 208, 191, 195, 153, 172, 197, 200, 192, 182, 177, 164},
        {221, 240, 240, 245, 242, 226, 234, 211, 213, 236, 236, 244, 232, 234, 233, 207, 236, 235, 247, 240, 243, 241, 233, 230, 234, 254, 250, 263, 254, 247, 248, 235, 230, 253, 260, 260, 259, 255, 241, 220, 234, 239, 252, 253, 257, 248, 233, 224, 229, 231, 226, 236, 242, 224, 229, 202, 212, 226, 210, 224, 214, 224, 199, 180},
        {432, 440, 451, 448, 437, 439, 439, 422, 428, 435, 442, 440, 438, 426, 428, 410, 429, 436, 432, 429, 418, 414, 412, 407, 428, 427, 437, 431, 413, 409, 411, 406, 421, 425, 428, 424, 421, 414, 396, 392, 412, 410, 413, 422, 408, 401, 371, 379, 408, 412, 411, 410, 401, 397, 379, 389, 414, 416, 419, 422, 415, 410, 403, 406},
        {681, 688, 700, 691, 697, 666, 627, 650, 626, 656, 669, 675, 676, 652, 608, 606, 610, 630, 663, 671, 677, 665, 610, 613, 611, 646, 664, 684, 698, 679, 669, 640, 639, 636, 663, 681, 683, 672, 651, 624, 572, 650, 662, 674, 664, 669, 619, 572, 576, 608, 638, 644, 648, 593, 532, 466, 570, 593, 603, 655, 608, 578, 473, 450},
        {-53, -34, -26, -12, -18, 0, 14, -34, -26, 5, 7, 6, 18, 23, 28, 9, -16, 11, 20, 35, 41, 40, 35, 2, -19, 12, 31, 41, 44, 42, 29, 6, -23, -2, 27, 41, 45, 32, 17, 1, -27, -2, 9, 24, 23, 17, 4, -8, -29, -15, -8, -3, 6, 1, -14, -33, -56, -57, -37, -25, -38, -17, -33, -62}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 59, 72, 66, 73, 65, 54, 36, 15, 28, 31, 23, 16, 15, 1, -19, -36, 9, 6, 17, 15, 1, 7, -26, -15, -3, -13, -22, -13, -20, -9, -17, -15, -6, -20, -23, -19, -23, -15, -25, -5, -17, -11, -15, -12, -9, -10, 0, -7, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 121, 103, 112, 99, 98, 91, 106, 117, 106, 109, 80, 63, 68, 69, 78, 99, 60, 57, 36, 31, 36, 36, 56, 57, 40, 34, 21, 17, 18, 20, 38, 44, 4, 13, 13, 7, 10, 5, 26, 9, 7, 5, 8, -2, -13, 0, 11, 8, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {29, 22, 12, 1, -16, -44, -42, -3, 0};
    public static int doubledPawnPenalty = -42;
    public static int bishopPairBonusMG = 50;
    public static int bishopPairBonusEG = 54;

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
