using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;

public class TexelTuner
{
    TuneParam[] parameters;
    Evaluation evaluation;
    SearchLogger logger;
    int numParameters = 1;
    //1.43 for external, 0.75 for internal
    public float K = 0.75f;

    //Loading from eval, loading from file, and saving to file is known working + all dependancies

    public TexelTuner(SearchLogger logger)
    {
        this.logger = logger;
        evaluation = new Evaluation(logger);
    }

   public void TuneFromFile(string paramInput, string paramOutput, string positionsFile, int batchSize)
{
    LoadParams(paramInput);
    UpdateAllParamsInEval();

    string[] fens = File.ReadAllLines(positionsFile);
    RandomisePositions(fens);
    (string, double)[] fenAndResult = convertFensToResults(fens);
    int posCount = fenAndResult.Length;

    bool hasImproved = true;
    Random rand = new Random();

    // Calculate starting global MeanSquareError
    double meanSquareError = CalculateMeanSquareError(posCount, fenAndResult);
    double bestGlobalMSE = meanSquareError;
    int stagnantIterations = 0;
    int maxStagnant = posCount/batchSize; // stop if no improvement once basically all positions have been used

    Console.WriteLine($"Starting global MSE = {meanSquareError}");

    Stopwatch stopwatch = new Stopwatch();
    while (hasImproved && stagnantIterations < maxStagnant)
    {
        hasImproved = false;
        stopwatch.Restart();

        // Reorder parameters based on counters
        parameters = parameters.OrderByDescending(p => p.counter).ToArray();

        Console.WriteLine("Started next iteration");
        for (int paramNum = 0; paramNum < numParameters; paramNum++)
        {

            // Sample a mini-batch
            var batch = SampleBatch(fenAndResult, batchSize, rand);
            double localMeanSquareError = CalculateMeanSquareError(batch.Length, batch);
            
            int startingParamValue = parameters[paramNum].value;
            int newParamValue = startingParamValue + parameters[paramNum].delta;

            UpdateParamInEval(parameters[paramNum].name, newParamValue, parameters[paramNum].index);
            double newMeanSquareError = CalculateMeanSquareError(batch.Length, batch);

            if (newMeanSquareError < localMeanSquareError)
            {
                hasImproved = true;
                parameters[paramNum].value = newParamValue;
                //if (Math.Abs(parameters[paramNum].delta) < 2) { parameters[paramNum].delta *= 2; }
                parameters[paramNum].counter++;
                meanSquareError = newMeanSquareError;
                continue;
            }
            else
            {
                // Try opposite direction
                parameters[paramNum].delta *= -1;
                //parameters[paramNum].delta = (parameters[paramNum].delta == 1) ? -parameters[paramNum].delta : -(parameters[paramNum].delta / 2);
                newParamValue = startingParamValue + parameters[paramNum].delta;

                UpdateParamInEval(parameters[paramNum].name, newParamValue, parameters[paramNum].index);
                newMeanSquareError = CalculateMeanSquareError(batch.Length, batch);

                if (newMeanSquareError > localMeanSquareError)
                {
                    parameters[paramNum].delta *= -1;
                    //parameters[paramNum].delta = (Math.Abs(parameters[paramNum].delta) == 1) ? -parameters[paramNum].delta : -(parameters[paramNum].delta / 2);
                    UpdateParamInEval(parameters[paramNum].name, startingParamValue, parameters[paramNum].index);
                }
                else
                {
                    hasImproved = true;
                    //if (Math.Abs(parameters[paramNum].delta) < 8) { parameters[paramNum].delta *= 2; }
                    parameters[paramNum].value = newParamValue;
                    parameters[paramNum].counter++;
                    meanSquareError = newMeanSquareError;
                }
            }
        }

        // At end of iteration, recompute global MSE and reset baseline
        double globalMSE = CalculateMeanSquareError(posCount, fenAndResult);
        meanSquareError = globalMSE;
        Console.WriteLine($"Iteration finished. Global MSE = {globalMSE}");

            if (globalMSE < bestGlobalMSE - 1e-6) // improvement threshold
            {
                bestGlobalMSE = globalMSE;
                stagnantIterations = 0;
                // Save good parameters
                SaveParams(paramOutput);
            }
            else
            {
                stagnantIterations++;
                Console.WriteLine($"No global improvement. Stagnant count = {stagnantIterations}/{maxStagnant}");
                //Load the last best params
                LoadParams(paramOutput);
                UpdateAllParamsInEval();
            }

        
        stopwatch.Stop();
        Console.WriteLine("Finished in: " + stopwatch.Elapsed);
    }
}

