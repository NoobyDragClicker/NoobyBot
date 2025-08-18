using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Data;

public class UIManager : MonoBehaviour
{
/*****************************************************************************
    TODO:
    - displaying captured pieces (sorted and customizable)
    - displaying move number, chess notation and bot eval (when available)
    - allow arrows to be drawn
    - as much front end stuff as I can fit

*****************************************************************************/
    //UI:
    [SerializeField]
    GameObject startScreen;
    [SerializeField]
    TMP_Dropdown whitePlayerType;
    [SerializeField]
    TMP_Dropdown blackPlayerType;
    [SerializeField]
    TMP_InputField clockStartTime;
    [SerializeField]
    TMP_InputField customPosition;
    [SerializeField]
    TMP_InputField gameReviewPathInput;

    //Prefabs
    [SerializeField]
    Tile tilePrefab;
    [SerializeField]
    MoveSelected moveSelectedPrefab;
    [SerializeField]
    GameObject debugPrefab;
    [SerializeField]
    DisplayPiece displayPiecePrefab;
    BookLoader bookLoader;

    //Display for who won
    public SpriteRenderer blackIndicator;
    public SpriteRenderer whiteIndicator;
    [SerializeField]
    Color green;
    [SerializeField]
    Color red;
    [SerializeField]
    Color yellow;
    List<Move> promotionMoves = new List<Move>();
    List<MoveSelected> possiblePlayerMoves = new List<MoveSelected>();
    List<GameObject> debugPrefabs = new List<GameObject>();
    List<DisplayPiece> displayPieces = new List<DisplayPiece>();

    public BoardManager boardManager;
    public bool isDebugMode = true;
    bool isGameReview = false;
    List<Move> gameMoves;
    int currentGameMoveIndex = 0;
    int selectedPiece;
    
    [SerializeField]
    TMP_Text whiteClock;
    [SerializeField]
    TMP_Text blackClock;

    Player.ClockType clockType = Player.ClockType.Regular;
    bool useCustomPos = false;
    bool isMoveWaiting;
    bool hasGameEnded = false;
    BoardManager.ResultStatus result;

    [SerializeField]
    AISettings testSettings = new AISettings(20, 10, 16, false);
    [SerializeField]
    AISettings oldSettings = new AISettings( 20, 10, 16, false);

    void Start(){
        bookLoader = new BookLoader();
        boardManager = new BoardManager(0, bookLoader);
        boardManager.moveMade += NewMove;
        boardManager.gameFinished += EndGame;
    }

    void Update(){
        if (isMoveWaiting){ UpdateBoard(boardManager.boardNumber);  isMoveWaiting = false; }
        if (hasGameEnded) { DisplayResult(result); }
        if (boardManager.gameStatus == BoardManager.GameStatus.Playing && clockType != Player.ClockType.None)
        {
            UpdateClock();
        }
    }
    void NewMove(int boardNumber)
    {
        isMoveWaiting = true;
    }

    void UpdateClock(){
        int whiteSecondsRemaining = Mathf.Max(0, boardManager.getWhiteMSRemaining() / 1000);
        int blackSecondsRemaining = Mathf.Max(0, boardManager.getBlackMSRemaining() / 1000);

        int numWhiteMinutes = (int) (whiteSecondsRemaining/60);
        int numWhiteSeconds = (int) (whiteSecondsRemaining - numWhiteMinutes * 60);
        
        int numBlackMinutes = (int) (blackSecondsRemaining/60);
        int numBlackSeconds = (int) (blackSecondsRemaining - numBlackMinutes * 60);

        whiteClock.text = $"{numWhiteMinutes:00}:{numWhiteSeconds:00}";
        blackClock.text = $"{numBlackMinutes:00}:{numBlackSeconds:00}";
        
    }

