using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Lod
{
    [Range(0,5000)]
    public float Distance;
    public Mesh Mesh;

    [NonSerialized]
    public List<Matrix4x4[]> Matrices;

    [NonSerialized]
    public int BatchCount;

    [NonSerialized]
    public int NumInLastBatch;
}
