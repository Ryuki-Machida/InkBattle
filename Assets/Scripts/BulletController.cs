using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弾を制御するコンポーネント
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BulletController : MonoBehaviour
{
    /// <summary>表示する色</summary>
    [SerializeField] Material _materialColor = default;
    /// <summary>付けるインク</summary>
    [SerializeField] GameObject _inkPrefab = default;

    Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        GetComponent<MeshRenderer>().material.color = _materialColor.color;  // 色指定
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Finish"))
        {
            GameObject ink = Instantiate(_inkPrefab, this.transform.position,
                Quaternion.FromToRotation(Vector3.up, collision.contacts[0].normal));
        }
        Destroy(this.gameObject);
    }
}
