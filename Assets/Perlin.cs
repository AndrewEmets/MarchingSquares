using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perlin : MonoBehaviour
{
    [SerializeField] private Vector2 pos;
    [SerializeField] private float p;

    void OnValidate()
    {
        p = Mathf.PerlinNoise(pos.x, pos.y);
    }
}
