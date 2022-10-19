using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using Sirenix.OdinInspector;

[RequireComponent(typeof(BoxCollider))]
public class TrapBehaviour : MonoBehaviour
{
    #region Public variables
    public DataTrap Data;
    #endregion

    #region Private variables
    private int _damage;
    private float _coolDown;
    private float _price;

    private BoxCollider _boxCollider;
    private Transform _myTransform;

    private bool _isReadyToAttack = true;

    private CoroutineHandle _updateCoroutine;

    [SerializeField, ReadOnly]
    private Animator _animator;
    private int _idAttack;

    // list of enemies inside of trap
    private List<EnemyBehaviour> _enemiesInside = new List<EnemyBehaviour>();
    #endregion

    #region GETTERS AND SETTERS
    [ShowInInspector, ReadOnly]
    public int Damage { get => _damage; private set => _damage = value; }
    [ShowInInspector, ReadOnly]
    public float CoolDown { get => _coolDown; private set => _coolDown = value; }
    [ShowInInspector, ReadOnly]
    public float Price { get => _price; private set => _price = value; }

    public Vector3Int CenterPosition { get; private set; }
    #endregion

    public void SetUp(DataTrap data)
    {
        _damage = data.Damage;
        _coolDown = data.CoolDown;
        _price = data.Price;
    }
    
    private IEnumerator<float> MyUpdateCoroutine()
    {
        yield return Timing.WaitForSeconds(_coolDown);
        if(_enemiesInside.Count == 0)
        {
            _isReadyToAttack = true;
        }
        else
        {
            Attack(_enemiesInside);
        }
        yield return 0f;
    }

    public void Attack(List<EnemyBehaviour> enemies)
    {
        // Call animation
        _animator.SetTrigger(_idAttack);

        _isReadyToAttack = false;
        int length = _enemiesInside.Count;
        for (int i = 0; i < length; ++i)
        {
            enemies[i].TakeDamage(_damage);
            if (enemies[i].IsDie)
            {
                enemies.Remove(enemies[i]);
                --length;
                --i;
            }
        }

        _updateCoroutine = Timing.RunCoroutine(MyUpdateCoroutine());
    }

    #region UTILS
    public Vector3[] GetColliderVertexPositionsLocal()
    {
        var vertices = new Vector3[4];
        vertices[0] = _boxCollider.center + new Vector3(-_boxCollider.size.x, -_boxCollider.size.y, -_boxCollider.size.z) * 0.5f;
        vertices[1] = _boxCollider.center + new Vector3(_boxCollider.size.x, -_boxCollider.size.y, -_boxCollider.size.z) * 0.5f;
        vertices[2] = _boxCollider.center + new Vector3(_boxCollider.size.x, -_boxCollider.size.y, _boxCollider.size.z) * 0.5f;
        vertices[3] = _boxCollider.center + new Vector3(-_boxCollider.size.x, -_boxCollider.size.y, _boxCollider.size.z) * 0.5f;

        return vertices;
    }

    public void CalculateSizeCells()
    {
        var vertices = GetColliderVertexPositionsLocal();
        Vector3Int[] vector3Ints = new Vector3Int[vertices.Length];

        int length = vector3Ints.Length;
        for (int i = 0; i < length; ++i)
        {
            Vector3 worldPos = _myTransform.TransformPoint(vertices[i]);
            vector3Ints[i] = BuildingSystem.Instance.GetGridLayout.WorldToCell(worldPos);
        }

        CenterPosition = new Vector3Int(Mathf.Abs((vector3Ints[0] - vector3Ints[1]).x),
            Mathf.Abs((vector3Ints[0] - vector3Ints[3]).y),
            1);
    }

    #endregion

    #region UNITY METHODS

    private void Start()
    {
        _idAttack = Animator.StringToHash("Attack");
        SetUp(Data);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out EnemyBehaviour enemy))
        {
            if (!_enemiesInside.Contains(enemy))
            {
                _enemiesInside.Add(enemy);
            }
        }

        if (_isReadyToAttack)
        {
            Attack(_enemiesInside);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out EnemyBehaviour enemy))
        {
            if (_enemiesInside.Contains(enemy))
            {
                _enemiesInside.Remove(enemy);
            }
        }
    }

    private void OnValidate()
    {
        Utils.ValidationUtility.SafeOnValidate(() =>
        {
            if (this == null) return;
            if (Data != null) SetUp(Data);
            if(_animator == null) _animator = GetComponent<Animator>();
            if(_boxCollider == null) _boxCollider = GetComponent<BoxCollider>();
            if(_myTransform == null) _myTransform = GetComponent<Transform>();
        });
    }
    #endregion
}
