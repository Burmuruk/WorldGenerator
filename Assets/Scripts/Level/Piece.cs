using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using WorldG.Architecture;

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
    private int versionID = -1;

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
            if (value == rotation) return;

            SideType[] sorted = new SideType[types.Length];

            for (int i = rotation - value, j = 0; j < types.Length; ++i, ++j) 
            {
                if (i >= types.Length) i = 0;
                else if (i < 0) i = types.Length - 1;

                sorted[j] = types[i];
            }

            types = sorted;
            rotation = value;

            CalculateEntrances();
        }
    }
    public Topping Topping { get => _topping; }
    public TileType TileType { get => _tileType; set => _tileType = value; }
    public int Version { get => versionID; }
    public ArrayList Components { get; set; } = null;

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

    public Piece(int rotation, TileType tileType, int versionID, bool completePiece = true)
    {
        piece = null;
        types = new SideType[6];
        Rotation = rotation;
        this.completePiece = completePiece;
        materialIdx = null;
        this.type = SideType.Grass;
        _topping = default;
        ToppingType = ToppingType.None;
        _entrances = null;
        _tileType = tileType;
    }

    public Piece(GameObject piece, TileType tileType, SideType sideType, SideType[] types, int[] materials, int rotation, bool completePiece, int id) :
        this(rotation, tileType, id, completePiece)
    {
        this.piece = piece;
        this.types = (SideType[])types.Clone();
        type = sideType;
        this.materialIdx = (int[])materials.Clone();
        versionID = id;
    }

    public void Deconstruct(out GameObject go, out TileType tileType, out SideType sideType, out SideType[] sideTypes, out int[] materials,
        out int rotation, out bool complete, out int id)
    {
        go = piece;
        tileType = _tileType;
        sideType = type;
        sideTypes = types;
        materials = materialIdx;
        rotation = this.rotation;
        complete = this.completePiece;
        id = versionID;
    }

    public IWorker SetTopping(Topping topping)
    {
        this._topping = topping;
        ToppingType = topping.Type;

        object component = _topping.Type switch
        {
            ToppingType.Rock => new ResourceWorker(),
            ToppingType.Tree => new ResourceWorker(),
            ToppingType.Woods => new ResourceWorker(),
            ToppingType.None => null,
            _ => new BuildingWorker()
        };

        if (component != null)
        {
            (Components??= new()).Add(component);
            var worker = (IWorker)component;
            worker.OnStop += RemoveTopping;

            return worker;
        }

        return null;
    }

    public int GetIdxMaterial(int idx)
    {
        return materialIdx[idx];
    }

    public void Rotate()
    {
        piece.transform.Rotate(new(0, rotation * 60, 0));
    }

    public void ResetRotation()
    {
        piece.transform.Rotate(new(0, rotation * 60, 0));
    }

    public void RemoveTopping()
    {
        _topping = default;
        ToppingType = ToppingType.None;
        Components = null;
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

    public void SetVersion(int version) => versionID = version;

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
