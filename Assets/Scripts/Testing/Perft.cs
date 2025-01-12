using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using UnityEngine;
using System.Threading.Tasks;


public class Perft : MonoBehaviour
{   
    //Passed position 5 and 6 so far
    //Failed all others thus far

    //BUG NO1: Pawn taking en passant after a pawn has blocked its pin, exposing the king - use position 3 to test
    //BUG NO2: castling problem? not sure, must rerun after fixing en passant
    MoveGenerator moveGenerator;
	// Timers
	System.Diagnostics.Stopwatch moveGenTimer = new Stopwatch();

    string[] testFens = {
        "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",  //Starting pos
        "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -", //Pos 2
        "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - ", //Pos 3
        "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq -", //Pos 4
        "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ -", //Pos 5
        "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - -" //Pos 6
    };
    int[] pos1Expected = {20, 400, 8,902, 197,281, 4,865,609, 119,060,324};
    int[] pos2Expected = {48, 2039, 97862, 4085603, 193690690}; //Max length: five
    int[] pos3Expected = {14, 191, 2812, 43238, 674624, 11030083};
    int[] pos4Expected = {6, 264, 9467, 422333, 15833292, 706045033};
    int[] pos5Expected = {44, 1486, 62379, 2103487, 89941194}; //Max length: five
    int[] pos6Expected = {46, 2,079, 89890, 3894594, 164075551, 6,923,051,137};


    /*public const string startingFEN = "";
    int captures;
    int ep;
    int castles;
    int promotions;*/
    public bool hasSuiteFinished;
    public bool test1Passed;
    public bool test2Passed;
    public bool test3Passed;
    public bool test4Passed;
    public bool test5Passed;
    public bool test6Passed;


    void Start(){
        test1Passed = true;
        test2Passed = true;
        test3Passed = true;
        test4Passed = true;
        test5Passed = true;
        test6Passed = true;
        moveGenerator = new MoveGenerator();
        hasSuiteFinished = false;

        moveGenTimer.Start();
        Task.Factory.StartNew (() => RunSuite(5), TaskCreationOptions.LongRunning);
    }

    public void Update(){
        if(hasSuiteFinished){
            hasSuiteFinished = false;
            moveGenTimer.Stop();
            UnityEngine.Debug.Log("Total time: " + moveGenTimer.Elapsed);
            UnityEngine.Debug.Log("Test 1 passed " + test1Passed);
            UnityEngine.Debug.Log("Test 2 passed " + test2Passed);
            UnityEngine.Debug.Log("Test 3 passed " + test3Passed);
            UnityEngine.Debug.Log("Test 4 passed " + test4Passed);
            UnityEngine.Debug.Log("Test 5 passed " + test5Passed);
            UnityEngine.Debug.Log("Test 6 passed " + test6Passed);
        }
    }



    void RunSuite(int maxDepth){
        bool hasFailed = false;
        //Test 1
        for(int depth = 1; depth <= maxDepth; depth ++){
            var result = Search(depth, new Board(testFens[0], moveGenerator));
            if(result != pos1Expected[depth-1]){ hasFailed = true;}
        }
        if(hasFailed){test1Passed = false;}
        hasFailed = false;
        
        //Test 2
        for(int depth = 1; depth < maxDepth; depth ++){
            var result = Search(depth, new Board(testFens[1], moveGenerator));
            if(result != pos2Expected[depth-1]){ hasFailed = true;}
        }
        if(hasFailed){test2Passed = false;}
        hasFailed = false;
        
        //Test 3
        for(int depth = 1; depth < maxDepth; depth ++){
            var result = Search(depth, new Board(testFens[2], moveGenerator));
            if(result != pos3Expected[depth-1]){ hasFailed = true;}
        }
        if(hasFailed){test3Passed = false;}
        hasFailed = false;

        //Test 4
        for(int depth = 1; depth < maxDepth; depth ++){
            var result = Search(depth, new Board(testFens[3], moveGenerator));
            if(result != pos4Expected[depth-1]){ hasFailed = true;}
        }
        if(hasFailed){test4Passed = false;}
        hasFailed = false;

        //Test 5
        for(int depth = 1; depth < maxDepth; depth ++){
            var result = Search(depth, new Board(testFens[4], moveGenerator));
            if(result != pos5Expected[depth-1]){ hasFailed = true;}
        }
        if(hasFailed){test5Passed = false;}
        hasFailed = false;

        //Test 6
        for(int depth = 1; depth < maxDepth; depth ++){
            var result = Search(depth, new Board(testFens[5], moveGenerator));
            if(result != pos6Expected[depth-1]){ hasFailed = true;}
        }
        if(hasFailed){test6Passed = false;}
        hasSuiteFinished = true;
        return;
    }


    int Search (int depth, Board board) {
		var moves = moveGenerator.GenerateLegalMoves(board, board.colorTurn);
		if (depth == 1) {
			return moves.Count;
		}

		int numLocalNodes = 0;

		for (int i = 0; i < moves.Count; i++) {
			board.Move(moves[i], true);
			int numNodesFromThisPosition = Search (depth - 1, board);
			numLocalNodes += numNodesFromThisPosition;
			board.UndoMove (moves[i]);
		}
		return numLocalNodes;
	}
}
