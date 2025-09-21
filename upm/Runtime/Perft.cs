using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

public class Perft
{

    // Timers
    Stopwatch moveGenTimer = new Stopwatch();

    const string depth6File = Engine.chessRoot+"/depth6only.txt";

    Dictionary<string, ulong> fenAndExpectedResult = new Dictionary<string, ulong>();
    List<String> failedFenPositions = new List<string>();
    List<String> failedQuiescence = new List<string>();
    int numPassed;
    ulong numTotal;
    ulong endNodesSearched;
    bool hasQuiescencePassed = true;
    SearchLogger logger;

    public Perft(SearchLogger logger)
    {
        this.logger = logger;
    }

    public void StartSearchDivide(Board board, int maxDepth)
    {
        try
        {
            Task.Factory.StartNew(() => SearchDivide(maxDepth, maxDepth, board), TaskCreationOptions.LongRunning);
        }
        catch (Exception e)
        {
            logger.AddToLog(e.Message, SearchLogger.LoggingLevel.Deadly);
            Console.WriteLine(e);
        }
    }

    public void StartSuite(int numPositions, int maxDepth, bool testQuiescence)
    {
        try
        {
            Task.Factory.StartNew(() => RunSuite(numPositions, maxDepth, testQuiescence), TaskCreationOptions.LongRunning);
        }
        catch (Exception e)
        {
            logger.AddToLog(e.Message, SearchLogger.LoggingLevel.Deadly);
            Console.WriteLine(e);
        }
        Console.WriteLine($"Started suite, depth {maxDepth}");
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
        numTotal = (ulong)fenAndExpectedResult.Count;
        for (int x = 0; x < fenAndExpectedResult.Count; x++)
        {
            string fenString = fenAndExpectedResult.ElementAt(x).Key;
            ulong expected = fenAndExpectedResult.ElementAt(x).Value;
            Board board = new Board();
            board.setPosition(fenString, logger);
            ulong result = 0;
            try
            {
                result = Search(maxDepth, board, testQuiescence);
            }
            catch (Exception e)
            {
                logger.AddToLog(e.Message, SearchLogger.LoggingLevel.Deadly);
                Console.WriteLine(e);
            }

            if (result != expected) { failedFenPositions.Add(fenString); }
            else { numPassed++; }

            if (testQuiescence && !hasQuiescencePassed)
            {
                failedQuiescence.Add(fenString);
                hasQuiescencePassed = true;
            }

            totalRun++;

            Console.WriteLine(totalRun.ToString());
        }

        moveGenTimer.Stop();
        Console.WriteLine("Passed " + numPassed);
        Console.WriteLine("Failed " + (numTotal - (ulong)numPassed));
        Console.WriteLine("Quiescence Failed " + failedQuiescence.Count);
        Console.WriteLine("Total time: " + moveGenTimer.Elapsed);
        Console.WriteLine("Total end nodes searched: " + endNodesSearched);
        Console.WriteLine("Nodes/second: " + (float)endNodesSearched / moveGenTimer.ElapsedMilliseconds * 1000f);

        logger.AddToLog("Passed " + numPassed, SearchLogger.LoggingLevel.Info);
        logger.AddToLog("Failed " + (numTotal - (ulong)numPassed), SearchLogger.LoggingLevel.Info);
        logger.AddToLog("Quiescence Failed " + failedQuiescence.Count, SearchLogger.LoggingLevel.Info);
        logger.AddToLog("Total time: " + moveGenTimer.Elapsed, SearchLogger.LoggingLevel.Info);
        logger.AddToLog("Total end nodes searched: " + endNodesSearched, SearchLogger.LoggingLevel.Info);
        logger.AddToLog("Nodes/second: " + (float)endNodesSearched / moveGenTimer.ElapsedMilliseconds * 1000f, SearchLogger.LoggingLevel.Info);

        Console.WriteLine("Failed:");
        logger.AddToLog("Failed:", SearchLogger.LoggingLevel.Info);
        for (int x = 0; x < failedFenPositions.Count; x++)
        {
            Console.WriteLine(failedFenPositions[x]);
            logger.AddToLog(failedFenPositions[x], SearchLogger.LoggingLevel.Info);
        }

        Console.WriteLine("Failed Quiescence:");
        logger.AddToLog("Failed Quiescence:", SearchLogger.LoggingLevel.Info);
        for (int x = 0; x < failedQuiescence.Count; x++)
        {
            Console.WriteLine(failedQuiescence[x]);
            logger.AddToLog(failedQuiescence[x], SearchLogger.LoggingLevel.Info);
        }

    }

    ulong Search(int depth, Board board, bool testQuiescence)
    {
        Span<Move> moves = stackalloc Move[218];
        MoveGenerator.GenerateLegalMoves(board, ref moves, board.colorTurn);
        int numCaptures = 0;
        int expectedCaptures = 0;
        if (testQuiescence)
        {
            Span<Move> captures = stackalloc Move[218];
            numCaptures = MoveGenerator.GenerateLegalMoves(board, ref captures, board.colorTurn, true);
        }

        //Regular perft
        if (depth == 1 && !testQuiescence)
        {
            endNodesSearched += (ulong)moves.Length;
            return (ulong)moves.Length;
        }
        //For testing quiescence
        else if (depth == 1)
        {
            for (int i = 0; i < moves.Length; i++)
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
            endNodesSearched += (ulong)moves.Length;
            return (ulong)moves.Length;
        }


        ulong numLocalNodes = 0;

        for (int i = 0; i < moves.Length; i++)
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
        Span<Move> moves = stackalloc Move[218];
        MoveGenerator.GenerateLegalMoves(board, ref moves, board.colorTurn);

        if (currentDepth == 1)
        {
            return (ulong)moves.Length;
        }

        ulong numLocalNodes = 0;

        for (int i = 0; i < moves.Length; i++)
        {
            board.Move(moves[i], true);
            ulong numMovesForThisNode = SearchDivide(startDepth, currentDepth - 1, board);
            numLocalNodes += numMovesForThisNode;
            board.UndoMove(moves[i]);

            if (currentDepth == startDepth)
            {
                numTotal += numMovesForThisNode;
                logger.AddToLog(Coord.GetUCIMoveNotation(moves[i]) + " " + numMovesForThisNode, SearchLogger.LoggingLevel.Info);
                Console.WriteLine(Coord.GetUCIMoveNotation(moves[i]) + " " + numMovesForThisNode);
                if (i == moves.Length - 1)
                {
                    logger.AddToLog(numTotal.ToString(), SearchLogger.LoggingLevel.Info);
                    Console.WriteLine(numTotal.ToString());
                }
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
