using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] PieceCollection PieceData;
    [SerializeField] Piece[,] pieces;
    [SerializeField] Vector2Int size = new Vector2Int(10, 10);
    //Piece piece = default;

    readonly Vector2Int[] directions =
    {
        new Vector2Int(1, -1),
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, -1),
    };

    readonly Dictionary<SideType, Probs> probs = new()
    {
        { SideType.Grass, new Probs((SideType.Grass, .70f), (SideType.Water, .02f), (SideType.Dust, .25f)) } ,
        { SideType.Water, new Probs((SideType.Grass, .15f), (SideType.Water, .75f), (SideType.Dust, .10f)) },
        { SideType.Dust, new Probs((SideType.Grass, .15f), (SideType.Water, .10f), (SideType.Dust, .75f)) },
    };

    struct Probs
    {
        (SideType type, int prob)[] probs;

        public Probs(params (SideType type, float prob)[] ps)
        {
            for (int i = 0; i < ps.Length; i++)
            {
                for (int j = 0; j < ps.Length; j++)
                {
                    if (ps[i].prob < ps[j].prob)
                    {
                        var value1 = ps[j];

                        ps[j] = ps[i];
                        ps[i] = value1;
                    }
                }
            }

            probs = new (SideType type, int prob)[ps.Length];
            int curVal = 0;

            for (int i = 0; i < ps.Length; i++)
            {
                curVal = (int)(ps[i].prob * 100 + curVal);
                probs[i] = (ps[i].type, curVal);
            }
        }

        public SideType GetHI(int rand)
        {
            SideType type = probs[0].type;



            if (rand < probs[0].prob)
            {
                type = probs[0].type;
            }
            else if (rand < probs[1].prob)
            {
                type = probs[1].type;
            }
            else if (rand < probs[2].prob)
            {
                type = probs[2].type;
            }

            return type;
        }
    }


    private void Start()
    {
        
        pieces = new Piece[size.x, size.y];
        PieceData.Initialize();
        
        CreatePiece((0, 0), SideType.None);
    }

    private void CreatePiece(in (float x, float y) cord, SideType prevSide)
    {
        
        if (cord.x < 0 || cord.x >= size.x ||
            cord.y < 0 || cord.y >= size.y) 
            return;
        
        ref var cur = ref pieces[(int)cord.x, (int)cord.y];

        if (!cur)
        {
            
            var piece = PieceData.GetPiece(TileType.Solid);
            cur = piece;

            Vector3 curPos = new Vector3(cord.x * 2, 0, cord.y * -1.75f);

            if ((cord.y % 2 != 0))
                curPos += Vector3.right;

            cur.piece = Instantiate(piece.piece, position: curPos, rotation: Quaternion.identity, parent: transform);
            PieceData.ChangeColor(cur, GetProbability(prevSide));
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

            CreatePiece((newPos.x, newPos.y), cur[i]);
        }
    }

    private SideType GetProbability(SideType type)
    {
        if (type == SideType.None)
            return SideType.Grass;

        return  probs[type].GetHI(UnityEngine.Random.Range(0, 100));
        
    }

    private void GetProbability(int x, int y, SideType sideType)
    {
        ref Piece piece = ref pieces[x, y];
        //var sides = new List<(int idx, SideType sideType)>();

        for (int i = 0; i < directions.Length; ++i)
        {
            if (x + directions[i].x is var nX && nX < 0 || nX > size.x ||
                y + directions[i].y is var nY && nY < 0 || nY > size.y)
                continue;

            ref var nextPiece = ref pieces[x + directions[i].x, y + directions[i].y];



            //if (nextPiece[i] != SideType.None)
            //{
            //    sides.Add((i, nextPiece[i]));
            //}
        }



        //if (sides.Count == 0)
        //{
        //    var rand = UnityEngine.Random.Range(0, 100);
        //    SideType nSideType = SideType.None;
        //    for (int i = GrassProbs.Length - 1; i > 0; i--)
        //    {
        //        if (rand <= GrassProbs[i].prob)
        //        {
        //            nSideType = GrassProbs[i].side;
        //        }
        //    }

        //    //PieceData.GetPiece(TileType.Solid, );
        //}
    }
}
