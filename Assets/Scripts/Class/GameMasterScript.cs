using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMasterScript : MonoBehaviour
{
    public static GameMasterScript Instance;
    public List<ScriptableObjectEjemplo> personas = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        
    }


    public void LlamarATodasLasPersonas()
    {
        personas.ForEach(n => { if (n) n.Gritar(); });
    }
}
