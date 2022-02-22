using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ポリゴンを制御するコンポーネント
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Polygon : MonoBehaviour
{
    /// <summary>作成する度数</summary>
    [SerializeField] int _areaAngle = default;
    /// <summary>何角形か,100で円</summary>
    [SerializeField] int _square = 100;
    /// <summary>表示する色</summary>
    [SerializeField] public Material _materialColor = default;
    /// <summary>大きさ</summary>
    [SerializeField] Vector3 _scale = default;

    Vector3[] _vertices; //頂点
    int[] _triangles;    //index

    void Start()
    {
        MakeParams();
        SetParams();
    }

    /// <summary>
    /// 作成
    /// </summary>
    void MakeParams()
    {
        List<Vector3> vertList = new List<Vector3>();
        List<int> triList = new List<int>();

        vertList.Add(new Vector3(0, 0, 0));  //原点

        float th, x, z;
        int max = _square * _areaAngle / 360;
        for (int i = 0; i <= max; i++)
        {
            th = i * _areaAngle / max + 0; // 0は始まる度数
            x = Mathf.Sin(th * Mathf.Deg2Rad);
            z = Mathf.Cos(th * Mathf.Deg2Rad);
            vertList.Add(new Vector3(x, 0, z));
            if (i <= max - 1)
            {
                triList.Add(0); triList.Add(i + 1); triList.Add(i + 2);
            }
        }
        _vertices = vertList.ToArray();
        _triangles = triList.ToArray();
    }

    void SetParams()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = _vertices;
        mesh.triangles = _triangles;

        // 法線とバウンディングの計算
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mesh.name = "arcMesh";
        transform.localScale = _scale;

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        // 色指定
        GetComponent<MeshRenderer>().material.color = _materialColor.color;
    }
}
