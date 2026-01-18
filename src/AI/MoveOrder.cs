using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

public class MoveOrder
{
    const int million = 1000000;
    const int HISTORY_MAX = 32768;
    const int HISTORY_MULTIPLE = 300;
    const int HISTORY_SUB = 250;

    Move[] killers = new Move[Search.MAX_GAME_PLY];
    
    //Color turn, from, to
    public int[,,] history = new int[2, 64, 64];
    int[] continuationHistory = new int[2 * 7 * 64 * 2 * 7 * 64];
    //Color turn, to, moved piece, captured piece
    int[,,,] captureHistory = new int[2, 64, 7, 7];

    public (Move, int)[] movesAndPieceTypes = new (Move, int)[Search.MAX_GAME_PLY];
    public enum Stage { TTMove, Other, Finished }

    static int[] MVV_LVA = {
        0, 0, 0, 0, 0, 0, 0, //None
        0, 6, 12, 18, 24, 30, 100 , //Pawn
        0, 5, 11, 17, 23, 29, 100, //Knight
        0, 4, 10, 16, 22, 28, 100 , //Bishop
        0, 3, 9, 15, 21, 27, 100, //Rook
        0, 2, 8, 14, 20, 26, 100 , //Queen
        0, 1, 7, 13, 19, 25, 100   //King
     };
    
    static int[] MVV = {0, 100, 300, 330, 500, 900, 0};
    public int[] ScoreMoves(Board board, Span<Move> moves, Move firstMove)
    {
        int[] moveScores = new int[moves.Length];
        for (int x = 0; x < moves.Length; x++)
        {
            Move move = moves[x];
            int score = 0;
            if (!firstMove.isNull() && move.GetIntValue() == firstMove.GetIntValue())
            {
                score = 8 * million;
            }
            else if (!killers[board.fullMoveClock].isNull() && move.GetIntValue() == killers[board.fullMoveClock].GetIntValue())
            {
                score = million;
            }
            else if (move.isCapture())
            {
                int movedPieceType;
                int capturedPieceType;
                //en passant
                if (move.flag == 7) { capturedPieceType = Piece.Pawn; }
                else { capturedPieceType = Piece.PieceType(board.board[moves[x].newIndex]); }
                movedPieceType = Piece.PieceType(board.board[moves[x].oldIndex]);

                //MVV LVA 
                score = million + 10 + MVV[capturedPieceType] * 15 + captureHistory[board.currentColorIndex, move.newIndex, movedPieceType, capturedPieceType];
            }
            else if (move.isPromotion())
            {
                score = million + GetPieceValue(move.PromotedPieceType()) * 10;
            }
            else if (history[board.currentColorIndex, move.oldIndex, move.newIndex] != 0)
            {
                score = history[board.currentColorIndex, move.oldIndex, move.newIndex];
                if(board.fullMoveClock > 0)
                {
                    score += continuationHistory[FlattenConthistIndex(board.oppositeColorIndex, movesAndPieceTypes[board.fullMoveClock - 1].Item2, movesAndPieceTypes[board.fullMoveClock - 1].Item1.newIndex, board.currentColorIndex, Piece.PieceType(board.board[move.oldIndex]), move.newIndex)];
                }
            }
            else
            {
                int attackedSquaresIndex = (Piece.Color(board.board[moves[x].oldIndex]) == Piece.White) ? Board.BlackIndex : Board.WhiteIndex;
                //Penalty for moving to attacked square
                if (BitboardHelper.ContainsSquare(board.gameStateHistory[board.fullMoveClock].attackedSquares[attackedSquaresIndex], move.newIndex))
                {
                    score -= 4;
                }

                //Bonus for developping
                if (Coord.IndexToFile(move.newIndex) >= 3 && Coord.IndexToFile(move.newIndex) <= 6 && Coord.IndexToRank(move.newIndex) >= 3 && Coord.IndexToRank(move.newIndex) <= 6)
                {
                    score += 2;
                }
                else if (Coord.IndexToFile(move.newIndex) >= 2 && Coord.IndexToFile(move.newIndex) <= 7 && Coord.IndexToRank(move.newIndex) >= 2 && Coord.IndexToRank(move.newIndex) <= 7)
                {
                    score += 1;
                }
            }
            moveScores[x] = score;
        }
        return moveScores;
    }

    public Move GetNextBestMove(int[] moveScores, Span<Move> moves, int currentMoveIndex)
    {
        if(currentMoveIndex > moves.Length - 1){ Console.WriteLine("issue"); }
        //Take the index the search is currently at
        int highest = currentMoveIndex;
        for (int i = highest + 1; i < moveScores.Length; i++)
        {
            //Find the next highest score
            if (moveScores[i] > moveScores[highest]) { highest = i; }
        }

        //Swap the next highest move into the spot that is about to be searched, hoping for a quick beta cutoff
        Move bestMove = moves[highest];
        int tempScore = moveScores[highest];
        moves[highest] = moves[currentMoveIndex];
        moveScores[highest] = moveScores[currentMoveIndex];
        moves[currentMoveIndex] = bestMove;
        moveScores[currentMoveIndex] = tempScore;
        return bestMove;
    }
    static int GetPieceValue(int pieceType)
    {
        switch (pieceType)
        {
            case Piece.Queen:
                return Evaluation.queenValue;
            case Piece.Rook:
                return Evaluation.rookValue;
            case Piece.Knight:
                return Evaluation.knightValue;
            case Piece.Bishop:
                return Evaluation.bishopValue;
            case Piece.Pawn:
                return Evaluation.pawnValue;
            //Could change this to prioritise king moves in endgame
            case Piece.King:
                return 1;
            default:
                return 0;
        }
    }

