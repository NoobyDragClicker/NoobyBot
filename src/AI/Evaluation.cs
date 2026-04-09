using System.Runtime.CompilerServices;
public class Evaluation
{

    int colorTurn;

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
    (65, 139), (70, 118), (54, 140), (66, 120), (50, 123), (51, 124), (33, 134), (20, 142),
    (73, 126), (73, 133), (104, 115), (105, 112), (112, 108), (153, 106), (132, 130), (104, 123),
    (54, 120), (63, 115), (66, 109), (72, 96), (91, 100), (86, 99), (76, 105), (75, 101),
    (42, 107), (51, 110), (58, 102), (75, 101), (74, 102), (75, 96), (60, 97), (56, 87),
    (36, 105), (41, 102), (50, 103), (52, 107), (65, 109), (65, 103), (72, 90), (58, 87),
    (41, 112), (51, 110), (47, 111), (51, 115), (56, 127), (87, 108), (80, 97), (56, 91),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)
    },
    {
    (155, 241), (164, 288), (213, 309), (248, 290), (259, 304), (212, 281), (176, 282), (190, 216),
    (236, 292), (272, 301), (288, 316), (305, 316), (282, 305), (336, 296), (255, 299), (278, 269),
    (249, 306), (293, 317), (329, 329), (329, 331), (365, 316), (359, 316), (325, 304), (286, 287),
    (250, 307), (273, 331), (299, 348), (328, 348), (309, 348), (333, 341), (285, 330), (292, 301),
    (234, 317), (255, 330), (279, 349), (285, 350), (296, 352), (284, 339), (280, 324), (250, 298),
    (215, 298), (243, 316), (261, 329), (269, 346), (284, 340), (272, 318), (269, 308), (237, 298),
    (199, 296), (213, 311), (230, 320), (251, 317), (251, 317), (255, 313), (237, 297), (234, 296),
    (155, 274), (209, 288), (200, 304), (221, 309), (226, 303), (237, 291), (216, 298), (184, 289)
    },
    {
    (213, 243), (177, 248), (164, 243), (134, 252), (160, 246), (162, 234), (165, 242), (185, 235),
    (203, 233), (195, 227), (189, 225), (184, 223), (195, 215), (196, 218), (191, 224), (187, 236),
    (205, 249), (214, 228), (205, 210), (216, 199), (208, 202), (254, 203), (230, 224), (235, 241),
    (196, 245), (189, 232), (202, 212), (223, 200), (213, 193), (210, 208), (189, 227), (209, 238),
    (191, 239), (186, 232), (192, 209), (212, 199), (211, 193), (190, 205), (191, 222), (201, 233),
    (198, 241), (208, 223), (202, 216), (194, 213), (200, 217), (205, 212), (210, 221), (218, 231),
    (210, 249), (211, 220), (212, 212), (191, 225), (200, 224), (214, 216), (233, 220), (214, 227),
    (203, 234), (218, 247), (199, 250), (191, 240), (199, 236), (197, 253), (210, 233), (224, 211)
    },
    {
    (312, 447), (290, 452), (289, 461), (279, 455), (299, 447), (292, 453), (292, 456), (337, 440),
    (320, 444), (321, 448), (327, 451), (342, 438), (332, 439), (348, 436), (346, 440), (366, 432),
    (306, 446), (326, 439), (321, 438), (314, 430), (336, 426), (351, 422), (374, 426), (353, 428),
    (305, 450), (315, 438), (310, 444), (313, 437), (321, 423), (327, 425), (325, 431), (331, 435),
    (290, 450), (292, 444), (291, 444), (299, 440), (310, 434), (296, 433), (312, 428), (306, 436),
    (289, 447), (286, 440), (291, 437), (293, 441), (308, 431), (308, 421), (333, 405), (319, 418),
    (290, 443), (292, 440), (300, 440), (302, 440), (308, 430), (319, 423), (326, 414), (304, 428),
    (307, 456), (307, 441), (309, 445), (318, 438), (326, 431), (321, 444), (323, 425), (308, 442)
    },
    {
    (496, 1047), (514, 1051), (542, 1066), (570, 1056), (568, 1062), (598, 1021), (596, 997), (542, 1032),
    (525, 1045), (512, 1076), (525, 1095), (525, 1103), (538, 1114), (554, 1091), (548, 1064), (570, 1052),
    (522, 1044), (527, 1058), (536, 1093), (545, 1104), (546, 1112), (596, 1091), (591, 1045), (574, 1055),
    (506, 1052), (510, 1078), (525, 1089), (524, 1114), (531, 1125), (535, 1109), (533, 1103), (535, 1075),
    (495, 1061), (508, 1073), (503, 1092), (514, 1114), (520, 1107), (514, 1100), (522, 1078), (520, 1066),
    (496, 1028), (500, 1063), (503, 1084), (502, 1086), (508, 1083), (510, 1082), (523, 1049), (519, 1030),
    (489, 1030), (503, 1033), (508, 1042), (512, 1048), (507, 1057), (522, 1021), (530, 982), (535, 951),
    (476, 1024), (482, 1033), (489, 1046), (496, 1073), (495, 1044), (482, 1017), (509, 980), (500, 975)
    },
    {
    (-75, -59), (-72, -21), (-45, -11), (-94, 11), (-67, -1), (-10, 7), (38, 3), (105, -81),
    (-88, -7), (-24, 19), (-74, 27), (6, 11), (-31, 27), (-26, 37), (44, 33), (16, 15),
    (-88, -2), (25, 14), (-60, 31), (-71, 40), (-49, 45), (20, 40), (6, 38), (-27, 8),
    (-41, -20), (-49, 8), (-70, 28), (-144, 46), (-127, 48), (-88, 44), (-72, 31), (-105, 12),
    (-46, -28), (-45, -4), (-93, 22), (-131, 39), (-130, 42), (-86, 28), (-79, 14), (-111, 4),
    (-23, -28), (-3, -10), (-47, 6), (-68, 21), (-63, 21), (-63, 16), (-24, 1), (-47, -6),
    (51, -28), (21, -14), (3, -5), (-28, 2), (-34, 9), (-11, 5), (20, -6), (29, -25),
    (31, -56), (66, -47), (40, -25), (-39, -13), (14, -25), (-29, -8), (46, -37), (43, -68)
    }
    };
    public static EvalPair[] passedPawnBonuses = {(0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (65, 139), (70, 118), (54, 140), (66, 120), (50, 123), (51, 124), (33, 134), (20, 142), (16, 105), (28, 107), (8, 88), (-4, 63), (-10, 67), (-20, 79), (-43, 86), (-56, 106), (10, 41), (11, 38), (21, 12), (12, 12), (-4, 9), (2, 16), (-23, 39), (-24, 42), (-5, 7), (-8, -4), (-18, -16), (-2, -28), (-14, -26), (-13, -14), (-20, 3), (-12, 5), (-4, -38), (-12, -32), (-13, -42), (1, -58), (-4, -59), (-11, -53), (-16, -29), (10, -46), (-10, -39), (-2, -50), (2, -60), (5, -76), (17, -87), (-1, -74), (13, -67), (3, -56), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)};
    public static EvalPair[] isolatedPawnPenalty = {(7, 11), (-0, 5), (-3, -3), (-12, -6), (-14, -15), (-6, -36), (-16, -45), (32, -69), (49, 49)};
    public static EvalPair doubledPawnPenalty = (0, -32);
    public static EvalPair protectedPawn = (12, 8);
    public static EvalPair isolatedExposed = (-11, -4);
    public static EvalPair[] pawnThreats = {(0, 0), (18, -4), (56, 17), (48, 41), (74, 7), (88, -46), (0, 0)};
    public static EvalPair[] friendlyKingDistPasser = {(0, 0), (-11, 54), (-16, 40), (-10, 22), (-2, 11), (5, 7), (24, 3), (11, 4)};
    public static EvalPair[] enemyKingDistPasser = {(0, 0), (-55, -36), (24, 1), (9, 26), (5, 35), (-3, 42), (-6, 45), (-29, 47)};
    public static EvalPair bishopPairBonus = (40, 45);
    public static EvalPair bishopMobility = (9, 14);
    public static EvalPair[] bishopThreats = {(0, 0), (-8, 16), (19, 31), (0, 0), (49, 21), (81, 26), (0, 0)};
    public static EvalPair rookOpenFile = (37, -9);
    public static EvalPair rookSemiOpenFile = (20, -7);
    public static EvalPair rookMobility = (0, 13);
    public static EvalPair rookKingRingAttack = (13, -6);
    public static EvalPair[] rookThreats = {(0, 0), (-14, 27), (3, 28), (10, 26), (0, 0), (89, -27), (0, 0)};
    public static EvalPair kingOpenFile = (-38, 3);
    public static EvalPair kingPawnShield = (19, -10);
    public static EvalPair tempo = (27, 18);
    


    int playerTurnMultiplier;
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
            eval += EvaluateBishopMobility(board, index, Board.WhiteIndex);
        }
        while (!blackBishops.Empty())
        {
            int index = blackBishops.PopLSB();
            eval -= EvaluateBishopMobility(board, index, Board.BlackIndex);
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

        if (board.colorTurn == Piece.White)
        {
            eval += tempo;
        }
        else
        {
            eval -= tempo;
        }
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
        
        Bitboard ourAttacks = BitboardHelper.GetAllPawnAttacks(ourPawns, currentColor);
        Bitboard threats = ourAttacks & board.sideBitboard[oppositeColorIndex];
        while (!threats.Empty())
        {
            int index = threats.PopLSB();
            int pieceType = board.PieceAt(index);
            score += pawnThreats[pieceType];
        }
        int defended = (ourAttacks & ourPawns).PopCount();
        score.mg += defended * protectedPawn.mg;
        score.eg += defended * protectedPawn.eg;
        return score;
    }

    EvalPair EvaluateBishopMobility(Board board, int pieceIndex, int colorIndex)
    {
        Bitboard simpleBishopMoves = BitboardHelper.GetBishopAttacks(pieceIndex, board.allPiecesBitboard);
        Bitboard threats = simpleBishopMoves & board.sideBitboard[1 - colorIndex];
        int numMoves = simpleBishopMoves.PopCount();
        EvalPair score = new EvalPair(numMoves * bishopMobility.mg, numMoves * bishopMobility.eg);
        while (!threats.Empty())
        {
            int index = threats.PopLSB();
            int pieceType = board.PieceAt(index);
            score += bishopThreats[pieceType];
        }
        return score;
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
        Bitboard threats = simpleRookMoves & board.sideBitboard[1 - colorIndex];
        Bitboard rookAttacks = simpleRookMoves & BitboardHelper.kingRing[1 - colorIndex, board.GetPieces(1 - colorIndex, Piece.King).GetLSB()];
        int numMoves = simpleRookMoves.PopCount();
        int numAttacks = rookAttacks.PopCount();

        score.mg += numMoves * rookMobility.mg + numAttacks * rookKingRingAttack.mg;
        score.eg += numMoves * rookMobility.eg + numAttacks * rookKingRingAttack.eg;

        while (!threats.Empty())
        {
            int index = threats.PopLSB();
            int pieceType = board.PieceAt(index);
            score += rookThreats[pieceType];
        }
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvalPair operator +(EvalPair a, EvalPair b)
    {
        return new EvalPair
        {
            mg = a.mg + b.mg,
            eg = a.eg + b.eg
        };
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvalPair operator -(EvalPair a, EvalPair b)
    {
        return new EvalPair
        {
            mg = a.mg - b.mg,
            eg = a.eg - b.eg
        };
    }
}