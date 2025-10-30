using System.Collections.Generic;
using System.Linq;
using System;
public class OpeningBook
{
    BookLoader loader;
    List<Move[]> allLines;

    public OpeningBook(BookLoader bookLoader)
    {
        loader = bookLoader;
        allLines = loader.getAllLines();
    }

    public Move getBookMove(Board board){
        int depth = 0;//board.gameMoveHistory.Count();
        Move chosenMove = Search.nullMove;
        Dictionary<int, int> possibleMovesInPos = new Dictionary<int, int>();
        
        int totalLines = 0;
        int maxLines = allLines.Count();


        //First move of the game
        if (depth == 0){
            //Counting different options
            for (int x = 0; x < maxLines; x++){
                
                if (possibleMovesInPos.ContainsKey(allLines[x][depth].GetIntValue())){
                    possibleMovesInPos[allLines[x][depth].GetIntValue()] += 1;
                } else {
                    possibleMovesInPos[allLines[x][depth].GetIntValue()] = 1;    
                    totalLines++;
                }
            }
        } 
        else {
            int previousMoveVal = 0;//board.gameMoveHistory.Peek().GetIntValue();
            //Removing lines from previous player's move while simultaneously counting the different options
            for (int x = 0; x < maxLines; x++){
                if (allLines[x][depth - 1].GetIntValue() == previousMoveVal){
                    if (possibleMovesInPos.ContainsKey(allLines[x][depth].GetIntValue())){
                        possibleMovesInPos[allLines[x][depth].GetIntValue()] += 1;
                    } else{
                        possibleMovesInPos[allLines[x][depth].GetIntValue()] = 1;
                    }
                    totalLines++;
                }
                else {
                    allLines.RemoveAt(x);
                    maxLines--;
                    x--;
                }
            }
        }

        if (totalLines == 0) {
            return chosenMove;
        }

        //Questionable
        Random random = new Random();
        int chosenNum = random.Next(1, maxLines);
        int sumSoFar = 0;

        foreach (KeyValuePair<int, int> pair in possibleMovesInPos){
            //This is the chosen move
            if (chosenNum >= sumSoFar && chosenNum <= sumSoFar + pair.Value){
                chosenMove = Coord.getMoveFromIntValue(pair.Key);
                break;
            }
            sumSoFar += pair.Value;
        }

        int maxNewLines = allLines.Count();
        //Removing the lines that were not chosen
        for (int x = 0; x < maxNewLines; x++){
            if (allLines[x][depth].GetIntValue() != chosenMove.GetIntValue()){
                allLines.RemoveAt(x);
                maxNewLines--;
                x--;
            }
        }

        return chosenMove;
    }

}
