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
    (63, 132), (73, 114), (56, 133), (67, 116), (55, 117), (53, 117), (37, 129), (19, 134),
    (70, 121), (71, 134), (104, 110), (103, 110), (112, 104), (151, 102), (127, 131), (98, 119),
    (53, 113), (60, 117), (65, 105), (71, 93), (91, 96), (84, 95), (74, 106), (72, 95),
    (39, 100), (49, 112), (56, 98), (72, 99), (72, 98), (72, 92), (57, 99), (52, 82),
    (36, 98), (37, 104), (47, 100), (50, 105), (62, 107), (62, 99), (68, 92), (57, 81),
    (41, 106), (48, 114), (44, 109), (49, 114), (53, 126), (83, 106), (76, 100), (54, 86),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)
    },
    {
    (143, 234), (151, 279), (194, 302), (230, 285), (243, 300), (202, 275), (165, 278), (178, 211),
    (221, 282), (251, 293), (269, 308), (288, 309), (271, 299), (321, 288), (246, 290), (266, 262),
    (231, 298), (279, 308), (310, 321), (313, 321), (342, 309), (341, 309), (307, 298), (273, 280),
    (239, 297), (259, 322), (284, 338), (314, 337), (295, 337), (317, 333), (272, 321), (278, 293),
    (225, 308), (241, 320), (267, 338), (272, 338), (282, 341), (273, 329), (267, 315), (242, 290),
    (207, 289), (234, 307), (254, 318), (258, 334), (274, 328), (264, 309), (260, 300), (228, 290),
    (192, 286), (206, 301), (224, 309), (246, 306), (246, 307), (248, 302), (227, 289), (228, 288),
    (147, 263), (206, 278), (190, 293), (214, 297), (219, 293), (230, 281), (213, 289), (173, 277)
    },
    {
    (200, 236), (162, 244), (150, 241), (120, 250), (149, 246), (154, 231), (154, 238), (171, 229),
    (191, 226), (198, 218), (182, 220), (180, 220), (204, 207), (189, 214), (198, 217), (182, 228),
    (195, 242), (208, 221), (204, 205), (215, 194), (200, 199), (243, 200), (217, 220), (224, 237),
    (190, 238), (190, 224), (196, 208), (215, 199), (208, 191), (200, 206), (193, 219), (195, 236),
    (186, 233), (181, 225), (180, 211), (204, 199), (199, 195), (181, 205), (183, 218), (201, 225),
    (193, 237), (197, 222), (192, 215), (184, 214), (190, 218), (195, 212), (200, 218), (212, 229),
    (201, 248), (203, 219), (203, 211), (183, 222), (192, 222), (204, 213), (227, 219), (204, 225),
    (192, 231), (209, 239), (193, 242), (180, 233), (189, 229), (190, 246), (201, 227), (210, 207)
    },
    {
    (308, 449), (285, 454), (284, 462), (275, 457), (297, 449), (289, 456), (290, 459), (334, 444),
    (303, 456), (301, 464), (313, 467), (327, 455), (316, 456), (338, 446), (331, 449), (354, 438),
    (288, 458), (309, 454), (305, 454), (301, 449), (322, 441), (330, 437), (360, 436), (337, 436),
    (287, 459), (298, 450), (295, 456), (298, 448), (306, 435), (311, 437), (310, 442), (316, 442),
    (276, 453), (277, 449), (278, 449), (285, 443), (296, 438), (284, 438), (300, 432), (295, 438),
    (279, 447), (275, 441), (281, 437), (283, 440), (297, 431), (300, 422), (323, 408), (310, 419),
    (280, 441), (283, 438), (292, 437), (293, 437), (300, 428), (312, 420), (320, 411), (296, 425),
    (305, 453), (301, 439), (303, 443), (313, 436), (320, 429), (319, 441), (318, 424), (307, 440)
    },
    {
    (474, 1041), (490, 1047), (519, 1062), (547, 1054), (548, 1056), (578, 1012), (580, 987), (521, 1024),
    (509, 1026), (488, 1063), (503, 1084), (502, 1094), (516, 1104), (537, 1074), (525, 1051), (556, 1035),
    (509, 1023), (511, 1040), (518, 1078), (526, 1092), (534, 1098), (578, 1076), (576, 1028), (562, 1039),
    (493, 1033), (495, 1061), (509, 1073), (506, 1101), (513, 1112), (521, 1096), (520, 1089), (523, 1061),
    (487, 1044), (494, 1058), (490, 1077), (501, 1097), (505, 1093), (503, 1084), (511, 1066), (511, 1051),
    (486, 1013), (493, 1046), (494, 1067), (495, 1067), (500, 1065), (503, 1063), (515, 1033), (511, 1015),
    (482, 1017), (495, 1019), (504, 1026), (508, 1030), (503, 1040), (516, 1002), (523, 965), (527, 937),
    (471, 1006), (472, 1021), (482, 1031), (496, 1054), (489, 1027), (476, 1001), (501, 966), (493, 960)
    },
    {
    (-80, -63), (-85, -24), (-54, -13), (-103, 11), (-68, -3), (-10, 5), (42, 2), (104, -83),
    (-93, -9), (-28, 15), (-79, 24), (1, 10), (-35, 25), (-33, 35), (47, 29), (28, 12),
    (-92, -4), (23, 12), (-66, 30), (-77, 40), (-55, 42), (13, 38), (9, 34), (-26, 6),
    (-44, -22), (-54, 7), (-74, 28), (-150, 46), (-134, 47), (-93, 43), (-75, 30), (-103, 10),
    (-50, -29), (-50, -5), (-98, 22), (-139, 41), (-136, 43), (-91, 28), (-82, 14), (-113, 4),
    (-24, -30), (-6, -11), (-51, 7), (-73, 22), (-67, 23), (-67, 17), (-26, 1), (-49, -5),
    (51, -29), (22, -15), (3, -5), (-28, 2), (-34, 10), (-11, 6), (21, -5), (29, -23),
    (30, -56), (67, -48), (44, -26), (-40, -13), (19, -25), (-31, -7), (49, -35), (42, -66)
    }
    };
    public static EvalPair[] passedPawnBonuses = {(0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (63, 132), (73, 114), (56, 133), (67, 116), (55, 117), (53, 117), (37, 129), (19, 134), (15, 103), (26, 107), (10, 88), (-2, 63), (-10, 67), (-15, 77), (-42, 86), (-54, 103), (8, 40), (8, 39), (21, 13), (13, 13), (-5, 10), (5, 16), (-26, 40), (-24, 40), (-6, 7), (-10, -3), (-16, -14), (-2, -26), (-13, -24), (-9, -14), (-21, 4), (-11, 4), (-7, -38), (-14, -30), (-11, -40), (1, -55), (-4, -57), (-8, -52), (-16, -28), (9, -48), (-13, -40), (-2, -49), (4, -58), (5, -73), (18, -85), (3, -73), (13, -65), (3, -59), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)};
    public static EvalPair[] isolatedPawnCounts = {(0, 3), (0, 1), (0, -0), (0, -0), (0, -3), (0, -13), (0, -18), (0, -20), (0, 49)};
    public static EvalPair[] isolatedPawnPenalty = {(-5, 1), (-1, -10), (-9, -4), (-9, -7)};
    public static EvalPair doubledPawnPenalty = (0, -34);
    public static EvalPair protectedPawn = (11, 8);
    public static EvalPair isolatedExposed = (-9, -4);
    public static EvalPair[] friendlyKingDistPasser = {(0, 0), (-10, 53), (-15, 40), (-10, 21), (-2, 10), (5, 6), (25, 2), (12, 2)};
    public static EvalPair[] enemyKingDistPasser = {(0, 0), (-58, -32), (24, -0), (10, 24), (6, 34), (-1, 40), (-6, 43), (-30, 47)};
    public static EvalPair bishopPairBonus = (41, 51);
    public static EvalPair bishopMobility = (9, 14);
    public static EvalPair rookOpenFile = (40, -7);
    public static EvalPair rookSemiOpenFile = (12, 12);
    public static EvalPair rookMobility = (0, 12);
    public static EvalPair rookKingRingAttack = (13, -4);
    public static EvalPair kingOpenFile = (-39, 3);
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
            Bitboard stoppers = theirPawns & BitboardHelper.pawnPassedMask[currentColorIndex, index];
            bool passer = stoppers.Empty();
            int pushSquare = index + (currentColorIndex == Board.WhiteIndex ? -8 : 8);

            //Passed pawn
            if (passer) { 
                int psqtIndex = currentColorIndex == Board.WhiteIndex ? index : index ^ 56;
                score += passedPawnBonuses[psqtIndex]; 
                score += friendlyKingDistPasser[Coord.ChebyshevDist(ourKing, index)]; 
                score += enemyKingDistPasser[Coord.ChebyshevDist(theirKing, index)]; 
            }

            if (board.PieceAt(pushSquare) == Piece.Pawn && board.ColorAt(pushSquare) == currentColor) { score.eg += doubledPawnPenalty.eg; }
            if ((BitboardHelper.isolatedPawnMask[index] & ourPawns).Empty()) { 
                isolatedPawnCount++; 
                score += isolatedPawnPenalty[Coord.DistToEdge(index)];
                if ((stoppers & BitboardHelper.files[index%8]).Empty())
                {
                    score += isolatedExposed;
                }
            }
        }
        score += isolatedPawnCounts[isolatedPawnCount];
        
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