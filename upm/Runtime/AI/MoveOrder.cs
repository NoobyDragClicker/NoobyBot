using System;
using System.Collections.Generic;

public class MoveOrder
{
    const int million = 1000000;
    int[] moveScores;
    public void OrderMoves(Board board, List<Move> moves, Move firstMove, Move[,] killerMoves, int[,] history, AISettings aiSettings)
    {
        moveScores = new int[moves.Count];

        for (int x = 0; x < moves.Count; x++)
        {
            Move move = moves[x];
            int score = 0;
            if (firstMove != null && move.GetIntValue() == firstMove.GetIntValue())
            {
                score = 8 * million;
            }
            else if (killerMoves[board.plyFromStart, 0] != null && move.GetIntValue() == killerMoves[board.plyFromStart, 0].GetIntValue())
            {
                score = million + 3;
            }
            else if (killerMoves[board.plyFromStart, 1] != null && move.GetIntValue() == killerMoves[board.plyFromStart, 1].GetIntValue())
            {
                score = million + 2;
            }
            else if (killerMoves[board.plyFromStart, 2] != null && move.GetIntValue() == killerMoves[board.plyFromStart, 2].GetIntValue())
            {
                score = million + 1;
            }
            else if (move.isCapture())
            {
                int movedPieceValue;
                int capturedPieceValue;
                //en passant
                if (move.flag == 7)
                {
                    if (Piece.IsColour(board.board[moves[x].oldIndex], Piece.White))
                    {
                        capturedPieceValue = GetPieceValue(Piece.PieceType(board.board[moves[x].newIndex + 8]));
                    }
                    else
                    {
                        capturedPieceValue = GetPieceValue(Piece.PieceType(board.board[moves[x].newIndex - 8]));
                    }
                }
                else
                {
                    capturedPieceValue = GetPieceValue(Piece.PieceType(board.board[moves[x].newIndex]));
                }
                movedPieceValue = GetPieceValue(Piece.PieceType(board.board[moves[x].oldIndex]));

                //Basically MVV LVA, * 10 to give more space for killers 
                score = million + ((capturedPieceValue - movedPieceValue) * 10);
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

        Sort(moves);
        Array.Clear(moveScores, 0, moveScores.Length);
    }
    void Sort(List<Move> moves)
    {
        for (int i = 0; i < moves.Count - 1; i++)
        {
            for (int j = i + 1; j > 0; j--)
            {
                int swapIndex = j - 1;
                if (moveScores[swapIndex] < moveScores[j])
                {
                    (moves[j], moves[swapIndex]) = (moves[swapIndex], moves[j]);
                    (moveScores[j], moveScores[swapIndex]) = (moveScores[swapIndex], moveScores[j]);
                }
            }
        }
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

    public void OrderCaptures(Board board, List<Move> captures)
    {
        moveScores = new int[captures.Count];
        for (int x = 0; x < captures.Count; x++)
        {
            int capturedPieceValue;
            int movedPieceValue;
            int score;
            Move move = captures[x];
            //en passant
            if (move.flag == 7)
            {
                if (Piece.IsColour(board.board[captures[x].oldIndex], Piece.White))
                {
                    capturedPieceValue = GetPieceValue(Piece.PieceType(board.board[captures[x].newIndex + 8]));
                }
                else
                {
                    capturedPieceValue = GetPieceValue(Piece.PieceType(board.board[captures[x].newIndex - 8]));
                }
            }
            else
            {
                capturedPieceValue = GetPieceValue(Piece.PieceType(board.board[captures[x].newIndex]));
            }
            movedPieceValue = GetPieceValue(Piece.PieceType(board.board[captures[x].oldIndex]));

            //Basically MVV LVA, *10 to give more space for killers 
            score = million + ((capturedPieceValue - movedPieceValue) * 10);
            moveScores[x] = score;
        }

        Sort(captures);
    }
}
