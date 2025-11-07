using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Diagnostics;

public class SearchTester
{
    List<string> fenPositions = new List<string>();
    SearchLogger logger;
    int numTests;
    int currentTestNumber;
    bool isTestRunning = false;
    AISettings aiSettings = new AISettings(40, 16, 256);

    Search search;

    public SearchTester(SearchLogger logger)
    {
        this.logger = logger;
    }

    public void RunSearchSuite(int maxTests, int targetDepth)
    {
        if (!isTestRunning)
        {
            aiSettings.maxDepth = targetDepth;
            fenPositions = new List<string>();
            loadPositions(maxTests);
            isTestRunning = true;
            currentTestNumber = 0;
            Console.WriteLine($"Started suite with {fenPositions.Count()} positions, to a max depth of {targetDepth}");
            try
            {
                RunNextSearch(new Move(0, 0, false));
            } catch(Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
    }

    void RunNextSearch(Move chosenMove)
    {
        if (currentTestNumber != 0)
        {
            logger.startNewSearch();
        }
        if (currentTestNumber < numTests)
        {
            Board board = new Board();
            board.setPosition(fenPositions[currentTestNumber], logger);
            currentTestNumber++;
            search = new Search(board, aiSettings, logger);
            search.onSearchComplete += RunNextSearch;
            Task.Run(() => search.StartSearch(true));
        }
        else
        {
            isTestRunning = false;
            Console.WriteLine("Suite finished");
            SearchDiagnostics temp = logger.logAllSearches();
            Console.WriteLine($"Search time: {temp.totalSearchTime}");
            Console.WriteLine($"NPS: {temp.nodesSearched / temp.totalSearchTime.TotalMilliseconds * 1000}");
        }
    }

    public void RunBench()
    {
        loadPositions(20);
        aiSettings.maxDepth = 10;
        ulong nodes = 0;
        Stopwatch watch = new Stopwatch();
        watch.Start();
        foreach (string pos in fenPositions)
        {
            Board board = new Board();
            board.setPosition(pos, logger);
            search = new Search(board, aiSettings, logger);
            search.StartSearch(false);
            nodes += logger.currentDiagnostics.nodesSearched;
            logger.startNewSearch();
        }
        watch.Stop();
        Console.WriteLine($"{nodes} nodes {(nodes / (ulong)watch.ElapsedMilliseconds) * 1000} nps");
    }


    void loadPositions(int numPositions)
    {
        string[] lines = Perft.gamePositions;
        numPositions = numPositions > lines.Count() ? lines.Count() : numPositions;
        numTests = numPositions;

        for (int x = 0; x < numPositions; x++)
        {
            string[] info = lines[x].Split(";");
            fenPositions.Add(info[0]);
        }
    }
}