﻿using System;
using UnityEngine;

public enum ToppingType
{
    None,
    Castle,
    Mill,
    House,
    Tower,
    Wall,
    Tree,
    Woods,
    Rock,
    Archery,
    Barracks,
    Bridge,
    BridgePro,
    FarmPlot,
    Market,
    Lumbermill,
    Mine,
    Gate,
    Watermill,
    well
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

    private int _entrances;

    private Topping _topping;

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

            Rotate(value);
        }
    }
    public Topping Topping { get => _topping; }

    public int Entrances
    {
        get
        {
            if (_entrances != 0) return _entrances;

            foreach (var t in types)
                if (t == type) _entrances++;

            return _entrances;
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
        _topping = default;
        ToppingType = ToppingType.None;
        _entrances = 0;
    }

    public void SetTopping(Topping topping, int rotation = 0)
    {
        this._topping = topping;
        ToppingType = topping.Type;
    }

    public int GetIdxMaterial(int idx)
    {
        return materialIdx[idx];
    }

    public void Rotate(int value)
    {
        var last = types[0];
        int curIdx = 0;
        for (int i = 1; i < types.Length; i++)
        {
            curIdx += value;
            if (curIdx >= types.Length) curIdx -= types.Length;

            var cur = types[curIdx];
            types[curIdx] = last;
            last = cur;
        }
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
