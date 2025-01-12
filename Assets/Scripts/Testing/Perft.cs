using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perft : MonoBehaviour
{   
    //Passed position 5 and 6 so far
    //Failed all others thus far

    //BUG NO1: Pawn taking en passant after a pawn has blocked its pin, exposing the king - use position 3 to test
    //BUG NO2: castling problem? not sure, must rerun after fixing en passant
    	MoveGenerator moveGenerator;
		Board board;
		// Timers
		System.Diagnostics.Stopwatch makeMoveTimer;
		System.Diagnostics.Stopwatch unmakeMoveTimer;
		System.Diagnostics.Stopwatch moveGenTimer;
        public const string startingFEN = "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - -";
        int captures;
        int ep;
        int castles;
        int promotions;
    void Start(){
        moveGenerator = new MoveGenerator();
        board = new Board(startingFEN, moveGenerator);
        Debug.Log(Search(5));
        Debug.Log("Captures: " + captures);
        Debug.Log("EP: " + ep);
        Debug.Log("Castles: " + castles);
        Debug.Log("Promotions: " + promotions);

    }
    int Search (int depth) {
		var moves = moveGenerator.GenerateLegalMoves(board, board.colorTurn);
        for (int i = 0; i < moves.Count; i++) {
			if(moves[i].isCapture()){captures ++;}
            if(moves[i].flag == 7){ep++;}
            if(moves[i].flag == 5){castles++;}
            if(moves[i].isPromotion()){promotions++;}

		}
		if (depth == 1) {
			return moves.Count;
		}

		int numLocalNodes = 0;

		for (int i = 0; i < moves.Count; i++) {
			board.Move(moves[i], true);
			int numNodesFromThisPosition = Search (depth - 1);
			numLocalNodes += numNodesFromThisPosition;
			board.UndoMove (moves[i]);
		}
		return numLocalNodes;
	}
}