    public void StartGame(){
        int startTime = Convert.ToInt32(clockStartTime.text);
        string inputtedCustomStr = customPosition.text;
        bool whiteHuman = (whitePlayerType.value == 0) ? true: false;
        bool blackHuman = (blackPlayerType.value == 0) ? true: false;

        if(isGameReview){
            whiteHuman = true;
            blackHuman = true;
            clockType = Player.ClockType.None;
            gameMoves = GameLogger.ReadMovesFromLog(gameReviewPathInput.text);
        }

        AISettings whiteSettings = (whitePlayerType.value == 2) ? testSettings : oldSettings;
        AISettings blackSettings = (blackPlayerType.value == 2) ? testSettings : oldSettings;

        if (!(whiteHuman && blackHuman) && (whiteSettings.openingBookDepth > 0 || blackSettings.openingBookDepth > 0))
        {
            bookLoader.loadBook();
        }

        
        if(clockType != Player.ClockType.None){
            whiteClock.gameObject.SetActive(true);
            blackClock.gameObject.SetActive(true);
        }

        //Generates actual tiles
        for(int x = 0; x< 8; x++){
            for(int y = 0; y < 8; y++){
                var spawnedTile = Instantiate(tilePrefab, new Vector3(x, y, 0.01f), Quaternion.identity);
                spawnedTile.name = $"tile {x} {y}";

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);
            }
        }
        boardManager.StartGame(clockType, startTime, 1, useCustomPos, inputtedCustomStr, whiteSettings, blackSettings, 1,  whiteHuman:whiteHuman, blackHuman:blackHuman);
        startScreen.SetActive(false);
    }
    
    public void UpdateBoard(int boardNumber){
        Board board = boardManager.board;
        
        //Removing all displayed pieces
        for (int y = 0; y < displayPieces.Count; y++)
        {
            Destroy(displayPieces[y].gameObject);
        }
        displayPieces.Clear();
        

        //Destroy current moveselectors
        for (int z = 0; z < possiblePlayerMoves.Count; z++)
        {
            Destroy(possiblePlayerMoves[z].gameObject);
        }
        possiblePlayerMoves.Clear();

        //Destroy current debug prefabs
        for(int z = 0; z< debugPrefabs.Count; z++){
            Destroy(debugPrefabs[z]);
        }
        debugPrefabs.Clear();


        
        //Spawning them in based on board
        for (int x = 0; x < 64; x++)
        {
            if (board.board[x] != 0)
            {
                int pieceType = Piece.PieceType(board.board[x]);
                int rank = board.IndexToRank(x);
                int file = board.IndexToFile(x);
                DisplayPiece displayPiece = Instantiate(displayPiecePrefab, new Vector3(file - 1, rank - 1), Quaternion.identity);
                displayPiece.Init(pieceType, Piece.Color(board.board[x]));
                displayPieces.Add(displayPiece);
            }
        }
        if(isDebugMode){ShowAttackedSquares();}
    }

    public void SelectPiece(int x, int y){
        Board board = boardManager.board;
        //Can't select a piece for an AI
        if(boardManager.playerToMove is AIPlayer){ return;}

        //Destroy current moveselectors
        for(int z = 0; z< possiblePlayerMoves.Count; z++){
            Destroy(possiblePlayerMoves[z].gameObject);
        }
        possiblePlayerMoves.Clear();
        promotionMoves.Clear();

        //checking if it is already selected, and if yes, deselect
        if (selectedPiece == board.RankFileToIndex(x, y)){
            selectedPiece = -1;
            return;
        }

        //Getting possible moves and displaying
        selectedPiece = board.RankFileToIndex(x, y);
        List<Move> possibleMoves = new List<Move>();

        //If there is a piece on that square
        if(board.board[selectedPiece] != 0){
            possibleMoves = board.moveGenerator.GeneratePieceMove(board.board[selectedPiece], board.RankFileToIndex(x, y), board);
        }

        for(int z = 0; z< possibleMoves.Count; z++){
            var movePrefab = Instantiate(moveSelectedPrefab, new Vector3(board.IndexToFile(possibleMoves[z].newIndex) -1, board.IndexToRank(possibleMoves[z].newIndex) -1, -0.02f), Quaternion.identity);
            movePrefab.move = possibleMoves[z];
            //If its a promotion piece
            if(movePrefab.move.isPromotion() && movePrefab.move.flag == 1){
                movePrefab.name = "promotion piece";
                movePrefab.isPromoManager = true;
                possiblePlayerMoves.Add(movePrefab);
                promotionMoves.Add(possibleMoves[z]);
            } else if(movePrefab.move.isPromotion() && movePrefab.move.flag != 1){
                promotionMoves.Add(possibleMoves[z]);
                Destroy(movePrefab.gameObject);
            //Else
            } else{
                possiblePlayerMoves.Add(movePrefab);
            }
        }
    }

    public void SpawnPromotionPieces(Move move){
        Board board = boardManager.board;
        //Destroy current moveselectors
        for(int z = 0; z< possiblePlayerMoves.Count; z++){
            Destroy(possiblePlayerMoves[z].gameObject);
        }
        possiblePlayerMoves.Clear();

        for(int x = 0; x< promotionMoves.Count; x++){
            //For cases where there are multiple ways to promote, only spawn them if they are the selected move
            if(promotionMoves[x].newIndex == move.newIndex){
                var movePrefab = Instantiate(moveSelectedPrefab, new Vector3(board.IndexToFile(promotionMoves[x].newIndex) -1, board.IndexToRank(promotionMoves[x].newIndex) -1, -0.02f), Quaternion.identity);
                movePrefab.move = promotionMoves[x];
                movePrefab.SetPromotionRender();
                possiblePlayerMoves.Add(movePrefab);
            }

        }
    }
    
    //Debugging purposes
    void ShowAttackedSquares(){
        Board board = boardManager.board;
        //Destroy current debug things
        for(int z = 0; z< debugPrefabs.Count; z++){
            Destroy(debugPrefabs[z]);
        }
        debugPrefabs.Clear();

        int attackedSquaresIndex = (board.colorTurn == Piece.White) ? Board.BlackIndex : Board.WhiteIndex;
        for(int x = 0; x<64; x++){
            int rank = board.IndexToRank(x);
            int file = board.IndexToFile(x);
            if(board.attackedSquares[attackedSquaresIndex, x] == 1){
                
                var debug = Instantiate(debugPrefab, new Vector3(file-1, rank-1, -0.02f), Quaternion.identity);
                debugPrefabs.Add(debug);
            }
        }

    }

    public void DisplayResult(BoardManager.ResultStatus resultStatus){
        UpdateBoard(0);
        whiteIndicator.gameObject.SetActive(true);
        blackIndicator.gameObject.SetActive(true);
        if(resultStatus == BoardManager.ResultStatus.Draw){
            blackIndicator.color = yellow;
            whiteIndicator.color = yellow;
        } else if(resultStatus == BoardManager.ResultStatus.White_Won){
            whiteIndicator.color = green; 
            blackIndicator.color = red;
        } else if(resultStatus == BoardManager.ResultStatus.Black_Won){
            whiteIndicator.color = red; 
            blackIndicator.color = green;
        }
    }

    public void EndGame(BoardManager.ResultStatus resultStatus, int boardNumber)
    {
        result = resultStatus;
        hasGameEnded = true;
    }

    public void ShowDebugSquares(List<int> index)
    {
        Board board = boardManager.board;
        //Destroy current debug prefabs
        for (int z = 0; z < debugPrefabs.Count; z++)
        {
            Destroy(debugPrefabs[z]);
        }
        debugPrefabs.Clear();
        for (int x = 0; x < 64; x++)
        {
            int rank = board.IndexToRank(x);
            int file = board.IndexToFile(x);
            if (index.Contains(x))
            {
                var debug = Instantiate(debugPrefab, new Vector3(file - 1, rank - 1, -0.02f), Quaternion.identity);
                debugPrefabs.Add(debug);
            }
        }

    }
    //UI
    public void UpdateUseClock(bool input){
        if (input){ clockType = Player.ClockType.Regular; }
        else{ clockType = Player.ClockType.None; }
        
    }
    public void UpdateCustomPos(bool input){
        useCustomPos = input;
    }
    public void UpdateGameReview(bool input){
        isGameReview = input;
    }
    
    public void LogGame(){
        if(!isGameReview){
            GameLogger.LogGame(boardManager.board, 0);
        }
    }

    public void NextMove(){
        if(isGameReview && currentGameMoveIndex < gameMoves.Count){
            boardManager.board.Move(gameMoves[currentGameMoveIndex], false);
            UpdateBoard(boardManager.boardNumber);
            currentGameMoveIndex++;
        }
    }
    public void PreviousMove(){
        if(isGameReview && currentGameMoveIndex > -1){
            currentGameMoveIndex--;
            boardManager.board.UndoMove(gameMoves[currentGameMoveIndex]);
            UpdateBoard(boardManager.boardNumber);
        } 
    }
}
