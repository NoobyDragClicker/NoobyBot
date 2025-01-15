using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.Collections;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;
using System;
using System.Linq;


public class Perft : MonoBehaviour
{   
    //Last total time: 3:35(questionable)
    //BUG NO2: castling problem? not sure, must rerun after fixing en passant
    MoveGenerator moveGenerator;
    
	// Timers
	Stopwatch moveGenTimer = new Stopwatch();

    const string fileName = "./Assets/Scripts/Testing/standard.txt";


    public bool hasSuiteFinished;
    Dictionary<String, ulong> fenAndExpectedResult = new Dictionary<string, ulong>();
    List<String> failedFenPositions = new List<string>();
    int numPassed;
    int numTotal;


    void Start(){
        moveGenerator = new MoveGenerator();
        hasSuiteFinished = false;
        
        //UnityEngine.Debug.Log(SearchDivide(2, 2, new Board("k7/8/6P1/7p/8/8/K7/8 b - - 0 1", moveGenerator)));
        Task.Factory.StartNew (() => RunSuite(), TaskCreationOptions.LongRunning);
    }

    public void Update(){
        if(hasSuiteFinished){
            hasSuiteFinished = false;
            UnityEngine.Debug.Log("Passed " + numPassed);
            UnityEngine.Debug.Log("Failed " + (numTotal - numPassed));
            UnityEngine.Debug.Log("Total time: " + moveGenTimer.Elapsed);

            for(int x = 0; x< failedFenPositions.Count; x++){
                UnityEngine.Debug.Log(failedFenPositions[x]);
            }
        }
    }

    void RunSuite(){
        moveGenTimer.Start();
        GetDepthDict();
        numTotal = fenAndExpectedResult.Count;
        for(int x = 0; x < fenAndExpectedResult.Count; x++)
        {
            string fenString = fenAndExpectedResult.ElementAt(x).Key;
            ulong expected = fenAndExpectedResult.ElementAt(x).Value;
            var result = Search(5, new Board(fenString, moveGenerator));
            if(result != expected){failedFenPositions.Add(fenString);}
            else{numPassed ++;}
        }
        moveGenTimer.Stop();
        hasSuiteFinished = true;
        return;
    }

    ulong Search (int depth, Board board) {
		var moves = moveGenerator.GenerateLegalMoves(board, board.colorTurn);
        if (depth == 1) {
			return (ulong) moves.Count;
		}
		ulong numLocalNodes = 0;

		for (int i = 0; i < moves.Count; i++) {
			board.Move(moves[i], true);
			ulong numNodesFromThisPosition = Search (depth - 1, board);
			numLocalNodes += numNodesFromThisPosition;
			board.UndoMove (moves[i]);
		}
		return numLocalNodes;
	}

    //Prints the start index and how many moves stem from it
    ulong SearchDivide (int startDepth, int currentDepth, Board board) {
        var moves = moveGenerator.GenerateLegalMoves(board, board.colorTurn);

		if (currentDepth == 1) {
			return (ulong) moves.Count;
		}

		ulong numLocalNodes = 0;

		for (int i = 0; i < moves.Count; i++) {
			board.Move(moves[i], true);
			ulong numMovesForThisNode = SearchDivide(startDepth, currentDepth - 1, board);
			numLocalNodes += numMovesForThisNode;
			board.UndoMove (moves[i]);

			if (currentDepth == startDepth) {
			    UnityEngine.Debug.Log(moves[i].oldIndex + " " + " " + moves[i].newIndex + " " + numMovesForThisNode);
			}
		}
		return numLocalNodes;
    }

    void GetDepthDict(){
        string[] lines = File.ReadAllLines(fileName);
        for(int x = 0; x < lines.Length; x++){
            string[] info = lines[x].Split(";");
            ulong depth = ulong.Parse(info[5].Replace("D5 ", ""));
            fenAndExpectedResult.Add(info[0], depth);
        }
    }
}
