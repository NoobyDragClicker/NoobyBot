using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;

public class BookLoader
{
    const string bookPath = "./Assets/Scripts/Opening Book/book.txt";
    const string originalFile = "./Assets/Scripts/Opening Book/8moves_v3.pgn";
    List<Move[]> allLines = new List<Move[]>();
    bool hasLoaded;
    public BookLoader(){
        loadBook();
    }

    public List<Move[]> getAllLines(){
        var copiedList = allLines.ToList();  
        return copiedList;
    }

    void loadBook(){
        hasLoaded = true;
        if (!File.Exists(bookPath)){
            Debug.Log("No file found, trimming from main");
            trimOriginalFile();
        }
        string[] lines = File.ReadAllLines(bookPath);
        for (int x = 0; x < lines.Length; x++){
            allLines.Add(convertPGNLine(lines[x], x+1));
        }
    }

    void trimOriginalFile(){
        string[] lines = File.ReadAllLines(originalFile);
        //Stores the line fragments
        string[] fullLines = new string[69400/2];

        int currentIndex = 0;
        for (int x = 0; x < lines.Count(); x++){
            string currentLine = lines[x];
            //Removing blank/info lines
            if (currentLine != "" && currentLine[0] != '['){
                if (currentIndex % 2 == 0){
                    fullLines[currentIndex/2] = currentLine;
                } else{
                    fullLines[(currentIndex - 1) / 2] += " " + currentLine;
                }
                currentIndex++;
            }
        }
        File.WriteAllLines(bookPath, fullLines, Encoding.UTF8);
    }

    Move[] convertPGNLine(string line, int lineNum){
        string[] sections = line.Split(" ");
        Move[] moves = new Move[16];
        int index = 0;
        Board board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", new MoveGenerator());
        for (int x = 0; x<sections.Count(); x++){
            if (sections[x] != "1." && sections[x] != "2." && sections[x] != "3." && sections[x] != "4." && sections[x] != "5." && sections[x] != "6." && sections[x] != "7." && sections[x] != "8." && sections[x] != "1/2-1/2" && sections[x] != "0-1" && sections[x] != "1-0"){
                moves[index] = Coord.convertPGNMove(board, sections[x], lineNum);
                board.Move(moves[index], false);
                index++;
            }
        }
        return moves;
    }


}
