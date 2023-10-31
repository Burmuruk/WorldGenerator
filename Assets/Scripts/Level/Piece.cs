using System;
using System.Collections.Generic;
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
    [SerializeField] private GameObject piece;
    [SerializeField] private SideType type;
    [SerializeField] private SideType[] types;
    [SerializeField] private int[] materialIdx;
    [SerializeField] private bool completePiece;
    [SerializeField] private int rotation;

    private TileType _tileType;
    private int[] _entrances;
    private Topping _topping;

    public GameObject Prefab { get { return piece; } set => piece = value; }
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
    public SideType[] Types { get => types; }
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
            var last = types[0];
            int curIdx = 0;
            for (int i = 1; i < types.Length; i++)
            {
                curIdx += value - rotation;
                if (curIdx >= types.Length) curIdx -= types.Length;

                var cur = types[curIdx];
                types[curIdx] = last;
                last = cur;
            }

            types[0] = last;
            rotation = value;

            CalculateEntrances();
        }
    }
    public Topping Topping { get => _topping; }
    public TileType TileType { get => _tileType; set => _tileType = value; }

    public int[] Entrances
    {
        get
        {
            if (_entrances != null && _entrances.Length != 0) 
                return _entrances;

            CalculateEntrances();

            return _entrances;
        }
    }

    public Piece(int rotation, TileType tileType, bool completePiece = true)
    {
        piece = null;
        types = new SideType[6];
        this.completePiece = completePiece;
        this.rotation = rotation;
        materialIdx = null;
        this.type = SideType.Grass;
        _topping = default;
        ToppingType = ToppingType.None;
        _entrances = null;
        _tileType = tileType;
    }

    public Piece(GameObject piece, TileType tileType, SideType sideType, SideType[] types, int[] materials, int rotation, bool completePiece) :
        this(rotation, tileType, completePiece)
    {
        this.piece = piece;
        this.types = (SideType[])types.Clone();
        type = sideType;
        this.materialIdx = (int[])materials.Clone();
    }

    public void Deconstruct(out GameObject go, out TileType tileType, out SideType sideType, out SideType[] sideTypes, out int[] materials,
        out int rotation, out bool complete)
    {
        go = piece;
        tileType = _tileType;
        sideType = type;
        sideTypes = types;
        materials = materialIdx;
        rotation = this.rotation;
        complete = this.completePiece;
    }

    //public void Deconstruct(out GameObject go)
    //{
    //    go = piece;
    //}

    public void SetTopping(Topping topping)
    {
        this._topping = topping;
        ToppingType = topping.Type;
    }

    public int GetIdxMaterial(int idx)
    {
        return materialIdx[idx];
    }

    public void Rotate()
    {
        piece.transform.Rotate(new(0, rotation * 60, 0));
    }

    public void RemoveTopping()
    {
        _topping = default;
        ToppingType = ToppingType.None;
    }

    public void ResetEntrances() => _entrances = null;

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

    private void CalculateEntrances()
    {
        _entrances = null;

        SideType type = _tileType switch
        {
            TileType.None => SideType.None,
            TileType.Road => SideType.Road,
            _ => Type
        };

        List<int> entrances = new();
        for (int i = 0; i < types.Length; i++)
            if (types[i] == type)
            {
                entrances.Add(i);
            }

        if (entrances.Count > 0)
            _entrances = entrances.ToArray();
    }
}
