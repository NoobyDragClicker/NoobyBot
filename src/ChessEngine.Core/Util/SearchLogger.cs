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
        this.useDiagnostics = useDiagnostics;
        logPath = folderPath + name + " " + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Ticks.ToString() + ".txt";
        currentDiagnostics = new SearchDiagnostics();
        diagnostics.Add(currentDiagnostics);
    }

    public void startNewSearch()
    {
        diagnostics.Add(currentDiagnostics);
        currentDiagnostics = new SearchDiagnostics();
    }

    public void logSingleSearch()
    {
        string message = "SEARCH DIAGNOSTICS \n\n";
        message += "Total nodes searched: " + currentDiagnostics.nodesSearched + "\n";
        message += "Total time: " + currentDiagnostics.totalSearchTime + "\n";
        message += "Nodes/second: " + (currentDiagnostics.nodesSearched / currentDiagnostics.totalSearchTime.TotalMilliseconds * 1000).ToString() + "\n";

        message += "TT hits: " + currentDiagnostics.ttHits.ToString() + "\n";
        message += "TT stores: " + currentDiagnostics.ttStores.ToString() + "\n";

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
        message += "Re-search time: " + currentDiagnostics.reSearchTime + "\n";
        message += "Evaluation time: " + currentDiagnostics.evaluationTime + "\n";

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

    public void logAllSearches()
    {
        SearchDiagnostics totaldiagnostics = new SearchDiagnostics();
        totaldiagnostics.ttHits = 0;
        totaldiagnostics.ttEntries = 0;
        totaldiagnostics.nodesSearched = 0;
        totaldiagnostics.msPerIteration = new int[100];
        totaldiagnostics.numBestMovesPerIndex = new int[300];
        AddToLog($"Num of search diagnostics: {diagnostics.Count}");

        for (int index = 0; index < diagnostics.Count; index++)
        {
            totaldiagnostics.ttHits += diagnostics[index].ttHits;
            totaldiagnostics.ttStores += diagnostics[index].ttStores;
            totaldiagnostics.ttEntries += diagnostics[index].ttEntries;
            totaldiagnostics.ttOverwrites += diagnostics[index].ttOverwrites;

            totaldiagnostics.nodesSearched += diagnostics[index].nodesSearched;

            totaldiagnostics.timesReSearched_LMR += diagnostics[index].timesReSearched_LMR;
            totaldiagnostics.timesNotReSearched_LMR += diagnostics[index].timesNotReSearched_LMR;

            totaldiagnostics.timesReSearched_NMR += diagnostics[index].timesReSearched_NMR;
            totaldiagnostics.timesNotReSearched_NMR += diagnostics[index].timesNotReSearched_NMR;

            totaldiagnostics.totalSearchTime += diagnostics[index].totalSearchTime;
            totaldiagnostics.reSearchTime += diagnostics[index].reSearchTime;
            totaldiagnostics.moveGenTime += diagnostics[index].moveGenTime;
            totaldiagnostics.moveOrderTime += diagnostics[index].moveOrderTime;
            totaldiagnostics.quiescenceGenTime += diagnostics[index].quiescenceGenTime;
            totaldiagnostics.quiescenceTime += diagnostics[index].quiescenceTime;
            totaldiagnostics.makeUnmakeTime += diagnostics[index].makeUnmakeTime;
            totaldiagnostics.evaluationTime += diagnostics[index].evaluationTime;

            /*for (int iteration = 0; iteration < diagnostics[index].msPerIteration.Length; iteration++)
            {
                totaldiagnostics.msPerIteration[iteration] += diagnostics[index].msPerIteration[iteration];
            }

            for (int moveNumber = 0; moveNumber < diagnostics[index].numBestMovesPerIndex.Length; moveNumber++)
            {
                totaldiagnostics.numBestMovesPerIndex[moveNumber] += diagnostics[index].numBestMovesPerIndex[moveNumber];
            }*/
        }

        currentDiagnostics = totaldiagnostics;
        logSingleSearch();
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
    public TimeSpan reSearchTime;
    public TimeSpan moveGenTime;
    public TimeSpan moveOrderTime;
    public TimeSpan quiescenceGenTime;
    public TimeSpan quiescenceTime;
    public TimeSpan makeUnmakeTime;
    public TimeSpan evaluationTime;
    public int[] msPerIteration;
    public int[] numBestMovesPerIndex;
}
