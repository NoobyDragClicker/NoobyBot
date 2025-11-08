using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.ComponentModel;

public class Perft
{

    // Timers
    Stopwatch moveGenTimer = new Stopwatch();
    Dictionary<string, ulong> fenAndExpectedResult = new Dictionary<string, ulong>();
    List<String> failedFenPositions = new List<string>();
    List<String> failedQuiescence = new List<string>();
    int numPassed;
    ulong numTotal;
    ulong endNodesSearched;
    bool hasQuiescencePassed = true;
    SearchLogger logger;
    Stopwatch genTimer = new Stopwatch();
    Stopwatch make = new Stopwatch();
    Stopwatch unmake = new Stopwatch();

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
                result = Search(maxDepth, board, testQuiescence, false);
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
        Console.WriteLine("Total movegen time: " + genTimer.Elapsed);
        Console.WriteLine("Total make time: " + make.Elapsed);
        Console.WriteLine("Total unmake time: " + unmake.Elapsed);

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

    ulong Search(int depth, Board board, bool testQuiescence, bool batch)
    {
        if (depth == 0 && !batch)
        {
            endNodesSearched += 1;
            return 1;
        }
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
        if (depth == 1 && !testQuiescence && batch)
        {
            endNodesSearched += (ulong)moves.Length;
            return (ulong)moves.Length;
        }
        //For testing quiescence
        else if (depth == 1 && batch)
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
            //make.Start();
            board.Move(moves[i], true);
            //make.Stop();

            ulong numNodesFromThisPosition = Search(depth - 1, board, testQuiescence, batch);
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

        string[] lines = testPositions;
        numPositions = numPositions > lines.Count() ? lines.Count() : numPositions;

        for (int x = 0; x < numPositions; x++)
        {
            string[] info = lines[x].Split(";");
            ulong expectedResult = ulong.Parse(info[maxDepth].Replace($"D{maxDepth} ", ""));
            fenAndExpectedResult.Add(info[0], expectedResult);
        }
    }