    public void ConvertToLabeledFile(string inputFile, string outputFile)
    {
        string[] starting = File.ReadAllLines(inputFile);
        string[,] split = new string[starting.Length, 2];
        for (int index = 0; index < starting.Length; index++)
        {
            split[index, 0] = starting[index].Split('"')[0];
            split[index, 1] = starting[index].Split('"')[1];
        }
        string[] final = new string[starting.Length];
        for (int index = 0; index < starting.Length; index++)
        {
            final[index] = split[index, 0] + "; " + split[index, 1];
        }
        File.WriteAllLines(outputFile, final);

    } 


    //Splits fens into fens and their results
    public (string, double)[] convertFensToResults(string[] fens)
    {
        (string, double)[] pairs = new (string, double)[fens.Length];

        for (int index = 0; index < fens.Length; index++)
        {
            string[] fenAndResult = fens[index].Split(";");
            double result = 0.5;
            if (fenAndResult[1].Contains("1-0")) { result = 1.0; }
            else if (fenAndResult[1].Contains("0-1")) { result = 0.0; }
            pairs[index] = (fenAndResult[0], result);
        }
        return pairs;
    }

    //Creates a random batch
    public (string, double)[] SampleBatch((string, double)[] allData, int batchSize, Random rng)
    {
        var batch = new (string, double)[batchSize];
        for (int i = 0; i < batchSize; i++)
        {
            int idx = rng.Next(allData.Length);
            batch[i] = allData[idx];
        }
        return batch;
    }

