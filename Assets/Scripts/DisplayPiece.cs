using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayPiece : MonoBehaviour
{
    
    public bool isWhite; 
    [SerializeField] Sprite rook;
    [SerializeField] Sprite blackRook;
    [SerializeField] Sprite knight;
    [SerializeField] Sprite blackKnight;
    [SerializeField] Sprite bishop;
    [SerializeField] Sprite blackBishop;
    [SerializeField] Sprite pawn;
    [SerializeField] Sprite blackPawn;
    [SerializeField] Sprite king;
    [SerializeField] Sprite blackKing;
    [SerializeField] Sprite queen;
    [SerializeField] Sprite blackQueen;
    [SerializeField] SpriteRenderer render;
    [SerializeField] Transform pos;
    //[SerializeField] GameObject movePrefab;
    BoardManager boardManager;

    public void Init(int type, int color){
        boardManager = GameObject.Find("BoardManager").GetComponent<BoardManager>();

        isWhite = color == Piece.White;

        //Setting sprites
        if(type == Piece.Pawn){ 
            render.sprite = isWhite ? pawn : blackPawn;
        } else if(type == Piece.Knight){
            render.sprite = isWhite ? knight : blackKnight;
        }else if(type == Piece.Bishop){
            render.sprite = isWhite ? bishop : blackBishop;
        }else if(type == Piece.Rook){
            render.sprite = isWhite ? rook : blackRook;
        }else if(type == Piece.Queen){
            render.sprite = isWhite ? queen : blackQueen;
        }else if(type == Piece.King){
            render.sprite = isWhite ? king : blackKing;
        }
        pos.localScale = new UnityEngine.Vector3(.85f, .85f, .85f);

    }



    /*
    //Logic for clicking on piece
    public void Select(){
        //If its already selected, deselect and destroy circles
        if(isSelected){
            DestroyMovePrefabs();
            isSelected = false;
            boardManager.isPieceSelected = false;
        }
        //If not selected and none are, spawn in circles
        else if(!isSelected && !boardManager.isPieceSelected){
            var legalMoves = GetLegalMoves();
            movePrefabs = new List<GameObject>{};
            for(int z = 0; z < legalMoves.Count(); z++){
                var prefab = Instantiate(movePrefab, new Vector3(legalMoves[z].x, legalMoves[z].y, -1), Quaternion.identity);
                movePrefabs.Add(prefab);
            }
            isSelected = true;
            boardManager.isPieceSelected = true;
            boardManager.pieceSelected = this;
        } 
        //If not selected and another piece is, destroy their circles and replace with ours
        else if(!isSelected && boardManager.isPieceSelected){
            var legalMoves = GetLegalMoves();
            movePrefabs = new List<GameObject>{};
            for(int z = 0; z < legalMoves.Count(); z++){
                var prefab = Instantiate(movePrefab, new Vector3(legalMoves[z].x, legalMoves[z].y, -1), Quaternion.identity);
                movePrefabs.Add(prefab);
            }
            boardManager.pieceSelected.DestroyMovePrefabs();
            boardManager.pieceSelected.isSelected = false;
            isSelected = true;
            boardManager.isPieceSelected = true;
            boardManager.pieceSelected = this;
        }
    }

    public void DestroyMovePrefabs(){
        for(int x = 0; x < movePrefabs.Count(); x++){
            Destroy(movePrefabs[x]);
        }
    }

    //TODO: castling, en passant, promotion, pins, checks, king logic
    public List<Vector2Int> GetLegalMoves(){
        int x = Mathf.RoundToInt(pos.position.x);
        int y = Mathf.RoundToInt(pos.position.y);
        List<Vector2Int> legalMoves = new List<Vector2Int>{};
        if(boardManager.isWhiteTurn != isWhite){
            return legalMoves;
        }

        switch(pieceType){
            case PieceType.Null:
            {
                return legalMoves;
            }
            case PieceType.Pawn:
            {
                if(isWhite){
                    //Move forward 1 or 2
                    if((y + 1) <8 && !boardManager.IsPieceAtPos(x, y + 1)){
                        legalMoves.Add(new Vector2Int(x, y + 1));
                        if(isFirstMove && !boardManager.IsPieceAtPos(x, y + 2)){
                            legalMoves.Add(new Vector2Int(x, y + 2));
                        }
                    }
                    //Captures
                    if((y+ 1) < 8 && (x + 1) < 8 && boardManager.IsPieceAtPos(x+1, y+1)){
                        if(boardManager.GetPieceAtPos(x+1, y+1).isWhite ==false){
                            legalMoves.Add(new Vector2Int(x + 1, y + 1));
                        }
                    }
                    if((y+1) < 8 && (x-1) >=0  && boardManager.IsPieceAtPos(x-1, y+1)){
                        if(boardManager.GetPieceAtPos(x-1, y+1).isWhite ==false){
                            legalMoves.Add(new Vector2Int(x - 1, y + 1));
                        }
                    }
                    
                } else {
                    //Move forward 1 or 2
                    if((y-1 >=0) && !boardManager.IsPieceAtPos(x, y - 1)){
                        legalMoves.Add(new Vector2Int(x, y - 1));
                        if(isFirstMove && !boardManager.IsPieceAtPos(x, y - 2)){
                            legalMoves.Add(new Vector2Int(x, y - 2));
                        }
                    }
                    //Captures
                    if((y - 1) >=0 && (x + 1) < 8 && boardManager.IsPieceAtPos(x+1, y-1)){
                        if(boardManager.GetPieceAtPos(x+1, y-1).isWhite ==true){
                            legalMoves.Add(new Vector2Int(x + 1, y - 1));
                        }
                    }
                    if((y-1) >= 0 && (x-1) >= 0 && boardManager.IsPieceAtPos(x-1, y-1)){
                        if(boardManager.GetPieceAtPos(x-1, y-1).isWhite ==true){
                            legalMoves.Add(new Vector2Int(x - 1, y - 1));
                        }
                    }
                    
                }
                return legalMoves;
            }
            case PieceType.Knight:
            {
                List<Vector2Int> potentialMoves=new List<Vector2Int>{
                    new Vector2Int(x + 2, y + 1),
                    new Vector2Int(x + 2, y - 1),
                    new Vector2Int(x - 2, y + 1),
                    new Vector2Int(x - 2, y - 1),
                    new Vector2Int(x + 1, y + 2),
                    new Vector2Int(x - 1, y + 2),
                    new Vector2Int(x + 1, y - 2),
                    new Vector2Int(x - 1, y - 2),
                };

                for(int z = 0; z < 8; z++){
                    if(IsMoveInRange(potentialMoves[z])){
                        if(!boardManager.IsPieceAtPos(potentialMoves[z].x, potentialMoves[z].y)){
                            legalMoves.Add(potentialMoves[z]);
                        } else if(boardManager.GetPieceAtPos(potentialMoves[z].x, potentialMoves[z].y).isWhite != isWhite){
                            legalMoves.Add(potentialMoves[z]);
                        }
                    }
                }
                return legalMoves;
            }
            case PieceType.Bishop:
            {
                //+x, +y
                for(int z = 1; z <7; z++){

                    if(!IsMoveInRange(new Vector2Int(y + z, x + z))) {break;}

                    if(boardManager.IsPieceAtPos(x + z, y + z) == false){legalMoves.Add(new Vector2Int(x + z, y + z));}
                    else if(boardManager.GetPieceAtPos(x + z, y + z).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(x + z, y + z));
                        break;
                    } else{break;}
                }
                //+x, -y
                for(int z = 1; z <7; z++){

                    if(!IsMoveInRange(new Vector2Int(y - z, x + z))) {break;}

                    if(boardManager.IsPieceAtPos(x + z, y - z) == false){legalMoves.Add(new Vector2Int(x + z, y - z));}
                    else if(boardManager.GetPieceAtPos(x + z, y - z).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(x + z, y - z));
                        break;
                    } else{break;}
                }

                //-x, +y
                for(int z = 1; z <7; z++){

                    if(!IsMoveInRange(new Vector2Int(y + z, x - z))) {break;}

                    if(boardManager.IsPieceAtPos(x - z, y + z) == false){legalMoves.Add(new Vector2Int(x - z, y + z));}
                    else if(boardManager.GetPieceAtPos(x - z, y + z).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(x - z, y + z));
                        break;
                    } else{break;}
                }
                //-x, -y
                for(int z = 1; z <7; z++){

                    if(!IsMoveInRange(new Vector2Int(y - z, x - z))) {break;}

                    if(boardManager.IsPieceAtPos(x - z, y - z) == false){legalMoves.Add(new Vector2Int(x - z, y - z));}
                    else if(boardManager.GetPieceAtPos(x - z, y - z).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(x - z, y - z));
                        break;
                    } else{break;}
                }

                return legalMoves;
            }
            case PieceType.Rook:
            {               
                 //Positive X
                for(int z = x + 1; z<8; z++){
                    if(boardManager.IsPieceAtPos(z, y) == false){legalMoves.Add(new Vector2Int(z, y));}
                    else if(boardManager.GetPieceAtPos(z, y).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(z, y));
                        break;
                    } else{break;}
                }
                 //Negative X
                for(int z = x - 1; z>-1; z--){
                    if(boardManager.IsPieceAtPos(z, y) == false){legalMoves.Add(new Vector2Int(z, y));}
                    else if(boardManager.GetPieceAtPos(z, y).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(z, y));
                        break;
                    } else{break;}
                }
                //Positive Y
                for(int z = y + 1; z<8; z++){
                    if(boardManager.IsPieceAtPos(x, z) == false){legalMoves.Add(new Vector2Int(x, z));}
                    else if(boardManager.GetPieceAtPos(x, z).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(x, z));
                        break;
                    } else{break;}
                }
                //Negative Y
                for(int z = y - 1; z>-1; z--){
                    if(boardManager.IsPieceAtPos(x, z) == false){legalMoves.Add(new Vector2Int(x, z));}
                    else if(boardManager.GetPieceAtPos(x, z).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(x, z));
                        break;
                    }else{break;}
                }
                return legalMoves;
            }
            case PieceType.Queen:
            {
                //Bishop component
                //+x, +y
                for(int z = 1; z <7; z++){

                    if(!IsMoveInRange(new Vector2Int(y + z, x + z))) {break;}

                    if(boardManager.IsPieceAtPos(x + z, y + z) == false){legalMoves.Add(new Vector2Int(x + z, y + z));}
                    else if(boardManager.GetPieceAtPos(x + z, y + z).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(x + z, y + z));
                        break;
                    } else{break;}
                }
                //+x, -y
                for(int z = 1; z <7; z++){

                    if(!IsMoveInRange(new Vector2Int(y - z, x + z))) {break;}

                    if(boardManager.IsPieceAtPos(x + z, y - z) == false){legalMoves.Add(new Vector2Int(x + z, y - z));}
                    else if(boardManager.GetPieceAtPos(x + z, y - z).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(x + z, y - z));
                        break;
                    } else{break;}
                }

                //-x, +y
                for(int z = 1; z <7; z++){

                    if(!IsMoveInRange(new Vector2Int(y + z, x - z))) {break;}

                    if(boardManager.IsPieceAtPos(x - z, y + z) == false){legalMoves.Add(new Vector2Int(x - z, y + z));}
                    else if(boardManager.GetPieceAtPos(x - z, y + z).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(x - z, y + z));
                        break;
                    } else{break;}
                }
                //-x, -y
                for(int z = 1; z <7; z++){

                    if(!IsMoveInRange(new Vector2Int(y - z, x - z))) {break;}

                    if(boardManager.IsPieceAtPos(x - z, y - z) == false){legalMoves.Add(new Vector2Int(x - z, y - z));}
                    else if(boardManager.GetPieceAtPos(x - z, y - z).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(x - z, y - z));
                        break;
                    } else{break;}
                }
                //Rook component
                //Positive X
                for(int z = x + 1; z<8; z++){
                    if(boardManager.IsPieceAtPos(z, y) == false){legalMoves.Add(new Vector2Int(z, y));}
                    else if(boardManager.GetPieceAtPos(z, y).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(z, y));
                        break;
                    } else{break;}
                }
                 //Negative X
                for(int z = x - 1; z>-1; z--){
                    if(boardManager.IsPieceAtPos(z, y) == false){legalMoves.Add(new Vector2Int(z, y));}
                    else if(boardManager.GetPieceAtPos(z, y).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(z, y));
                        break;
                    } else{break;}
                }
                //Positive Y
                for(int z = y + 1; z<8; z++){
                    if(boardManager.IsPieceAtPos(x, z) == false){legalMoves.Add(new Vector2Int(x, z));}
                    else if(boardManager.GetPieceAtPos(x, z).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(x, z));
                        break;
                    } else{break;}
                }
                //Negative Y
                for(int z = y - 1; z>-1; z--){
                    if(boardManager.IsPieceAtPos(x, z) == false){legalMoves.Add(new Vector2Int(x, z));}
                    else if(boardManager.GetPieceAtPos(x, z).isWhite != isWhite){
                        legalMoves.Add(new Vector2Int(x, z));
                        break;
                    }else{break;}
                }
                return legalMoves;
            }
            case PieceType.King:
            {

                return legalMoves;
            }
        }
        return legalMoves;
    }

    //Promotion for pawns
    public void Promote(PieceType newPieceType){
        if(newPieceType == PieceType.Knight){
            render.sprite = isWhite ? knight : blackKnight;
        }else if(newPieceType == PieceType.Bishop){
            render.sprite = isWhite ? bishop : blackBishop;
        }else if(newPieceType == PieceType.Rook){
            render.sprite = isWhite ? rook : blackRook;
        }else if(newPieceType == PieceType.Queen){
            render.sprite = isWhite ? queen : blackQueen;
        }
        pieceType = newPieceType;
    }
    bool IsMoveInRange (Vector2Int move){
        if(move.y >=0 && move.y <8 && move.x >=0 && move.x <8){return true;}
        else{return false;}
    }*/
}
