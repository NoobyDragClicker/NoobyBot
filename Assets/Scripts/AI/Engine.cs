using System;

public class Engine
{
    AIPlayer player;
    AISettings aiSettings = new AISettings(40, 10, 16, false);
    Board board;
    BookLoader bookLoader;
    Perft perft;
    bool hasStartedGame = false;
    const string name = "Nooby Bot v1.0.7";


    static readonly string[] positionLabels = new[] { "position", "fen", "moves" };
    static readonly string[] goLabels = new[] { "go", "movetime", "wtime", "btime", "winc", "binc", "movestogo" };
    static readonly string[] perftLabels = new[] { "perft", "position", "perftSuite" };

    public Engine()
    {
        bookLoader = new BookLoader();
        player = new AIPlayer(name);
        player.onMoveChosen += MakeMove;
    }

    public void ReceiveCommand(string command)
    {
        command = command.Trim();
        string messageType = command.Split(' ')[0].ToLower();
        if (messageType != "isready") { player.logger.AddToLog("Received: " + command);}

        switch (messageType)
        {
            case "uci":
                Console.WriteLine("id name=NoobyBot");
                Console.WriteLine("id author=Me");
                Console.WriteLine("uciok");
                player.logger.AddToLog("uciok");
                break;
            case "isready":
                Console.WriteLine("readyok");
                break;
            case "ucinewgame":
                player.logger.AddToLog("##############################");
                board = new Board();
                bookLoader.loadBook();
                player.NewGame(board, aiSettings, bookLoader);
                hasStartedGame = true;
                break;

            case "position":
                if (!hasStartedGame)
                {
                    board = new Board();
                    bookLoader.loadBook();
                    player.NewGame(board, aiSettings, bookLoader);
                }
                ProcessPositionCommand(command);
                break;
            case "go":
                ProcessGoCommand(command);
                break;
            case "stop":
                player.search.EndSearch();
                break;
            case "quit":
                player.NotifyGameOver();
                break;
            case "d":
                player.logger.AddToLog("n/a");
                break;
            default:
                player.logger.AddToLog($"Unrecognized command: {messageType}");
                break;
        }
    }

    void MakeMove(Move move, string name)
    {
        player.logger.AddToLog("Reached make move");
        try
        {
            board.Move(move, false);
            Console.WriteLine("bestmove " + convertMoveToUCI(move));
            player.logger.AddToLog("bestmove " + convertMoveToUCI(move));
        }
        catch (Exception e)
        {
            player.logger.AddToLog(e.Message);
        }
        
    }

    //Sets up board position
    void ProcessPositionCommand(string message)
    {
        // FEN
        if (message.ToLower().Contains("startpos"))
        {
            board.setPosition(Board.startPos, new MoveGenerator());
            player.ResetOpeningBook(bookLoader);
        }
        else if (message.ToLower().Contains("fen"))
        {
            string customFen = TryGetLabelledValue(message, "fen", positionLabels);
            board.setPosition(customFen, new MoveGenerator());
            player.isInBook = false;
        }
        else
        {
            player.logger.AddToLog("Invalid position command (expected 'startpos' or 'fen')");
        }

        // Moves
        string allMoves = TryGetLabelledValue(message, "moves", positionLabels);
        if (!string.IsNullOrEmpty(allMoves))
        {
            string[] moveList = allMoves.Split(' ');
            foreach (string move in moveList)
            {
                board.Move(convertUCIMove(move), false);
            }

        }
    }

