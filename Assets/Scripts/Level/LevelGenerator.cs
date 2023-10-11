using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] PieceCollection PieceData;
    [SerializeField] Piece[,] pieces;
    [SerializeField] Vector2Int size = new Vector2Int(10, 10);

    readonly Vector2Int[] directions =
    {
        new Vector2Int(1, -1),
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, -1),
    };

    private void Start()
    {
        
        pieces = new Piece[size.x, size.y];
        
        
        CreatePiece((0, 0));
    }

    private void CreatePiece(in (float x, float y) cord)
    {
        
        if (cord.x < 0 || cord.x >= size.x ||
            cord.y < 0 || cord.y >= size.y) 
            return;
        
        ref var cur = ref pieces[(int)cord.x, (int)cord.y];

        if (!cur)
        {
            var piece = PieceData.GetPiece(TileType.Solid, SideType.Grass);
            cur = piece;

            Vector3 curPos = new Vector3(cord.x * 2, 0, cord.y * -1.75f);

            if ((cord.y % 2 != 0))
                curPos += Vector3.right;

            cur.piece = Instantiate(piece.piece, position: curPos, rotation: Quaternion.identity, parent: transform);
            PieceData.ChangeColor(cur, SideType.Water);
        }
        else
            return;

        for (int i = 0; i < directions.Length; i++)
        {
            var newCords = directions[i];

            Vector2 newPos = new Vector2
            {
                x = cord.x + ((cord.y % 2 == 0 && newCords.y != 0) ? 0 : newCords.x),
                y = cord.y + newCords.y
            };

            CreatePiece((newPos.x, newPos.y));
        }
    }

    public void ChangeColor(Piece piece, SideType sideType)
    {

    }
}
