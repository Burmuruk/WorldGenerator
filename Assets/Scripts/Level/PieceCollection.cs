using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using WorldG.Patrol;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;

public enum SideType
{
    None,
    Grass,
    Road,
    Mudd,
    Dust,
    Water,
    Woods,
}

public enum TileType
{
    None,
    Solid,
    Transition,
    Road,
}

public enum ToppingSide
{
    None,
    Connection,
    Entrance,
}

public enum CharacterType
{
    None,
    Archer,
    Knight,
    Hood,
    Skeleton,
    Viking,
    Citizen
}

[Serializable]
public struct Area
{
    [SerializeField] SideType type;
    [SerializeField] int minSize;
    [SerializeField] int maxSize;
    [SerializeField, Range(0, 1)] float randomDirection;
    //[SerializeField, Range(0, 1)] float randDirProb2;
    [SerializeField, Range(0, 6)] int deepSearch;

    [Space(), Header("Toppings")]
    [SerializeField] PieceRelation<ToppingType>[] toppingTypes;

    int _size;

    public SideType Type { get => type; }
    public int Size { get => _size; private set => _size = value; }
    public float RandomDirection { get => randomDirection; }
    public int DeepSearch { get => deepSearch; }
    public PieceRelation<ToppingType>[] ToppingTypes { get => toppingTypes; }

    public Area(SideType sideType)
    {
        toppingTypes = null;
        this.type = sideType;
        minSize = 0;
        maxSize = 10;
        randomDirection = 3;
        //randDirProb2 = 7;
        deepSearch = 2;
        this._size = 6;
    }

    public int GetRandomSize() => UnityEngine.Random.Range(minSize, maxSize);
}

[Serializable]
public struct Topping
{
    [SerializeField] ToppingType _type;
    [SerializeField] GameObject _piece;
    [SerializeField] bool _haveSpline;
    [SerializeField] ToppingSide[] _sides;
    PatrolController _patrol;

    public GameObject Prefab { get => _piece; }
    public ToppingType Type { get { return _type; } }
    public PatrolController Patrol 
    { 
        get 
        {
            if (!_haveSpline) return null;

            var patrol = _piece.GetComponent<PatrolController>();

            if (patrol == null) throw new NullReferenceException();

            return patrol;
        } 
    }

    public Topping(ToppingType type, GameObject piece, ToppingSide[] sides, bool haveSpline = false)
    {
        _type = type;
        _piece = piece;
        _sides = sides;
        _haveSpline = haveSpline;

        if (haveSpline)
            _patrol = new PatrolController();
        else
            _patrol = null;
    }

    public void SetPrefab(GameObject prefab) => _piece = prefab;

    public void Rotate(int rotation) =>
        _piece.transform.Rotate(new(0, rotation * 60, 0));
}

[Serializable]
public struct ToppingGroup
{
    [SerializeField] public ToppingType type;
    [SerializeField] public GameObject[] pieces;
    [SerializeField] public bool haveSpline;
    [SerializeField] public ToppingSide[] sides;
}

[Serializable]
public struct PieceRelation<T> where T : Enum
{
    [SerializeField] T type;
    [SerializeField] float probability;

    public T Type { get { return type; } }
    public float Probability { get { return probability; } }
}

[Serializable]
public struct Character
{
    [SerializeField] CharacterType _type;
    [SerializeField] GameObject _prefab;
    //[SerializeField] int _rotation;

    public CharacterType Type { get { return _type; } }
    public GameObject Prefab { get { return _prefab; } }

    public void SetPiece(GameObject piece)
    {
        _prefab = piece;
    }

    public void Rotate(int rotation)
    {
        _prefab.transform.Rotate(new(0, rotation * 60, 0));
    }
}

[Serializable]
public struct PieceRelations
{
    [SerializeField] SideType type;
    [SerializeField] PieceRelation<SideType>[] pieceRelations;
    [SerializeField] PieceRelation<ToppingType>[] toppingRelations;

    public SideType Type { get { return type; } }
    public PieceRelation<SideType>[] SideTypes { get { return pieceRelations; } }
    public PieceRelation<ToppingType>[] ToppingTypes { get { return toppingRelations; } }
}

