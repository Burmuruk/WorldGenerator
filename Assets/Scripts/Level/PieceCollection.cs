using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum TileType
{
    None,
    Grass,
    Road,
    Mudd,
    Dust,
    Water
}

[CreateAssetMenu(fileName = "Persona", menuName = "ScriptableObjects/LevelPiece", order = 2)]
public class PieceCollection : ScriptableObject
{
    [SerializeField] MaterialData[] materials;
    [SerializeField] Piece[] pieces;

    Dictionary<TileType, Piece> _pieces;

    public Piece GetPiece(TileType tileType)
    {
        var piece = pieces[0];

        return piece;
    }

    public void GetPiece(TileType tileType, params (int idx, TileType sideType)[] sides)
    {


        //return _pieces[tileType][]

    }
}

[Serializable]
public struct Piece
{
    [SerializeField] public GameObject piece;
    [SerializeField] public TileType[] types;
    [SerializeField] public bool completePiece;
    [SerializeField] public int rotation;
    public List<TileType> materials;

    public TileType this[int i] { get => types[i]; }

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
        materials = new();
        piece = null;
        types = new TileType[6];
        this.completePiece = completePiece;
        this.rotation = rotation;
    }

    public void SetMaterial(TileType material)
    {
        materials.Add(material);
    }
    
    public static bool operator == (Piece piece1, Piece piece2)
    {
        if (piece1.piece == piece2.piece)
            return true;
        else
            return false;
    }

    public static bool operator != (Piece piece1, Piece piece2)
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

[Serializable]
public struct MaterialData
{
    [SerializeField] Material material;
    [SerializeField] TileType type;
}