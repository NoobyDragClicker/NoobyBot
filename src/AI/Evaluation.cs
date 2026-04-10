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
    (44, 107), (48, 85), (26, 113), (38, 92), (20, 95), (22, 96), (-1, 107), (-8, 116),
    (18, 40), (12, 47), (38, 31), (45, 39), (53, 26), (96, 15), (75, 44), (51, 35),
    (-6, 29), (4, 22), (6, 16), (8, 4), (28, 7), (29, 3), (21, 9), (19, 5),
    (-17, 13), (-11, 16), (-2, 8), (14, 7), (10, 7), (19, -1), (1, 2), (1, -6),
    (-24, 9), (-19, 7), (-10, 9), (-8, 13), (5, 14), (7, 8), (18, -7), (1, -9),
    (-19, 15), (-9, 14), (-12, 16), (-11, 16), (-4, 30), (32, 11), (26, -1), (0, -8),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)
    },
    {
    (-113, -43), (-74, 17), (-33, 43), (2, 27), (24, 39), (-36, 13), (-62, 14), (-71, -66),
    (-1, 30), (27, 47), (32, 60), (50, 60), (54, 42), (88, 40), (25, 39), (44, 8),
    (21, 44), (53, 59), (92, 73), (103, 72), (119, 64), (141, 54), (83, 49), (75, 22),
    (29, 52), (60, 70), (84, 85), (108, 90), (98, 85), (122, 80), (81, 68), (78, 39),
    (12, 56), (40, 66), (64, 87), (68, 89), (74, 95), (71, 77), (65, 65), (28, 41),
    (-8, 34), (23, 56), (41, 69), (50, 85), (65, 82), (51, 60), (48, 50), (14, 37),
    (-23, 26), (-10, 48), (10, 59), (27, 58), (27, 57), (33, 54), (14, 38), (9, 36),
    (-66, 12), (-17, 9), (-23, 39), (-1, 46), (2, 43), (13, 31), (-10, 22), (-33, 22)
    },
    {
    (-2, 45), (-19, 49), (-34, 43), (-89, 58), (-46, 49), (-49, 40), (-23, 44), (-29, 31),
    (20, 30), (13, 37), (6, 34), (-7, 33), (8, 26), (16, 28), (4, 35), (1, 36),
    (25, 51), (35, 36), (25, 30), (40, 18), (33, 21), (74, 23), (55, 35), (56, 44),
    (13, 46), (11, 46), (26, 32), (51, 29), (38, 22), (34, 31), (9, 42), (27, 40),
    (11, 41), (8, 45), (13, 33), (34, 26), (35, 21), (12, 29), (12, 37), (19, 32),
    (15, 42), (29, 34), (24, 33), (14, 34), (20, 38), (26, 31), (30, 30), (36, 30),
    (28, 45), (27, 21), (31, 21), (10, 33), (18, 34), (34, 26), (51, 24), (33, 19),
    (19, 26), (34, 48), (14, 32), (9, 39), (16, 35), (10, 44), (30, 31), (46, 1)
    },
    {
    (33, 72), (14, 79), (11, 91), (3, 85), (25, 75), (22, 79), (20, 83), (62, 66),
    (37, 68), (39, 74), (46, 78), (62, 65), (48, 66), (68, 62), (68, 65), (92, 54),
    (22, 69), (46, 62), (35, 64), (31, 57), (52, 53), (65, 48), (101, 48), (72, 48),
    (21, 70), (35, 61), (29, 68), (33, 62), (41, 47), (45, 49), (46, 52), (50, 51),
    (6, 67), (8, 65), (12, 64), (19, 61), (29, 55), (12, 56), (29, 51), (21, 55),
    (3, 59), (5, 57), (10, 54), (12, 57), (26, 49), (25, 41), (51, 26), (36, 32),
    (4, 52), (7, 53), (17, 55), (19, 53), (26, 43), (34, 39), (43, 30), (20, 38),
    (20, 55), (22, 52), (23, 59), (33, 51), (41, 44), (36, 49), (40, 38), (22, 39)
    },
    {
    (80, 225), (96, 241), (121, 267), (142, 267), (155, 262), (164, 240), (178, 200), (131, 221),
    (128, 235), (114, 267), (123, 295), (110, 314), (125, 330), (153, 298), (146, 275), (173, 258),
    (124, 240), (126, 260), (134, 295), (146, 305), (147, 318), (199, 295), (200, 247), (183, 259),
    (105, 257), (115, 274), (127, 284), (126, 315), (133, 326), (140, 309), (140, 303), (140, 277),
    (102, 246), (110, 271), (109, 285), (116, 315), (122, 306), (119, 300), (125, 279), (126, 265),
    (101, 220), (107, 249), (109, 277), (106, 277), (113, 278), (115, 274), (129, 245), (124, 232),
    (96, 221), (107, 224), (114, 222), (116, 237), (112, 243), (127, 214), (133, 179), (140, 154),
    (83, 220), (90, 219), (97, 230), (102, 238), (102, 230), (87, 213), (105, 196), (98, 198)
    },
    {
    (-55, -68), (-34, -30), (-3, -19), (-57, 6), (-20, -8), (12, 3), (42, -2), (124, -95),
    (-94, -0), (-22, 24), (-59, 29), (44, 8), (5, 27), (-5, 42), (43, 40), (-18, 23),
    (-100, 4), (18, 19), (-54, 33), (-57, 42), (-31, 47), (44, 43), (12, 43), (-26, 13),
    (-49, -18), (-55, 9), (-78, 31), (-153, 49), (-130, 50), (-90, 47), (-77, 35), (-126, 17),
    (-56, -28), (-53, -3), (-103, 23), (-137, 41), (-136, 44), (-91, 30), (-89, 17), (-130, 5),
    (-32, -27), (-0, -10), (-49, 8), (-69, 23), (-68, 24), (-67, 19), (-26, 3), (-56, -6),
    (51, -28), (23, -14), (4, -3), (-29, 4), (-36, 12), (-14, 5), (21, -7), (29, -25),
    (35, -62), (70, -51), (40, -31), (-45, -15), (12, -35), (-32, -11), (47, -43), (47, -74)
    }
    };
    public static EvalPair[] pieceValues = {(69, 117), (271, 355), (247, 325), (333, 627), (529, 1212)};
    public static EvalPair[] passedPawnBonuses = {(0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (44, 107), (48, 85), (26, 113), (38, 92), (20, 95), (22, 96), (-1, 107), (-8, 116), (21, 121), (32, 123), (15, 103), (-4, 65), (-11, 75), (-21, 95), (-48, 102), (-61, 127), (17, 47), (14, 45), (21, 22), (12, 19), (-3, 16), (2, 24), (-29, 49), (-24, 53), (-3, 7), (-5, -6), (-19, -13), (-3, -28), (-14, -25), (-15, -10), (-24, 7), (-11, 4), (-1, -42), (-11, -38), (-12, -47), (1, -62), (-5, -64), (-9, -57), (-17, -35), (15, -52), (-7, -45), (3, -57), (4, -66), (6, -80), (20, -91), (5, -81), (21, -81), (9, -64), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)};
    public static EvalPair[] isolatedPawnPenalty = {(8, 10), (1, 6), (-3, -3), (-14, -5), (-18, -14), (-8, -39), (-21, -49), (24, -71), (49, 49)};
    public static EvalPair doubledPawnPenalty = (0, -36);
    public static EvalPair protectedPawn = (13, 9);
    public static EvalPair isolatedExposed = (-11, -10);
    public static EvalPair[] pawnThreats = {(0, 0), (17, 1), (68, 26), (58, 59), (86, 21), (89, -11), (0, 0)};
    public static EvalPair[] friendlyKingDistPasser = {(0, 0), (-10, 58), (-18, 43), (-13, 21), (-5, 10), (3, 5), (24, 1), (11, 0)};
    public static EvalPair[] enemyKingDistPasser = {(0, 0), (-57, -49), (32, -4), (12, 25), (6, 38), (-6, 47), (-13, 52), (-38, 55)};
    public static EvalPair bishopPairBonus = (26, 60);
    public static EvalPair bishopMobility = (8, 9);
    public static EvalPair[] bishopThreats = {(0, 0), (-8, 15), (20, 35), (0, 0), (52, 34), (79, 68), (0, 0)};
    public static EvalPair[] knightThreats = {(0, 0), (-13, 8), (0, 0), (34, 35), (67, 20), (61, -37), (0, 0)};
    public static EvalPair rookOpenFile = (36, 1);
    public static EvalPair rookSemiOpenFile = (20, -0);
    public static EvalPair rookMobility = (1, 7);
    public static EvalPair rookKingRingAttack = (19, -8);
    public static EvalPair[] rookThreats = {(0, 0), (-15, 21), (2, 23), (14, 19), (0, 0), (80, 23), (0, 0)};
    public static EvalPair kingOpenFile = (-44, 6);
    public static EvalPair kingPawnShield = (20, -10);
    public static EvalPair tempo = (33, 28);
    
    public Evaluation()
    {
        for(int pieceType = 1; pieceType < 6; pieceType++)
        {
            for (int index = 0; index < 64; index++)
            {
                PSQT[pieceType, index] += pieceValues[pieceType - 1];
            }
        }
    }

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

        Bitboard whiteKnights = board.GetPieces(Board.WhiteIndex, Piece.Knight);
        Bitboard blackKnights = board.GetPieces(Board.BlackIndex, Piece.Knight);

        Bitboard whiteRooks = board.GetPieces(Board.WhiteIndex, Piece.Rook);
        Bitboard blackRooks = board.GetPieces(Board.BlackIndex, Piece.Rook);

        while (!whiteRooks.Empty())
        {
            int index = whiteRooks.PopLSB();
            eval += EvaluateRook(board, index, Board.WhiteIndex);
        }

        while (!blackRooks.Empty())
        {
            int index = blackRooks.PopLSB();
            eval -= EvaluateRook(board, index, Board.BlackIndex);
        }

        while (!whiteBishops.Empty())
        {
            int index = whiteBishops.PopLSB();
            eval += EvaluateBishop(board, index, Board.WhiteIndex);
        }
        while (!blackBishops.Empty())
        {
            int index = blackBishops.PopLSB();
            eval -= EvaluateBishop(board, index, Board.BlackIndex);
        }

        while (!whiteKnights.Empty())
        {
            int index = whiteKnights.PopLSB();
            eval += EvaluateKnight(board, index, Board.WhiteIndex);
        }
        while (!blackKnights.Empty())
        {
            int index = blackKnights.PopLSB();
            eval -= EvaluateKnight(board, index, Board.BlackIndex);
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

    EvalPair EvaluateBishop(Board board, int pieceIndex, int colorIndex)
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

    EvalPair EvaluateRook(Board board, int pieceIndex, int colorIndex)
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

    EvalPair EvaluateKnight(Board board, int pieceIndex, int colorIndex)
    {
        EvalPair score = new EvalPair();
        Bitboard threats = BitboardHelper.knightAttacks[pieceIndex] & board.sideBitboard[1 - colorIndex];
        while (!threats.Empty())
        {
            int index = threats.PopLSB();
            int pieceType = board.PieceAt(index);
            score += knightThreats[pieceType];
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