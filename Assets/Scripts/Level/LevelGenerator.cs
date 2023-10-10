using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] PieceCollection PieceData;
    [SerializeField] Piece[,] pieces;
    [SerializeField] Vector2Int size = new Vector2Int(10, 10);

    readonly (Vector2Int cord, Vector2 pos)[] directions =
    {
        (new Vector2Int(1, -1), new Vector2(1, 1.75f)),
        (new Vector2Int(1, 0), new Vector2(2, 0)),
        (new Vector2Int(1, 1), new Vector2(1, -1.75f)),
        (new Vector2Int(-1, 1), new Vector2(-1, -1.75f)),
        (new Vector2Int(-1, 0), new Vector2(-1, 0)),
        (new Vector2Int(-1, -1), new Vector2(-1, 1.75f)),
    };

    private void Start()
    {
        
        pieces = new Piece[size.x, size.y];
        var piece = PieceData.GetPiece(TileType.Grass);
        print(piece.piece);
        CreatePiece(piece , (0, 0));
    }

    private void CreatePiece(Piece piece, in (float x, float y) position)
    {
        
        if (position.x < 0 || position.x >= size.x ||
            position.y < 0 || position.y >= size.y) 
            return;
        
        ref var cur = ref pieces[(int)position.x, (int)position.y];
        //print(cur.piece);



        for (int i = 0; i < 6; i++)
        {
            print(position);
            Vector2 newPos = new Vector2
            {
                x = position.x + (position.x % 2 != 0 ? directions[i].cord.x : 0),
                y = position.y + directions[i].cord.y
            };

            cur.piece = Instantiate(piece.piece, position: newPos, rotation: Quaternion.identity, parent: transform);

            CreatePiece(piece, (newPos.x, newPos.y));
        }
    }
}
