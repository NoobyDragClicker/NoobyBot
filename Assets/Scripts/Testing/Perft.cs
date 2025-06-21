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
using UnityEditor.MPE;


public class Perft : MonoBehaviour
{
    MoveGenerator moveGenerator;

    // Timers
    Stopwatch moveGenTimer = new Stopwatch();

    const string depth6File = "./Assets/Scripts/Testing/depth6only.txt";


    Dictionary<string, ulong> fenAndExpectedResult = new Dictionary<string, ulong>();
    List<String> failedFenPositions = new List<string>();
    List<String> failedQuiescence = new List<string>();
    [SerializeField]
    int numPassed;
    int numTotal;
    ulong endNodesSearched;
    bool hasQuiescencePassed = true;

    public void StartSearchDivide(string startString, int maxDepth)
    {
        moveGenerator = new MoveGenerator();
        try
        {
            Task.Factory.StartNew(() => SearchDivide(maxDepth, maxDepth, new Board(startString, moveGenerator)), TaskCreationOptions.LongRunning);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e.Message);
        }
    }

    public void StartSuite(int numPositions, int maxDepth, bool testQuiescence)
    {
        moveGenerator = new MoveGenerator();
        try
        {
            Task.Factory.StartNew(() => RunSuite(numPositions, maxDepth, testQuiescence), TaskCreationOptions.LongRunning);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e.Message);
        }
        UnityEngine.Debug.Log("Started suite");
    }

    void RunSuite(int numPositions, int maxDepth, bool testQuiescence)
    {
        int totalRun = 0;

        if (maxDepth > 6)
        {
            maxDepth = 6;
        }

        //Load positions and results
        GetDepthDict(numPositions, maxDepth);

        moveGenTimer.Start();
        numTotal = fenAndExpectedResult.Count;
        for (int x = 0; x < fenAndExpectedResult.Count; x++)
        {
            string fenString = fenAndExpectedResult.ElementAt(x).Key;
            ulong expected = fenAndExpectedResult.ElementAt(x).Value;

            var result = Search(maxDepth, new Board(fenString, moveGenerator), testQuiescence);

            if (result != expected) { failedFenPositions.Add(fenString); }
            else { numPassed++; }

            if (testQuiescence && !hasQuiescencePassed)
            {
                failedQuiescence.Add(fenString);
                hasQuiescencePassed = true;
            }

            totalRun++;

            UnityEngine.Debug.Log(totalRun);
        }

        moveGenTimer.Stop();
        UnityEngine.Debug.Log("Passed " + numPassed);
        UnityEngine.Debug.Log("Failed " + (numTotal - numPassed));
        UnityEngine.Debug.Log("Quiescence Failed " + failedQuiescence.Count);
        UnityEngine.Debug.Log("Total time: " + moveGenTimer.Elapsed);
        UnityEngine.Debug.Log("Total end nodes searched: " + endNodesSearched);
        UnityEngine.Debug.Log("Nodes/second: " + (float)endNodesSearched / moveGenTimer.ElapsedMilliseconds * 1000f);

        UnityEngine.Debug.Log("Failed:");
        for (int x = 0; x < failedFenPositions.Count; x++)
        {
            UnityEngine.Debug.Log(failedFenPositions[x]);
        }

        UnityEngine.Debug.Log("Failed Quiescence:");
        for (int x = 0; x < failedQuiescence.Count; x++)
        {
            UnityEngine.Debug.Log(failedQuiescence[x]);
        }

    }

    ulong Search(int depth, Board board, bool testQuiescence)
    {
        var moves = moveGenerator.GenerateLegalMoves(board, board.colorTurn);
        int numCaptures = 0;
        int expectedCaptures = 0;
        if (testQuiescence)
        {
            numCaptures = moveGenerator.GenerateLegalMoves(board, board.colorTurn, true).Count();
        }

        //Regular perft
        if (depth == 1 && !testQuiescence)
        {
            endNodesSearched += (ulong)moves.Count;
            return (ulong)moves.Count;
        }
        //For testing quiescence
        else if (depth == 1)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                if (moves[i].isCapture())
                {
                    expectedCaptures++;
                }
            }
            if (expectedCaptures != numCaptures)
            {
                hasQuiescencePassed = false;
            }
            endNodesSearched += (ulong)moves.Count;
            return (ulong)moves.Count();
        }


        ulong numLocalNodes = 0;

        for (int i = 0; i < moves.Count; i++)
        {
            if (testQuiescence && moves[i].isCapture())
            {
                expectedCaptures++;
            }

            board.Move(moves[i], true);
            ulong numNodesFromThisPosition = Search(depth - 1, board, testQuiescence);
            numLocalNodes += numNodesFromThisPosition;
            board.UndoMove(moves[i]);
        }

        if (testQuiescence && expectedCaptures != numCaptures)
        {
            hasQuiescencePassed = false;
        }

        return numLocalNodes;
    }

    //Prints the start index and how many moves stem from it
    ulong SearchDivide(int startDepth, int currentDepth, Board board)
    {
        var moves = moveGenerator.GenerateLegalMoves(board, board.colorTurn);

        if (currentDepth == 1)
        {
            return (ulong)moves.Count;
        }

        ulong numLocalNodes = 0;

        for (int i = 0; i < moves.Count; i++)
        {
            board.Move(moves[i], true);
            ulong numMovesForThisNode = SearchDivide(startDepth, currentDepth - 1, board);
            numLocalNodes += numMovesForThisNode;
            board.UndoMove(moves[i]);

            if (currentDepth == startDepth)
            {
                UnityEngine.Debug.Log(moves[i].oldIndex + " " + " " + moves[i].newIndex + " " + numMovesForThisNode);
            }
        }
        return numLocalNodes;
    }

    void GetDepthDict(int numPositions, int maxDepth)
    {

        string[] lines = File.ReadAllLines(depth6File);
        numPositions = numPositions > lines.Count() ? lines.Count() : numPositions;

        for (int x = 0; x < numPositions; x++)
        {
            string[] info = lines[x].Split(";");
            ulong expectedResult = ulong.Parse(info[maxDepth].Replace($"D{maxDepth} ", ""));
            fenAndExpectedResult.Add(info[0], expectedResult);
        }
    }

}
