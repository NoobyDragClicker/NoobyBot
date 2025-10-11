using System;
using System.Collections.Generic;

public class MoveOrder
{
    const int million = 1000000;
    static int[] MVV_LVA = {
        0, 0, 0, 0, 0, 0, 0, //None
        0, 6, 12, 18, 24, 30, 100 , //Pawn
        0, 5, 11, 17, 23, 29, 100, //Knight
        0, 4, 10, 16, 22, 28, 100 , //Bishop
        0, 3, 9, 15, 21, 27, 100, //Rook
        0, 2, 8, 14, 20, 26, 100 , //Queen
        0, 1, 7, 13, 19, 25, 100   //King
     };
    public int[] ScoreMoves(Board board, Span<Move> moves, Move firstMove, Move[,] killerMoves, int[,] history, AISettings aiSettings)
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
            else if (!killerMoves[board.fullMoveClock, 0].isNull() && move.GetIntValue() == killerMoves[board.fullMoveClock, 0].GetIntValue())
            {
                score = million + 3;
            }
            else if (!killerMoves[board.fullMoveClock, 1].isNull() && move.GetIntValue() == killerMoves[board.fullMoveClock, 1].GetIntValue())
            {
                score = million + 2;
            }
            else if (!killerMoves[board.fullMoveClock, 2].isNull() && move.GetIntValue() == killerMoves[board.fullMoveClock, 2].GetIntValue())
            {
                score = million + 1;
            }
            else if (move.isCapture())
            {
                int movedPieceType;
                int capturedPieceType;
                //en passant
                if (move.flag == 7)
                {
                    capturedPieceType = Piece.Pawn;
                }
                else
                {
                    capturedPieceType = Piece.PieceType(board.board[moves[x].newIndex]);
                }
                movedPieceType = Piece.PieceType(board.board[moves[x].oldIndex]);

                //Basically MVV LVA, * 10 to give more space for killers 
                score = million + 10 + MVV_LVA[(movedPieceType * 7) + capturedPieceType];
            }
            else if (move.isPromotion())
            {
                score = million + GetPieceValue(move.PromotedPieceType()) * 10;
            }
            else if (history[move.oldIndex, move.newIndex] != 0)
            {
                score = history[move.oldIndex, move.newIndex] + 100000;
            }
            else
            {
                int attackedSquaresIndex = (Piece.Color(board.board[moves[x].oldIndex]) == Piece.White) ? Board.BlackIndex : Board.WhiteIndex;
                //Penalty for moving to attacked square
                if (BitboardHelper.ContainsSquare(board.attackedSquares[attackedSquaresIndex], move.newIndex))
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

    public void GetNextBestMove(int[] moveScores, Span<Move> moves, int currentMoveIndex)
    {
        //Take the index the search is currently at
        int highest = currentMoveIndex;
        for (int i = highest + 1; i < moveScores.Length; i++)
        {
            //Find the next highest score
            if (moveScores[i] > moveScores[highest]) { highest = i; }
        }

        //Swap the next highest move into the spot that is about to be searched, hoping for a quick beta cutoff
        Move tempMove = moves[highest];
        int tempScore = moveScores[highest];
        moves[highest] = moves[currentMoveIndex];
        moveScores[highest] = moveScores[currentMoveIndex];
        moves[currentMoveIndex] = tempMove;
        moveScores[currentMoveIndex] = tempScore;
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
