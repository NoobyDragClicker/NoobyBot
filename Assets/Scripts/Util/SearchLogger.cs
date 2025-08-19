using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


#if UNITY_EDITOR
using UnityEngine;
#endif

public class SearchLogger
{
    string logPath;
    bool useDiagnostics;
    List<SearchDiagnostics> diagnostics = new List<SearchDiagnostics>();
    public SearchDiagnostics currentDiagnostics;
    public SearchLogger(string name, string folderPath, bool useDiagnostics)
    {
        logPath = folderPath + name + " " + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Ticks.ToString() + ".txt";
        currentDiagnostics = new SearchDiagnostics();
        diagnostics.Add(currentDiagnostics);
    }

    public void startNewSearch()
    {
        currentDiagnostics = new SearchDiagnostics();
        diagnostics.Add(currentDiagnostics);
    }

    public void logSingleSearch()
    {
        string message = "SEARCH DIAGNOSTICS \n\n";
        message += "Total nodes searched: " + currentDiagnostics.nodesSearched + "\n";
        message += "Total time: " + currentDiagnostics.totalSearchTime + "\n";
        message += "Nodes/second: " + (currentDiagnostics.nodesSearched / currentDiagnostics.totalSearchTime.TotalMilliseconds * 1000).ToString() + "\n";

        message += "TT hits: " + currentDiagnostics.ttHits.ToString() + "\n";
        message += "TT stores: " + currentDiagnostics.ttHits.ToString() + "\n";

        message += "LMR total uses: " + (currentDiagnostics.timesReSearched_LMR + currentDiagnostics.timesNotReSearched_LMR).ToString() + "\n";
        message += "LMR successes: " + currentDiagnostics.timesNotReSearched_LMR.ToString() + "\n";
        message += "LMR re-searches: " + currentDiagnostics.timesReSearched_LMR.ToString() + "\n";

        message += "NMR total uses: " + (currentDiagnostics.timesReSearched_NMR + currentDiagnostics.timesNotReSearched_NMR).ToString() + "\n";
        message += "NMR successes: " + currentDiagnostics.timesNotReSearched_NMR.ToString() + "\n";
        message += "NMR re-searches: " + currentDiagnostics.timesReSearched_NMR.ToString() + "\n";

        message += "Move gen time: " + currentDiagnostics.moveGenTime + "\n";
        message += "Move order time: " + currentDiagnostics.moveOrderTime + "\n";
        message += "Quiescence time: " + currentDiagnostics.quiescenceTime + "\n";
        message += "Quiescence move gen time: " + currentDiagnostics.quiescenceGenTime + "\n";
        message += "Make/unmake time: " + currentDiagnostics.makeUnmakeTime + "\n";
        message += "Evaluation time: " + currentDiagnostics.moveGenTime + "\n";

        message += "New best move found on: \n";

        for (int moveNum = 0; moveNum < currentDiagnostics.numBestMovesPerIndex.Count(); moveNum++)
        {
            if (currentDiagnostics.numBestMovesPerIndex[moveNum] != 0)
            {
                message += $"{moveNum + 1}. {currentDiagnostics.numBestMovesPerIndex[moveNum]} | ";
            }
        }
        message += "\n";

        message += "Time spend per depth: \n";
        for (int depth = 0; depth < currentDiagnostics.msPerIteration.Count(); depth++)
        {
            if (currentDiagnostics.msPerIteration[depth] != 0 && depth > 20) { break; }
            else
            {
                message += $"{depth + 1}. {currentDiagnostics.msPerIteration[depth]} | ";
            }  
        }
        message += "\n\n";
        if (useDiagnostics)
        {
            AddToLog(message);
        }
    }

    public void logAllSearches() {

    }

    public void AddToLog(string message)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                writer.WriteLine(message);
            }
        }
        catch (Exception) { }
        ;

#if UNITY_EDITOR
        Debug.Log(message);
#endif
    }
}

public struct SearchDiagnostics
{
    public ulong ttHits;
    public ulong ttStores;
    public ulong ttEntries;
    public ulong ttOverwrites;

    //Nodes that had all legal moves generated for them (not quiescence, no TT)
    public ulong nodesSearched;

    //Any time a re-search was required - doesn't include how many times were generated
    public ulong timesReSearched_LMR;
    public ulong timesNotReSearched_LMR;

    public ulong timesReSearched_NMR;
    public ulong timesNotReSearched_NMR;

    public TimeSpan totalSearchTime;
    public TimeSpan moveGenTime;
    public TimeSpan moveOrderTime;
    public TimeSpan quiescenceGenTime;
    public TimeSpan quiescenceTime;
    public TimeSpan makeUnmakeTime;
    public TimeSpan evaluationTime;
    public int[] msPerIteration;
    public int[] numBestMovesPerIndex;
}
