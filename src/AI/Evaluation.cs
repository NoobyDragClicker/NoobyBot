using System;
using System.Drawing;
using System.Linq.Expressions;
using System.Net.Security;

public class Evaluation
{

    int colorTurn;

    SearchLogger logger;

    //Unused in actual eval
    public static int pawnValue = 90;
    public static int knightValue = 336;
    public static int bishopValue = 366;
    public static int rookValue = 538;
    public static int queenValue = 1024;
    
    public static EvalPair[,] PSQT = {
    {
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)
    },
    {
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (60, 134), (75, 111), (55, 133), (67, 113), (56, 115), (54, 116), (41, 125), (17, 136),
    (66, 121), (75, 125), (103, 109), (103, 109), (112, 102), (151, 102), (131, 123), (94, 119),
    (49, 113), (65, 105), (64, 105), (70, 92), (91, 95), (83, 95), (80, 96), (69, 95),
    (35, 100), (55, 99), (55, 97), (71, 98), (72, 97), (71, 92), (62, 87), (49, 82),
    (32, 98), (41, 93), (45, 100), (49, 104), (61, 106), (60, 100), (72, 81), (53, 81),
    (39, 105), (54, 101), (45, 108), (50, 112), (54, 124), (84, 105), (83, 88), (53, 85),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)
    },
    {
    (143, 234), (151, 280), (193, 303), (230, 285), (243, 300), (202, 275), (165, 279), (179, 211),
    (221, 282), (252, 293), (269, 309), (288, 309), (271, 299), (321, 288), (246, 289), (266, 262),
    (231, 298), (278, 308), (309, 321), (312, 322), (342, 309), (341, 309), (306, 297), (272, 281),
    (238, 298), (258, 322), (283, 338), (314, 337), (294, 338), (316, 333), (271, 321), (277, 294),
    (224, 309), (240, 320), (266, 338), (271, 338), (281, 341), (272, 330), (266, 315), (241, 290),
    (206, 289), (233, 307), (253, 318), (257, 335), (273, 329), (263, 309), (258, 300), (226, 291),
    (190, 287), (204, 301), (222, 309), (245, 307), (244, 307), (247, 302), (226, 289), (227, 289),
    (145, 263), (205, 278), (189, 294), (213, 297), (217, 294), (229, 281), (213, 289), (172, 278)
    },
    {
    (200, 236), (161, 244), (150, 240), (120, 251), (149, 246), (154, 231), (153, 238), (171, 229),
    (191, 226), (199, 217), (182, 220), (180, 220), (204, 207), (189, 214), (199, 216), (181, 228),
    (194, 242), (207, 221), (203, 205), (214, 194), (199, 199), (242, 200), (216, 220), (223, 237),
    (190, 238), (189, 224), (196, 207), (214, 199), (208, 191), (200, 205), (192, 219), (194, 236),
    (186, 233), (180, 225), (179, 211), (203, 198), (198, 195), (180, 205), (182, 218), (200, 226),
    (192, 237), (196, 223), (192, 215), (184, 214), (189, 218), (194, 212), (199, 218), (210, 229),
    (199, 249), (202, 219), (202, 211), (182, 222), (191, 222), (203, 213), (226, 219), (203, 225),
    (192, 231), (208, 239), (192, 243), (179, 233), (188, 230), (189, 247), (200, 227), (209, 208)
    },
    {
    (308, 449), (285, 454), (283, 463), (275, 457), (296, 449), (289, 456), (290, 459), (333, 444),
    (302, 456), (302, 463), (312, 467), (327, 455), (316, 456), (337, 447), (331, 449), (353, 438),
    (287, 458), (308, 454), (304, 454), (301, 449), (322, 441), (330, 437), (358, 436), (336, 437),
    (287, 459), (298, 450), (294, 456), (297, 448), (305, 435), (310, 437), (309, 442), (316, 442),
    (277, 452), (276, 449), (277, 449), (285, 443), (295, 438), (283, 438), (299, 433), (295, 438),
    (278, 446), (274, 441), (281, 437), (282, 440), (296, 431), (299, 422), (323, 408), (309, 418),
    (280, 441), (282, 438), (291, 437), (292, 437), (299, 428), (311, 420), (319, 411), (296, 425),
    (304, 453), (300, 439), (303, 443), (312, 436), (319, 429), (318, 442), (317, 424), (306, 440)
    },
    {
    (474, 1041), (489, 1047), (519, 1062), (547, 1054), (548, 1056), (578, 1012), (579, 987), (521, 1024),
    (510, 1025), (489, 1061), (504, 1083), (501, 1094), (516, 1104), (537, 1074), (525, 1050), (556, 1035),
    (509, 1022), (511, 1040), (518, 1077), (525, 1092), (533, 1098), (578, 1076), (575, 1028), (562, 1039),
    (493, 1033), (495, 1061), (509, 1073), (505, 1101), (512, 1112), (520, 1096), (519, 1089), (523, 1061),
    (486, 1045), (493, 1058), (490, 1077), (501, 1097), (504, 1093), (502, 1084), (510, 1066), (511, 1051),
    (485, 1014), (492, 1047), (493, 1067), (495, 1067), (499, 1065), (502, 1063), (514, 1033), (510, 1015),
    (481, 1017), (495, 1019), (503, 1026), (507, 1030), (502, 1040), (515, 1002), (522, 965), (526, 938),
    (470, 1006), (471, 1021), (481, 1031), (495, 1055), (488, 1027), (475, 1002), (500, 966), (493, 960)
    },
    {
    (-82, -63), (-86, -24), (-55, -13), (-103, 10), (-69, -3), (-10, 5), (42, 2), (104, -82),
    (-95, -9), (-28, 15), (-79, 24), (-1, 10), (-35, 25), (-34, 35), (47, 29), (28, 12),
    (-93, -4), (22, 12), (-67, 30), (-79, 40), (-57, 43), (11, 38), (8, 34), (-27, 6),
    (-44, -22), (-55, 7), (-74, 28), (-151, 46), (-135, 47), (-94, 43), (-76, 30), (-104, 10),
    (-50, -29), (-50, -5), (-99, 22), (-140, 41), (-137, 43), (-91, 28), (-82, 14), (-112, 3),
    (-24, -30), (-7, -11), (-51, 7), (-73, 22), (-67, 23), (-67, 17), (-27, 2), (-48, -5),
    (51, -29), (22, -15), (3, -5), (-27, 2), (-34, 9), (-11, 6), (21, -5), (30, -23),
    (29, -56), (67, -48), (44, -26), (-40, -13), (19, -25), (-31, -7), (49, -35), (42, -65)
    }
    };
    public static EvalPair[] passedPawnBonuses = {(0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (60, 134), (75, 111), (55, 133), (67, 113), (56, 115), (54, 116), (41, 125), (17, 136), (13, 107), (25, 109), (9, 87), (-3, 60), (-8, 64), (-15, 75), (-41, 86), (-57, 107), (5, 45), (5, 42), (21, 11), (12, 10), (-4, 7), (5, 14), (-27, 42), (-26, 44), (-7, 12), (-12, 2), (-18, -15), (-3, -28), (-13, -27), (-11, -15), (-23, 8), (-12, 8), (-8, -32), (-14, -26), (-12, -42), (0, -57), (-4, -60), (-10, -55), (-16, -25), (9, -43), (-14, -34), (-3, -44), (2, -60), (3, -75), (17, -88), (1, -75), (14, -62), (2, -54), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)};
    public static EvalPair[] isolatedPawnPenalty = {(0, 25), (0, 9), (0, -6), (0, -18), (0, -33), (0, -55), (0, -70), (0, -75), (0, 49)};
    public static EvalPair doubledPawnPenalty = (0, -31);
    public static EvalPair protectedPawn = (14, 6);
    public static EvalPair[] friendlyKingDistPasser = {(0, 0), (-12, 55), (-16, 42), (-12, 23), (-4, 12), (3, 8), (22, 4), (10, 5)};
    public static EvalPair[] enemyKingDistPasser = {(0, 0), (-60, -29), (22, 2), (8, 26), (5, 36), (-3, 42), (-7, 45), (-32, 49)};
    public static EvalPair bishopPairBonus = (40, 51);
    public static EvalPair bishopMobility = (9, 14);
    public static EvalPair rookOpenFile = (40, -7);
    public static EvalPair rookSemiOpenFile = (11, 13);
    public static EvalPair rookMobility = (0, 12);
    public static EvalPair rookKingRingAttack = (13, -4);
    public static EvalPair kingOpenFile = (-40, 4);
    public static EvalPair kingPawnShield = (19, -10);
    


    int playerTurnMultiplier;
    public Evaluation(SearchLogger logger)
    {
        this.logger = logger;
    }
    public int EvaluatePosition(Board board)
    {
        colorTurn = board.colorTurn;
        playerTurnMultiplier = (colorTurn == Piece.White) ? 1 : -1;
        int boardVal = IncrementalCount(board);
        return boardVal;
    }

    int IncrementalCount(Board board)
    {
        if (IsTrivialDraw(board)){ return 0; }
        const int totalPhase = 24;
        EvalPair eval = board.gameStateHistory[board.fullMoveClock].PSQTVal;

        Bitboard whiteBishops = board.GetPieces(Board.WhiteIndex, Piece.Bishop);
        Bitboard blackBishops = board.GetPieces(Board.BlackIndex, Piece.Bishop);

        Bitboard whiteRooks = board.GetPieces(Board.WhiteIndex, Piece.Rook);
        Bitboard blackRooks = board.GetPieces(Board.BlackIndex, Piece.Rook);

        while (!whiteRooks.Empty())
        {
            int index = whiteRooks.PopLSB();
            eval += EvaluateRookMobility(board, index, Board.WhiteIndex);
        }

        while (!blackRooks.Empty())
        {
            int index = blackRooks.PopLSB();
            eval -= EvaluateRookMobility(board, index, Board.BlackIndex);
        }

        while (!whiteBishops.Empty())
        {
            int index = whiteBishops.PopLSB();
            eval += EvaluateBishopMobility(board, index);
        }
        while (!blackBishops.Empty())
        {
            int index = blackBishops.PopLSB();
            eval -= EvaluateBishopMobility(board, index);
        }

        int whiteKingIndex = board.GetPieces(Board.WhiteIndex, Piece.King).GetLSB();
        int blackKingIndex = board.GetPieces(Board.BlackIndex, Piece.King).GetLSB();
        eval += EvaluateKingSafety(board, whiteKingIndex, Piece.White);
        eval -= EvaluateKingSafety(board, blackKingIndex, Piece.Black);

        eval += EvaluatePawnStructure(board, Board.WhiteIndex);
        eval -= EvaluatePawnStructure(board, Board.BlackIndex);


        if(board.pieceCounts[Board.WhiteIndex, Piece.Bishop] >= 2){ eval += bishopPairBonus; }
        if(board.pieceCounts[Board.BlackIndex, Piece.Bishop] >= 2){ eval -= bishopPairBonus; }

        int phase = (4 * (board.pieceCounts[Board.WhiteIndex, Piece.Queen] + board.pieceCounts[Board.BlackIndex, Piece.Queen])) + (2 * (board.pieceCounts[Board.WhiteIndex, Piece.Rook] + board.pieceCounts[Board.BlackIndex, Piece.Rook]));
        phase += board.pieceCounts[Board.WhiteIndex, Piece.Knight] + board.pieceCounts[Board.BlackIndex, Piece.Knight] + board.pieceCounts[Board.WhiteIndex, Piece.Bishop] + board.pieceCounts[Board.BlackIndex, Piece.Bishop];

        
        if (phase > 24) { phase = 24; }
        return (eval.mg * phase + eval.eg * (totalPhase - phase)) / totalPhase * playerTurnMultiplier;
    }

    bool IsTrivialDraw(Board board)
    {
        if (board.pieceCounts[Board.WhiteIndex, Piece.Pawn] + board.pieceCounts[Board.BlackIndex, Piece.Pawn] != 0) { return false; }
        if (board.pieceCounts[Board.WhiteIndex, Piece.Rook] + board.pieceCounts[Board.BlackIndex, Piece.Rook] + board.pieceCounts[Board.WhiteIndex, Piece.Queen] + board.pieceCounts[Board.BlackIndex, Piece.Queen] != 0) { return false; }

        int wBishops = board.pieceCounts[Board.WhiteIndex, Piece.Bishop];
        int wKnights = board.pieceCounts[Board.WhiteIndex, Piece.Knight];
        int wTotal = wBishops + wKnights;
        int bBishops = board.pieceCounts[Board.BlackIndex, Piece.Bishop];
        int bKnights = board.pieceCounts[Board.BlackIndex, Piece.Knight];
        int bTotal = bBishops + bKnights;

        //KB vs KB, KN vs KN, KB vs KN
        if(wTotal <= 1 && bTotal <= 1){ return true; }
        //KNN vs K
        else if ((bTotal == 0 && wKnights == 2 && wTotal == 2) || (wTotal == 0 && bKnights == 2 && bTotal == 2)) { return true; }
        return false;
    }

    EvalPair EvaluateKingSafety(Board board, int kingIndex, int kingColor)
    {
        EvalPair score = new EvalPair();

        int currentColorIndex = kingColor == Piece.White ? Board.WhiteIndex : Board.BlackIndex;
        if(((board.sideBitboard[currentColorIndex] ^ 1ul << kingIndex) & BitboardHelper.files[kingIndex % 8]).Empty())
        {
            score += kingOpenFile;
        }
        
        int direction = kingColor == Piece.White ? -1 : 1;
        int frontSquare = kingIndex + (direction * 8);

        if(frontSquare >= 0 && frontSquare <= 63)
        {
            if(board.GetPieces(currentColorIndex, Piece.Pawn).ContainsSquare(frontSquare))
            {
                score += kingPawnShield;
            }
        }

        return score;
    }

    EvalPair EvaluatePawnStructure(Board board, int currentColorIndex)
    {
        EvalPair score = new EvalPair();
        int oppositeColorIndex = 1 - currentColorIndex;
        int currentColor = currentColorIndex == Board.WhiteIndex ? Piece.White : Piece.Black;
        Bitboard ourPawns = board.GetPieces(currentColorIndex, Piece.Pawn);
        Bitboard ourPawnsTemp = ourPawns;
        Bitboard theirPawns = board.GetPieces(oppositeColorIndex, Piece.Pawn);
        int isolatedPawnCount = 0;

        int ourKing = board.GetPieces(currentColorIndex, Piece.King).GetLSB();
        int theirKing = board.GetPieces(oppositeColorIndex, Piece.King).GetLSB();

        while (!ourPawnsTemp.Empty())
        {
            int index = ourPawnsTemp.PopLSB();
            bool passer = (theirPawns & BitboardHelper.pawnPassedMask[currentColorIndex, index]).Empty();
            int pushSquare = index + (currentColorIndex == Board.WhiteIndex ? -8 : 8);

            //Passed pawn
            if (passer) { 
                int psqtIndex = currentColorIndex == Board.WhiteIndex ? index : index ^ 56;
                score += passedPawnBonuses[psqtIndex]; 
                score += friendlyKingDistPasser[Coord.ChebyshevDist(ourKing, index)]; 
                score += enemyKingDistPasser[Coord.ChebyshevDist(theirKing, index)]; 
            }

            if (board.PieceAt(pushSquare) == Piece.Pawn && board.ColorAt(pushSquare) == currentColor) { score.eg += doubledPawnPenalty.eg; }
            if ((BitboardHelper.isolatedPawnMask[index] & ourPawns).Empty()) { isolatedPawnCount++; }
        }
        score += isolatedPawnPenalty[isolatedPawnCount];
        
        int defended = (BitboardHelper.GetAllPawnAttacks(ourPawns, currentColor) & ourPawns).PopCount();
        score.mg += defended * protectedPawn.mg;
        score.eg += defended * protectedPawn.eg;
        return score;
    }

    EvalPair EvaluateBishopMobility(Board board, int pieceIndex)
    {
        Bitboard simpleBishopMoves = BitboardHelper.GetBishopAttacks(pieceIndex, board.allPiecesBitboard);
        int numMoves = simpleBishopMoves.PopCount();
        return new EvalPair(numMoves * bishopMobility.mg, numMoves * bishopMobility.eg);
    }

    EvalPair EvaluateRookMobility(Board board, int pieceIndex, int colorIndex)
    {
        EvalPair score = new EvalPair();
        if((BitboardHelper.files[pieceIndex % 8] & board.GetPieces(colorIndex, Piece.Pawn)).Empty()){ 
            //None of our their pawns
            if((BitboardHelper.files[pieceIndex % 8] & board.GetPieces(1 - colorIndex, Piece.Pawn)).Empty())
            {
                score += rookOpenFile; 
            }
            else
            {
                score += rookSemiOpenFile; 
            }
        }

        Bitboard simpleRookMoves = BitboardHelper.GetRookAttacks(pieceIndex, board.allPiecesBitboard);
        Bitboard rookAttacks = simpleRookMoves & BitboardHelper.kingRing[1 - colorIndex, board.GetPieces(1 - colorIndex, Piece.King).GetLSB()];
        int numMoves = simpleRookMoves.PopCount();
        int numAttacks = rookAttacks.PopCount();

        score.mg += numMoves * rookMobility.mg + numAttacks * rookKingRingAttack.mg;
        score.eg += numMoves * rookMobility.eg + numAttacks * rookKingRingAttack.eg;
        return score;
    }

}

public struct EvalPair
{
    public int mg;
    public int eg;
    public EvalPair(int mg, int eg)
    {
        this.mg = mg;
        this.eg = eg;
    }
    public static implicit operator EvalPair((int mg, int eg) value)=> new EvalPair(value.mg, value.eg);

    public static EvalPair operator +(EvalPair a, EvalPair b)
    {
        return new EvalPair
        {
            mg = a.mg + b.mg,
            eg = a.eg + b.eg
        };
    }
    public static EvalPair operator -(EvalPair a, EvalPair b)
    {
        return new EvalPair
        {
            mg = a.mg - b.mg,
            eg = a.eg - b.eg
        };
    }
}