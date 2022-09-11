using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Enemy.asset", menuName = "HordeVR/Create Scriptable Enemy")]
public class DataEnemy : ScriptableObject
{
    public Enemy EnemyType;
    public int Damage;
    public int HP;
    public float Speed;
    public int Reward;
    public EnemyBehaviour Prefab;

    public int MaxPool = 20;
}
