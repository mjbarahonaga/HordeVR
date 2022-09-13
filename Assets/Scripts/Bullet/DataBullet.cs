using UnityEngine;

[CreateAssetMenu(fileName = "Bullet.asset", menuName = "HordeVR/Create Scriptable Bullet")]
public class DataBullet : ScriptableObject
{
    public Bullet BulletType;
    public int Damage;
    public float Speed;
    public BulletBehaviour Prefab;

    public int MaxPool = 100;
}
