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
    (63, 134), (73, 112), (55, 134), (67, 115), (55, 116), (52, 117), (38, 127), (19, 136),
    (68, 124), (71, 130), (103, 111), (103, 110), (112, 104), (150, 103), (128, 127), (97, 121),
    (51, 116), (60, 111), (65, 106), (72, 92), (91, 96), (84, 96), (74, 101), (71, 98),
    (37, 103), (48, 106), (55, 99), (72, 98), (73, 98), (72, 93), (57, 94), (51, 84),
    (34, 101), (36, 99), (47, 101), (50, 105), (62, 107), (61, 100), (68, 87), (56, 83),
    (39, 109), (47, 108), (44, 110), (49, 113), (54, 126), (83, 107), (76, 95), (53, 89),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)
    },
    {
    (143, 234), (151, 279), (194, 302), (230, 285), (243, 300), (202, 275), (165, 279), (178, 211),
    (221, 282), (251, 293), (269, 308), (288, 309), (271, 299), (321, 288), (246, 290), (266, 262),
    (231, 298), (279, 308), (310, 321), (313, 321), (342, 309), (341, 309), (307, 297), (273, 281),
    (239, 297), (259, 322), (284, 338), (314, 337), (295, 337), (317, 333), (272, 321), (278, 294),
    (225, 308), (241, 320), (267, 338), (272, 338), (282, 341), (273, 329), (267, 315), (242, 290),
    (207, 289), (234, 307), (254, 318), (258, 334), (274, 328), (264, 309), (260, 300), (228, 290),
    (192, 286), (205, 301), (224, 309), (246, 306), (246, 307), (248, 302), (227, 289), (228, 288),
    (147, 263), (206, 278), (190, 293), (214, 297), (219, 293), (230, 281), (213, 289), (173, 277)
    },
    {
    (200, 236), (162, 244), (150, 240), (120, 250), (149, 245), (154, 231), (153, 238), (171, 229),
    (191, 226), (198, 218), (182, 220), (180, 220), (204, 207), (189, 214), (198, 216), (182, 228),
    (195, 242), (208, 221), (204, 205), (215, 193), (200, 198), (243, 200), (217, 220), (224, 237),
    (190, 237), (190, 224), (197, 207), (214, 199), (208, 191), (200, 205), (193, 219), (195, 236),
    (186, 233), (181, 225), (180, 211), (204, 198), (199, 195), (181, 205), (183, 218), (201, 225),
    (193, 237), (197, 222), (192, 215), (184, 214), (190, 217), (195, 212), (200, 218), (212, 229),
    (201, 248), (203, 219), (203, 211), (183, 221), (192, 222), (204, 213), (227, 219), (204, 225),
    (192, 231), (209, 239), (193, 242), (180, 233), (189, 229), (190, 246), (201, 227), (210, 207)
    },
    {
    (308, 449), (285, 454), (284, 462), (275, 457), (297, 449), (288, 456), (290, 459), (333, 444),
    (303, 456), (301, 464), (313, 467), (327, 455), (316, 456), (338, 447), (331, 449), (354, 438),
    (288, 458), (309, 454), (305, 454), (301, 449), (322, 441), (330, 437), (360, 436), (337, 436),
    (287, 459), (298, 450), (295, 456), (298, 448), (306, 435), (311, 437), (310, 442), (316, 442),
    (276, 453), (277, 449), (278, 449), (285, 443), (296, 438), (284, 438), (300, 432), (295, 438),
    (279, 447), (275, 441), (281, 437), (283, 440), (297, 431), (300, 422), (324, 408), (310, 418),
    (280, 441), (283, 438), (292, 437), (293, 437), (300, 428), (312, 419), (320, 411), (296, 425),
    (305, 453), (301, 439), (303, 442), (313, 436), (320, 429), (319, 441), (318, 423), (307, 440)
    },
    {
    (474, 1041), (490, 1047), (519, 1062), (547, 1054), (548, 1056), (578, 1012), (580, 987), (521, 1024),
    (509, 1026), (488, 1063), (504, 1084), (502, 1094), (516, 1104), (537, 1074), (525, 1051), (556, 1035),
    (509, 1023), (511, 1040), (518, 1078), (526, 1092), (534, 1098), (578, 1076), (576, 1028), (562, 1039),
    (493, 1034), (495, 1062), (509, 1073), (506, 1101), (513, 1112), (521, 1096), (520, 1089), (523, 1061),
    (486, 1044), (494, 1058), (490, 1078), (501, 1097), (505, 1093), (503, 1084), (511, 1066), (511, 1051),
    (486, 1013), (493, 1047), (494, 1068), (495, 1068), (500, 1065), (503, 1064), (515, 1033), (511, 1015),
    (482, 1017), (495, 1019), (504, 1026), (508, 1030), (503, 1040), (516, 1002), (523, 965), (527, 938),
    (471, 1007), (472, 1021), (482, 1031), (496, 1055), (489, 1027), (476, 1001), (502, 966), (493, 960)
    },
    {
    (-81, -63), (-86, -24), (-55, -13), (-104, 11), (-69, -3), (-10, 4), (42, 2), (104, -82),
    (-94, -9), (-28, 15), (-79, 24), (0, 10), (-35, 25), (-33, 35), (47, 29), (28, 12),
    (-92, -4), (23, 12), (-66, 30), (-78, 40), (-56, 42), (13, 38), (9, 34), (-26, 6),
    (-44, -22), (-54, 7), (-74, 28), (-150, 46), (-134, 47), (-93, 43), (-75, 30), (-103, 10),
    (-49, -29), (-50, -5), (-98, 22), (-139, 41), (-136, 43), (-91, 28), (-82, 14), (-113, 3),
    (-24, -30), (-6, -11), (-50, 7), (-73, 22), (-67, 23), (-67, 17), (-26, 1), (-49, -5),
    (51, -29), (22, -15), (3, -5), (-27, 2), (-34, 10), (-11, 6), (21, -5), (29, -23),
    (30, -56), (67, -47), (44, -26), (-40, -13), (19, -25), (-31, -7), (49, -35), (42, -65)
    }
    };
    public static EvalPair[] passedPawnBonuses = {(0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (63, 134), (73, 112), (55, 134), (67, 115), (55, 116), (52, 117), (38, 127), (19, 136), (17, 105), (28, 107), (8, 88), (-3, 62), (-11, 66), (-16, 77), (-41, 85), (-54, 105), (11, 42), (10, 39), (20, 13), (12, 12), (-6, 9), (3, 16), (-24, 40), (-23, 42), (-4, 9), (-8, -2), (-18, -15), (-3, -27), (-14, -26), (-10, -15), (-19, 4), (-10, 6), (-5, -35), (-12, -30), (-13, -41), (0, -56), (-5, -59), (-10, -53), (-15, -28), (11, -45), (-11, -38), (-1, -49), (2, -58), (4, -74), (17, -87), (1, -74), (14, -65), (4, -56), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)};
    public static EvalPair[] isolatedPawnCounts = {(0, 1), (0, 1), (0, 0), (0, 1), (0, -1), (0, -10), (0, -15), (0, -18), (0, 49)};
    public static EvalPair isolatedPawnPenalty = (-7, -6);
    public static EvalPair doubledPawnPenalty = (0, -32);
    public static EvalPair protectedPawn = (11, 8);
    public static EvalPair isolatedExposed = (-10, -4);
    public static EvalPair[] friendlyKingDistPasser = {(0, 0), (-10, 54), (-15, 40), (-10, 21), (-2, 11), (6, 6), (25, 3), (13, 3)};
    public static EvalPair[] enemyKingDistPasser = {(0, 0), (-57, -31), (25, 0), (11, 24), (7, 34), (-1, 41), (-5, 44), (-30, 47)};
    public static EvalPair bishopPairBonus = (41, 51);
    public static EvalPair bishopMobility = (9, 14);
    public static EvalPair rookOpenFile = (40, -7);
    public static EvalPair rookSemiOpenFile = (12, 13);
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
                score += isolatedPawnPenalty;
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