    public void UpdateMoveOrderTables(int depth, int fullMoveClock, int colorIndex)
    {
        Move move = movesAndPieceTypes[fullMoveClock].Item1;
        int movedPieceType = movesAndPieceTypes[fullMoveClock].Item2;

        killers[fullMoveClock] = move;

        int bonus = HISTORY_MULTIPLE * depth - HISTORY_SUB;
        ApplyHistoryBonus(move.oldIndex, move.newIndex, bonus, colorIndex);

        if(fullMoveClock > 0)
        {
            ApplyContHistBonus(movesAndPieceTypes[fullMoveClock - 1].Item1, movesAndPieceTypes[fullMoveClock - 1].Item2, move, movedPieceType, colorIndex, bonus);
        }
    }

    int CalculateNewScore(int score, int bonus)
    {
        int clampedBonus = Math.Clamp(bonus, -HISTORY_MAX, HISTORY_MAX);
        return score + clampedBonus - score * Math.Abs(clampedBonus) / HISTORY_MAX;
    }

    void ApplyHistoryBonus(int oldIndex, int newIndex, int bonus, int colorIndex)
    {
        history[colorIndex, oldIndex, newIndex] = CalculateNewScore(history[colorIndex, oldIndex, newIndex], bonus);
    }

    public void ApplyCaptHistBonus(int colorIndex, int newIndex, int movedPieceType, int capturedPieceType, int bonus)
    {
        captureHistory[colorIndex, newIndex, movedPieceType, capturedPieceType] = CalculateNewScore(captureHistory[colorIndex, newIndex, movedPieceType, capturedPieceType], bonus);
    }

    void ApplyContHistBonus(Move previousMove, int previousPiece, Move currentMove, int currentPiece, int colorIndex, int bonus)
    {
        int contHistIndex = FlattenConthistIndex(1 - colorIndex, previousPiece, previousMove.newIndex, colorIndex, currentPiece, currentMove.newIndex);
        continuationHistory[contHistIndex] = CalculateNewScore(continuationHistory[contHistIndex], bonus);
    }
    
    int FlattenConthistIndex(int prevColor, int prevPiece, int prevTo, int currColor, int currPiece, int currTo)
    {
        return ((((prevColor * 7 + prevPiece) * 64 + prevTo) * 2 + currColor) * 7 + currPiece) * 64 + currTo;
    }

    public void ApplyQuietPenalties(ref Span<Move> moves, int startNum, int depth, Board board)
    {
        for (int i = startNum - 1; i >= 0; i--)
        {
            if (!moves[i].isCapture())
            {
                ApplyHistoryBonus(moves[i].oldIndex, moves[i].newIndex, -(300 * depth - 250), board.currentColorIndex);
                if(board.fullMoveClock > 0)
                {
                    ApplyContHistBonus(movesAndPieceTypes[board.fullMoveClock - 1].Item1, movesAndPieceTypes[board.fullMoveClock - 1].Item2, moves[i], Piece.PieceType(board.board[moves[i].oldIndex]), board.currentColorIndex, -(300 * depth - 250));
                }
            }
        }
    }

    public void ApplyNoisyPenalties(ref Span<Move> moves, int startNum, int depth, Board board)
    {
        for (int i = startNum - 1; i >= 0; i--)
        {
            if (moves[i].isCapture())
            {
                ApplyCaptHistBonus(board.currentColorIndex, moves[i].newIndex, Piece.PieceType(board.board[moves[i].oldIndex]), Piece.PieceType(board.board[moves[i].newIndex]), -(300 * depth - 250));
            }
        }
    }
    
    public int[] ScoreCaptures(Board board, Span<Move> captures)
    {
        int[] moveScores = new int[captures.Length];
        for (int x = 0; x < captures.Length; x++)
        {
            int capturedPieceType;
            int movedPieceType;
            int score;
            Move move = captures[x];
            //en passant
            if (move.flag == 7)
            {
                capturedPieceType = Piece.Pawn;
            }
            else
            {
                capturedPieceType = Piece.PieceType(board.board[captures[x].newIndex]);
            }
            movedPieceType = Piece.PieceType(board.board[captures[x].oldIndex]);

            score = MVV_LVA[(movedPieceType * 7) + capturedPieceType];
            moveScores[x] = score;
        }

        return moveScores;
    }
}
