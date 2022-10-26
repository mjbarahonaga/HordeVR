using UnityEngine;


[CreateAssetMenu(fileName = "Enemy.asset", menuName = "HordeVR/Create Scriptable Enemy")]
public class DataEnemy : ScriptableObject
{
    public Enemy EnemyType;
    public int Damage;
    public int HP;
    public int IncreasePerLevel;
    public float Speed;
    public int Reward;
    public EnemyBehaviour Prefab;

    public int MaxPool = 20;
}
