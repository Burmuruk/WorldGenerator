using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum SideType
{
    None,
    Grass,
    Road,
    Mudd,
    Dust,
    Water
}

public enum TileType
{
    None,
    Solid,
    Transition,
    Road,
}

[Serializable]
public struct Area
{
    [SerializeField] SideType type;
    [SerializeField] int minSize;
    [SerializeField] int maxSize;
    [SerializeField, Range(0, 1)] bool randomDirection;
    [SerializeField, Range(0, 1)] float randDirProb2;
    [SerializeField, Range(0, 6)] int deepSearch;
    [Space(), Header("Toppings")]
    [SerializeField] ToppingType toppingType;
    [SerializeField, Range(0, 1)]float toppingProb;

    int _size;

    public SideType Type { get => type; }
    public int Size { get => _size; private set => _size = value; }

    public Area(SideType sideType, ToppingType toppingType = ToppingType.None)
    {
        this.toppingType = toppingType;
        toppingProb = 0f;
        this.type = sideType;
        minSize = 0;
        maxSize = 10;
        randomDirection = false;
        randDirProb2 = 7;
        deepSearch = 2;
        this._size = 6;
    }

    public int RandomSize() => UnityEngine.Random.Range(minSize, maxSize);
}

[CreateAssetMenu(fileName = "Persona", menuName = "ScriptableObjects/LevelPiece", order = 2)]
public class PieceCollection : ScriptableObject
{
    [SerializeField] MaterialData[] materials;
    [SerializeField] Piece[] pieces;
    [SerializeField] PieceRelations[] relations;
    [SerializeField] Area[] areas;
    [SerializeField] bool initialized = false;

    [Serializable]
    public struct PieceRelation
    {
        [SerializeField] SideType type;
        [SerializeField] float probability;

        public SideType Type { get { return type; } }
        public float Probability { get { return probability; } }
    }

    [Serializable]
    public struct PieceRelations
    {
        [SerializeField] SideType type;
        [SerializeField] List<PieceRelation> relations;

        public SideType Type { get { return type; } }
        public List<PieceRelation> Relations { get { return relations; } }
    }

    Dictionary<TileType, List<Piece>> _pieces;
    Dictionary<SideType, Material> _materials;
    Dictionary<SideType, Area> _areas;
    Dictionary<SideType, Probabilities<SideType>> _pieceProbs;
    Dictionary<SideType, Probabilities<ToppingType>> _toppingProbs;

    public void Initialize()
    {
        //if (initialized) return;

        _pieces = new ();

        var chunck = new List<List<Piece>>()
        {
            new (),
            new (),
            new (),
        };

        for (int i = 0; i < pieces.Length; ++i)
        {
            SideType lastType = pieces[i][0];
            bool isSolid = true;

            foreach (var tileType in pieces[i].types)
            {
                if (tileType == SideType.Road)
                {
                    chunck[0].Add(pieces[i]);
                    isSolid = false;
                    break;
                }
                else if (lastType != tileType)
                {
                    chunck[1].Add(pieces[i]);
                    isSolid = false;
                    break;
                }
                else
                    isSolid = true;
            }

            if (isSolid)
                chunck[2].Add(pieces[i]);
        }

        _pieces.Add(TileType.Road, chunck[0]);
        _pieces.Add(TileType.Transition, chunck[1]);
        _pieces.Add(TileType.Solid, chunck[2]);

        _materials = new();
        foreach (var mat in materials)
        {
            _materials.Add(mat.type, mat.material); 
        }

        _areas = new();
        foreach (var area in areas)
            _areas.Add(area.Type, area);

        _pieceProbs = new();
        foreach (var piece in relations)
        {
            List<(SideType, float)> values = new();

            foreach (var relation in piece.Relations)
                values.Add((relation.Type, relation.Probability));

            _pieceProbs.Add(piece.Type, new(piece.Type, values.ToArray()));
        }

        initialized = true;
    }

    public Piece GetPiece(TileType tileType)
    {
        //Initialize();

        var rand = UnityEngine.Random.Range(0, _pieces[tileType].Count);

        return _pieces[tileType][rand];
    }

    public void GetPiece(TileType tileType, params (int idx, SideType sideType)[] sides)
    {


        //return _pieces[tileType][]

    }

    public Area GetArea(SideType type)
    {
        return _areas[type];
    }

    public void ChangeType(Piece piece, SideType target, int idx = 0)
    {
        if (target == SideType.None) return;

        var renderer = piece.piece.GetComponentInChildren<MeshRenderer>(false);

        renderer.materials[piece.GetIdxMaterial(idx)].CopyPropertiesFromMaterial(_materials[target]);

        piece.Type = target;
    }

    public Probabilities<SideType> GetProbability(SideType type) => _pieceProbs[type];

    public void ChangeColor(Piece piece, SideType target, int idx = 0)
    {
        if (target == SideType.None) return;

        var renderer = piece.piece.GetComponentInChildren<MeshRenderer>(false);

        renderer.materials[piece.GetIdxMaterial(idx)].CopyPropertiesFromMaterial(_materials[target]);
    }
}

[Serializable]
public struct MaterialData
{
    [SerializeField] public Material material;
    [SerializeField] public SideType type;
}