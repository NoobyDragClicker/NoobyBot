using System;
using System.Collections.Generic;
using System.IO;

public class Engine
{
    AIPlayer player;
    AISettings aiSettings = new AISettings(100, 0, 16);
    Board board;
    //BookLoader bookLoader;
    SearchLogger logger;
    SearchLogger testingLogger;
    bool hasStartedGame = false;
    const string name = "Nooby Bot Dev";
    public const string chessRoot = "C:/Users/Spencer/Desktop/Chess";
    public const string tuningRoot = chessRoot + "/Tuning/";


    static readonly string[] positionLabels = { "position", "fen", "moves" };
    static readonly string[] goLabels = { "go", "movetime", "wtime", "btime", "winc", "binc", "movestogo" };
    static readonly string[] perftLabels = { "perft", "position", "perftSuite" };
    static readonly string[] searchLabels = { "search", "depth", "positions" };
    static readonly string[] tuneLabels = { "positions", "kval", "continue" };


    public Engine()
    {
        board = new Board();
        logger = new SearchLogger(name, SearchLogger.LoggingLevel.Warning);
        testingLogger = new SearchLogger(name + "test", SearchLogger.LoggingLevel.Diagnostics);
        player = new AIPlayer(name, logger);
        player.onMoveChosen += MakeMove;
    }

    public void ReceiveCommand(string command)
    {
        command = command.Trim();
        string messageType = command.Split(' ')[0].ToLower();
        if (messageType != "isready") { player.logger.AddToLog("Received: " + command, SearchLogger.LoggingLevel.Info);}

        switch (messageType)
        {
            case "uci":
                Console.WriteLine("id name=NoobyBot");
                Console.WriteLine("id author=Me");
                Console.WriteLine("option name Hash type spin default 16 min 1 max 4096 ");
                Console.WriteLine("option name Threads type spin default 1 min 1 max 1 ");
                Console.WriteLine("uciok");
                player.logger.AddToLog("uciok", SearchLogger.LoggingLevel.Info);
                break;
            case "isready":
                Console.WriteLine("readyok");
                break;
            case "setoption":
                if (command.Contains("name Hash value "))
                {
                    int hashSize = TryGetLabelledValueInt(command, "value", new string[] { "test" });
                    aiSettings = new AISettings(100, 0, hashSize);
                }
                if(command.Contains("name Threads value"))
                {
                    break;
                }
                break;
            case "ucinewgame":
                board = new Board();
                player.NewGame(board, aiSettings);
                hasStartedGame = true;
                break;
            case "bench":
                SearchTester tester = new SearchTester(logger);
                tester.RunBench();
                break;
            case "position":
                if (!hasStartedGame)
                {
                   board = new Board();
                    player.NewGame(board, aiSettings);
                    hasStartedGame = true;
                }
                ProcessPositionCommand(command);
                break;
            case "go":
                ProcessGoCommand(command);
                break;
            case "test":
                ProcessTestCommand(command);
                break;
            case "tune":
                Tuner tuner = new Tuner();
                tuner.Tune("C:/Users/Spencer/Desktop/Chess/lichess-big3-resolved/lichess-big3-resolved.book", 30, 4096);
                break;
            case "stop":
                player.search.EndSearch();
                break;
            case "quit":
                player.NotifyGameOver();
                break;
            default:
                player.logger.AddToLog($"Unrecognized command: {messageType}", SearchLogger.LoggingLevel.Warning);
                break;
        }
    }

    void MakeMove(Move move, string name)
    {
        Console.WriteLine("bestmove " + convertMoveToUCI(move));
    }

    //Sets up board position
    void ProcessPositionCommand(string message)
    {
        // FEN
        if (message.ToLower().Contains("startpos"))
        {
            board.setPosition(Board.startPos, player.logger);
        }
        else if (message.ToLower().Contains("fen"))
        {
            string customFen = TryGetLabelledValue(message, "fen", positionLabels);
            board.setPosition(customFen, player.logger);
        }
        else
        {
            player.logger.AddToLog("Invalid position command (expected 'startpos' or 'fen')", SearchLogger.LoggingLevel.Warning);
        }

        // Moves
        string allMoves = TryGetLabelledValue(message, "moves", positionLabels);
        if (!string.IsNullOrEmpty(allMoves))
        {
            string[] moveList = allMoves.Split(' ');
            foreach (string move in moveList)
            {
                board.MakeMove(convertUCIMove(move), false);
            }
        }
    }

    void ProcessTestCommand(string message)
    {
        if (message.Contains("perft"))
        {
            Perft perft = new Perft(testingLogger);
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
            Evaluation evaluator = new Evaluation(testingLogger);
            Console.WriteLine(evaluator.EvaluatePosition(board));
        }
        else if (message.Contains("search"))
        {
            SearchTester tester = new SearchTester(testingLogger);
            int targetDepth = 6;
            int numPositions = 500;

            if (message.Contains("depth"))
            {
                targetDepth = TryGetLabelledValueInt(message, "depth", searchLabels);
            }
            if (message.Contains("positions"))
            {
                numPositions = TryGetLabelledValueInt(message, "positions", searchLabels);
            }
            tester.RunSearchSuite(numPositions, targetDepth);
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
        else if (message.Contains("infinite"))
        {
            player.NotifyToMove(TimeSpan.Zero, TimeSpan.Zero, Player.ClockType.Infinite);
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

        int movedPieceType = board.PieceAt(startSquare);
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