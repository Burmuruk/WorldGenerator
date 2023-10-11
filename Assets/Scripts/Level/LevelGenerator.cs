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
        (new Vector2Int(-1, 0), new Vector2(-2, 0)),
        (new Vector2Int(-1, -1), new Vector2(-1, 1.75f)),
    };

    private void Start()
    {
        
        pieces = new Piece[size.x, size.y];
        var piece = PieceData.GetPiece(TileType.Grass);
        print(piece.piece);
        CreatePiece(piece , (0, 0));
    }

    private void CreatePiece(Piece piece, in (float x, float y) cord, int dirIdx = 0)
    {
        
        if (cord.x < 0 || cord.x >= size.x ||
            cord.y < 0 || cord.y >= size.y) 
            return;
        
        ref var cur = ref pieces[(int)cord.x, (int)cord.y];

        if (!cur)
        {
            Vector3 curPos = new Vector3(cord.x * 2, 0, cord.y * -1.75f);

            if ((cord.y % 2 != 0))
                curPos += Vector3.right;

            cur.piece = Instantiate(piece.piece, position: curPos, rotation: Quaternion.identity, parent: transform);
        }
        else
            return;

        for (int i = 0; i < directions.Length; i++)
        {
            print(cord);
            var newCords = directions[i].cord;

            Vector2 newPos = new Vector2
            {
                x = cord.x + ((cord.y % 2 == 0 && newCords.y != 0) ? 0 : newCords.x),
                y = cord.y + newCords.y
            };

            CreatePiece(piece, (newPos.x, newPos.y), i);
        }
    }
}
