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

    public void Init(int type, int color){

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
}

