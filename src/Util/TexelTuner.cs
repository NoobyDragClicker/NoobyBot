using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

public class TexelTuner
{
    TuneParam[] parameters;
    Evaluation evaluation;
    SearchLogger logger;
    int numParameters = 1;
    public float K = 1.00f;

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

    public double CalculateMeanSquareError(int numPosToUse, (string, double)[] fenStrings)
    {
        double sum = 0;
        object lockObj = new object();
        Parallel.For(0, numPosToUse, i =>
        {
            Evaluation evaluator = new Evaluation(logger);
            Board board = new Board();
            board.setPosition(fenStrings[i].Item1, logger);

            int eval = (board.colorTurn == Piece.White) ? evaluator.EvaluatePosition(board) : -evaluator.EvaluatePosition(board);
            double sigmoidEval = Sigmoid(eval);
            double diff = fenStrings[i].Item2 - sigmoidEval;

            lock (lockObj) sum += diff * diff;
        });
        return sum / numPosToUse;
    }

    public double Sigmoid(double s)
    {
        return 1.0 / (1.0 + Math.Pow(10, -K * s / 400.0));
    }

    //Primary function, takes a full match output and converts to a .epd file of quiet fens
    public void ConvertPGNFileToQuietFens(string inputPath, string outputPath, int numGames, int? targetPerPhase, SearchLogger logger, (double, double)[] resultRatio)
    {
        Random rand = new Random();
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Console.WriteLine("Removing PGN filler");
        List<GameInfo> games = RemovePGNFiller(inputPath);
        Console.WriteLine("PGN filler removed");

        numGames = (numGames > games.Count) ? games.Count : numGames;
        Console.WriteLine($"{numGames} games found");
        List<FenInfo> fensInfo = new List<FenInfo>();

        //Extract all the quiet fens, disregard distribution of positions
        Console.WriteLine("Converting games");
        for (int gameIndex = 1; gameIndex < numGames; gameIndex++)
        {
            if (games != null)
            {
                fensInfo.AddRange(ConvertGameToQuietFens(games[gameIndex]));
            }
        }
        Console.WriteLine("Extracted " + fensInfo.Count() + " positions");

        //Thanks ChatGPT
        // 1) Group positions by (Phase, Result). The dictionary key is an anonymous object { Phase, Result }.
        var grouped = fensInfo
            .GroupBy(p => new { p.phase, p.result })
            .ToDictionary(g => g.Key, g => g.ToList());

        // 2) Compute how many positions exist in each phase (sum over results).
        var phaseTotals = fensInfo.GroupBy(p => p.phase)
            .ToDictionary(g => g.Key, g => g.Count());

        int minPerPhase = phaseTotals.Values.Min();
        int perPhase = targetPerPhase ?? minPerPhase;

        var filtered = new List<string>();

        // 4) For each phase, allocate samples to results according to resultRatios and randomly take from that bucket.
        foreach (var phase in phaseTotals.Keys)
        {
            int totalPhaseSamples = perPhase;

            foreach (var kvp in resultRatio)
            {
                double result = kvp.Item1;
                double ratio = kvp.Item2;

                // integer truncation here — fractional pieces are dropped
                int countForBucket = (int)(totalPhaseSamples * ratio);

                // get the list for this (phase, result) if it exists
                grouped.TryGetValue(new { phase, result }, out var bucket);

                if (bucket != null && bucket.Count > 0)
                {
                    // random sample: OrderBy(x => _rand.Next()) shuffles then Take()
                    var sampled = bucket.OrderBy(x => rand.Next()).Take(countForBucket);
                    foreach (FenInfo current in sampled)
                    {
                        string resultStr = "1/2-1/2";
                        if (current.result == 0.0) { resultStr = "0-1"; }
                        else if (current.result == 1.0) { resultStr = "1-0"; }
                        filtered.Add(current.fen + "; " + resultStr);
                    }
                }
                // if bucket missing or empty, nothing is added for this (phase,result)
            }
        }


        File.AppendAllLines(outputPath, filtered);
        stopwatch.Stop();
        logger.AddToLog(stopwatch.Elapsed.ToString(), SearchLogger.LoggingLevel.Diagnostics);
        Console.WriteLine("Completed in: " + stopwatch.Elapsed);
    }