    //Synchronises the clock and tells the player to move
    void ProcessGoCommand(string message)
    {
        if (message.Contains("movetime"))
        {
            int moveTimeMs = TryGetLabelledValueInt(message, "movetime", goLabels, 0);
            player.NotifyToMove(TimeSpan.FromMilliseconds(moveTimeMs), TimeSpan.Zero, Player.ClockType.PerMove);
        }
        else if (message.Contains("perft"))
        {
            perft = new Perft(player.logger);
            int depth = 6;
            if (message.Contains("depth"))
            {
                depth = TryGetLabelledValueInt(message, "depth", perftLabels);
            }

            bool quiescence = message.Contains("quiescence") ? true : false;

            if (message.Contains("suite"))
            {
                perft.StartSuite(120, depth, quiescence);
            }
            else
            {
                perft.StartSearchDivide(board, depth);
            }

        }
        else if (message.Contains("static"))
        {
            Evaluation evaluator = new Evaluation();
            Console.WriteLine(evaluator.EvaluatePosition(board, new AISettings()));
        }
        else
        {
            int timeRemainingWhiteMs = TryGetLabelledValueInt(message, "wtime", goLabels, 0);
            int timeRemainingBlackMs = TryGetLabelledValueInt(message, "btime", goLabels, 0);
            int incrementWhiteMs = TryGetLabelledValueInt(message, "winc", goLabels, 0);
            int incrementBlackMs = TryGetLabelledValueInt(message, "binc", goLabels, 0);

            if (board.colorTurn == Piece.White)
            {
                timeRemainingWhiteMs = (timeRemainingWhiteMs == 0) ? 500 : timeRemainingWhiteMs;
                player.NotifyToMove(TimeSpan.FromMilliseconds(timeRemainingWhiteMs), TimeSpan.FromMilliseconds(incrementWhiteMs), Player.ClockType.Regular);
            }
            else
            {
                timeRemainingBlackMs = (timeRemainingBlackMs == 0) ? 500 : timeRemainingBlackMs;
                player.NotifyToMove(TimeSpan.FromMilliseconds(timeRemainingBlackMs), TimeSpan.FromMilliseconds(incrementBlackMs), Player.ClockType.Regular);
            }
        }

    }

    //Gets the int value from a received message by removing the label 
    static int TryGetLabelledValueInt(string text, string label, string[] allLabels, int defaultValue = 100)
    {
        string valueString = TryGetLabelledValue(text, label, allLabels, defaultValue + "");
        if (int.TryParse(valueString.Split(' ')[0], out int result))
        {
            return result;
        }
        return defaultValue;
    }

    //Removes labels such as fen, moves, from a received message
    static string TryGetLabelledValue(string text, string label, string[] allLabels, string defaultValue = "")
    {
        text = text.Trim();
        if (text.Contains(label))
        {
            //Removing the label from the start
            int valueStart = text.IndexOf(label) + label.Length;
            int valueEnd = text.Length;

            foreach (string otherID in allLabels)
            {
                if (otherID != label && text.Contains(otherID))
                {
                    int otherIDStartIndex = text.IndexOf(otherID);

                    //Finding another label as the end
                    if (otherIDStartIndex > valueStart && otherIDStartIndex < valueEnd)
                    {
                        valueEnd = otherIDStartIndex;
                    }
                }
            }

            return text.Substring(valueStart, valueEnd - valueStart).Trim();
        }
        return defaultValue;
    }

    public Move convertUCIMove(string moveName)
    {
        int startSquare = Coord.NotationToIndex(moveName.Substring(0, 2));
        int targetSquare = Coord.NotationToIndex(moveName.Substring(2, 2));

        int movedPieceType = Piece.PieceType(board.board[startSquare]);
        bool isCapture = false;

        // Figure out move flag
        int flag = 0;

        if (movedPieceType == Piece.Pawn)
        {
            // Promotion
            if (moveName.Length > 4)
            {
                flag = moveName[^1] switch
                {
                    'q' => 1,
                    'r' => 4,
                    'n' => 3,
                    'b' => 2,
                    _ => 0
                };
            }
            // Double pawn push
            else if (Math.Abs(Coord.IndexToRank(targetSquare) - Coord.IndexToRank(startSquare)) == 2)
            {
                flag = 6;
            }
            // En-passant
            else if (Coord.IndexToFile(startSquare) != Coord.IndexToFile(targetSquare) && board.board[targetSquare] == Piece.None)
            {
                flag = 7;
                isCapture = true;
            }
        }
        else if (movedPieceType == Piece.King)
        {
            if (Math.Abs(Coord.IndexToFile(startSquare) - Coord.IndexToFile(targetSquare)) > 1)
            {
                flag = 5;
            }
        }
        if (board.board[targetSquare] != Piece.None) { isCapture = true; }

        return new Move(startSquare, targetSquare, isCapture, flag);
    }


    public static string convertMoveToUCI(Move move)
    {
        string moveFlag = "";
        switch (move.flag)
        {
            case 1:
                moveFlag = "q";
                break;
            case 2:
                moveFlag = "b";
                break;
            case 3:
                moveFlag = "n";
                break;
            case 4:
                moveFlag = "r";
                break;
            default:
                break;
        }

        string moveString = Coord.GetNotationFromIndex(move.oldIndex) + Coord.GetNotationFromIndex(move.newIndex) + moveFlag;
        return moveString;
    }
}