[CreateAssetMenu(fileName = "Persona", menuName = "ScriptableObjects/LevelPiece", order = 2)]
public class PieceCollection : ScriptableObject
{
    [SerializeField] MaterialData[] materials;
    [SerializeField] Topping[] toppings;
    [SerializeField] ToppingGroup[] toppingGroups;
    [SerializeField] Piece[] pieces;
    [SerializeField] Character[] characters;
    [SerializeField] PieceRelations[] relations;
    [SerializeField] Area[] areas;
    [SerializeField] bool initialized = false;

    Dictionary<TileType, List<Piece>> _pieces;
    Dictionary<SideType, Material> _materials;
    Dictionary< CharacterType, Character> _characters;
    Dictionary<SideType, Area> _areas;
    Dictionary<ToppingType, List<Topping>> _toppings;
    Dictionary<SideType, Probabilities<ToppingType>> _toppingAreas;
    Dictionary<SideType, Probabilities<SideType>> _pieceProbs;
    Dictionary<SideType, Probabilities<ToppingType>> _toppingProbs;

    public void Initialize()
    {
        //if (initialized) return;
        //Pieces
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
            pieces[i].ResetEntrances();

            foreach (var sideType in pieces[i].Types)
            {
                if (sideType == SideType.Road)
                {
                    pieces[i].TileType = TileType.Road;
                    chunck[0].Add(pieces[i]);
                    isSolid = false;
                    break;
                }
                else if (lastType != sideType)
                {
                    pieces[i].TileType = TileType.Transition;
                    chunck[1].Add(pieces[i]);
                    isSolid = false;
                    break;
                }
                else
                {
                    pieces[i].TileType = TileType.Solid;
                    isSolid = true;
                }
            }

            if (isSolid)
                chunck[2].Add(pieces[i]);
        }

        _pieces.Add(TileType.Road, chunck[0]);
        _pieces.Add(TileType.Transition, chunck[1]);
        _pieces.Add(TileType.Solid, chunck[2]);

        //Materials
        _materials = new();
        foreach (var mat in materials)
        {
            _materials.Add(mat.type, mat.material); 
        }

        //Pieces' probabilities
        _pieceProbs = new();
        foreach (var piece in relations)
        {
            List<(SideType, float)> values = new();

            foreach (var relation in piece.SideTypes)
                values.Add((relation.Type, relation.Probability));

            _pieceProbs.Add(piece.Type, new(piece.Type, values.ToArray()));
        }

        //toppings
        _toppings = new();
        foreach (var topping in toppings)
        {
            if (!_toppings.ContainsKey(topping.Type))
                _toppings.Add(topping.Type, new());
            
            _toppings[topping.Type].Add(topping);
        }

        //toppings groups
        foreach (var group in toppingGroups)
        {
            if (!_toppings.ContainsKey(group.type))
                _toppings.Add(group.type, new());

            foreach (var topp in group.pieces)
                foreach (var piece in group.pieces)
                {
                    _toppings[group.type].Add(new Topping(group.type, piece, group.sides, group.haveSpline));
                }
        }

        //topping Probabilities
        _toppingProbs = new();
        foreach (var relation in relations)
        {
            List<(ToppingType type, float prob)> toppings = new();

            foreach (var topp in relation.ToppingTypes)
            {
                toppings.Add((topp.Type, topp.Probability));
            }

            _toppingProbs.Add(relation.Type, new(ToppingType.None, toppings.ToArray()));
        }

        //Areas
        _areas = new();
        _toppingAreas = new();
        foreach (var area in areas)
        {
            _areas.Add(area.Type, area);

            List<(ToppingType type, float prob)> toppings = new();

            foreach (var topp in area.ToppingTypes)
            {
                toppings.Add((topp.Type, topp.Probability));
            }

            _toppingAreas.Add(area.Type, new(ToppingType.None, toppings.ToArray()));
        }

        //Areas' toppings

        //Characters
        _characters = new();
        foreach (var character in characters)
        {
            _characters.Add(character.Type,  character);
        }


