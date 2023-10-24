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

[CreateAssetMenu(fileName = "Persona", menuName = "ScriptableObjects/LevelPiece", order = 2)]
public class PieceCollection : ScriptableObject
{
    [SerializeField] MaterialData[] materials;
    [SerializeField] Piece[] pieces;
    [SerializeField] bool initialized = false;
    [SerializeField] Hi ho;

    [Serializable]
    public record Hi
    {
        public int value;
    }

    Dictionary<TileType, List<Piece>> _pieces;
    Dictionary<SideType, Material> _materials;

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

    public void ChangeColor(Piece piece, SideType target, int idx = 0)
    {
        if (target == SideType.None) return;

        var renderer = piece.piece.GetComponentInChildren<MeshRenderer>(false);

        renderer.materials[piece.GetIdxMaterial(idx)].CopyPropertiesFromMaterial(_materials[target]);

        piece.Type = target;
    }
}

[Serializable]
public struct MaterialData
{
    [SerializeField] public Material material;
    [SerializeField] public SideType type;
}