using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] PieceCollection PieceData;
    [SerializeField] Piece[,] pieces;
    [SerializeField] Vector2Int size = new Vector2Int(10, 10);
    Piece piece = default;

    readonly Vector2Int[] directions =
    {
        new Vector2Int(1, -1),
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, -1),
    };

    readonly (SideType side, float prob)[] startProbs =
    {
        ( SideType.Grass, 100 ),
        ( SideType.Dust, 25 ),
        ( SideType.Water, 10 ),
    };

    private void Start()
    {
        
        pieces = new Piece[size.x, size.y];
        PieceData.Initialize();
        
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
            GetProbability();
            piece = PieceData.GetPiece(TileType.Solid, SideType.Grass);
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

    private void GetProbability(int x, int y, SideType sideType)
    {
        ref Piece piece = ref pieces[x, y];
        var sides = new List<(int idx, SideType sideType)>();

        for (int i = 0; i < directions.Length; ++i)
        {
            if (x + directions[i].x is var nX && nX < 0 || nX > size.x ||
                y + directions[i].y is var nY && nY < 0 || nY > size.y)
                continue;

            ref var nextPiece = ref pieces[x + directions[i].x, y + directions[i].y];

            if (nextPiece[i] != SideType.None)
            {
                sides.Add((i, nextPiece[i]));
            }
        }

        if (sides.Count == 0)
        {
            var rand = UnityEngine.Random.Range(0, 100);
            SideType nSideType = SideType.None;
            for (int i = startProbs.Length - 1; i > 0; i--)
            {
                if (rand <= startProbs[i].prob)
                {
                    nSideType = startProbs[i].side;
                }
            }

            PieceData.GetPiece(TileType.Solid, );
        }
    }

    public void ChangeColor(Piece piece, SideType sideType)
    {

    }
}