    public static string[] testPositions = {
        "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 ;D1 20 ;D2 400 ;D3 8902 ;D4 197281 ;D5 4865609 ;D6 119060324",
        "4k3/8/8/8/8/8/8/4K2R w K - 0 1 ;D1 15 ;D2 66 ;D3 1197 ;D4 7059 ;D5 133987 ;D6 764643",
        "4k3/8/8/8/8/8/8/R3K3 w Q - 0 1 ;D1 16 ;D2 71 ;D3 1287 ;D4 7626 ;D5 145232 ;D6 846648",
        "4k2r/8/8/8/8/8/8/4K3 w k - 0 1 ;D1 5 ;D2 75 ;D3 459 ;D4 8290 ;D5 47635 ;D6 899442",
        "r3k3/8/8/8/8/8/8/4K3 w q - 0 1 ;D1 5 ;D2 80 ;D3 493 ;D4 8897 ;D5 52710 ;D6 1001523",
        "4k3/8/8/8/8/8/8/R3K2R w KQ - 0 1 ;D1 26 ;D2 112 ;D3 3189 ;D4 17945 ;D5 532933 ;D6 2788982",
        "r3k2r/8/8/8/8/8/8/4K3 w kq - 0 1 ;D1 5 ;D2 130 ;D3 782 ;D4 22180 ;D5 118882 ;D6 3517770",
        "8/8/8/8/8/8/6k1/4K2R w K - 0 1 ;D1 12 ;D2 38 ;D3 564 ;D4 2219 ;D5 37735 ;D6 185867",
        "8/8/8/8/8/8/1k6/R3K3 w Q - 0 1 ;D1 15 ;D2 65 ;D3 1018 ;D4 4573 ;D5 80619 ;D6 413018",
        "4k2r/6K1/8/8/8/8/8/8 w k - 0 1 ;D1 3 ;D2 32 ;D3 134 ;D4 2073 ;D5 10485 ;D6 179869",
        "r3k3/1K6/8/8/8/8/8/8 w q - 0 1 ;D1 4 ;D2 49 ;D3 243 ;D4 3991 ;D5 20780 ;D6 367724",
        "r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1 ;D1 26 ;D2 568 ;D3 13744 ;D4 314346 ;D5 7594526 ;D6 179862938",
        "r3k2r/8/8/8/8/8/8/1R2K2R w Kkq - 0 1 ;D1 25 ;D2 567 ;D3 14095 ;D4 328965 ;D5 8153719 ;D6 195629489",
        "r3k2r/8/8/8/8/8/8/2R1K2R w Kkq - 0 1 ;D1 25 ;D2 548 ;D3 13502 ;D4 312835 ;D5 7736373 ;D6 184411439",
        "r3k2r/8/8/8/8/8/8/R3K1R1 w Qkq - 0 1 ;D1 25 ;D2 547 ;D3 13579 ;D4 316214 ;D5 7878456 ;D6 189224276",
        "1r2k2r/8/8/8/8/8/8/R3K2R w KQk - 0 1 ;D1 26 ;D2 583 ;D3 14252 ;D4 334705 ;D5 8198901 ;D6 198328929",
        "2r1k2r/8/8/8/8/8/8/R3K2R w KQk - 0 1 ;D1 25 ;D2 560 ;D3 13592 ;D4 317324 ;D5 7710115 ;D6 185959088",
        "r3k1r1/8/8/8/8/8/8/R3K2R w KQq - 0 1 ;D1 25 ;D2 560 ;D3 13607 ;D4 320792 ;D5 7848606 ;D6 190755813",
        "4k3/8/8/8/8/8/8/4K2R b K - 0 1 ;D1 5 ;D2 75 ;D3 459 ;D4 8290 ;D5 47635 ;D6 899442",
        "4k3/8/8/8/8/8/8/R3K3 b Q - 0 1 ;D1 5 ;D2 80 ;D3 493 ;D4 8897 ;D5 52710 ;D6 1001523",
        "4k2r/8/8/8/8/8/8/4K3 b k - 0 1 ;D1 15 ;D2 66 ;D3 1197 ;D4 7059 ;D5 133987 ;D6 764643",
        "r3k3/8/8/8/8/8/8/4K3 b q - 0 1 ;D1 16 ;D2 71 ;D3 1287 ;D4 7626 ;D5 145232 ;D6 846648",
        "4k3/8/8/8/8/8/8/R3K2R b KQ - 0 1 ;D1 5 ;D2 130 ;D3 782 ;D4 22180 ;D5 118882 ;D6 3517770",
        "r3k2r/8/8/8/8/8/8/4K3 b kq - 0 1 ;D1 26 ;D2 112 ;D3 3189 ;D4 17945 ;D5 532933 ;D6 2788982",
        "8/8/8/8/8/8/6k1/4K2R b K - 0 1 ;D1 3 ;D2 32 ;D3 134 ;D4 2073 ;D5 10485 ;D6 179869",
        "8/8/8/8/8/8/1k6/R3K3 b Q - 0 1 ;D1 4 ;D2 49 ;D3 243 ;D4 3991 ;D5 20780 ;D6 367724",
        "4k2r/6K1/8/8/8/8/8/8 b k - 0 1 ;D1 12 ;D2 38 ;D3 564 ;D4 2219 ;D5 37735 ;D6 185867",
        "r3k3/1K6/8/8/8/8/8/8 b q - 0 1 ;D1 15 ;D2 65 ;D3 1018 ;D4 4573 ;D5 80619 ;D6 413018",
        "r3k2r/8/8/8/8/8/8/R3K2R b KQkq - 0 1 ;D1 26 ;D2 568 ;D3 13744 ;D4 314346 ;D5 7594526 ;D6 179862938",
        "r3k2r/8/8/8/8/8/8/1R2K2R b Kkq - 0 1 ;D1 26 ;D2 583 ;D3 14252 ;D4 334705 ;D5 8198901 ;D6 198328929",
        "r3k2r/8/8/8/8/8/8/2R1K2R b Kkq - 0 1 ;D1 25 ;D2 560 ;D3 13592 ;D4 317324 ;D5 7710115 ;D6 185959088",
        "r3k2r/8/8/8/8/8/8/R3K1R1 b Qkq - 0 1 ;D1 25 ;D2 560 ;D3 13607 ;D4 320792 ;D5 7848606 ;D6 190755813",
        "1r2k2r/8/8/8/8/8/8/R3K2R b KQk - 0 1 ;D1 25 ;D2 567 ;D3 14095 ;D4 328965 ;D5 8153719 ;D6 195629489",
        "2r1k2r/8/8/8/8/8/8/R3K2R b KQk - 0 1 ;D1 25 ;D2 548 ;D3 13502 ;D4 312835 ;D5 7736373 ;D6 184411439",
        "r3k1r1/8/8/8/8/8/8/R3K2R b KQq - 0 1 ;D1 25 ;D2 547 ;D3 13579 ;D4 316214 ;D5 7878456 ;D6 189224276",
        "8/1n4N1/2k5/8/8/5K2/1N4n1/8 w - - 0 1 ;D1 14 ;D2 195 ;D3 2760 ;D4 38675 ;D5 570726 ;D6 8107539",
        "8/1k6/8/5N2/8/4n3/8/2K5 w - - 0 1 ;D1 11 ;D2 156 ;D3 1636 ;D4 20534 ;D5 223507 ;D6 2594412",
        "8/8/4k3/3Nn3/3nN3/4K3/8/8 w - - 0 1 ;D1 19 ;D2 289 ;D3 4442 ;D4 73584 ;D5 1198299 ;D6 19870403",
        "K7/8/2n5/1n6/8/8/8/k6N w - - 0 1 ;D1 3 ;D2 51 ;D3 345 ;D4 5301 ;D5 38348 ;D6 588695",
        "k7/8/2N5/1N6/8/8/8/K6n w - - 0 1 ;D1 17 ;D2 54 ;D3 835 ;D4 5910 ;D5 92250 ;D6 688780",
        "8/1n4N1/2k5/8/8/5K2/1N4n1/8 b - - 0 1 ;D1 15 ;D2 193 ;D3 2816 ;D4 40039 ;D5 582642 ;D6 8503277",
        "8/1k6/8/5N2/8/4n3/8/2K5 b - - 0 1 ;D1 16 ;D2 180 ;D3 2290 ;D4 24640 ;D5 288141 ;D6 3147566",
        "8/8/3K4/3Nn3/3nN3/4k3/8/8 b - - 0 1 ;D1 4 ;D2 68 ;D3 1118 ;D4 16199 ;D5 281190 ;D6 4405103",
        "K7/8/2n5/1n6/8/8/8/k6N b - - 0 1 ;D1 17 ;D2 54 ;D3 835 ;D4 5910 ;D5 92250 ;D6 688780",
        "k7/8/2N5/1N6/8/8/8/K6n b - - 0 1 ;D1 3 ;D2 51 ;D3 345 ;D4 5301 ;D5 38348 ;D6 588695",
        "B6b/8/8/8/2K5/4k3/8/b6B w - - 0 1 ;D1 17 ;D2 278 ;D3 4607 ;D4 76778 ;D5 1320507 ;D6 22823890",
        "8/8/1B6/7b/7k/8/2B1b3/7K w - - 0 1 ;D1 21 ;D2 316 ;D3 5744 ;D4 93338 ;D5 1713368 ;D6 28861171",
        "k7/B7/1B6/1B6/8/8/8/K6b w - - 0 1 ;D1 21 ;D2 144 ;D3 3242 ;D4 32955 ;D5 787524 ;D6 7881673",
        "K7/b7/1b6/1b6/8/8/8/k6B w - - 0 1 ;D1 7 ;D2 143 ;D3 1416 ;D4 31787 ;D5 310862 ;D6 7382896",
        "B6b/8/8/8/2K5/5k2/8/b6B b - - 0 1 ;D1 6 ;D2 106 ;D3 1829 ;D4 31151 ;D5 530585 ;D6 9250746",
        "8/8/1B6/7b/7k/8/2B1b3/7K b - - 0 1 ;D1 17 ;D2 309 ;D3 5133 ;D4 93603 ;D5 1591064 ;D6 29027891",
        "k7/B7/1B6/1B6/8/8/8/K6b b - - 0 1 ;D1 7 ;D2 143 ;D3 1416 ;D4 31787 ;D5 310862 ;D6 7382896",
        "K7/b7/1b6/1b6/8/8/8/k6B b - - 0 1 ;D1 21 ;D2 144 ;D3 3242 ;D4 32955 ;D5 787524 ;D6 7881673",
        "7k/RR6/8/8/8/8/rr6/7K w - - 0 1 ;D1 19 ;D2 275 ;D3 5300 ;D4 104342 ;D5 2161211 ;D6 44956585",
        "R6r/8/8/2K5/5k2/8/8/r6R w - - 0 1 ;D1 36 ;D2 1027 ;D3 29215 ;D4 771461 ;D5 20506480 ;D6 525169084",
        "7k/RR6/8/8/8/8/rr6/7K b - - 0 1 ;D1 19 ;D2 275 ;D3 5300 ;D4 104342 ;D5 2161211 ;D6 44956585",
        "R6r/8/8/2K5/5k2/8/8/r6R b - - 0 1 ;D1 36 ;D2 1027 ;D3 29227 ;D4 771368 ;D5 20521342 ;D6 524966748",
        "K7/8/8/3Q4/4q3/8/8/7k w - - 0 1 ;D1 6 ;D2 35 ;D3 495 ;D4 8349 ;D5 166741 ;D6 3370175",
        "6qk/8/8/8/8/8/8/7K b - - 0 1 ;D1 22 ;D2 43 ;D3 1015 ;D4 4167 ;D5 105749 ;D6 419369",
        "6KQ/8/8/8/8/8/8/7k b - - 0 1 ;D1 2 ;D2 36 ;D3 143 ;D4 3637 ;D5 14893 ;D6 391507",
        "K7/8/8/3Q4/4q3/8/8/7k b - - 0 1 ;D1 6 ;D2 35 ;D3 495 ;D4 8349 ;D5 166741 ;D6 3370175",
        "8/8/8/8/8/K7/P7/k7 w - - 0 1 ;D1 3 ;D2 7 ;D3 43 ;D4 199 ;D5 1347 ;D6 6249",
        "8/8/8/8/8/7K/7P/7k w - - 0 1 ;D1 3 ;D2 7 ;D3 43 ;D4 199 ;D5 1347 ;D6 6249",
        "K7/p7/k7/8/8/8/8/8 w - - 0 1 ;D1 1 ;D2 3 ;D3 12 ;D4 80 ;D5 342 ;D6 2343",
        "7K/7p/7k/8/8/8/8/8 w - - 0 1 ;D1 1 ;D2 3 ;D3 12 ;D4 80 ;D5 342 ;D6 2343",
        "8/2k1p3/3pP3/3P2K1/8/8/8/8 w - - 0 1 ;D1 7 ;D2 35 ;D3 210 ;D4 1091 ;D5 7028 ;D6 34834",
        "8/8/8/8/8/K7/P7/k7 b - - 0 1 ;D1 1 ;D2 3 ;D3 12 ;D4 80 ;D5 342 ;D6 2343",
        "8/8/8/8/8/7K/7P/7k b - - 0 1 ;D1 1 ;D2 3 ;D3 12 ;D4 80 ;D5 342 ;D6 2343",
        "K7/p7/k7/8/8/8/8/8 b - - 0 1 ;D1 3 ;D2 7 ;D3 43 ;D4 199 ;D5 1347 ;D6 6249",
        "7K/7p/7k/8/8/8/8/8 b - - 0 1 ;D1 3 ;D2 7 ;D3 43 ;D4 199 ;D5 1347 ;D6 6249",
        "8/2k1p3/3pP3/3P2K1/8/8/8/8 b - - 0 1 ;D1 5 ;D2 35 ;D3 182 ;D4 1091 ;D5 5408 ;D6 34822",
        "8/8/8/8/8/4k3/4P3/4K3 w - - 0 1 ;D1 2 ;D2 8 ;D3 44 ;D4 282 ;D5 1814 ;D6 11848",
        "4k3/4p3/4K3/8/8/8/8/8 b - - 0 1 ;D1 2 ;D2 8 ;D3 44 ;D4 282 ;D5 1814 ;D6 11848",
        "8/8/7k/7p/7P/7K/8/8 w - - 0 1 ;D1 3 ;D2 9 ;D3 57 ;D4 360 ;D5 1969 ;D6 10724",
        "8/8/k7/p7/P7/K7/8/8 w - - 0 1 ;D1 3 ;D2 9 ;D3 57 ;D4 360 ;D5 1969 ;D6 10724",
        "8/8/3k4/3p4/3P4/3K4/8/8 w - - 0 1 ;D1 5 ;D2 25 ;D3 180 ;D4 1294 ;D5 8296 ;D6 53138",
        "8/3k4/3p4/8/3P4/3K4/8/8 w - - 0 1 ;D1 8 ;D2 61 ;D3 483 ;D4 3213 ;D5 23599 ;D6 157093",
        "8/8/3k4/3p4/8/3P4/3K4/8 w - - 0 1 ;D1 8 ;D2 61 ;D3 411 ;D4 3213 ;D5 21637 ;D6 158065",
        "k7/8/3p4/8/3P4/8/8/7K w - - 0 1 ;D1 4 ;D2 15 ;D3 90 ;D4 534 ;D5 3450 ;D6 20960",
        "8/8/7k/7p/7P/7K/8/8 b - - 0 1 ;D1 3 ;D2 9 ;D3 57 ;D4 360 ;D5 1969 ;D6 10724",
        "8/8/k7/p7/P7/K7/8/8 b - - 0 1 ;D1 3 ;D2 9 ;D3 57 ;D4 360 ;D5 1969 ;D6 10724",
        "8/8/3k4/3p4/3P4/3K4/8/8 b - - 0 1 ;D1 5 ;D2 25 ;D3 180 ;D4 1294 ;D5 8296 ;D6 53138",
        "8/3k4/3p4/8/3P4/3K4/8/8 b - - 0 1 ;D1 8 ;D2 61 ;D3 411 ;D4 3213 ;D5 21637 ;D6 158065",
        "8/8/3k4/3p4/8/3P4/3K4/8 b - - 0 1 ;D1 8 ;D2 61 ;D3 483 ;D4 3213 ;D5 23599 ;D6 157093",
        "k7/8/3p4/8/3P4/8/8/7K b - - 0 1 ;D1 4 ;D2 15 ;D3 89 ;D4 537 ;D5 3309 ;D6 21104",
        "7k/3p4/8/8/3P4/8/8/K7 w - - 0 1 ;D1 4 ;D2 19 ;D3 117 ;D4 720 ;D5 4661 ;D6 32191",
        "7k/8/8/3p4/8/8/3P4/K7 w - - 0 1 ;D1 5 ;D2 19 ;D3 116 ;D4 716 ;D5 4786 ;D6 30980",
        "k7/8/8/7p/6P1/8/8/K7 w - - 0 1 ;D1 5 ;D2 22 ;D3 139 ;D4 877 ;D5 6112 ;D6 41874",
        "k7/8/7p/8/8/6P1/8/K7 w - - 0 1 ;D1 4 ;D2 16 ;D3 101 ;D4 637 ;D5 4354 ;D6 29679",
        "k7/8/8/6p1/7P/8/8/K7 w - - 0 1 ;D1 5 ;D2 22 ;D3 139 ;D4 877 ;D5 6112 ;D6 41874",
        "k7/8/6p1/8/8/7P/8/K7 w - - 0 1 ;D1 4 ;D2 16 ;D3 101 ;D4 637 ;D5 4354 ;D6 29679",
        "k7/8/8/3p4/4p3/8/8/7K w - - 0 1 ;D1 3 ;D2 15 ;D3 84 ;D4 573 ;D5 3013 ;D6 22886",
        "k7/8/3p4/8/8/4P3/8/7K w - - 0 1 ;D1 4 ;D2 16 ;D3 101 ;D4 637 ;D5 4271 ;D6 28662",
        "7k/3p4/8/8/3P4/8/8/K7 b - - 0 1 ;D1 5 ;D2 19 ;D3 117 ;D4 720 ;D5 5014 ;D6 32167",
        "7k/8/8/3p4/8/8/3P4/K7 b - - 0 1 ;D1 4 ;D2 19 ;D3 117 ;D4 712 ;D5 4658 ;D6 30749",
        "k7/8/8/7p/6P1/8/8/K7 b - - 0 1 ;D1 5 ;D2 22 ;D3 139 ;D4 877 ;D5 6112 ;D6 41874",
        "k7/8/7p/8/8/6P1/8/K7 b - - 0 1 ;D1 4 ;D2 16 ;D3 101 ;D4 637 ;D5 4354 ;D6 29679",
        "k7/8/8/6p1/7P/8/8/K7 b - - 0 1 ;D1 5 ;D2 22 ;D3 139 ;D4 877 ;D5 6112 ;D6 41874",
        "k7/8/6p1/8/8/7P/8/K7 b - - 0 1 ;D1 4 ;D2 16 ;D3 101 ;D4 637 ;D5 4354 ;D6 29679",
        "k7/8/8/3p4/4p3/8/8/7K b - - 0 1 ;D1 5 ;D2 15 ;D3 102 ;D4 569 ;D5 4337 ;D6 22579",
        "k7/8/3p4/8/8/4P3/8/7K b - - 0 1 ;D1 4 ;D2 16 ;D3 101 ;D4 637 ;D5 4271 ;D6 28662",
        "7k/8/8/p7/1P6/8/8/7K w - - 0 1 ;D1 5 ;D2 22 ;D3 139 ;D4 877 ;D5 6112 ;D6 41874",
        "7k/8/p7/8/8/1P6/8/7K w - - 0 1 ;D1 4 ;D2 16 ;D3 101 ;D4 637 ;D5 4354 ;D6 29679",
        "7k/8/8/1p6/P7/8/8/7K w - - 0 1 ;D1 5 ;D2 22 ;D3 139 ;D4 877 ;D5 6112 ;D6 41874",
        "7k/8/1p6/8/8/P7/8/7K w - - 0 1 ;D1 4 ;D2 16 ;D3 101 ;D4 637 ;D5 4354 ;D6 29679",
        "k7/7p/8/8/8/8/6P1/K7 w - - 0 1 ;D1 5 ;D2 25 ;D3 161 ;D4 1035 ;D5 7574 ;D6 55338",
        "k7/6p1/8/8/8/8/7P/K7 w - - 0 1 ;D1 5 ;D2 25 ;D3 161 ;D4 1035 ;D5 7574 ;D6 55338",
        "3k4/3pp3/8/8/8/8/3PP3/3K4 w - - 0 1 ;D1 7 ;D2 49 ;D3 378 ;D4 2902 ;D5 24122 ;D6 199002",
        "7k/8/8/p7/1P6/8/8/7K b - - 0 1 ;D1 5 ;D2 22 ;D3 139 ;D4 877 ;D5 6112 ;D6 41874",
        "7k/8/p7/8/8/1P6/8/7K b - - 0 1 ;D1 4 ;D2 16 ;D3 101 ;D4 637 ;D5 4354 ;D6 29679",
        "7k/8/8/1p6/P7/8/8/7K b - - 0 1 ;D1 5 ;D2 22 ;D3 139 ;D4 877 ;D5 6112 ;D6 41874",
        "7k/8/1p6/8/8/P7/8/7K b - - 0 1 ;D1 4 ;D2 16 ;D3 101 ;D4 637 ;D5 4354 ;D6 29679",
        "k7/7p/8/8/8/8/6P1/K7 b - - 0 1 ;D1 5 ;D2 25 ;D3 161 ;D4 1035 ;D5 7574 ;D6 55338",
        "k7/6p1/8/8/8/8/7P/K7 b - - 0 1 ;D1 5 ;D2 25 ;D3 161 ;D4 1035 ;D5 7574 ;D6 55338",
        "3k4/3pp3/8/8/8/8/3PP3/3K4 b - - 0 1 ;D1 7 ;D2 49 ;D3 378 ;D4 2902 ;D5 24122 ;D6 199002",
        "8/Pk6/8/8/8/8/6Kp/8 w - - 0 1 ;D1 11 ;D2 97 ;D3 887 ;D4 8048 ;D5 90606 ;D6 1030499",
        "n1n5/1Pk5/8/8/8/8/5Kp1/5N1N w - - 0 1 ;D1 24 ;D2 421 ;D3 7421 ;D4 124608 ;D5 2193768 ;D6 37665329",
        "8/PPPk4/8/8/8/8/4Kppp/8 w - - 0 1 ;D1 18 ;D2 270 ;D3 4699 ;D4 79355 ;D5 1533145 ;D6 28859283",
        "n1n5/PPPk4/8/8/8/8/4Kppp/5N1N w - - 0 1 ;D1 24 ;D2 496 ;D3 9483 ;D4 182838 ;D5 3605103 ;D6 71179139",
        "8/Pk6/8/8/8/8/6Kp/8 b - - 0 1 ;D1 11 ;D2 97 ;D3 887 ;D4 8048 ;D5 90606 ;D6 1030499",
        "n1n5/1Pk5/8/8/8/8/5Kp1/5N1N b - - 0 1 ;D1 24 ;D2 421 ;D3 7421 ;D4 124608 ;D5 2193768 ;D6 37665329",
        "8/PPPk4/8/8/8/8/4Kppp/8 b - - 0 1 ;D1 18 ;D2 270 ;D3 4699 ;D4 79355 ;D5 1533145 ;D6 28859283",
        "n1n5/PPPk4/8/8/8/8/4Kppp/5N1N b - - 0 1 ;D1 24 ;D2 496 ;D3 9483 ;D4 182838 ;D5 3605103 ;D6 71179139"
    };

