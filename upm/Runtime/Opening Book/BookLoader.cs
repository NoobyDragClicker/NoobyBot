using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


public class BookLoader
{
    const string bookPath = "C:/Users/Spencer/Desktop/Chess/book.txt";
    const string originalFile = "C:/Users/Spencer/Desktop/Chess/8moves_v3.pgn";
    List<Move[]> allLines = new List<Move[]>();
    bool isLoaded = false;


    public List<Move[]> getAllLines()
    {
        var copiedList = allLines.ToList();
        return copiedList;
    }

    public void loadBook()
    {
        if (!File.Exists(bookPath))
        {
            trimOriginalFile();
        }
        if (!isLoaded)
        {
            string[] lines = File.ReadAllLines(bookPath);
            for (int x = 0; x < lines.Length; x++)
            {
                allLines.Add(convertIntLine(lines[x]));
            }
            isLoaded = true;
        }
    }

    //Trims to just the PGN moves
    void trimOriginalFile()
    {
        string[] lines = File.ReadAllLines(originalFile);
        //Stores the line fragments
        string[] fullLines = new string[69400 / 2];

        int currentIndex = 0;
        for (int x = 0; x < lines.Count(); x++)
        {
            string currentLine = lines[x];
            //Removing blank/info lines
            if (currentLine != "" && currentLine[0] != '[')
            {
                if (currentIndex % 2 == 0)
                {
                    fullLines[currentIndex / 2] = currentLine;
                }
                else
                {
                    fullLines[(currentIndex - 1) / 2] += " " + currentLine;
                }
                currentIndex++;
            }
        }

        //Converting from moves to int notation
        string[] fullIntLines = new string[69400 / 2];

        for (int x = 0; x < 69400 / 2; x++)
        {
            //Generate the moves from the pgn lines
            Move[] moves = convertPGNLine(fullLines[x], x);
            string line = "";
            for (int y = 0; y < moves.Count(); y++)
            {
                //Convert to int and add to the line
                line += moves[y].GetIntValue().ToString() + ", ";
            }
            fullIntLines[x] = line;
        }

        File.WriteAllLines(bookPath, fullIntLines, Encoding.UTF8);
    }


    //PGN to move
    Move[] convertPGNLine(string line, int lineNum)
    {
        string[] sections = line.Split(" ");
        Move[] moves = new Move[16];
        int index = 0;
        Board board = new Board();
        board.setPosition(Board.startPos);

        for (int x = 0; x < sections.Count(); x++)
        {
            if (sections[x] != "1." && sections[x] != "2." && sections[x] != "3." && sections[x] != "4." && sections[x] != "5." && sections[x] != "6." && sections[x] != "7." && sections[x] != "8." && sections[x] != "1/2-1/2" && sections[x] != "0-1" && sections[x] != "1-0")
            {
                moves[index] = Coord.convertPGNMove(board, sections[x], lineNum);
                board.Move(moves[index], false);
                index++;
            }
        }
        return moves;
    }

    Move[] convertIntLine(string line)
    {
        string[] sections = line.Split(", ");
        
        Move[] moves = new Move[16];
        //-1 to remove blank at end
        for (int x = 0; x < sections.Count() - 1; x++)
        {
            moves[x] = Coord.getMoveFromIntValue(int.Parse(sections[x]));
        }
        return moves;
    }


}
