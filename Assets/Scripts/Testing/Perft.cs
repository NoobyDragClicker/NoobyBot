using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;


public class Perft : MonoBehaviour
{   
    //BUG NO1: Pawn taking en passant after a pawn has blocked its pin, exposing the king - use position 3 to test
    //BUG NO2: castling problem? not sure, must rerun after fixing en passant
    MoveGenerator moveGenerator;
	// Timers
	System.Diagnostics.Stopwatch moveGenTimer = new Stopwatch();

    string[] testFens = {
        "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",  //Starting pos
        "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -", //Pos 2
        "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -", //Pos 3
        "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq -", //Pos 4
        "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8", //Pos 5
        "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - -" //Pos 6
    };
    int[] pos1Expected = {20, 400, 8902, 197281, 4865609, 119060324};
    int[] pos2Expected = {48, 2039, 97862, 4085603, 193690690}; //Max length: five
    int[] pos3Expected = {14, 191, 2812, 43238, 674624, 11030083};
    int[] pos4Expected = {6, 264, 9467, 422333, 15833292, 706045033};
    int[] pos5Expected = {44, 1486, 62379, 2103487, 89941194}; //Max length: five
    long[] pos6Expected = {46, 2079, 89890, 3894594, 164075551, 6923051137};


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

    int captures;
    int ep;
    int castles;
    int promotions;
    int checks;
    int doubleChecks;
    int checkmates;


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

    public void RunExtraInfo(int maxDepth, string testFen){
        UnityEngine.Debug.Log(Search(maxDepth, new Board(testFen, moveGenerator)));
        UnityEngine.Debug.Log("Captures: " +  captures);
        UnityEngine.Debug.Log("EP: " +  ep);
        UnityEngine.Debug.Log("Castles: " +  castles);
        UnityEngine.Debug.Log("promotions: " +  promotions);
        UnityEngine.Debug.Log("Checks: " +  checks);
        UnityEngine.Debug.Log("Double checks: " +  doubleChecks);
        UnityEngine.Debug.Log("Checkmates: " + checkmates);
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
        for(int depth = 1; depth <= maxDepth; depth ++){
            var result = Search(depth, new Board(testFens[1], moveGenerator));
            if(result != pos2Expected[depth-1]){ hasFailed = true;}
        }
        if(hasFailed){test2Passed = false;}
        hasFailed = false;
        
        //Test 3
        for(int depth = 1; depth <= maxDepth; depth ++){
            var result = Search(depth, new Board(testFens[2], moveGenerator));
            if(result != pos3Expected[depth-1]){ hasFailed = true;}
        }
        if(hasFailed){test3Passed = false;}
        hasFailed = false;

        //Test 4
        for(int depth = 1; depth <= maxDepth; depth ++){
            var result = Search(depth, new Board(testFens[3], moveGenerator));
            if(result != pos4Expected[depth-1]){ hasFailed = true;}
        }
        if(hasFailed){test4Passed = false;}
        hasFailed = false;

        //Test 5
        for(int depth = 1; depth <= maxDepth; depth ++){
            var result = Search(depth, new Board(testFens[4], moveGenerator));
            if(result != pos5Expected[depth-1]){ hasFailed = true;}
        }
        if(hasFailed){test5Passed = false;}
        hasFailed = false;

        //Test 6
        for(int depth = 1; depth <= maxDepth; depth ++){
            var result = Search(depth, new Board(testFens[5], moveGenerator));
            if(result != pos6Expected[depth-1]){ hasFailed = true;}
        }
        if(hasFailed){test6Passed = false;}
        hasSuiteFinished = true;
        return;
    }

    int Search (int depth, Board board) {
		var moves = moveGenerator.GenerateLegalMoves(board, board.colorTurn);
        /*if(moves.Count == 0){
            if(board.isCurrentPlayerInCheck){
                checkmates++;
                return 1;
            }
        }*/
        if (depth == 1) {
			return moves.Count;
		}
        /*for (int i = 0; i < moves.Count; i++) {
			if(moves[i].isCapture()){captures ++;}
            if(moves[i].flag == 7){ep++;}
            if(moves[i].flag == 5){castles++;}
            if(moves[i].isPromotion()){promotions++;}
		}*/
		int numLocalNodes = 0;

		for (int i = 0; i < moves.Count; i++) {
			board.Move(moves[i], true);
			int numNodesFromThisPosition = Search (depth - 1, board);
			numLocalNodes += numNodesFromThisPosition;
            /*if(board.isCurrentPlayerInCheck){checks++;}
            if(board.isCurrentPlayerInDoubleCheck){doubleChecks++;}*/
			board.UndoMove (moves[i]);
		}
		return numLocalNodes;
	}

    //Prints the start index and how many moves stem from it
    int SearchDivide (int startDepth, int currentDepth, Board board) {
        var moves = moveGenerator.GenerateLegalMoves(board, board.colorTurn);

		if (currentDepth == 1) {
			return moves.Count;
		}
		int numLocalNodes = 0;
		for (int i = 0; i < moves.Count; i++) {
			board.Move(moves[i], true);
			int numMovesForThisNode = SearchDivide(startDepth, currentDepth - 1, board);
			numLocalNodes += numMovesForThisNode;
			board.UndoMove (moves[i]);

			if (currentDepth == startDepth) {
			    UnityEngine.Debug.Log(moves[i].oldIndex + " " + " " + moves[i].newIndex + " " + numMovesForThisNode);
			}
		}
		return numLocalNodes;
    }
}
