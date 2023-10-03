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
public class Piece : ScriptableObject
{
    [SerializeField] (Material, TileType) materials;
    [SerializeField] PieceData[] pieces;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
public struct PieceData
{
    [SerializeField] GameObject piece;
    [SerializeField] TileType[] types;
    [SerializeField] bool completePiece;
    [SerializeField] int rotation;

    public int Rotation
    {
        get => rotation;
        set
        {
            rotation = value;
        }
    }

    public PieceData(int rotation, bool completePiece = true)
    {
        piece = null;
        types = new TileType[6];
        this.completePiece = completePiece;
        this.rotation = rotation;
    }
}

public struct MaterialData
{
    [SerializeField] 
}