    //Sets all the params in eval to the loaded params
    public void UpdateAllParamsInEval()
    {
        foreach (TuneParam parameter in parameters)
        {
            UpdateParamInEval(parameter.name, parameter.value, parameter.index);
        }
    }
    public void CreateCodeFromParams(string inputFile, string outputFile)
    {
        LoadParams(inputFile);
        int pawnValue = 0;
        int bishopValue = 0;
        int knightValue = 0;
        int rookValue = 0;
        int queenValue = 0;

        int[] mg_pawn_table = new int[64];
        int[] eg_pawn_table = new int[64];

        int[] mg_knight_table = new int[64];
        int[] eg_knight_table = new int[64];

        int[] mg_bishop_table = new int[64];
        int[] eg_bishop_table = new int[64];

        int[] mg_rook_table = new int[64];
        int[] eg_rook_table = new int[64];

        int[] mg_queen_table = new int[64];
        int[] eg_queen_table = new int[64];

        int[] mg_king_table = new int[64];
        int[] eg_king_table = new int[64];

        int[] passedPawnBonuses = new int[8];
        int[] isolatedPawnPenalty = new int[9];

        foreach (TuneParam param in parameters)
        {
            switch (param.name)
            {
                case "pawnVal": pawnValue = param.value; break;
                case "bishopVal": bishopValue = param.value; break;
                case "knightVal": knightValue = param.value; break;
                case "rookVal": rookValue = param.value; break;
                case "queenVal": queenValue = param.value; break;
                case "mg_pawn_table": mg_pawn_table[param.index] = param.value; break;
                case "eg_pawn_table": eg_pawn_table[param.index] = param.value; break;

                case "mg_bishop_table": mg_bishop_table[param.index] = param.value; break;
                case "eg_bishop_table": eg_bishop_table[param.index] = param.value; break;

                case "mg_knight_table": mg_knight_table[param.index] = param.value; break;
                case "eg_knight_table": eg_knight_table[param.index] = param.value; break;

                case "mg_rook_table": mg_rook_table[param.index] = param.value; break;
                case "eg_rook_table": eg_rook_table[param.index] = param.value; break;

                case "mg_queen_table": mg_queen_table[param.index] = param.value; break;
                case "eg_queen_table": eg_queen_table[param.index] = param.value; break;

                case "mg_king_table": mg_king_table[param.index] = param.value; break;
                case "eg_king_table": eg_king_table[param.index] = param.value; break;

                case "passedPawnBonuses": passedPawnBonuses[param.index] = param.value; break;
                case "isolatedPawnPenalty": isolatedPawnPenalty[param.index] = param.value; break;
                default: logger.AddToLog("Param not found: " + param.name, SearchLogger.LoggingLevel.Warning); break;
            }
        }
        string[] codeSnippets = {$"public static int pawnValue = {pawnValue};",
            $"public static int knightValue = {knightValue};",
            $"public static int bishopValue = {bishopValue};",
            $"public static int rookValue = {rookValue};",
            $"public static int queenValue = {queenValue};",
            $"public int[] mg_pawn_table =  {{{string.Join(", ", mg_pawn_table)}}};",
            $"public int[] eg_pawn_table = {{{string.Join(", ", eg_pawn_table)}}};",

            $"public int[] mg_knight_table = {{{string.Join(", ", mg_knight_table)}}};",
            $"public int[] eg_knight_table = {{{string.Join(", ", eg_knight_table)}}};",

            $"public int[] mg_bishop_table = {{{string.Join(", ", mg_bishop_table)}}};",
            $"public int[] eg_bishop_table = {{{string.Join(", ", eg_bishop_table)}}};",

            $"public int[] mg_rook_table = {{{string.Join(", ", mg_rook_table)}}};",
            $"public int[] eg_rook_table = {{{string.Join(", ", eg_rook_table)}}};",

            $"public int[] mg_queen_table = {{{string.Join(", ", mg_queen_table)}}};",
            $"public int[] eg_queen_table = {{{string.Join(", ", eg_queen_table)}}};",

            $"public int[] mg_king_table = {{{string.Join(", ", mg_king_table)}}};",
            $"public int[] eg_king_table = {{{string.Join(", ", eg_king_table)}}};",

            $"public int[] passedPawnBonuses = {{{string.Join(", ", passedPawnBonuses)}}};",
            $"public int[] isolatedPawnPenalty = {{{string.Join(", ", isolatedPawnPenalty)}}};" };

        File.WriteAllLines(outputFile, codeSnippets);


    }

    //Initially tuning K by brute force
    public float TuneKVal(string positionsFile)
    {
        bool hasImproved = true;
        string[] fens = File.ReadAllLines(positionsFile);
        (string, double)[] fensAndResult = convertFensToResults(fens);
        double meanSquareError = CalculateMeanSquareError(fens.Length, fensAndResult);
        int dir = 1;

        int iteration = 0;
        Console.WriteLine("Starting first iteration");
        while (hasImproved && iteration < 100000)
        {
            iteration++;
            hasImproved = false;
            K += 0.01f * dir;
            double newMeanSquareError = CalculateMeanSquareError(fens.Length, fensAndResult);
            if (newMeanSquareError < meanSquareError) { hasImproved = true; meanSquareError = newMeanSquareError; }
            else
            {
                dir *= -1;
                K += 0.02f * dir;
                newMeanSquareError = CalculateMeanSquareError(fens.Length, fensAndResult);
                if (newMeanSquareError < meanSquareError) { hasImproved = true; meanSquareError = newMeanSquareError; }
            }
            Console.WriteLine(iteration + " " + K + " " + meanSquareError);
        }
        return K;
    }

