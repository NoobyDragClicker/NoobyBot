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
    (64, 134), (75, 111), (56, 133), (68, 114), (56, 116), (53, 117), (39, 127), (19, 136),
    (71, 122), (75, 128), (104, 110), (103, 110), (113, 103), (150, 103), (130, 126), (98, 120),
    (54, 114), (65, 109), (65, 105), (72, 92), (91, 96), (84, 96), (78, 99), (73, 97),
    (40, 102), (54, 103), (56, 99), (72, 98), (73, 98), (71, 93), (61, 91), (53, 83),
    (37, 100), (41, 96), (47, 101), (50, 105), (62, 107), (61, 101), (72, 85), (58, 82),
    (43, 107), (53, 105), (45, 110), (50, 113), (54, 126), (83, 107), (81, 92), (56, 87),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)
    },
    {
    (143, 234), (151, 280), (194, 302), (230, 285), (243, 300), (202, 275), (165, 279), (178, 211),
    (220, 283), (252, 293), (269, 309), (288, 309), (271, 299), (321, 288), (246, 289), (266, 262),
    (231, 298), (279, 308), (309, 321), (313, 321), (342, 309), (341, 309), (307, 297), (272, 281),
    (239, 297), (258, 322), (284, 338), (314, 337), (295, 337), (316, 333), (272, 321), (278, 294),
    (224, 308), (241, 320), (267, 338), (271, 338), (282, 341), (272, 330), (266, 315), (241, 290),
    (206, 289), (234, 307), (254, 318), (258, 334), (274, 329), (264, 309), (259, 300), (227, 290),
    (191, 287), (205, 301), (223, 309), (246, 306), (245, 307), (247, 302), (227, 289), (228, 288),
    (146, 263), (206, 278), (190, 294), (213, 297), (218, 294), (230, 281), (213, 289), (173, 277)
    },
    {
    (200, 236), (161, 244), (150, 240), (120, 250), (149, 246), (154, 231), (153, 238), (171, 229),
    (190, 226), (199, 217), (182, 220), (180, 220), (204, 207), (189, 214), (199, 216), (181, 228),
    (194, 242), (208, 221), (204, 205), (215, 193), (199, 198), (243, 200), (216, 220), (223, 237),
    (190, 238), (189, 224), (196, 207), (214, 199), (208, 191), (200, 205), (192, 219), (194, 236),
    (185, 233), (181, 225), (180, 211), (203, 198), (199, 195), (180, 205), (183, 218), (200, 225),
    (192, 237), (197, 222), (192, 215), (184, 214), (189, 217), (195, 212), (199, 218), (211, 229),
    (200, 248), (203, 219), (202, 211), (182, 222), (191, 222), (203, 213), (226, 219), (203, 225),
    (192, 231), (208, 239), (192, 243), (180, 233), (188, 229), (190, 246), (200, 227), (209, 207)
    },
    {
    (308, 449), (285, 454), (284, 463), (275, 457), (297, 449), (289, 456), (290, 459), (333, 444),
    (303, 456), (301, 464), (313, 467), (327, 455), (316, 456), (338, 447), (331, 449), (354, 438),
    (288, 458), (309, 454), (305, 454), (301, 449), (323, 441), (330, 437), (360, 436), (336, 437),
    (287, 459), (299, 450), (295, 456), (298, 448), (306, 435), (311, 437), (310, 442), (316, 442),
    (276, 453), (277, 449), (278, 449), (285, 443), (296, 438), (284, 438), (300, 433), (295, 439),
    (278, 447), (275, 441), (281, 437), (282, 440), (297, 431), (300, 422), (323, 408), (310, 418),
    (280, 441), (283, 438), (292, 437), (293, 437), (300, 428), (312, 420), (320, 411), (296, 425),
    (305, 453), (301, 439), (303, 443), (313, 436), (320, 429), (319, 442), (318, 424), (307, 440)
    },
    {
    (474, 1041), (490, 1047), (519, 1062), (547, 1054), (548, 1056), (578, 1012), (579, 987), (521, 1024),
    (508, 1026), (489, 1062), (504, 1083), (502, 1094), (516, 1104), (537, 1074), (526, 1050), (556, 1036),
    (509, 1023), (511, 1040), (518, 1078), (526, 1092), (534, 1098), (578, 1076), (576, 1028), (562, 1039),
    (493, 1034), (495, 1062), (509, 1073), (506, 1101), (513, 1112), (521, 1096), (520, 1089), (523, 1061),
    (486, 1045), (494, 1058), (490, 1078), (501, 1097), (505, 1093), (503, 1084), (510, 1066), (511, 1051),
    (486, 1013), (493, 1047), (493, 1068), (495, 1068), (500, 1066), (503, 1063), (515, 1033), (510, 1015),
    (482, 1017), (495, 1019), (503, 1026), (508, 1030), (503, 1040), (516, 1002), (523, 965), (526, 937),
    (471, 1006), (471, 1021), (481, 1031), (495, 1055), (489, 1027), (475, 1001), (501, 966), (493, 960)
    },
    {
    (-81, -63), (-86, -24), (-55, -13), (-103, 10), (-69, -3), (-10, 4), (42, 2), (104, -82),
    (-94, -9), (-28, 15), (-79, 24), (0, 10), (-35, 25), (-33, 35), (47, 29), (28, 12),
    (-92, -4), (23, 12), (-66, 30), (-78, 40), (-55, 42), (13, 38), (9, 34), (-26, 6),
    (-44, -22), (-54, 7), (-74, 28), (-150, 46), (-134, 47), (-92, 43), (-75, 30), (-103, 10),
    (-50, -29), (-50, -5), (-98, 22), (-139, 41), (-136, 43), (-91, 28), (-82, 14), (-113, 3),
    (-24, -30), (-6, -11), (-50, 7), (-73, 22), (-67, 23), (-67, 17), (-26, 2), (-49, -5),
    (51, -29), (22, -15), (3, -5), (-27, 2), (-34, 10), (-11, 6), (21, -5), (29, -23),
    (29, -56), (67, -47), (44, -26), (-40, -13), (19, -25), (-31, -7), (49, -35), (42, -65)
    }
    };
    public static EvalPair[] passedPawnBonuses = {(0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (64, 134), (75, 111), (56, 133), (68, 114), (56, 116), (53, 117), (39, 127), (19, 136), (15, 106), (26, 108), (9, 87), (-2, 61), (-10, 65), (-15, 76), (-42, 85), (-54, 106), (9, 43), (8, 40), (21, 12), (13, 11), (-4, 9), (5, 15), (-26, 40), (-24, 43), (-6, 10), (-11, -1), (-17, -15), (-2, -28), (-13, -26), (-9, -15), (-22, 6), (-11, 7), (-8, -34), (-15, -28), (-12, -42), (1, -57), (-4, -60), (-9, -54), (-17, -27), (8, -44), (-15, -36), (-4, -47), (3, -59), (5, -74), (18, -88), (2, -75), (12, -64), (2, -55), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)};
    public static EvalPair[] isolatedPawnPenalty = {(0, 17), (0, 6), (0, -4), (0, -12), (0, -23), (0, -42), (0, -55), (0, -60), (0, 49)};
    public static EvalPair doubledPawnPenalty = (0, -33);
    public static EvalPair protectedPawn = (12, 7);
    public static EvalPair isolatedExposed = (-16, -1);
    public static EvalPair[] friendlyKingDistPasser = {(0, 0), (-11, 54), (-16, 41), (-11, 22), (-3, 11), (5, 7), (24, 3), (12, 3)};
    public static EvalPair[] enemyKingDistPasser = {(0, 0), (-58, -31), (24, 1), (10, 25), (6, 34), (-2, 41), (-7, 44), (-31, 48)};
    public static EvalPair bishopPairBonus = (41, 51);
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
                if ((stoppers & BitboardHelper.files[index%8]).Empty())
                {
                    score += isolatedExposed;
                }
            }
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