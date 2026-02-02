using System;
using System.Linq.Expressions;
using System.Net.Security;

public class Evaluation
{

    int colorTurn;

    int numWhiteIsolatedPawns;
    int numBlackIsolatedPawns;
    SearchLogger logger;

    //Unused in actual eval
    public static int pawnValue = 90;
    public static int knightValue = 336;
    public static int bishopValue = 366;
    public static int rookValue = 538;
    public static int queenValue = 1024;
    
    public static int[,] mg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 60, 72, 63, 70, 64, 54, 37, 16, 52, 69, 98, 93, 99, 140, 127, 82, 39, 62, 66, 64, 88, 77, 80, 61, 23, 54, 53, 70, 67, 60, 62, 41, 24, 51, 49, 47, 64, 51, 78, 51, 25, 50, 46, 36, 53, 70, 85, 45, 0, 0, 0, 0, 0, 0, 0, 0},
        {138, 86, 140, 156, 152, 140, 77, 153, 184, 217, 235, 236, 228, 280, 186, 212, 182, 242, 272, 279, 302, 288, 269, 219, 209, 230, 250, 279, 268, 283, 236, 241, 200, 208, 230, 237, 245, 234, 235, 209, 180, 202, 218, 219, 232, 223, 226, 195, 163, 168, 196, 209, 209, 210, 185, 195, 107, 182, 153, 169, 181, 195, 183, 127},
        {204, 150, 140, 136, 136, 153, 115, 166, 204, 232, 211, 196, 225, 222, 221, 202, 211, 233, 233, 256, 235, 273, 242, 238, 201, 212, 238, 250, 249, 243, 216, 208, 190, 209, 220, 236, 236, 218, 216, 201, 208, 213, 212, 218, 218, 212, 219, 218, 211, 209, 220, 199, 209, 217, 222, 211, 174, 205, 193, 171, 177, 183, 187, 183},
        {275, 240, 253, 252, 270, 220, 201, 270, 293, 295, 311, 317, 313, 330, 301, 322, 268, 285, 298, 303, 318, 320, 314, 314, 256, 260, 276, 280, 294, 294, 282, 295, 240, 239, 248, 262, 269, 259, 268, 271, 240, 238, 250, 246, 255, 259, 291, 272, 236, 240, 255, 252, 256, 267, 285, 253, 259, 255, 272, 272, 281, 268, 280, 265},
        {435, 414, 431, 454, 452, 434, 446, 462, 490, 469, 482, 472, 492, 514, 497, 525, 489, 484, 497, 503, 506, 543, 542, 543, 479, 475, 483, 485, 493, 498, 488, 506, 466, 475, 465, 484, 479, 478, 488, 491, 473, 475, 466, 467, 470, 475, 491, 495, 466, 468, 477, 476, 476, 487, 501, 501, 453, 445, 455, 468, 462, 448, 472, 455},
        {-16, -21, -16, -20, -13, -2, 9, 1, -17, -4, -21, -4, -8, -2, 20, 14, -23, 15, -22, -28, -18, 11, 13, -1, -15, -17, -24, -70, -64, -35, -28, -31, -17, -19, -50, -76, -82, -52, -45, -56, 5, 10, -24, -41, -37, -37, 3, -7, 71, 43, 28, 3, -1, 15, 67, 71, 70, 104, 78, -18, 44, 3, 93, 82}
    };

    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 126, 103, 115, 98, 97, 94, 106, 118, 105, 103, 95, 89, 87, 79, 107, 103, 98, 86, 88, 72, 75, 79, 80, 81, 87, 80, 82, 80, 80, 78, 71, 69, 86, 79, 85, 86, 87, 82, 71, 72, 93, 86, 89, 89, 102, 89, 74, 74, 0, 0, 0, 0, 0, 0, 0, 0},
        {152, 187, 217, 208, 219, 186, 173, 130, 197, 207, 221, 229, 214, 202, 200, 179, 212, 220, 232, 235, 218, 222, 206, 194, 206, 231, 249, 244, 245, 240, 231, 206, 219, 236, 248, 249, 248, 240, 219, 198, 202, 215, 231, 244, 243, 220, 211, 196, 192, 208, 217, 214, 215, 209, 192, 196, 151, 176, 199, 205, 191, 182, 185, 165},
        {223, 237, 245, 247, 242, 228, 236, 210, 212, 232, 238, 241, 232, 230, 232, 208, 241, 239, 254, 240, 240, 244, 236, 232, 235, 256, 253, 263, 255, 250, 252, 234, 230, 256, 259, 257, 259, 253, 242, 221, 235, 239, 253, 254, 256, 251, 231, 228, 233, 229, 227, 241, 245, 223, 236, 201, 213, 225, 206, 224, 216, 227, 201, 182},
        {435, 444, 453, 450, 439, 437, 441, 426, 431, 439, 444, 440, 434, 423, 425, 415, 433, 434, 435, 432, 422, 415, 416, 408, 433, 426, 434, 432, 414, 414, 412, 403, 424, 424, 431, 430, 421, 414, 399, 398, 415, 410, 413, 424, 413, 404, 373, 381, 411, 412, 412, 413, 401, 396, 383, 390, 411, 416, 423, 422, 416, 412, 408, 410},
        {682, 692, 705, 696, 701, 665, 630, 656, 632, 665, 677, 683, 684, 661, 614, 615, 619, 634, 666, 677, 679, 670, 615, 620, 619, 652, 667, 689, 704, 683, 671, 649, 643, 641, 671, 685, 689, 675, 653, 628, 580, 657, 670, 682, 666, 670, 627, 578, 585, 613, 647, 644, 651, 596, 545, 469, 574, 596, 610, 657, 613, 576, 483, 458},
        {-55, -34, -23, -13, -19, -2, 14, -36, -25, 7, 10, 4, 16, 24, 28, 10, -16, 14, 25, 31, 43, 38, 38, 8, -25, 10, 32, 43, 46, 43, 31, 5, -27, -1, 24, 41, 45, 32, 19, 3, -31, -9, 8, 23, 25, 20, 2, -10, -28, -13, -8, -2, 5, 3, -11, -34, -59, -55, -37, -25, -44, -18, -37, -61}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 60, 72, 63, 70, 64, 54, 37, 16, 25, 31, 20, 15, 16, 2, -22, -31, 17, 6, 14, 19, 2, 7, -23, -17, -1, -14, -20, -11, -23, -11, -17, -15, -8, -15, -27, -21, -26, -17, -21, -5, -12, -9, -16, -13, -9, -12, 2, -10, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 126, 103, 115, 98, 97, 94, 106, 118, 118, 112, 89, 66, 66, 78, 86, 107, 70, 69, 41, 38, 37, 44, 66, 65, 53, 48, 26, 21, 25, 24, 49, 56, 16, 30, 15, 9, 10, 9, 38, 18, 21, 20, 10, 3, -8, 7, 22, 17, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {42, 22, 7, -3, -17, -41, -47, -6, 0};
    public static int doubledPawnPenalty = -27;
    public static int bishopPairBonusMG = 50;
    public static int bishopPairBonusEG = 51;

    int playerTurnMultiplier;
    public Evaluation(SearchLogger logger)
    {
        this.logger = logger;
    }
    public int EvaluatePosition(Board board)
    {
        colorTurn = board.colorTurn;
        playerTurnMultiplier = (colorTurn == Piece.White) ? 1 : -1;

        numWhiteIsolatedPawns = 0;
        numBlackIsolatedPawns = 0;
        int boardVal = IncrementalCount(board);
        return boardVal;
    }

    int IncrementalCount(Board board)
    {
        const int totalPhase = 24;
        int mgScore = board.gameStateHistory[board.fullMoveClock].mgPSQTVal;
        int egScore = board.gameStateHistory[board.fullMoveClock].egPSQTVal;

        int pawnMGEval = 0;
        int pawnEGEval = 0;

        ulong whitePawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)];
        ulong blackPawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)];

        while (whitePawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref whitePawns);
            (int, int) pawnBonus = EvaluatePawnStrength(board, index, Piece.White);
            mgScore += pawnBonus.Item1;
            egScore += pawnBonus.Item2;
        }

        while (blackPawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackPawns);
            (int, int) pawnBonus = EvaluatePawnStrength(board, index, Piece.Black);
            mgScore -= pawnBonus.Item1;
            egScore -= pawnBonus.Item2;
        }

        egScore += isolatedPawnPenalty[numWhiteIsolatedPawns];
        egScore -= isolatedPawnPenalty[numBlackIsolatedPawns];


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

    (int, int) EvaluatePawnStrength(Board board, int pawnIndex, int pawnColor)
    {
        int mgBonus = 0;
        int egBonus = 0;
        if (pawnColor == Piece.White)
        {
            //Passed pawn
            if ((board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)] & BitboardHelper.wPawnPassedMask[pawnIndex]) == 0) { 
                mgBonus += passedPawnBonuses[0, pawnIndex]; 
                egBonus += passedPawnBonuses[1, pawnIndex]; 
            }
            //Doubled pawn penalty
            if (board.PieceAt(pawnIndex - 8) == Piece.Pawn && board.ColorAt(pawnIndex - 8) == Piece.White) { egBonus += doubledPawnPenalty; }
            if ((BitboardHelper.isolatedPawnMask[pawnIndex] & board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)]) == 0) { numWhiteIsolatedPawns++; }
        }
        else
        {
            //Passed pawn
            if ((board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)] & BitboardHelper.bPawnPassedMask[pawnIndex]) == 0) { 
                mgBonus += passedPawnBonuses[0, pawnIndex ^ 56]; 
                egBonus += passedPawnBonuses[1, pawnIndex ^ 56]; 
            }
            //Doubled pawn penalty
            if (board.PieceAt(pawnIndex + 8) == Piece.Pawn && board.ColorAt(pawnIndex + 8) == Piece.Black) { egBonus += doubledPawnPenalty; }
            if((BitboardHelper.isolatedPawnMask[pawnIndex] & board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)]) == 0){ numBlackIsolatedPawns++; }
        }

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