    public void RandomisePositions(string[] positions)
    {
        Random rand = new Random();
        for (int index = 0; index < positions.Length; index++)
        {
            int newIndex = rand.Next(0, positions.Length);
            string temp = positions[newIndex];
            positions[newIndex] = positions[index];
            positions[index] = temp;
        }
    }

    public double CalculateMeanSquareError(int numPosToUse, (string, double)[] fenStrings)
    {
        Board board = new Board();
        double sum = 0;
        for (int index = 0; index < numPosToUse; index++)
        {
            board.setPosition(fenStrings[index].Item1, logger);

            int whiteRelativeEval = (board.colorTurn == Piece.White) ? evaluation.EvaluatePosition(board) : -1 * evaluation.EvaluatePosition(board);
            double sigmoidEval = Sigmoid(whiteRelativeEval);

            double diff = fenStrings[index].Item2 - sigmoidEval;
            sum += diff * diff;
        }
        return sum / numPosToUse;
    }

    public double Sigmoid(double s)
    {
        return 1.0 / (1.0 + Math.Pow(10, -K * s / 400.0));
    }

    void UpdateParamInEval(string name, int newEval, int index)
    {

        switch (name)
        {
            case "pawnVal": Evaluation.pawnValue = newEval; break;
            case "knightVal": Evaluation.knightValue = newEval; break;
            case "bishopVal": Evaluation.bishopValue = newEval; break;
            case "rookVal": Evaluation.rookValue = newEval; break;
            case "queenVal": Evaluation.queenValue = newEval; break;

            case "mg_pawn_table": evaluation.mg_pawn_table[index] = newEval; break;
            case "eg_pawn_table": evaluation.eg_pawn_table[index] = newEval; break;

            case "mg_knight_table": evaluation.mg_knight_table[index] = newEval; break;
            case "eg_knight_table": evaluation.eg_knight_table[index] = newEval; break;

            case "mg_bishop_table": evaluation.mg_bishop_table[index] = newEval; break;
            case "eg_bishop_table": evaluation.eg_bishop_table[index] = newEval; break;

            case "mg_rook_table": evaluation.mg_rook_table[index] = newEval; break;
            case "eg_rook_table": evaluation.eg_rook_table[index] = newEval; break;

            case "mg_queen_table": evaluation.mg_queen_table[index] = newEval; break;
            case "eg_queen_table": evaluation.eg_queen_table[index] = newEval; break;

            case "mg_king_table": evaluation.mg_king_table[index] = newEval; break;
            case "eg_king_table": evaluation.eg_king_table[index] = newEval; break;

            case "isolatedPawnPenalty": evaluation.isolatedPawnPenalty[index] = newEval; break;
            case "passedPawnBonuses": evaluation.passedPawnBonuses[index] = newEval; break;
            default: logger.AddToLog("No parameter found: " + name, SearchLogger.LoggingLevel.Deadly); break;
        }

    }

    //Converts from the name of the parameter and its value to the actual object
    public TuneParam ConvertParam(string name, int paramValue)
    {
        TuneParam param = new TuneParam();
        param.name = name;
        param.counter = 0;
        param.delta = 1;
        param.index = -1;
        param.value = paramValue;
        return param;
    }

    public TuneParam[] ConvertArrayToParams(string name, int[] paramValues)
    {
        TuneParam[] parameters = new TuneParam[paramValues.Length];

        for (int index = 0; index < paramValues.Length; index++)
        {
            parameters[index] = ConvertParam(name, paramValues[index]);
            parameters[index].index = index;
        }
        return parameters;
    }

    //Loads parameters from a given parameter file
    public void LoadParams(string inputFile)
    {
        string[] paramStrings = File.ReadAllLines(inputFile);
        parameters = new TuneParam[paramStrings.Length];
        numParameters = paramStrings.Length;

        for (int index = 0; index < paramStrings.Length; index++)
        {
            parameters[index] = new TuneParam();
            parameters[index].ConvertFromString(paramStrings[index]);
        }
    }