    //Returns full game lines from a full match output, 1 game/line
    public static List<GameInfo> RemovePGNFiller(string inputGames)
    {
        string[] lines = File.ReadAllLines(inputGames);

        List<GameInfo> gameLines = new List<GameInfo>();

        int numGameLines = 0;
        GameInfo line = new GameInfo();
        bool hasGameBeenFound = false;
        for (int index = 0; index < lines.Count(); index++)
        {
            //Not random filler
            if (lines[index] != "" && lines[index] != "\n")
            {
                if (lines[index][0] == '[')
                {
                    if (lines[index].Contains("Result"))
                    {
                        if (hasGameBeenFound)
                        {
                            if (line.startFen == null | line.startFen == "")
                            {
                                line.startFen = Board.startPos;
                            }
                            gameLines.Add(line);
                            line = new GameInfo();
                        }
                        else
                        {
                            hasGameBeenFound = true;
                        }
                        numGameLines++;
                        if (lines[index].Contains("1/2-1/2"))
                        {
                            line.result = 0.5;
                        }
                        else if (lines[index].Contains("1-0"))
                        {
                            line.result = 1.0;
                        }
                        else if (lines[index].Contains("0-1"))
                        {
                            line.result = 0.0;
                        }

                    }
                    else if (lines[index].Contains("FEN"))
                    {
                        line.startFen = lines[index].Substring(6);
                    }
                    else if (lines[index].Contains("TimeControl"))
                    {
                        lines[index] = lines[index].Trim('"');
                        lines[index] = lines[index].Trim('[');
                        lines[index] = lines[index].Trim(']');
                        string tc = lines[index].Split(" ")[1];
                        line.timeControl = tc;
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (lines[index][0] != '[')
                {
                    line.moves += lines[index];
                }
            }
        }

        gameLines.Add(line);
        return gameLines;
    }

    //Returns all the semiquiet fens from a game line
    public List<FenInfo> ConvertGameToQuietFens(GameInfo game)
    {
        string[] turns = game.moves.Split(". ");
        double resultNum = game.result;
        string tc = game.timeControl;

        (Move, string)[] moveInfoPairs = new (Move, string)[turns.Length * 2];

        Board board = new Board();
        board.setPosition(game.startFen, logger);

        List<FenInfo> quietFens = new List<FenInfo>();
        Evaluation evaluator = new Evaluation(logger);
        Search search = new Search(board, new AISettings(40, 16, 64), logger);

        int currentMoveNum = 0;
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
                if (!board.gameStateHistory[board.fullMoveClock].isInCheck)
                {
                    Span<Move> legalMoves = stackalloc Move[256];
                    MoveGenerator.GenerateLegalMoves(board, ref legalMoves, board.colorTurn, true);
                    //No captures
                    if (legalMoves.Length == 0)
                    {
                        FenInfo current = new FenInfo();
                        current.fen = board.ConvertToFEN();
                        current.timeControl = tc;
                        current.result = resultNum;
                        current.phase = phasePerBoard(board);

                        quietFens.Add(current);
                    }
                    else
                    {
                        int eval = evaluator.EvaluatePosition(board);

                        int qEval = search.QuiescenceSearch(99999, -99999, 0);
                        if (Math.Abs(qEval - eval) < 50)
                        {
                            FenInfo current = new FenInfo();
                            current.fen = board.ConvertToFEN();
                            current.timeControl = tc;
                            current.result = resultNum;
                            current.phase = phasePerBoard(board);
                            
                            quietFens.Add(current);
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
                    if (!board.gameStateHistory[board.fullMoveClock].isInCheck)
                    {
                        Span<Move> legalMoves = stackalloc Move[256];
                        MoveGenerator.GenerateLegalMoves(board, ref legalMoves, board.colorTurn, true);
                        //No captures
                        if (legalMoves.Length == 0)
                        {
                            FenInfo current = new FenInfo();
                            current.fen = board.ConvertToFEN();
                            current.timeControl = tc;
                            current.result = resultNum;
                            current.phase = phasePerBoard(board);
                            
                            quietFens.Add(current);
                        }
                        else
                        {
                            int eval = evaluator.EvaluatePosition(board);

                            int qEval = search.QuiescenceSearch(99999, -99999, 0);
                            if (Math.Abs(qEval - eval) < 50)
                            {
                                FenInfo current = new FenInfo();
                                current.fen = board.ConvertToFEN();
                                current.timeControl = tc;
                                current.result = resultNum;
                                current.phase = phasePerBoard(board);
                                
                                quietFens.Add(current);
                            }
                        }
                    }
                }
            }
        }
        return quietFens;
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

    void UpdateParamInEval(string name, int newEval, int index)
    {

        switch (name)
        {
            case "pawnVal": Evaluation.pawnValue = newEval; break;
            case "knightVal": Evaluation.knightValue = newEval; break;
            case "bishopVal": Evaluation.bishopValue = newEval; break;
            case "rookVal": Evaluation.rookValue = newEval; break;
            case "queenVal": Evaluation.queenValue = newEval; break;

            case "mg_pawn_table": Evaluation.mg_PSQT[Piece.Pawn, index] = newEval; break;
            case "eg_pawn_table": Evaluation.eg_PSQT[Piece.Pawn, index] = newEval; break;

            case "mg_knight_table": Evaluation.mg_PSQT[Piece.Knight, index] = newEval; break;
            case "eg_knight_table": Evaluation.eg_PSQT[Piece.Knight, index] = newEval; break;

            case "mg_bishop_table": Evaluation.mg_PSQT[Piece.Bishop, index] = newEval; break;
            case "eg_bishop_table": Evaluation.eg_PSQT[Piece.Bishop, index] = newEval; break;

            case "mg_rook_table": Evaluation.mg_PSQT[Piece.Rook, index] = newEval; break;
            case "eg_rook_table": Evaluation.eg_PSQT[Piece.Rook, index] = newEval; break;

            case "mg_queen_table": Evaluation.mg_PSQT[Piece.Queen, index] = newEval; break;
            case "eg_queen_table": Evaluation.eg_PSQT[Piece.Queen, index] = newEval; break;

            case "mg_king_table": Evaluation.mg_PSQT[Piece.King, index] = newEval; break;
            case "eg_king_table": Evaluation.eg_PSQT[Piece.King, index] = newEval; break;

            case "isolatedPawnPenalty": Evaluation.isolatedPawnPenalty[index] = newEval; break;
            case "passedPawnBonuses": Evaluation.passedPawnBonuses[index] = newEval; break;
            default: logger.AddToLog("No parameter found: " + name, SearchLogger.LoggingLevel.Deadly); break;
        }

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
            $"public static int[] mg_pawn_table =  {{{string.Join(", ", mg_pawn_table)}}};",
            $"public static int[] eg_pawn_table = {{{string.Join(", ", eg_pawn_table)}}};",

            $"public static int[] mg_knight_table = {{{string.Join(", ", mg_knight_table)}}};",
            $"public static int[] eg_knight_table = {{{string.Join(", ", eg_knight_table)}}};",

            $"public static int[] mg_bishop_table = {{{string.Join(", ", mg_bishop_table)}}};",
            $"public static int[] eg_bishop_table = {{{string.Join(", ", eg_bishop_table)}}};",

            $"public static int[] mg_rook_table = {{{string.Join(", ", mg_rook_table)}}};",
            $"public static int[] eg_rook_table = {{{string.Join(", ", eg_rook_table)}}};",

            $"public static int[] mg_queen_table = {{{string.Join(", ", mg_queen_table)}}};",
            $"public static int[] eg_queen_table = {{{string.Join(", ", eg_queen_table)}}};",

            $"public static int[] mg_king_table = {{{string.Join(", ", mg_king_table)}}};",
            $"public static int[] eg_king_table = {{{string.Join(", ", eg_king_table)}}};",

            $"public static int[] passedPawnBonuses = {{{string.Join(", ", passedPawnBonuses)}}};",
            $"public static int[] isolatedPawnPenalty = {{{string.Join(", ", isolatedPawnPenalty)}}};" };

        File.WriteAllLines(outputFile, codeSnippets);


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
        /*
        loadedParams.AddRange(ConvertArrayToParams("mg_pawn_table", Evaluation.mg_PSQT[]));
        loadedParams.AddRange(ConvertArrayToParams("eg_pawn_table", Evaluation.eg_pawn_table));

        loadedParams.AddRange(ConvertArrayToParams("mg_knight_table", Evaluation.mg_knight_table));
        loadedParams.AddRange(ConvertArrayToParams("eg_knight_table", Evaluation.eg_knight_table));

        loadedParams.AddRange(ConvertArrayToParams("mg_bishop_table", Evaluation.mg_bishop_table));
        loadedParams.AddRange(ConvertArrayToParams("eg_bishop_table", Evaluation.eg_bishop_table));

        loadedParams.AddRange(ConvertArrayToParams("mg_rook_table", Evaluation.mg_rook_table));
        loadedParams.AddRange(ConvertArrayToParams("eg_rook_table", Evaluation.eg_rook_table));

        loadedParams.AddRange(ConvertArrayToParams("mg_queen_table", Evaluation.mg_queen_table));
        loadedParams.AddRange(ConvertArrayToParams("eg_queen_table", Evaluation.eg_queen_table));

        loadedParams.AddRange(ConvertArrayToParams("mg_king_table", Evaluation.mg_king_table));
        loadedParams.AddRange(ConvertArrayToParams("eg_king_table", Evaluation.eg_king_table));

        loadedParams.AddRange(ConvertArrayToParams("passedPawnBonuses", Evaluation.passedPawnBonuses));
        loadedParams.AddRange(ConvertArrayToParams("isolatedPawnPenalty", Evaluation.isolatedPawnPenalty));*/
        
        
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

    //Converts the external labeled fens file to our format
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
            final[index] = split[index, 0] + " ; " + split[index, 1] + "60+0.6";
        }
        File.WriteAllLines(outputFile, final);

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

public struct FenInfo()
{
    public string fen { get; set; }
    public double result { get; set; }
    public string timeControl { get; set; }
    public int phase { get; set; }
}

public struct GameInfo()
{
    public string startFen { get; set; }
    public double result { get; set; }
    public string timeControl { get; set; }
    public string moves { get; set; }
}