        initialized = true;
    }

    public Piece GetPiece(TileType tileType)
    {
        //Initialize();

        var rand = UnityEngine.Random.Range(0, _pieces[tileType].Count);

        return _pieces[tileType][rand];
    }

    public Piece GetRandomPiece(TileType tileType)
    {
        //Initialize();

        var rand = UnityEngine.Random.Range(0, _pieces[tileType].Count);

        return _pieces[tileType][rand];
    }

    public Piece GetPiece(TileType tileType, params (int idx, SideType sideType)[] sides)
    {
        var piece = FindPieceByRoads(tileType, sides);

        if (piece == null) 
            return default;

        var (go, ttype, sType, sTypes, mats, rot, comp) = piece;
        var copy = new Piece(go, ttype, sType, sTypes, mats, rot, comp);
        
        bool matched = RotateUntilMatch(ref copy, sides);

        if (!matched) 
            return default;

        return copy;

        bool RotateUntilMatch(ref Piece copy, in (int idx, SideType sideType)[] sides)
        {
            bool finished = false;

            for (int i = 0; i < copy.Types.Length; i++)
            {
                for (int j = 0; j < sides.Length; j++)
                {
                    finished = true;
                    if (sides[j].sideType != copy[sides[j].idx])
                    {
                        finished = false;
                        break;
                    }
                }

                if (finished)
                    break;
                else
                    copy.Rotation += 1;
            }

            return finished;
        }
    }

    private Piece FindPieceByRoads(TileType tileType, params (int idx, SideType sideType)[] sides)
    {
        var roads = _pieces[tileType];
        List<int> selectedRoads = new();

        for (int i = 0; i < roads.Count; i++)
            if (roads[i].Entrances != null && roads[i].Entrances.Length == sides.Length)
                selectedRoads.Add(i);

        if (selectedRoads.Count <= 0) return default;

        if (selectedRoads.Count == 1) return roads[selectedRoads[0]];

        List<uint> distances = new();
        uint total = 0;
        string disS1 = "";
        for (int i = 1; i < sides.Length; i++)
        {
            var value = (uint)Mathf.Abs(sides[i].idx - sides[i - 1].idx);
            disS1 += value.ToShortString();
            distances.Add(value);
            total += value;
        }
        disS1 += ((uint)Mathf.Abs(roads[0].Types.Length - total)).ToString();
        int count = disS1.Length;
        for (int i = 0; i < count; i++)
            disS1 += disS1[i];
        
        int idx = 0;
        bool founded = false;

        for (idx = 0; idx < selectedRoads.Count; idx++)
        {
            List<uint> distances2 = new();
            var cur = roads[selectedRoads[idx]];
            uint total2 = 0;
            string disS2 = "";

            for (int j = 1; j < cur.Entrances.Length; j++)
            {
                uint value;
                founded = false;
                total2 += value = (uint)Mathf.Abs(cur.Entrances[j] - cur.Entrances[j - 1]);
                disS2 += value.ToShortString();
            }

            disS2 += ((uint)Mathf.Abs(roads[0].Types.Length - total2)).ToString();

            if (disS1.Contains(disS2))
            {
                founded = true;
                break;
            }
        }

        return founded ? roads[selectedRoads[idx]] : default;
    }

    public Area GetArea(SideType type)
    {
        return _areas[type];
    }

    public Topping? GetRandomTopp(SideType sideType)
    {
        var nextType = _toppingProbs[sideType].GetNextType(UnityEngine.Random.Range(0, 100));

        if (nextType == ToppingType.None) return null;

        return _toppings[nextType][UnityEngine.Random.Range(0, _toppings[nextType].Count)];
    }

    public Topping GetTopping(ToppingType type)
    {
        int idx = 0;

        if (_toppings[type].Count > 1)
            idx = UnityEngine.Random.Range(0, _toppings[type].Count);
        
        return _toppings[type][idx];
    }

    public Character GetCharacter(CharacterType type)
    {
        return _characters[type];
    }

    public void ChangeType(Piece piece, SideType target, int idx = 0)
    {
        if (target == SideType.None) return;

        if (piece)
        {
            var renderer = piece.Prefab.GetComponentInChildren<MeshRenderer>(false);

            renderer.materials[piece.GetIdxMaterial(idx)].CopyPropertiesFromMaterial(_materials[target]);
        }

        piece.Type = target;
    }

    public Probabilities<SideType> GetProbability(SideType type) => _pieceProbs[type];

    public void ChangeColor(Piece piece, SideType target, int idx = 0)
    {
        if (target == SideType.None) return;

        var renderer = piece.Prefab.GetComponentInChildren<MeshRenderer>(false);

        renderer.materials[piece.GetIdxMaterial(idx)].CopyPropertiesFromMaterial(_materials[target]);
    }
}

[Serializable]
public struct MaterialData
{
    [SerializeField] public Material material;
    [SerializeField] public SideType type;
}