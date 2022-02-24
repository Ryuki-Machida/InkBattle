using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    /// <summary>向く方向</summary>
    [SerializeField] GameObject _mainCamera;
    /// <summary>移動速度</summary>
    [SerializeField] float _movingSpeed = default;
    /// <summary>ターンの速さ</summary>
    [SerializeField] float _turnSpeed = default;
    /// <summary>ジャンプ力</summary>
    [SerializeField] float _jumpPower = default;
    /// <summary>アタッチされてるコライダー</summary>
    [SerializeField] Collider[] _colliders;
    /// <summary>砲弾として生成するプレハブ</summary>
    [SerializeField] GameObject _shellPrefab = default;
    /// <summary>発射間隔</summary>
    [SerializeField] float _interval = default;
    /// <summary>砲口を指定する</summary>
    [SerializeField] Transform _muzzle = default;
    /// <summary>持っているシューター</summary>
    [SerializeField] GameObject _shooter = default;
    /// <summary>攻撃し続ける時間</summary>
    [SerializeField] float _attackTime = default;
    /// <summary>インク量</summary>
    [SerializeField] GameObject _inkSlider = default;
    /// <summary>インクが無いことを知らせる</summary>
    [SerializeField] GameObject _noticePanel = default;
    /// <summary>Rayを飛ばす距離</summary>
    [SerializeField] float _maxRayDistance = 1f;

    /// <summary>地面にいるか</summary>
    bool _isGround;
    /// <summary>発射時間</summary>
    float _time;
    /// <summary>インクの回復スピード</summary>
    float _inkHealSpeed = 0.5f;

    Rigidbody _rb;
    Animator _anim;
    SkinnedMeshRenderer[] _skinnedMesh;
    Slider _slider;
    RaycastHit _hit;

    Status _status;

    void Start()
    {
        _colliders[0] = GetComponent<CapsuleCollider>();
        _colliders[1] = GetComponent<BoxCollider>();
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        _skinnedMesh = GetComponentsInChildren<SkinnedMeshRenderer>();
        _slider = _inkSlider.GetComponent<Slider>();
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

        PlayerAction();
    }
    
    /// <summary>
    /// 動作を増やす時はここに書く
    /// </summary>
    void PlayerAction()
    {
        _slider.value = _attackTime;

        _time += Time.deltaTime;

        if (Input.GetButton("Fire1") && _status == Status.Human)
        {
            _attackTime -= Time.deltaTime;

            this.transform.rotation = Quaternion.Euler(0, _mainCamera.transform.rotation.eulerAngles.y, 0);
            _anim.SetBool("Attack", true);

            if (_attackTime > 0)
            {
                if (_time > _interval)
                {
                    Instantiate(_shellPrefab, _muzzle.position, _mainCamera.transform.rotation);
                    _time = 0;
                }
            }
            else
            {
                _attackTime = 0;　//0より下げないように
                _noticePanel.SetActive(true);
            }
        }
        else
        {
            if (_attackTime <= 11)　//11sより上げないように
            {
                _attackTime += Time.deltaTime * _inkHealSpeed;　//インク回復
                _noticePanel.SetActive(false);
            }
        }

        if (Input.GetButtonUp("Fire1"))
        {
            _anim.SetBool("Attack", false);
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
            _shooter.SetActive(true);
        }

        if (_status == Status.Squid)
        {
            _colliders[0].enabled = false;
            _colliders[1].enabled = true;
            _shooter.SetActive(false);
            Ray();
        }
    }

    /// <summary>
    /// 壁を登れるか判断する
    /// </summary>
    void Ray()
    {
        Physics.Raycast(this.transform.position, this.transform.forward, out _hit, _maxRayDistance);
        Debug.DrawRay(this.transform.position, this.transform.forward * _hit.distance, Color.red);

        //改良絶対必要
        float hitx = _hit.transform.rotation.eulerAngles.x;
        float hitz = _hit.transform.rotation.eulerAngles.z;
        float x = _hit.transform.rotation.eulerAngles.x;
        float z = _hit.transform.rotation.eulerAngles.z;

        if (_hit.collider.CompareTag("Ink"))　//壁移動
        {
            float v = Input.GetAxisRaw("Vertical");
            float h = Input.GetAxisRaw("Horizontal");

            if (hitx > 0 || x > 0 || hitz > 0 || z > 0)
            {
                this.transform.rotation = Quaternion.FromToRotation(Vector3.up, _hit.normal);
                this.transform.position += this.transform.localScale.y / 1.98f * _hit.normal;
                Vector3 dir = new Vector3(h, v, 0).normalized;
                _rb.velocity = dir * _movingSpeed;
            }
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

        if (other.gameObject.CompareTag("Ink"))
        {
            //インクの中

            if (_status == Status.Human)
            {
                for (int i = 0; i < _skinnedMesh.Length; i++)
                {
                    _skinnedMesh[i].enabled = true;
                }
            }

            if (_status == Status.Squid)
            {
                for (int i = 0; i < _skinnedMesh.Length; i++)
                {
                    _skinnedMesh[i].enabled = false;
                }
                _inkHealSpeed = 2.5f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ink"))
        {
            for (int i = 0; i < _skinnedMesh.Length; i++)
            {
                _skinnedMesh[i].enabled = true;
            }

            _inkHealSpeed = 0.5f;
        }
    }
}
