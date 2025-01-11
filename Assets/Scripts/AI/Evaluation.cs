using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Evaluation
{
    
    int colorTurn;
    public const int pawnValue = 100;
    public const int knightValue = 300;
    public const int bishopValue = 310;
    public const int rookValue = 500;
    public const int queenValue = 900;

    int[] pawnPieceTable = {
		0,  0,  0,  0,  0,  0,  0,  0,
		50, 50, 50, 50, 50, 50, 50, 50,
		10, 10, 20, 30, 30, 20, 10, 10,
		5,  5, 10, 25, 25, 10,  5,  5,
		0,  0,  0, 20, 20,  0,  0,  0,
		5, -5,-10,  0,  0,-10, -5,  5,
		5, 10, 10,-20,-20, 10, 10,  5,
		0,  0,  0,  0,  0,  0,  0,  0
    };
    int[] kingPieceTable_mg = {
        -40, -40, -40, -40, -40, -40, -40, -40,
        -40, -40, -40, -40, -40, -40, -40, -40,
        -40, -40, -40, -40, -40, -40, -40, -40,
        -40, -40, -40, -40, -40, -40, -40, -40,
        -30, -30, -40, -45, -45, -45, -30, -30,
        -15, -20, -25, -30, -30, -25, -20, -15,
        0, 0, -5, -10, -10, 10, 0, 0,
        5, 15, -10, -20, -20, -10, 20, 30,
    };
    int[] kingPieceTable_eg = {
        -40, -40, -40, -40, -40, -40, -40, -40,
        -40, -40, -40, -40, -40, -40, -40, -40,
        -40, -40, -40, -40, -40, -40, -40, -40,
        -40, -40, -40, -40, -40, -40, -40, -40,
        -30, -30, -40, -45, -45, -45, -30, -30,
        -15, -20, -25, -30, -30, -25, -20, -15,
        0, 0, -5, -10, -10, 10, 0, 0,
        5, 15, -10, -20, -20, -10, 20, 30,
    };
    int[] knightPieceTable = {
		-50,-40,-30,-30,-30,-30,-40,-50,
		-40,-20,  0,  0,  0,  0,-20,-40,
		-30,  0, 10, 15, 15, 10,  0,-30,
		-30,  5, 15, 20, 20, 15,  5,-30,
		-30,  0, 15, 20, 20, 15,  0,-30,
		-30,  5, 10, 15, 15, 10,  5,-30,
		-40,-20,  0,  5,  5,  0,-20,-40,
		-50,-40,-30,-30,-30,-30,-40,-50,
	};

    int[] bishopPieceTable = {
		-20,-10,-10,-10,-10,-10,-10,-20,
		-10,  0,  0,  0,  0,  0,  0,-10,
		-10,  0,  5, 10, 10,  5,  0,-10,
		-10,  5,  5, 10, 10,  5,  5,-10,
		-10,  0, 10, 10, 10, 10,  0,-10,
		-10, 10, 10, 10, 10, 10, 10,-10,
		-10,  5,  0,  0,  0,  0,  5,-10,
		-20,-10,-10,-10,-10,-10,-10,-20,
	};

	int[] rookPieceTable = {
		0,  0,  0,  0,  0,  0,  0,  0,
		5, 10, 10, 10, 10, 10, 10,  5,
		-5,  0,  0,  0,  0,  0,  0, -5,
		-5,  0,  0,  0,  0,  0,  0, -5,
		-5,  0,  0,  0,  0,  0,  0, -5,
		-5,  0,  0,  0,  0,  0,  0, -5,
		-5,  0,  0,  0,  0,  0,  0, -5,
		0,  0,  0,  5,  5,  0,  0,  0
    };

	int[] queenPieceTable = {
		-20,-10,-10, -5, -5,-10,-10,-20,
		-10,  0,  0,  0,  0,  0,  0,-10,
		-10,  0,  5,  5,  5,  5,  0,-10,
		-5,  0,  5,  5,  5,  5,  0, -5,
		0,  0,  5,  5,  5,  5,  0, -5,
		-10,  5,  5,  5,  5,  5,  0,-10,
		-10,  0,  5,  0,  0,  0,  0,-10,
		-20,-10,-10, -5, -5,-10,-10,-20
	};



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
        //Loops through each index on the board
        for(int x = 0; x< 64; x++){
            int pieceType = Piece.PieceType(board.board[x]);
            int pieceColor = Piece.Color(board.board[x]);
            if(board.board[x] != 0){
                switch (pieceType){
                    case Piece.Pawn: if(pieceColor == Piece.White){materialCount += pawnValue + pawnPieceTable[x];} else{materialCount -= pawnValue + pawnPieceTable[63-x];} break;
                    case Piece.Knight: if(pieceColor == Piece.White){materialCount += knightValue + knightPieceTable[x];} else{materialCount -= knightValue + knightPieceTable[63-x];} break;
                    case Piece.Bishop: if(pieceColor == Piece.White){materialCount += bishopValue + bishopPieceTable[x];} else{materialCount -= bishopValue + bishopPieceTable[63-x];} break;
                    case Piece.Rook: if(pieceColor == Piece.White){materialCount += rookValue + rookPieceTable[x];} else{materialCount -= rookValue + rookPieceTable[63-x];} break;
                    case Piece.Queen: if(pieceColor == Piece.White){materialCount += queenValue + queenPieceTable[x];} else{materialCount -= queenValue + queenPieceTable[63-x];} break;
                    case Piece.King: if(pieceColor == Piece.White){materialCount +=kingPieceTable_mg[x];} else{materialCount -=kingPieceTable_mg[63-x];} break;
                }
            }
        }

        stopwatch.Stop();
        return materialCount * playerTurnMultiplier;
    }


}
