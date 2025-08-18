using System;
using System.IO;

#if UNITY_EDITOR
using UnityEngine;
#endif

public class SearchLogger
{
    string logPath;
    public SearchLogger(string name, string folderPath)
    {
        logPath = folderPath + name + " " + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Ticks.ToString() + ".txt";
    }

    public void AddToLog(string message)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                writer.WriteLine(message);
            }
        } catch (Exception e){};
        
        #if UNITY_EDITOR
        Debug.Log(message);
        #endif
    }
}
