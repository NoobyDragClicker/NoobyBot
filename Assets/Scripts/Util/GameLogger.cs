using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class GameLogger
{
    const string errorPath = "./Assets/Scripts/Logs/Debug/";
    const string savesPath = "./Assets/Scripts/Logs/Saves/";

    public static void LogGame(Board board, int pathNumber){
        string path = errorPath + pathNumber.ToString() + ".txt";
        Move[] moves = board.gameMoveHistory.ToArray();
        string[] moveStrings = new string[board.gameMoveHistory.Count];
        int arrayIndex = 0;
        //Convert the moves to strings in the order they happened
        for(int x = moves.Count() - 1; x >= 0; x--){
            moveStrings[x] = ConvertMoveForFile(moves[arrayIndex]);
            arrayIndex++;
        }

        using (StreamWriter writer = new StreamWriter(path))
        {
            foreach (string moveString in moveStrings){
                writer.WriteLine(moveString);
            }
        }
    }

    public static List<Move> ReadMovesFromLog(string path){
        string[] moveStrings = File.ReadAllLines(path);
        List<Move> moves = new List<Move>();
        foreach (string moveString in moveStrings){
            moves.Add(ConvertToMove(moveString));
        }
        return moves;
    }

    public static string ConvertMoveForFile(Move move){
        string moveString = move.oldIndex.ToString() + " " + move.newIndex.ToString() + " " + (move.isCapture() ? "1": "0") + " " + move.flag.ToString();
        return moveString;
    }

    public static Move ConvertToMove(string moveString){
        string[] sections = moveString.Split(" ");

        int startIndex = int.Parse(sections[0]);
        int newIndex = int.Parse(sections[1]);
        bool isCapture = sections[2] == "0" ? false : true;
        int flag = int.Parse(sections[3]);

        return new Move(startIndex, newIndex, isCapture, flag);
    }

    
}
