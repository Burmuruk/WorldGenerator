using System;
using UnityEngine;

public enum ToppingType
{
    None,
    Castle,
    Farm,
    House,
    Tower,
    Wall,
    Tree,
    Wood,
    Rock,
}

[Serializable]
public record Piece
{
    [SerializeField] public GameObject piece;
    [SerializeField] private SideType type;
    [SerializeField] public SideType[] types;
    [SerializeField] public int[] materialIdx;
    [SerializeField] public bool completePiece;
    [SerializeField] public int rotation;

    private GameObject topping;

    public SideType Type 
    { 
        get => type; 
        set
        {
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == type)
                {
                    types[i] = value;
                }
            }

            type = value;
        }
    }
    public ToppingType ToppingType
    {
        get;
        private set;
    }

    public SideType this[int i] { get => types[i]; }

    public int Rotation
    {
        get => rotation;
        set
        {
            rotation = value;
        }
    }

    public Piece(int rotation, bool completePiece = true)
    {
        piece = null;
        types = new SideType[6];
        this.completePiece = completePiece;
        this.rotation = rotation;
        materialIdx = null;
        this.type = SideType.Grass;
        topping = null;
        ToppingType = ToppingType.None;
    }

    public void SetTopping(GameObject topping, ToppingType type)
    {
        ToppingType = type;
        this.topping = topping;
    }

    public int GetIdxMaterial(int idx)
    {
        return materialIdx[idx];
    }

    //public override bool Equals(object obj)
    //{
    //    return obj is Piece piece &&
    //           EqualityComparer<GameObject>.Default.Equals(this.piece, piece.piece) &&
    //           EqualityComparer<SideType[]>.Default.Equals(types, piece.types);
    //}

    public override int GetHashCode()
    {
        return HashCode.Combine(piece, types);
    }

    //public static bool operator ==(Piece piece1, Piece piece2)
    //{
    //    if (piece1.piece == piece2.piece)
    //        return true;
    //    else
    //        return false;
    //}

    //public static bool operator !=(Piece piece1, Piece piece2)
    //{
    //    if (piece1.piece != piece2.piece)
    //        return true;
    //    else
    //        return false;
    //}

    public static bool operator !(Piece piece) => piece == null || !piece.piece;

    public static bool operator true(Piece piece1) => piece1 != null && piece1.piece == true;

    public static bool operator false(Piece piece1) => piece1.piece == false;
}
