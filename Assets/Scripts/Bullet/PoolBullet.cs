using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolBullet : MonoBehaviour
{
    public DataBullet BulletScriptable;
    public Transform ParentContainer;
    public IObjectPool<BulletBehaviour> Pool;

    public Bullet BulletType { get => BulletScriptable.BulletType; }

    public BulletBehaviour CreateFunc()
    {
        var bullet = Instantiate(BulletScriptable.Prefab, ParentContainer);
        bullet.Data = BulletScriptable;
        bullet.PoolReference = this;
        bullet.IntanceBullet();
        return bullet;
    }

    public void OnTakeFromPool(BulletBehaviour bullet) => bullet.TakeFromPool();

    public void OnReturnToPool(BulletBehaviour bullet) => bullet.ReturnToPool();

    public void OnDestroyBullet(BulletBehaviour bullet) => Destroy(bullet);
}
