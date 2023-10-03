using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Persona", menuName = "ScriptableObjects/PersonaSO", order = 1)]
public class ScriptableObjectEjemplo : ScriptableObject
{
    public string nombre;
    public int edad;
    public bool vivo;

    public void Gritar()
    {
        Debug.Log($"Mi nombre es: {nombre} y tengo: {edad} años.");
    }
}