    //Saves parameters from the eval function to a file;
    public void SaveParametersFromEval(string outputFile)
    {
        List<TuneParam> loadedParams = new List<TuneParam>();
        //Piece values
        loadedParams.Add(ConvertParam("pawnVal", Evaluation.pawnValue));
        loadedParams.Add(ConvertParam("knightVal", Evaluation.knightValue));
        loadedParams.Add(ConvertParam("bishopVal", Evaluation.bishopValue));
        loadedParams.Add(ConvertParam("rookVal", Evaluation.rookValue));
        loadedParams.Add(ConvertParam("queenVal", Evaluation.queenValue));
        
        loadedParams.AddRange(ConvertArrayToParams("mg_pawn_table", evaluation.mg_pawn_table));
        loadedParams.AddRange(ConvertArrayToParams("eg_pawn_table", evaluation.eg_pawn_table));

        loadedParams.AddRange(ConvertArrayToParams("mg_knight_table", evaluation.mg_knight_table));
        loadedParams.AddRange(ConvertArrayToParams("eg_knight_table", evaluation.eg_knight_table));

        loadedParams.AddRange(ConvertArrayToParams("mg_bishop_table", evaluation.mg_bishop_table));
        loadedParams.AddRange(ConvertArrayToParams("eg_bishop_table", evaluation.eg_bishop_table));

        loadedParams.AddRange(ConvertArrayToParams("mg_rook_table", evaluation.mg_rook_table));
        loadedParams.AddRange(ConvertArrayToParams("eg_rook_table", evaluation.eg_rook_table));

        loadedParams.AddRange(ConvertArrayToParams("mg_queen_table", evaluation.mg_queen_table));
        loadedParams.AddRange(ConvertArrayToParams("eg_queen_table", evaluation.eg_queen_table));

        loadedParams.AddRange(ConvertArrayToParams("mg_king_table", evaluation.mg_king_table));
        loadedParams.AddRange(ConvertArrayToParams("eg_king_table", evaluation.eg_king_table));

        loadedParams.AddRange(ConvertArrayToParams("passedPawnBonuses", evaluation.passedPawnBonuses));
        loadedParams.AddRange(ConvertArrayToParams("isolatedPawnPenalty", evaluation.isolatedPawnPenalty));
        
        
        numParameters = loadedParams.Count();
        parameters = new TuneParam[numParameters];
        for (int index = 0; index < numParameters; index++)
        {
            parameters[index] = loadedParams[index];
        }
        SaveParams(outputFile);
    }

    //Saves the current parameters to a file
    public void SaveParams(string outputFile)
    {
        string[] paramStrings = new string [numParameters];
        for (int index = 0; index < numParameters; index++)
        {
            paramStrings[index] = parameters[index].GetStringVal();
        }
        File.WriteAllLines(outputFile, paramStrings);
    }

    //Primary function, takes a full match output and converts to a .epd file of quiet fens
    public void ConvertPGNFileToQuietFens(string inputPath, string outputPath, int numGames, SearchLogger logger)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Console.WriteLine("Removing PGN filler");
        string[] games = RemovePGNFiller(inputPath);
        Console.WriteLine("PGN filler removed");



        numGames = (numGames > games.Length) ? games.Length : numGames;
        Console.WriteLine($"{numGames} games found");
        List<string> fens = new List<string>();

        //Extract all the quiet fens, disregard distribution of positions
        Console.WriteLine("Converting games");
        for (int gameIndex = 1; gameIndex < numGames; gameIndex++)
        {
            if (games != null)
            {
                fens.AddRange(ConvertGameToQuietFens(games[gameIndex]));
            }
        }
        Console.WriteLine("Extracted " + fens.Count() + " positions");

