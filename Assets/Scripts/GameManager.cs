using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /// <summary>インクの配列</summary>
    GameObject[] _inkObjects;

    void Start()
    {
        
    }

    void Update()
    {
        InkReset();
    }

    /// <summary>
    /// マップにあるインク全てを消す
    /// </summary>
    void InkReset()
    {
        _inkObjects = GameObject.FindGameObjectsWithTag("Ink");

        if (Input.GetButtonDown("Reset"))
        {
            for (int i = 0; i < _inkObjects.Length; i++)
            {
                Destroy(_inkObjects[i]);
            }
        }
    }
}
