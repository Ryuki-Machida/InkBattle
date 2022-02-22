using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状態
/// </summary>
public enum Status
{
    Human,
    Squid
}

public class PlayerController : MonoBehaviour
{
    /// <summary>移動速度</summary>
    [SerializeField] float _movingSpeed = default;
    /// <summary>ターンの速さ</summary>
    [SerializeField] float _turnSpeed = default;
    /// <summary>ジャンプ力</summary>
    [SerializeField] float _jumpPower = default;
    /// <summary>アタッチされてるコライダー</summary>
    [SerializeField] Collider[] _colliders;
    /// <summary>地面にいるか</summary>
    bool _isGround;

    Rigidbody _rb;
    Animator _anim;

    Status _status;

    void Start()
    {
        _colliders[0] = GetComponent<CapsuleCollider>();
        _colliders[1] = GetComponent<BoxCollider>();
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
    }

    void Update()
    {
        PlayerMove();
        CurrentState();
    }

    /// <summary>
    /// 動き
    /// </summary>
    void PlayerMove()
    {
        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        Vector3 dir = Vector3.forward * v + Vector3.right * h;

        if (dir == Vector3.zero)
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
        else
        {
            // カメラを基準に入力が上下=奥/手前, 左右=左右にキャラクターを向ける
            dir = Camera.main.transform.TransformDirection(dir);    // メインカメラを基準に入力方向のベクトルを変換する
            dir.y = 0;  // y 軸方向はゼロにして水平方向のベクトルにする

            // 入力方向に滑らかに回転させる
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * _turnSpeed);  // Slerp を使うのがポイント

            Vector3 velo = dir.normalized * _movingSpeed; // 入力した方向に移動する
            velo.y = _rb.velocity.y;   // ジャンプした時の y 軸方向の速度を保持する
            _rb.velocity = velo;   // 計算した速度ベクトルをセットする
        }

        if (Input.GetButtonDown("Fire2"))
        {
            _status = Status.Squid;
            _anim.SetBool("Squid", true);
        }

        if (Input.GetButtonUp("Fire2"))
        {
            _status = Status.Human;
            _anim.SetBool("Squid", false);
        }

        if (Input.GetButtonDown("Jump") && _isGround)
        {
            _isGround = false;
            _rb.AddForce(Vector3.up * _jumpPower, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// 今の状態に応じていろいろ変更する
    /// </summary>
    void CurrentState()
    {
        if (_status == Status.Human)
        {
            _colliders[0].enabled = true;
            _colliders[1].enabled = false;
        }

        if (_status == Status.Squid)
        {
            _colliders[0].enabled = false;
            _colliders[1].enabled = true;
        }
    }

    /// <summary>
    /// Update の後に呼び出される。Update の結果を元に何かをしたい時に使う。
    /// </summary>
    void LateUpdate()
    {
        // 水平方向の速度を求めて Animator Controller のパラメーターに渡す
        Vector3 horizontalVelocity = _rb.velocity;
        horizontalVelocity.y = 0;
        _anim.SetFloat("Speed", horizontalVelocity.magnitude);
    }

    private void OnTriggerEnter(Collider other)
    {
        _isGround = true;
    }
}
