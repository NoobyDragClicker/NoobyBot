using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSelected : MonoBehaviour
{
    public Move move;
    public bool isPromotion;
    public bool isPromoManager;
    UIManager uiManager;
    BoardManager boardManager;
    [SerializeField] SpriteRenderer render;
    [SerializeField] Color white;
    [SerializeField] Sprite bishop;
    [SerializeField] Sprite rook;
    [SerializeField] Sprite knight;
    [SerializeField] Sprite queen;
    [SerializeField] Sprite bbishop;
    [SerializeField] Sprite brook;
    [SerializeField] Sprite bknight;
    [SerializeField] Sprite bqueen;
    [SerializeField] Transform pos;

    void Start(){
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
    }

    //Only used for promotion
    public void SetPromotionRender(){
        render.color = white;
        //Black promotion
        if(move.newIndex > 55){
            switch (move.flag){
                case 3:render.sprite = bknight; pos.position = new Vector3(-1, 1f, pos.position.z); break;
                case 2:render.sprite = bbishop; pos.position = new Vector3(-1, 2f, pos.position.z);break;
                case 1:render.sprite = bqueen; pos.position = new Vector3(-1, 0f, pos.position.z); break;
                case 4:render.sprite = brook; pos.position = new Vector3(-1, 3f, pos.position.z); break;
            }
        //White promotion
        }else{
            switch (move.flag){
                case 3:render.sprite = knight; pos.position = new Vector3(-1, 6f, pos.position.z); break;
                case 2:render.sprite = bishop; pos.position = new Vector3(-1, 5f, pos.position.z); break;
                case 1:render.sprite = queen;  pos.position = new Vector3(-1, 7f, pos.position.z); break;
                case 4:render.sprite = rook;  pos.position = new Vector3(-1, 4f, pos.position.z); break;
            }
        }
    }

    void OnMouseDown(){
        if(!isPromoManager){
            uiManager.boardManager.playerToMove.ChoseMove(move);
        //First dot spawned, no specific piece affiliated, just forces player to promote once selected
        } else{
            uiManager.SpawnPromotionPieces(move);
        }
    }
}
