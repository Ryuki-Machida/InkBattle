using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弾を制御するコンポーネント
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BulletController : MonoBehaviour
{
    /// <summary>飛ばす距離</summary>
    [SerializeField] float _distanceToFly = default;
    /// <summary>表示する色</summary>
    [SerializeField] Material _materialColor = default;
    /// <summary>付けるインク</summary>
    [SerializeField] GameObject _inkPrefab = default;
    Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        GetComponent<MeshRenderer>().material.color = _materialColor.color;　//色指定

        GameObject camera = GameObject.Find("Main Camera");
        float x = camera.transform.rotation.eulerAngles.x;

        if (360 >= x && x >= 270)　//上方向の角度270～360
        {
            _rb.velocity = this.transform.forward * _distanceToFly + transform.up * x / 70;
        }
        else if (90 >= x && x >= 0)　//下方向の角度0～90
        {
            float angle = 360 + x;　//360から0になる為、足して361度からにしている

            if (angle <= 375)
            {
                _rb.velocity = this.transform.forward * _distanceToFly + transform.up * angle / 70;
            }
            else
            {
                _rb.velocity = this.transform.forward * _distanceToFly + transform.up * angle / 95;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Finish"))
        {
            //法線で角度を変える
            GameObject ink = Instantiate(_inkPrefab, this.transform.position,
                Quaternion.FromToRotation(Vector3.up, collision.contacts[0].normal));
        }

        if (!collision.gameObject.CompareTag("Bullet"))　//弾同士で消える為
            Destroy(this.gameObject);
    }
}