    public static string[] gamePositions = {
        "r2qkr2/p1pp1ppp/1pn1pn2/2P5/3Pb3/2N1P3/PP3PPP/R1B1KB1R b KQq - c9 ; 0-1",
        "r4rk1/3bppb1/p3q1p1/1p1p3p/2pPn3/P1P1PN1P/1PB1QPPB/1R3RK1 b - - c9 ; 1/2-1/2",
        "4Q3/8/8/8/6k1/4K2p/3N4/5q2 b - - c9 ; 0-1",
        "r4rk1/1Qpbq1bp/p1n2np1/3p1p2/3P1P2/P1NBPN1P/1P1B2P1/R4RK1 b - - c9 ; 0-1",
        "r1bqk2r/2p2ppp/2p5/p3pn2/1bB5/2NP2P1/PPP1NP1P/R1B1K2R w KQkq - c9 ; 0-1",
        "8/8/4kp2/8/5K2/6p1/6P1/8 b - - c9 ; 1/2-1/2",
        "r4rk1/3p2pp/p7/1pq2p2/2n2P2/P2Q3P/2P1NRP1/R5K1 w - - c9 ; 1/2-1/2",
        "2rqk1n1/p6p/1p1pp3/8/4P3/P1b5/R2N1PPP/3QR1K1 w - - c9 ; 1-0",
        "1r4k1/2qb1pb1/2p2P1p/8/p7/N1BB3P/P5P1/2Q2R1K b - - c9 ; 1-0",
        "R7/1r6/5p2/8/P4k2/8/1p6/4K3 w - - c9 ; 0-1",
        "r3k2r/1p1nqp2/p2p2pp/2pP4/6QN/2N5/PPP2PPP/R1B3K1 w kq - c9 ; 1-0",
        "6KN/8/8/q7/5k2/8/8/8 w - - c9 ; 0-1",
        "4r1k1/p2P1ppp/P7/R3Pp2/5b2/3K1q2/1r3P2/3R4 w - - c9 ; 0-1",
        "8/1k6/1P6/3R3p/6p1/6r1/8/4K3 w - - c9 ; 1/2-1/2",
        "2rq1rk1/5pbp/p3p1p1/1p1PPn2/5P2/P1NQ2P1/1B5P/R4RK1 b - - c9 ; 1/2-1/2",
        "4rrk1/1pp4p/p2pb1pq/8/2Pn4/2NB3P/PP3PP1/1R2R1K1 b - - c9 ; 0-1",
        "2kr4/Qp3p1p/6p1/2PnB1q1/3N4/P7/5PPP/4R1K1 b - - c9 ; 1-0",
        "1k2q2r/p1p1bpp1/1pn1p2p/4P2B/2P1b3/4BN2/PP2QPPP/3R2K1 b - - c9 ; 1-0",
        "b7/2p2ppk/1p2p2p/p3P2P/P3pK2/1P2P3/1bP1BPP1/3R4 b - - c9 ; 1-0",
        "r1b1kb1r/pp4pp/2n1pn2/8/3N1B2/5N2/Pq2PPPP/R2QKB1R w KQkq - c9 ; 1/2-1/2",
        "8/7P/5k2/p1K5/P7/1P5r/8/8 w - - c9 ; 0-1",
        "r1b1k2r/pp1p1p1p/2n2np1/2q5/2PP4/8/PBP1BPPP/1R1QK2R b Kkq - c9 ; 0-1",
        "r2q1rk1/1pnbbpp1/p1np3p/P1p1p3/4P3/1P1P1NP1/2PB1PBP/R2Q1RK1 w - - c9 ; 0-1",
        "2r3k1/r4ppp/1q1bb3/8/3p3P/1P3B2/P2BQPP1/RR4K1 b - - c9 ; 0-1",
        "8/8/4k1p1/2r5/2P1R1KP/4P3/5P2/8 b - - c9 ; 1-0",
        "3rk2r/pb2ppb1/1pp2npp/8/2P4B/2N3P1/PP2PPBP/2R2RK1 b k - c9 ; 0-1",
        "8/8/3k4/8/8/3KP3/2Q5/8 w - - c9 ; 1-0",
        "r1b1kb1r/pp2pppp/1q3n2/3p4/PP6/2P2N2/3NPPPP/R2QKB1R b KQkq - c9 ; 1/2-1/2",
        "r1b2rk1/1p2b1pp/p2qp3/P3np2/8/2PP2N1/5PPP/RBBQR1K1 b - - c9 ; 1/2-1/2",
        "2bb1rk1/1p4p1/2p1p2p/8/Q3P3/P4N2/1P2NPPP/6K1 b - - c9 ; 1-0",
        "4r1k1/7p/pp2N3/2pR1p2/P1p5/2P4P/1P4P1/6K1 w - - c9 ; 1-0",
        "4n1k1/2p2pp1/2P4p/8/1N1p1P1P/Pp4P1/1P3P2/6K1 w - - c9 ; 1-0",
        "8/3N2k1/1p2p2p/8/2pP1p2/r6P/3r1P2/5RK1 w - - c9 ; 0-1",
        "3rr1k1/1pp1bpp1/4q2p/nP1pP3/7P/1QPR1NB1/5PP1/3R2K1 w - - c9 ; 1/2-1/2",
        "r4rk1/ppqn2pp/2pb1n2/3pp3/8/3P2P1/PPPN1PBP/R1BQK2R w KQ - c9 ; 1-0",
        "r1bqkb1r/1p2nppp/p1n5/2ppP3/8/2PB1N2/PP1P1PPP/R1BQ1RK1 b kq - c9 ; 0-1",
        "8/1n4k1/8/3K4/1P6/3N4/8/8 w - - c9 ; 1-0",
        "2r2b2/2q3k1/2n1bppp/pB1p4/N2P4/4P3/PR3PP1/1Q3RK1 b - - c9 ; 1-0",
        "r1b2rk1/pp2bppp/8/1N1Pp3/2p1P3/PnN1q3/RPQ1B1PP/5RK1 w - - c9 ; 1/2-1/2",
        "8/1p6/2b5/2b5/2P5/8/2K2k2/8 b - - c9 ; 0-1",
        "r4rk1/5p1p/4p1p1/p1p1B1b1/PpQ5/1P1P4/2P2PKP/R3R3 b - - c9 ; 1-0",
        "3rkb1r/2p2ppp/p4n2/1p2p3/2BnPP2/P1N1B3/1PP2P1P/R1K4R w k - c9 ; 1/2-1/2",
        "r3kb1r/1pp1nppp/2n5/1p2P3/4N1b1/4B1P1/PPP2P1P/R3K1NR w kq - c9 ; 0-1",
        "r4rk1/pbpqbppn/1pn1p3/3p4/P1PP1B2/4PN2/1P1N1PPP/R2Q1RK1 b - - c9 ; 0-1",
        "8/1p4k1/8/1n2P3/5P2/p2K4/5r2/R7 w - - c9 ; 0-1",
        "r4rk1/1pp3pp/8/p1nPR2Q/P7/1P5P/2P2qP1/3R2K1 w - - c9 ; 0-1",
        "r1bqk2r/pp1n1pp1/3bpn2/3p3p/3P3P/5NP1/PP1NPPB1/R1BQK2R b KQkq - c9 ; 0-1",
        "8/5kp1/1K5p/8/6P1/8/8/8 b - - c9 ; 0-1",
        "2r5/1p1kb1p1/4pP2/pP1p2p1/1n1P4/1N2B3/1P3PPP/5RK1 b - - c9 ; 0-1",
        "r4rk1/pp2qp1p/2np1p2/3p2N1/3P4/2P1P1P1/PP4BP/RN1b1RK1 w - - c9 ; 0-1",
        "8/8/4p1k1/2p3p1/8/8/1q6/4K3 b - - c9 ; 0-1",
        "8/5Q2/8/5K2/3p1P1Q/p7/2k5/8 w - - c9 ; 1-0",
        "r3r1k1/2p1np2/pp1p3p/6p1/3PN3/2PBP2P/P5P1/1R1Q2K1 w - - c9 ; 1-0",
        "8/k7/5K2/8/5Q2/8/8/8 b - - c9 ; 1-0",
        "r5r1/2p2pk1/1pb4p/p3R3/P1P1p3/1P4P1/2PN2KP/R7 b - - c9 ; 0-1",
        "r2q1rk1/1p2bppp/p2pbn2/8/1n2P3/1NNP4/1B2BPPP/R2Q1RK1 w - - c9 ; 1/2-1/2",
        "8/8/4p3/2p3P1/2P1k1P1/8/3K4/8 b - - c9 ; 1-0",
        "r1b1k1nr/pp3ppp/2pqp3/3n4/3P4/5N2/PPP1QPPP/R3KB1R w KQkq - c9 ; 0-1",
        "8/2R2B2/6k1/4r3/7K/2pb4/8/8 b - - c9 ; 1/2-1/2",
        "r5k1/1ppb3p/p2P2P1/3p1p2/8/1QPB3P/PP1b2P1/R5K1 b - - c9 ; 1-0",
        "8/p7/1p2k3/2p5/b1P1BP1B/3K4/1b6/8 w - - c9 ; 0-1",
        "8/8/5b2/1P1k4/8/4R3/K7/7q w - - c9 ; 0-1",
        "8/8/6k1/7p/5K1P/5PP1/8/5R2 w - - c9 ; 1-0",
        "r1bq1rk1/pp1nbpp1/7p/2p1p3/P3P3/2N5/1PPBBPPP/R2Q1RK1 w - - c9 ; 1-0",
        "3R4/6p1/4p1P1/n1p1P1k1/8/4K1p1/8/8 w - - c9 ; 1/2-1/2",
        "r5k1/pB1b1ppp/3qpb2/8/3n1N2/6P1/4PP1P/2BQ1RK1 b - - c9 ; 1/2-1/2",
        "5r2/6k1/8/6p1/1p2pb1P/1P5b/2P1BK2/6R1 w - - c9 ; 0-1",
        "8/4k3/5b1p/6pP/8/1Bp2P2/4K3/8 b - - c9 ; 1/2-1/2",
        "r1bq1rk1/pp1n2p1/3bpn1p/3p1p2/3P4/1N1Q1NP1/PP2PP1P/R1B2RKB b - - c9 ; 1/2-1/2",
        "8/2R5/8/4PKPk/8/5r2/8/8 w - - c9 ; 1-0",
        "2r3k1/pp2p1b1/8/3q3P/3p3p/3P3P/PP1N1r1K/R2QR3 w - - c9 ; 0-1",
        "rn1qk2r/ppp1bppp/8/3pP3/3P4/2N2N2/PPQP2PP/R1B1K2R b KQkq - c9 ; 1-0",
        "4Q3/8/p2k4/7P/5RP1/2K5/P7/8 b - - c9 ; 1-0",
        "r4r2/q4pkp/p2b4/3p4/P4P2/3QP2P/P5P1/3R1RK1 w - - c9 ; 1/2-1/2",
        "k7/8/5P1K/8/8/8/5q2/7q w - - c9 ; 0-1",
        "2r3k1/pp3p1p/2B1b1p1/2n1p3/8/2N4P/PPP2PP1/2KR4 w - - c9 ; 1-0",
        "R7/5k2/3p1p2/2p1pbpP/8/2K1PP2/8/8 w - - c9 ; 1/2-1/2",
        "3r4/7k/7p/2B3p1/8/5nPP/8/2R4K w - - c9 ; 1/2-1/2",
        "rq2r1k1/pbp4p/1p4p1/3n1p2/PPQPnP2/5N2/1B3PBP/R4RK1 b - - c9 ; 1/2-1/2",
        "r1bqk3/ppp2ppr/5p2/8/1P1N4/8/PP3PPP/R1BQK2R b KQq - c9 ; 1-0",
        "8/8/p5pk/2R5/1nP2p1P/4r3/4BK2/8 w - - c9 ; 1/2-1/2",
        "8/2R5/7k/4r3/5RPP/3K4/8/8 b - - c9 ; 1-0",
        "1q5r/4k2p/2pbbpr1/3ppp1Q/8/P1P1PP1P/4B1P1/1NKR2R1 b - - c9 ; 0-1",
        "r2q1rk1/p1p2p2/2p2bp1/4p2p/N3P3/2P2QPP/PP3P2/R3K2R w KQ - c9 ; 1/2-1/2",
        "2kr2r1/p1p1bp2/2p1b2p/6P1/P3Q3/2PP3P/q2B1P2/2KR1BR1 w - - c9 ; 0-1",
        "8/5R1k/3P3p/8/2P4P/1p6/r7/4K3 b - - c9 ; 1-0",
        "4nr2/6pp/pkp3b1/2p5/8/2P2N1P/PP1N1PP1/R5K1 w - - c9 ; 1-0",
        "8/5Nk1/8/p4p1P/5K2/1P6/P7/8 w - - c9 ; 1-0",
        "r1bqk2r/ppp1bp1p/2np1n2/6N1/2P3p1/6P1/PP1PPPBP/R1BQK1NR b KQkq - c9 ; 0-1",
        "r4rk1/pp1bqppp/2pb1n2/3p4/2PPp3/PPN1P3/1B2BPPP/R2Q1RK1 w - - c9 ; 0-1",
        "r2qk2r/pb1pbppp/1pn1p3/1Np5/2P5/1P3N1P/PB1PBRP1/R2Q2K1 b kq - c9 ; 1-0",
        "r2q1rk1/p1p1bppp/4p3/2np4/5Pb1/4PN1P/PP1QB1P1/RN3RK1 b - - c9 ; 0-1",
        "3r2k1/pp4p1/6qp/8/P1b5/5P1P/4NP1K/4R2R w - - c9 ; 0-1",
        "r2qkb1Q/p4p1p/2p3p1/3p4/6b1/2P2NP1/P1P2PBP/R3K2R b KQq - c9 ; 1-0",
        "rnbq1rk1/p3bppp/5n2/2pp4/8/2N1PN2/PP2BPPP/R1BQ1RK1 w - - c9 ; 1/2-1/2",
        "1r3rk1/3bqpp1/1ppp3p/p7/P1PPPnP1/4NB2/2QB1PP1/1R3RK1 b - - c9 ; 1-0",
        "8/1p4k1/p7/b7/4P1Bp/5K2/2P5/8 w - - c9 ; 1/2-1/2",
        "r1bqk2r/pppp1ppp/5n2/4n3/2PP4/P5P1/5P1P/R1BQKBNR b KQkq - c9 ; 1/2-1/2",
        " 1k2r3/pqp2p1p/1p1p1Q2/r4P2/4b3/P1N2B2/1PP3PP/4RRK1 b - - c9 ; 1-0",
        "2b2rk1/4pp1p/3q2p1/p2p4/Pp1Q4/1P2PNP1/5PBP/R5K1 w - - c9 ; 1-0",
        "1r4k1/8/6N1/pb6/4P2p/P2pKP2/1P4P1/2R5 b - - c9 ; 1-0",
        "3r2k1/2p1nppp/1p4b1/8/p1B5/1PP1P3/1P4PP/4R1K1 b - - c9 ; 0-1",
        "2b3k1/1r3r2/p2B4/3P1ppp/1b6/1B2P3/5PPP/R5K1 w - - c9 ; 0-1",
        "5k2/5p2/6bR/pP6/P2p4/5r2/3K4/8 w - - c9 ; 0-1",
        "r2q1rk1/p1pp1ppp/1pn1p3/4P3/2BP4/P1P2P2/P4P1P/R1BQ1RK1 b - - c9 ; 1-0",
        "2r2r2/3qbp1k/2p2p1p/p2p4/8/PP2PN1P/3N1PP1/Q2R2K1 w - - c9 ; 0-1",
        "5rk1/pb1q1p2/1p2p2p/1P4p1/3b4/4Q1PP/PP2PPB1/2R3K1 w - - c9 ; 0-1",
        "rnbq1rk1/p1pn1ppp/p7/4p3/1b1PP3/5N2/1P3PPP/RNBQKB1R w KQ - c9 ; 1-0",
        "r2r2k1/1bp2pbp/6p1/1BpPp1q1/2P1PnN1/P1N4P/1P3PP1/3RR1K1 b - - c9 ; 0-1",
        "8/6Q1/5P2/3p4/4k1Pp/7K/8/4q3 w - - c9 ; 1-0",
        "4b1k1/8/1K4p1/2Q3P1/8/8/r7/8 b - - c9 ; 1/2-1/2",
        "7r/1pkb2p1/1pn1pp2/1B1p4/Pb2n2p/4PN1P/1PP1KPP1/R2R4 b - - c9 ; 0-1",
        "8/8/6p1/5p1p/1k5P/4PP2/5KP1/8 w - - c9 ; 1-0",
        "8/7k/p2p3p/5p2/8/2B3P1/q6K/4Q3 w - - c9 ; 1-0",
        "1r6/2b3k1/2p1bp1q/p1p1p3/Pr1pP1p1/1P1P2P1/QNPN1P2/3R1RK1 b - - c9 ; 0-1",
        "r1bqnrk1/1p2bppp/p3p3/2p1P3/5P2/1P6/PBPPB1PP/RN1Q1RK1 w - - c9 ; 1-0",
        "r6k/5p1p/3B2p1/p7/4P3/4bPP1/PPP3BP/1R3b1K b - - c9 ; 1-0",
        "8/8/2KB4/8/p7/P1pb4/8/3k4 w - - c9 ; 0-1",
        "rnbqk2r/ppp2pbp/4p1p1/3P4/3P1B2/4P3/PP2BPPP/R1Q1K2R b KQkq - c9 ; 0-1",
        "r4rk1/1qp1b1pp/2p1pn2/p1P2p2/1P1P1P2/1PB1PP2/3QN2P/R4RK1 b - - c9 ; 1/2-1/2",
        "2n2rk1/5ppp/4p3/4P3/8/P2Q2P1/1P5P/1KR4R w - - c9 ; 1-0",
        "r3kr2/pp2ppbp/5np1/2p5/2P3P1/2N1P2P/PPn1BP2/R1B2RK1 w - - c9 ; 1/2-1/2",
        "8/5p2/3Qb1k1/1P5p/7P/3N4/2q2P2/7K b - - c9 ; 1/2-1/2",
        "8/8/8/8/Q7/K3k3/2q5/8 b - - c9 ; 1/2-1/2",
        "3r1rk1/1Bpn2b1/p3q2p/4pp2/3n3P/3PB1P1/PPP2Q2/R2N1RK1 w - - c9 ; 1-0",
        "5k2/1p6/8/4p1RN/2P4P/3P1KP1/1r6/q7 b - - c9 ; 0-1",
        "8/8/4k1p1/7p/7P/3K4/8/2R5 b - - c9 ; 1-0",
        "8/8/8/4K3/8/7r/6k1/8 w - - c9 ; 0-1",
        "rn2k2r/pp2ppbp/6p1/5b2/q2PpB2/4PN2/PP3PPP/RN3K1R w kq - c9 ; 0-1",
        "5bk1/1p2pp2/p5pp/P2n4/1P1Pp2P/1Q4P1/1B2PP2/5K2 b - - c9 ; 1-0",
        "8/3k4/7R/1R4P1/1P6/P3n2P/2r5/4K3 b - - c9 ; 1-0",
        "r2qk2r/pb1p2pp/1pnP4/6p1/3p1n2/5N2/PPP1BPPP/R2Q1RK1 w kq - c9 ; 0-1",
        "r2r2k1/BQp1bpp1/5n1p/8/8/2Pq2P1/PP3P1P/R4RK1 b - - c9 ; 0-1",
        "r2B1rk1/pppb1pbp/n2p2p1/4p3/1PP1P3/2n2N2/P4PPP/R2QKB1R w KQ - c9 ; 1-0",
        "1n1q1rk1/Q2bpp1p/6p1/8/8/2b3P1/P3PPKP/R1B2R2 w - - c9 ; 0-1",
        "3r2k1/pp1r2bp/5pp1/2P1p2n/1PP4Q/R4P2/P4P1P/2B1R1K1 b - - c9 ; 1-0",
        "8/4kpp1/7p/7K/4Q2P/4P3/5rP1/8 b - - c9 ; 1-0",
        "r1bqk2r/3pnppp/p7/1pbP4/8/2NQ1N2/PP3PPP/R1B1KB1R w KQkq - c9 ; 1-0",
        "rnb1kb1r/pp2pp2/2q2n1p/3N2p1/3P4/P4NB1/1P2PPPP/2RQKB1R b Kkq - c9 ; 1-0",
        "2r2rk1/pp3pbp/3p1pp1/2n5/1qbN4/2N1P3/PPBB1PPP/R2R2K1 w - - c9 ; 0-1",
        "4k1R1/p7/5p2/2p5/P1P2P2/7K/1r5p/8 b - - c9 ; 0-1",
        "6k1/p2r2p1/R6p/PP3p2/4p2P/6P1/2r2P2/4R1K1 b - - c9 ; 1/2-1/2",
        "1k5r/pp1Pr1p1/n2b1pp1/4pn2/1PP1N3/P4NB1/2Q2PPP/R4RK1 w - - c9 ; 1-0",
        "8/8/4R3/5K2/8/8/8/3q1k2 b - - c9 ; 0-1",
        "8/5K2/2k4p/R7/2N5/5p2/PP6/8 w - - c9 ; 1-0",
        "1r1qkbnr/1pp2ppp/p1n1p3/3p4/3N4/1P1PP2P/PB1P1PP1/RNQ1K2R b KQk - c9 ; 0-1",
        "rnbq1k1r/4p1b1/1p4p1/pBpn3p/5P2/4P3/PP1P2PP/RNBQ1RK1 w - - c9 ; 1/2-1/2",
        "8/8/p2r1k2/R5p1/1PR5/5K2/6P1/1r6 w - - c9 ; 1/2-1/2",
        "4k3/p7/p3r3/R5PK/3B3P/2P5/8/2r5 w - - c9 ; 1-0",
        "4r1k1/4q3/b4pp1/N3n2p/3B4/4P3/P1Q2PP1/2R1K3 b - - c9 ; 0-1",
        "7r/1pk3p1/p4pB1/4p1p1/bPPr4/3P3P/5PP1/4R1K1 w - - c9 ; 0-1",
        "r1b1k2r/pp2bppp/1q1p1n2/2p5/3pP3/2NP2PP/PPPB1PB1/R2QKR2 w Qkq - c9 ; 0-1",
        "3qk2r/1p3pp1/4p2p/4nb2/2Pp4/3P3P/4BPP1/2B1NNK1 b k - c9 ; 0-1",
        "8/7K/3R4/8/8/8/7k/8 b - - c9 ; 1-0",
        "1r4k1/8/8/p2p3b/4pq2/P3R3/5PP1/5RK1 w - - c9 ; 0-1",
        "2kr1b1r/1pp2ppp/p7/2P5/1P6/3b1NP1/PB1P3P/RNK5 b - - c9 ; 0-1",
        "5r1k/1p2n3/p5pp/3PPp2/8/P2N2P1/1PR3BP/7K w - - c9 ; 1-0",
        "4Q3/8/1k6/8/8/5R2/6P1/6K1 w - - c9 ; 1-0",
        "4r1k1/1p3pp1/p2qp2p/3pn3/1PrQP3/2P2B2/P4PPP/3R1RK1 w - - c9 ; 1/2-1/2",
        "5k2/2pb1ppR/3q4/N7/2Pp3Q/6P1/P4PPK/1r6 b - - c9 ; 1-0",
        "8/8/8/R7/P6k/1K6/8/r6q b - - c9 ; 0-1",
        "3rkb1r/1p2pppp/p1n3q1/2p2bB1/2P5/3P1N1P/PP3PP1/R1KQ1B1R w k - c9 ; 0-1",
        "rq3rk1/1p3pb1/p1ppn1p1/PP2p3/2P1P3/3PN1P1/3Q2BP/1R3RK1 b - - c9 ; 1/2-1/2",
        "rnbq1rk1/pp2ppbp/2p3p1/3p4/1B1Pn3/4PNPP/PPP2PB1/RN1Q1RK1 b - - c9 ; 0-1",
        "r1b1k2r/pp1pbppp/4p3/4q3/8/P1P5/1B2BPPP/R2Q1RK1 w kq - c9 ; 1/2-1/2",
        "1r2r1k1/1pqb1p2/2pp1p1p/p1n5/P1P1PNnP/1P4P1/2QN1P2/3RRBK1 w - - c9 ; 1/2-1/2",
        "8/p5k1/6p1/1P2p1p1/3pq1P1/3N3P/4r3/6K1 w - - c9 ; 0-1",
        "7k/8/8/p7/P1K3RP/5r2/8/8 b - - c9 ; 1-0",
        "8/8/8/8/4kq2/8/K7/8 b - - c9 ; 0-1",
        "1r1q1rk1/3b1pb1/p2p2pp/1ppP4/3pPP2/1P1P3P/PB1QB1P1/R3R1K1 b - - c9 ; 1/2-1/2",
        "8/5p2/3R3k/6p1/6Pn/7P/5K2/8 b - - c9 ; 1-0",
        "8/8/5p2/8/6k1/3K4/8/4Rrq1 w - - c9 ; 0-1",
        "r1b1kb1r/pp1n1pp1/7p/3pP2n/7B/5N2/PqPNBPPP/R2Q1RK1 b kq - c9 ; 1-0",
        "5rk1/pp4pp/4p1q1/3bPp2/PP2n3/8/4BPPP/2BQ1RK1 b - - c9 ; 0-1",
        "2Q5/8/3k4/6p1/8/p4KP1/r3P2P/8 w - - c9 ; 1-0",
        "8/N7/1p2kb2/p1p5/P1P2P2/1P1p2K1/6P1/8 b - - c9 ; 0-1",
        "7k/2p3b1/5Q2/pP1Pp2p/7P/1P4P1/4BPK1/q7 w - - c9 ; 1-0",
        "3k4/5R2/3P4/6P1/p6p/r7/8/4K3 w - - c9 ; 1-0",
        "8/8/2k4P/7K/8/6P1/8/5Q2 w - - c9 ; 1-0",
        "8/8/R5K1/4k1P1/8/8/8/1r6 b - - c9 ; 1-0",
        "7R/5n2/8/6k1/5p2/P3p3/2P1K3/8 w - - c9 ; 1-0",
        "r5k1/4Qpbp/p4np1/8/5B2/3P2P1/bq3P1P/R4RK1 w - - c9 ; 1-0",
        "rn2qrk1/ppP2pp1/5n1p/4p3/4P1b1/2PB1N2/PP3PPP/R1BQ1RK1 b - - c9 ; 1-0",
        "6k1/3b3p/1b1Bp2P/2p2p2/2p1PP2/p1P2B2/P4K2/8 b - - c9 ; 1/2-1/2",
        "r5k1/1p3ppp/p7/3Q4/1PPp2n1/P7/1B3qPP/R5K1 w - - c9 ; 0-1",
        "r3k2r/p1pbp2p/1pn3pb/8/3qN3/8/PPPNQPPP/R1B2RK1 w kq - c9 ; 1-0",
        "r3kb1r/pp1b1ppp/2n2n2/qB6/4p3/2N1BN2/PP3PPP/R2Q1RK1 w kq - c9 ; 1-0",
        "8/6p1/8/5k2/8/2N4n/8/5K2 w - - c9 ; 1/2-1/2",
        "8/4b3/6p1/3k4/7n/1K6/NB6/8 w - - c9 ; 1/2-1/2",
        "8/4k3/5p1K/pN6/P3n3/8/8/8 w - - c9 ; 1/2-1/2",
        "r3kbnr/1p3ppp/4p3/nNPpP3/8/4B3/P1P2PPP/3RK2R w Kkq - c9 ; 1-0",
        "r4q1k/ppp1b1p1/2np3p/4pb2/2P5/P1NQ4/1P1B1PPP/R4RK1 w - - c9 ; 1/2-1/2",
        "8/8/6k1/8/2N2P2/5K2/8/8 b - - c9 ; 1-0",
        "rnbq1rk1/pp4bp/4pnp1/8/3P3P/2NB1N2/PP3PP1/R1BQK2R w KQ - c9 ; 1-0",
        "r1bqkb1r/pp3ppp/3p4/2pPn3/8/5NP1/PP2PPBP/R1BQK2R b KQkq - c9 ; 0-1",
        "5k2/R7/6pp/8/2K1N1P1/2P5/7P/8 b - - c9 ; 1-0",
        "1k6/8/1pp1p1p1/4PpBp/1PK2P2/6q1/1Q6/8 w - - c9 ; 1-0",
        "8/k1B5/P4p2/1P6/8/K2n4/8/8 w - - c9 ; 1-0",
        "r2qkr2/5ppp/ppp1pnb1/8/8/2N5/PPP1BPPP/R3QRK1 w q - c9 ; 1/2-1/2",
        "8/4k3/r2p4/3p4/3P1K2/8/2B2P2/8 w - - c9 ; 0-1",
        "3r1kr1/p1p3pp/n7/1p1p4/3Q4/7P/PPP2P1P/RN2R1K1 b - - c9 ; 1-0",
        "1kr4r/p3b1p1/2p1pp2/7p/2RP4/P3PPPN/1P5P/5RK1 b - - c9 ; 1-0",
        "8/8/8/4p1p1/5kP1/R4P2/5K2/3r4 w - - c9 ; 1/2-1/2",
        "8/8/8/2K5/p7/k7/8/8 b - - c9 ; 0-1",
        "8/1b6/8/1P1k1p1p/1B3P1P/P2pp3/2r3P1/R4K2 b - - c9 ; 0-1",
        "8/8/1r4p1/4R3/2K2k1p/8/4Q3/8 w - - c9 ; 1-0",
        "1r6/p1kr1pp1/2pbbn1p/2p1p3/2P1P3/P1N2N1P/1P1RBPP1/3R3K b - - c9 ; 0-1",
        "r1bq1rk1/pp1n1pbp/2p1p3/3p4/2PPNp2/4PN1P/PPQ1BPP1/R4RK1 w - - c9 ; 1-0",
        "2r2rk1/pQ2bpp1/4p1p1/2qp4/2n2B2/2P2NP1/P3PPB1/2R2RK1 w - - c9 ; 1-0",
        "8/5R1p/4p3/3pk3/8/N3P3/P2PKP1q/8 w - - c9 ; 1/2-1/2",
        "rn2kbr1/pp3p1p/1q3p2/2p5/2b5/1P2P1P1/P4P1P/RN1QK1NR b KQq - c9 ; 0-1",
        "8/7P/8/1Q6/B7/2k1K3/3p4/8 b - - c9 ; 1-0",
        "2kr1b1r/1pp2ppp/p7/8/8/3PPN1P/Pn3PP1/3RKB1R w K - c9 ; 1/2-1/2",
        "rn1qk2r/1bpp1p1p/1p2pbp1/p7/2P2P2/2NP1N2/PP2B1PP/R1BQ1RK1 w kq - c9 ; 1-0",
        "r1bq1rk1/pp1n3p/2p2bp1/4pp2/2P5/2N2NP1/PP2PPBP/R1Q2RK1 w - - c9 ; 1/2-1/2",
        "1r5k/5bp1/4q2p/3pP3/1P1P3P/2Q5/2B5/4R1K1 b - - c9 ; 0-1",
        "r1b1k1nr/p4ppp/2p1p3/1q1p4/1b1P1B2/2N5/PPP2PPP/R1Q1K2R w KQkq - c9 ; 0-1",
        "r2qk2r/pp1n1p2/2pbpnbp/3p2p1/8/2NP1NP1/PPPBQPBP/R4RK1 w kq - c9 ; 0-1",
        "8/p6p/4p1k1/8/6q1/4Q3/4KP2/8 w - - c9 ; 0-1",
        "8/r5kp/2P1R1p1/8/4P2P/5PP1/6K1/8 w - - c9 ; 1-0",
        "r2qr1k1/2p2pbp/p1B1bnp1/8/3P4/2P3P1/PP3P1P/RNBQ1RK1 b - - c9 ; 0-1",
        "8/8/8/5k2/8/r2pK3/5P2/8 w - - c9 ; 0-1",
        "2Q1k2r/pp3p2/1q6/3p3p/8/P4NP1/1P3P1P/2R3K1 b - - c9 ; 1-0",
        "8/8/7p/2P4k/q7/P7/8/1K6 w - - c9 ; 0-1",
        "r7/8/p1pk4/Pp4Pp/1PpPb3/2P5/5K2/8 b - - c9 ; 0-1",
        "r1bq1rk1/ppp1bppp/2n2n2/8/2BP4/1P1NP3/PB3PPP/RN1QK2R b KQ - c9 ; 1/2-1/2",
        "8/p2pk1pp/bp1N4/4P3/1RPp4/5QPP/P5K1/8 b - - c9 ; 1-0",
        "5rk1/pqr1b1pp/2p1b3/2P1pp2/Q1P4N/1N1PBPPn/P6P/1R3R1K w - - c9 ; 1/2-1/2",
        "8/8/6R1/5p1p/5k2/8/1r6/5K2 b - - c9 ; 0-1",
        "r2q1rk1/ppp2ppp/2n5/3p4/3P2QN/3BP1P1/PP1K2PP/R6R b - - c9 ; 1-0",
        "r1b3k1/pp4p1/4ppr1/8/PB3P1p/7P/2P3B1/3R1RK1 w - - c9 ; 1-0",
        "8/8/5kpp/8/4p3/6P1/5P1P/6K1 w - - c9 ; 1/2-1/2",
        "2r2rk1/pp3pp1/5n1p/3pn3/8/P2BP1N1/1P3PPP/1R2K2R w - - c9 ; 1/2-1/2",
        "rnbq1rk1/2p1ppb1/p4npp/8/1p1PN3/3BPNB1/PP3PPP/R2QK2R b KQ - c9 ; 1-0",
        "3q1rk1/1pp1bppp/r1n1p1b1/3p4/3P4/P3PN2/1PPN1PPP/R1BQ1RK1 b - - c9 ; 1/2-1/2",
        "r2qk1nr/pp1n1pp1/2p1p3/P2P3p/1b4b1/1P3N2/1B1PPPPP/RN1QKB1R b KQkq - c9 ; 1/2-1/2",
        "6k1/8/7p/7P/6P1/5q1K/8/8 w - - c9 ; 0-1",
        "Q7/7B/2K2b2/8/1p3k2/8/8/8 b - - c9 ; 1-0",
        "8/8/2k2R2/5p2/8/8/1r6/5K2 b - - c9 ; 1/2-1/2",
        "r1q3k1/1p2p1b1/p2pPrpp/2p1n3/P3QP2/1PP5/4B1PP/R1B2RK1 b - - c9 ; 1-0",
        "3r2k1/1q4p1/1p2p2p/p1p1P3/P3RPP1/1P2P3/6QP/6K1 b - - c9 ; 0-1",
        "8/4R3/1R6/2r2B2/3k2P1/7P/5PK1/8 w - - c9 ; 1-0",
        "8/1pQ5/p3P2k/4p2p/8/2P1P3/qP5r/2KR4 w - - c9 ; 0-1",
        "r1bqkbnr/5p1p/p1p1p1p1/2p5/2P2P2/5N2/PP1PB1PP/RNBQK2R w KQkq - c9 ; 1-0",
        "r5kr/1p4pp/n4p2/5N2/1B1P4/8/2P2RPP/1b5K w - - c9 ; 0-1",
        "8/8/2k2K2/1p1pp3/8/P1P2Pr1/1R6/8 w - - c9 ; 1/2-1/2",
        "r4rk1/ppp2p1p/6p1/3pPp2/3P4/4B3/PPP3PP/3R1RK1 w - - c9 ; 1-0",
        "r2q1rk1/1p2ppb1/2p2n2/p5R1/P1PP4/1PNQ3P/5PP1/3R2K1 b - - c9 ; 1/2-1/2",
        "6k1/6p1/8/4p1B1/8/3Q2K1/8/q7 b - - c9 ; 1/2-1/2",
        "r4rk1/ppp1nppp/2n5/2p1p1N1/4P3/2Pq2PP/PP3PB1/R1BK3R w - - c9 ; 0-1",
        "Q4k2/2qp2pp/np6/p7/2P1N2r/P4P2/1P6/R4RK1 b - - c9 ; 1-0",
        "8/6p1/7p/7P/6k1/1r6/R2K4/8 w - - c9 ; 1/2-1/2",
        "r1bqk2r/p4pb1/1npp3p/1p4p1/3P4/2N2NP1/PPPQBPP1/2KR3R w kq - c9 ; 1/2-1/2",
        "4r1k1/3b1p2/1q2n2p/2p3p1/2PpNPPb/1p1P3P/P2B2B1/R4QK1 w - - c9 ; 1-0",
        "8/pp3k2/8/2P4P/P7/1P1RK3/8/5r2 b - - c9 ; 1-0",
        "1r6/p3nkp1/2p3p1/2q1pp2/2b4Q/8/P2RPPBP/2B3K1 w - - c9 ; 0-1",
        "8/r6p/3p4/1p4R1/1P1K4/7k/6p1/8 b - - c9 ; 0-1",
        "8/5ppk/2B2n2/1N3R2/3Pp2p/1r2P2P/6PK/8 w - - c9 ; 1-0",
        "3r1rk1/1pb2pp1/1qp2n1p/p7/P2pP3/1P1N2P1/1BP1Q2P/R4R1K w - - c9 ; 1-0",
        "rn1q2k1/pp2prbp/2p2np1/8/3PP3/2N5/PP3PPP/R1BR2K1 b - - c9 ; 0-1",
        "5rk1/p2b4/Pp1r3b/1PpPpPp1/2B4p/5R1P/1P3BP1/5RK1 b - - c9 ; 1/2-1/2",
        "2k2b1r/ppprp1pn/4p2p/PP6/5P2/4P3/3P2PP/R1B2RK1 w - - c9 ; 0-1",
        "2Q5/p4k2/6p1/4B3/P7/8/5PPP/5RK1 b - - c9 ; 1-0",
        "r6r/1pq1kpp1/p2bp1p1/7n/Q1NP4/2P1P2P/PP4P1/R1B1R2K w - - c9 ; 1-0",
        "5rk1/4np2/4p1p1/p1pp4/8/P1Pn2P1/1q3PBP/R4RK1 w - - c9 ; 0-1",
        "r1b1kbr1/pp1n2p1/2ppp2p/B7/2PPQ2P/5N2/PP3PP1/R3KB1R b KQq - c9 ; 1-0",
        "rn2kb1r/pp3ppp/1q2b3/2p1Q3/2B1PB2/1P6/5PPP/R2K2NR b kq - c9 ; 0-1",
        "8/4k3/p6R/8/1p2p3/4P1P1/1P3P1P/6K1 b - - c9 ; 1-0",
        "r1bqkr2/ppp2pbp/2n1p1p1/4P3/3Pp3/5N2/PPP1BPPP/R1BQ1RK1 w q - c9 ; 0-1",
        "r2qk1nr/p2npp1p/1p1p2p1/8/2Pb4/2NB1P1P/PP3P1R/R1BQK3 w Qkq - c9 ; 0-1",
        "8/8/1r6/4k3/PPR5/1K6/8/8 w - - c9 ; 1-0",
        "r1b1k2r/pp1p2Bp/1qn1pn2/5p2/3P4/2P2N1P/P3PPP1/RN2KB1R b KQkq - c9 ; 0-1",
        "r2qk2r/pp2bp1p/2p1nn2/3p2p1/8/3BPN1Q/PPPP1PPP/R1B1K2R w KQkq - c9 ; 1-0",
        "6Q1/5R2/2r4K/8/7k/8/8/8 w - - c9 ; 1-0     ",
        "Q7/P5p1/3k4/5p1p/8/2P5/5K1P/8 b - - c9 ; 1-0",
        "q4rk1/2p1pp2/p5pB/4b3/8/P5P1/1PP2P2/R4QK1 b - - c9 ; 1/2-1/2"
    };
}
