using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    None,
    Grass
}

[CreateAssetMenu(fileName = "Persona", menuName = "ScriptableObjects/LevelPiece", order = 2)]
public class Piece : ScriptableObject
{
    [SerializeField] GameObject prefab;
    [SerializeField] TileType[] types;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
