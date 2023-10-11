using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public struct Piece
{
    [SerializeField] public GameObject piece;
    [SerializeField] public SideType[] types;
    [SerializeField] public int[] materialIdx;
    [SerializeField] public bool completePiece;
    [SerializeField] public int rotation;

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
    }

    public int[] GetMaterial()
    {
        return materialIdx;
    }

    public override bool Equals(object obj)
    {
        return obj is Piece piece &&
               EqualityComparer<GameObject>.Default.Equals(this.piece, piece.piece) &&
               EqualityComparer<SideType[]>.Default.Equals(types, piece.types);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(piece, types);
    }

    public static bool operator ==(Piece piece1, Piece piece2)
    {
        if (piece1.piece == piece2.piece)
            return true;
        else
            return false;
    }

    public static bool operator !=(Piece piece1, Piece piece2)
    {
        if (piece1.piece != piece2.piece)
            return true;
        else
            return false;
    }

    public static bool operator !(Piece piece) => !piece.piece;

    public static bool operator true(Piece piece1) => piece1.piece == true;

    public static bool operator false(Piece piece1) => piece1.piece == false;
}
