using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using System.Threading.Tasks;

public class TournamentManager : MonoBehaviour
{
    const int numBoards = 4;
    const int startTime = 60;
    const int incrementMS = 500;
    const int maxGames = 20;

    int testPlayerWins, oldPlayerWins, draws;

    [SerializeField]
    Tile tile;
    [SerializeField]
    DisplayPiece displayPiecePrefab;

    [SerializeField]
    TextMeshProUGUI newPlayerWins;
    [SerializeField]
    TextMeshProUGUI pastPlayerWins;
    [SerializeField]
    TextMeshProUGUI drawsDisplay;

    List<DisplayPiece> displayPieces = new List<DisplayPiece>();

    [SerializeField]
    AISettings testSettings = new AISettings(true, 20, 16, true, false, false, false);
    [SerializeField]
    AISettings oldSettings = new AISettings(true, 20, 16, false, false, false, false);

    BoardManager[] boards = new BoardManager[numBoards];
    BookLoader bookLoader;
    int numGamesPerBoard = maxGames / numBoards;
    int[] numGamesPlayedPerBoard = new int[numBoards];
    int[] numGamesFinishedPerBoard = new int[numBoards];
    bool[] isWhiteTest = new bool[numBoards];
    int totalGamesPlayed = 0;
    int testPlayerWinsWithBlack = 0;
    int oldPlayerWinsWithBlack = 0;

    bool isMoveWaiting;
    int boardWaiting;

    void Start()
    {
        Application.targetFrameRate = 144;
        bookLoader = new BookLoader();
        if (testSettings.openingBookDepth > 0 || oldSettings.openingBookDepth > 0)
        {
            bookLoader.loadBook();
        }
        for (int boardNumber = 0; boardNumber < numBoards; boardNumber++)
        {
            if (boardNumber % 2 == 0) { isWhiteTest[boardNumber] = true; }
            else { isWhiteTest[boardNumber] = false; }

            BoardManager currentBoard = new BoardManager(boardNumber, bookLoader);
            boards[boardNumber] = currentBoard;
            currentBoard.moveMade += NewMove;
            currentBoard.gameFinished += FinishedGame;
        }
    }
    void Update()
    {
        if (isMoveWaiting) { UpdateBoard(boardWaiting); isMoveWaiting = false; }
        for (int x = 0; x < numBoards; x++)
        {
            boards[x].Update();
        }
    }
    public void StartTournament()
    {
        for (int x = 0; x < numBoards; x++)
        {
            int offsetX = (x % 2 == 0) ? 0 : 12;
            int offsetY = (x < (numBoards / 2)) ? 0 : 12;
            SpawnBoard(offsetX, offsetY);
            StartGame(x);
        }
    }
    void StartGame(int boardNumber)
    {
        AISettings whiteSettings = isWhiteTest[boardNumber] ? testSettings : oldSettings;
        AISettings blackSettings = isWhiteTest[boardNumber] ? oldSettings : testSettings;

        boards[boardNumber].StartGame(true, startTime, incrementMS, false, "", whiteSettings, blackSettings, totalGamesPlayed);
        numGamesPlayedPerBoard[boardNumber]++;
        totalGamesPlayed++;
        Debug.Log("Game started: " + totalGamesPlayed);
    }
    void SpawnBoard(int offsetX, int offsetY)
    {
        //Generates actual tiles
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var spawnedTile = Instantiate(tile, new Vector3(x + offsetX, y + offsetY, 0.01f), Quaternion.identity);
                spawnedTile.name = $"tile {x + offsetX} {y + offsetY}";

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);
            }
        }
    }
    void FinishedGame(BoardManager.ResultStatus result, int boardNumber)
    {
        if (result == BoardManager.ResultStatus.Draw) { draws++; Debug.Log("draw"); }
        else if (result == BoardManager.ResultStatus.White_Won)
        {
            if (isWhiteTest[boardNumber] == true)
            {
                testPlayerWins++;
                Debug.Log("test player won");
            }
            else
            {
                oldPlayerWins++;
                Debug.Log("old player won");
            }
        }
        else if (result == BoardManager.ResultStatus.Black_Won)
        {
            if (isWhiteTest[boardNumber] == true)
            {
                oldPlayerWins++;
                oldPlayerWinsWithBlack++;
                Debug.Log("old player won");
            }
            else
            {
                testPlayerWins++;
                testPlayerWinsWithBlack++;
                Debug.Log("test player won");
            }
        }
        numGamesFinishedPerBoard[boardNumber]++;

        if (numGamesFinishedPerBoard[boardNumber] < numGamesPerBoard)
        {
            Task.Delay(1000).ContinueWith((t) => StartGame(boardNumber));
        }

        bool isFinished = true;
        for (int index = 0; index < numBoards; index++)
        {
            if (numGamesFinishedPerBoard[index] < numGamesPerBoard) { isFinished = false; }
        }
        if (isFinished) { FinishTournament(); }
    }
    void FinishTournament()
    {
        Debug.Log("Test player wins: " + testPlayerWins);
        Debug.Log("Test player wins with black: " + testPlayerWinsWithBlack);
        Debug.Log("Old player wins: " + oldPlayerWins);
        Debug.Log("Old player wins with black: " + oldPlayerWinsWithBlack);
        Debug.Log("Draws: " + draws);

    }
    public void UpdateBoard(int boardNumber)
    {
        for (int x = 0; x < displayPieces.Count; x++)
        {
            Destroy(displayPieces[x].gameObject);
        }
        displayPieces.Clear();

        for (int z = 0; z < numBoards; z++)
        {
            Board board = boards[z].board;
            if (board == null) { break; }
            int offsetX = (z % 2 == 0) ? 0 : 12;
            int offsetY = (z < numBoards / 2) ? 0 : 12;
            for (int x = 0; x < 64; x++)
            {
                if (board.board[x] != 0)
                {
                    int pieceType = Piece.PieceType(board.board[x]);
                    int rank = board.IndexToRank(x);
                    int file = board.IndexToFile(x);
                    DisplayPiece displayPiece = Instantiate(displayPiecePrefab, new Vector3((file - 1) + offsetX, (rank - 1) + offsetY), Quaternion.identity);
                    displayPiece.Init(pieceType, Piece.Color(board.board[x]));
                    displayPieces.Add(displayPiece);
                }
            }
        }
        newPlayerWins.text = "Test Player Wins: " + testPlayerWins;
        pastPlayerWins.text = "Old Player Wins: " + oldPlayerWins;
        drawsDisplay.text = "Draws: " + draws;
    }

    void NewMove(int boardNumber)
    {
        isMoveWaiting = true;
        boardWaiting = boardNumber;
    }
}
