using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;

public class SearchTester
{
    List<string> fenPositions = new List<string>();
    const string positionsFile = Engine.chessRoot+"/depth6only.txt";
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
            RunNextSearch(new Move(0, 0, false));
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
            search = new Search(board, aiSettings, new Move[1024, 3], new int[64, 64], logger);
            search.onSearchComplete += RunNextSearch;
            Task.Run(() => search.StartSearch());
        }
        else
        {
            isTestRunning = false;
            Console.WriteLine("Suite finished");
            try
            {
                logger.logAllSearches();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    void loadPositions(int numPositions)
    {
        string[] lines = File.ReadAllLines(positionsFile);
        numPositions = numPositions > lines.Count() ? lines.Count() : numPositions;
        numTests = numPositions;

        for (int x = 0; x < numPositions; x++)
        {
            string[] info = lines[x].Split(";");
            fenPositions.Add(info[0]);
        }
    }





}