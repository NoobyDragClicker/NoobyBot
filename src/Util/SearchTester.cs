using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Diagnostics;

public class SearchTester
{
    List<string> fenPositions = new List<string>();
    int numTests;
    int currentTestNumber;
    bool isTestRunning = false;
    AISettings aiSettings = new AISettings(40, 16, 256);
    Search search;

    public void RunBench()
    {
        loadPositions(12);
        aiSettings.maxDepth = 14;
        ulong nodes = 0;
        Stopwatch watch = new Stopwatch();
        watch.Start();
        foreach (string pos in fenPositions)
        {
            Board board = new Board();
            board.setPosition(pos);
            search = new Search(board, aiSettings);
            search.StartSearch(false);
            nodes += search.nodeCount;
        }
        watch.Stop();
        Console.WriteLine($"{nodes} nodes {(nodes * 1000 / (ulong)watch.ElapsedMilliseconds) } nps");
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