        //Equal distribution
        Console.WriteLine("Counting phase numbers");
        int[] phaseNums = numPerPhase(convertFensToResults(fens.ToArray()));
        Console.WriteLine(string.Join(", ", phaseNums));
        int total = 0;
        foreach (int phaseNum in phaseNums){ total += phaseNum; }
        int expectedPerPhase = total / phaseNums.Length;
        int[] boundPerThousand = new int[phaseNums.Length];
        Console.WriteLine($"TOTAL: {total}  PER PHASE: {expectedPerPhase}");
        for (int index = 0; index < phaseNums.Length; index++)
        {
            if (phaseNums[index] < expectedPerPhase) { boundPerThousand[index] = 0; }
            else
            {
                float expectedToPass = ((float)expectedPerPhase / (float)phaseNums[index]) * 1000f;
                boundPerThousand[index] = (int)(1000 - expectedToPass);
            }
        }
        Console.WriteLine(string.Join(", ", boundPerThousand));
        List<string> chosenFens = new List<string>();
        Random rand = new Random();
        Board board = new Board();
        for (int index = 0; index < fens.Count; index++)
        {
            board.setPosition(fens[index], logger);
            int phase = phasePerBoard(board);
            if (boundPerThousand[phase] != 0)
            {
                int random = rand.Next(0, 1000);
                if(random > boundPerThousand[phase]){ chosenFens.Add(fens[index]); }
            } else{ chosenFens.Add(fens[index]); }
        }

