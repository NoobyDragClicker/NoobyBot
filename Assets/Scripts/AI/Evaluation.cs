using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Evaluation
{
    
    int colorTurn;
    const int pawnValue = 100;
    const int knightValue = 300;
    const int bishopValue = 310;
    const int rookValue = 500;
    const int queenValue = 900;

    int playerTurnMultiplier;
    public System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    public int EvaluatePosition(Board board){
        colorTurn = board.colorTurn;
        playerTurnMultiplier = (colorTurn == Piece.White) ? 1 : -1;
        return CountMaterial(board);

    }

    int CountMaterial(Board board){
        stopwatch.Start();
        int materialCount = 0;
        List<int> whitePawnIndex = new List<int>();
        List<int> blackPawnIndex = new List<int>();
        List<int> whiteKnightIndex = new List<int>();
        List<int> blackKnightIndex = new List<int>();
        List<int> whiteBishopIndex = new List<int>();
        List<int> blackBishopIndex = new List<int>();
        List<int> whiteRookIndex = new List<int>();
        List<int> blackRookIndex = new List<int>();
        List<int> whiteQueenIndex = new List<int>();
        List<int> blackQueenIndex = new List<int>();
        int blackKingIndex;
        int whiteKingIndex;


        for(int x = 0; x< 64; x++){
            int pieceType = Piece.PieceType(board.board[x]);
            int pieceColor = Piece.Color(board.board[x]);
            if(board.board[x] != 0){
                switch (pieceType){
                    case Piece.Pawn: if(pieceColor == Piece.White){whitePawnIndex.Add(x);} else{blackPawnIndex.Add(x);} break;
                    case Piece.Knight: if(pieceColor == Piece.White){whiteKnightIndex.Add(x);} else{blackKnightIndex.Add(x);} break;
                    case Piece.Bishop: if(pieceColor == Piece.White){whiteBishopIndex.Add(x);} else{blackBishopIndex.Add(x);} break;
                    case Piece.Rook: if(pieceColor == Piece.White){whiteRookIndex.Add(x);} else{blackRookIndex.Add(x);} break;
                    case Piece.Queen: if(pieceColor == Piece.White){whiteQueenIndex.Add(x);} else{blackQueenIndex.Add(x);} break;
                    case Piece.King: if(pieceColor == Piece.White){whiteKingIndex = x;} else{blackKingIndex = x;} break;
                }
            }
        }
        materialCount += (whitePawnIndex.Count * pawnValue) - (blackPawnIndex.Count * pawnValue);
        materialCount += (whiteBishopIndex.Count * bishopValue) - (blackBishopIndex.Count * bishopValue);
        materialCount += (whiteKnightIndex.Count * knightValue) - (blackKnightIndex.Count * knightValue);
        materialCount += (whiteRookIndex.Count * rookValue) - (whiteRookIndex.Count * rookValue);
        materialCount += (whiteQueenIndex.Count * queenValue) - (blackQueenIndex.Count * queenValue);
        stopwatch.Stop();

        return materialCount * playerTurnMultiplier;
    }
}