        File.AppendAllLines(outputPath, chosenFens);
        stopwatch.Stop();
        logger.AddToLog(stopwatch.Elapsed.ToString(), SearchLogger.LoggingLevel.Diagnostics);
        Console.WriteLine("Completed in: " + stopwatch.Elapsed);
    }

    //Returns full game lines from a full match output, 1 game/line
    public static string[] RemovePGNFiller(string inputGames)
    {
        string[] lines = File.ReadAllLines(inputGames);

        string[] gameLines = new string[lines.Count()];

        int numGameLines = 0;
        for (int index = 0; index < lines.Count(); index++)
        {
            //Not random filler
            if (lines[index] != "" && lines[index][0] != '[' && lines[index] != "\n")
            {
                if (lines[index].Substring(0, 2) == "1.")
                {
                    numGameLines++;
                    gameLines[numGameLines] += lines[index];
                }
                else
                {
                    gameLines[numGameLines] += lines[index];
                }
            }
        }
        string[] actualGameLines = gameLines[0..numGameLines];
        return actualGameLines;
    }

    public int[] numPerPhase((string, double)[] fensAndResults)
    {
        Board board = new Board();
        int[] phaseNums = new int[25];

        foreach ((string, double) info in fensAndResults)
        {
            
            board.setPosition(info.Item1, logger);
            int phase = phasePerBoard(board);
            phaseNums[phase]++;
        }
        return phaseNums;
    }

    public void positionDiagnostic(string filePath)
    {
        (string, double)[] fenAndResults = convertFensToResults(File.ReadAllLines(filePath));
        int[] phaseNums = numPerPhase(fenAndResults);
        Console.WriteLine(string.Join(", ", phaseNums));

        int numDraws = 0;
        int numWhiteWins = 0;
        int numBlackWins = 0;
        foreach ((string, double) pair in fenAndResults)
        {
            if (pair.Item2 == 0.5)
            {
                numDraws++;
            }
            else if (pair.Item2 == 1.0)
            {
                numWhiteWins++;
            }
            else if (pair.Item2 == 0.0)
            {
                numBlackWins++;
            }
        }

        Console.WriteLine($"TOTAL: {numDraws + numWhiteWins + numBlackWins} | Draws: {numDraws} | White Wins: {numWhiteWins} | Black Wins: {numBlackWins}");
    }

    public int phasePerBoard(Board board)
    {
        int phase = 0;
        for (int index = 0; index < 64; index++)
        {
            int pieceType = Piece.PieceType(board.board[index]);
            switch (pieceType)
            {
                case Piece.Queen: phase += 4; break;
                case Piece.Bishop: phase += 1; break;
                case Piece.Knight: phase += 1; break;
                case Piece.Rook: phase += 2; break;
            }
        }
        if (phase > 24) { phase = 24; }
        return phase;
    }


    //Returns all the semiquiet fens from a game line
    public List<string> ConvertGameToQuietFens(string game)
    {
        string[] turns = game.Split(". ");

        (Move, string)[] moveInfoPairs = new (Move, string)[turns.Length * 2];
        Board board = new Board();
        board.setPosition(Board.startPos, logger);

        List<string> quietFens = new List<string>();
        Evaluation evaluator = new Evaluation(logger);
        Search search = new Search(board, new AISettings(40, 16, 64), new Move[1024, 3], new int[64, 64], logger);

        int currentMoveNum = 0;
        string result = "";
        //Index = 1 because the first part is just the number
        for (int index = 1; index < turns.Length; index++)
        {
            string[] movesInTurn = turns[index].Split("}");
            string[] whiteMoveTotal = movesInTurn[0].Split("{");
            Move whiteMove = Coord.convertUCIMove(board, whiteMoveTotal[0].Trim());
            moveInfoPairs[currentMoveNum] = (whiteMove, whiteMoveTotal[1]);
            board.Move(whiteMove, true);
            //Not book move
            if (!whiteMoveTotal[1].Contains("book") && !whiteMoveTotal[1].Contains("M"))
            {
                board.GenerateMoveGenInfo();
                if (!board.isCurrentPlayerInCheck)
                {
                    Span<Move> legalMoves = stackalloc Move[256];
                    MoveGenerator.GenerateLegalMoves(board, ref legalMoves, board.colorTurn, true);
                    //No captures
                    if (legalMoves.Length == 0)
                    {
                        quietFens.Add(board.ConvertToFEN());
                    }
                    else
                    {
                        int eval = evaluator.EvaluatePosition(board);

                        int qEval = search.QuiescenceSearch(99999, -99999, 0);
                        if (Math.Abs(qEval - eval) < 50)
                        {
                            quietFens.Add(board.ConvertToFEN());
                        }
                    }
                }
            }

            if (index != turns.Length - 1)
            {
                string[] blackMoveTotal = movesInTurn[1].Split("{");
                Move blackMove = Coord.convertUCIMove(board, blackMoveTotal[0].Trim());
                moveInfoPairs[currentMoveNum] = (blackMove, blackMoveTotal[1]);
                board.Move(blackMove, true);
                //Not book move
                if (!blackMoveTotal[1].Contains("book") && !blackMoveTotal[1].Contains("M"))
                {
                    board.GenerateMoveGenInfo();
                    if (!board.isCurrentPlayerInCheck)
                    {
                        Span<Move> legalMoves = stackalloc Move[256];
                        MoveGenerator.GenerateLegalMoves(board, ref legalMoves, board.colorTurn, true);
                        //No captures
                        if (legalMoves.Length == 0)
                        {
                            quietFens.Add(board.ConvertToFEN());
                        }
                        else
                        {
                            int eval = evaluator.EvaluatePosition(board);

                            int qEval = search.QuiescenceSearch(99999, -99999, 0);
                            if (Math.Abs(qEval - eval) < 50)
                            {
                                quietFens.Add(board.ConvertToFEN());
                            }
                        }
                    }
                }
            }
            else
            {
                result = movesInTurn.Length == 2 ? movesInTurn[1] : movesInTurn[2];
            }
        }

        for (int x = 0; x < quietFens.Count; x++)
        {
            quietFens[x] += ";" + result;
        }

        return quietFens;
    }
}

public struct TuneParam
{
    public void ConvertFromString(string input)
    {
        string[] components = input.Split(" ");
        name = components[0];
        value = int.Parse(components[1]);
        counter = int.Parse(components[2]);
        delta = int.Parse(components[3]);
        index = int.Parse(components[4]);
    }
    public string name;
    //Stores the current tuned value
    public int value;
    //Number of times it has been changed: more, change earlier
    public int counter;
    //How much it is currently changing by
    public int delta;
    //Stores the index in its array in eval
    public int index;

    public string GetStringVal()
    {
        return name + " " + value.ToString() + " " + counter.ToString() + " " + delta.ToString() + " " + index.ToString();
    }
}