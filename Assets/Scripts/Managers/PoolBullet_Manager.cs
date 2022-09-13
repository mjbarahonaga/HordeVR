using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public enum Bullet
{
    None,
    Default
}

public class PoolBullet_Manager : Singleton<PoolBullet_Manager>
{
    public List<PoolBullet> BulletsPool;

    protected override void Awake()
    {
        base.Awake();

        InitPools();
    }

    public void InitPools()
    {
        int length = BulletsPool.Count;
        for (int i = 0; i < length; ++i)
        {
            BulletsPool[i].Pool = new ObjectPool<BulletBehaviour>(
                BulletsPool[i].CreateFunc,
                BulletsPool[i].OnTakeFromPool,
                BulletsPool[i].OnReturnToPool,
                BulletsPool[i].OnDestroyBullet,
                false, BulletsPool[i].BulletScriptable.MaxPool,
                BulletsPool[i].BulletScriptable.MaxPool);

            List<BulletBehaviour> tmp = new List<BulletBehaviour>();
            int howMany = BulletsPool[i].BulletScriptable.MaxPool;
            for (int j = 0; j < howMany; ++j)
            {
                tmp.Add(BulletsPool[i].Pool.Get());

            }
            for (int j = 0; j < howMany; ++j)
            {
                BulletsPool[i].Pool.Release(tmp[j]);
            }
        }
    }

    public void SpawnBullet(Bullet bulletType, Vector3 startPosition, Vector3 forward)
    {
        int length = BulletsPool.Count;
        for (int i = 0; i < length; ++i)
        {
            if (BulletsPool[i].BulletType == bulletType)
            {
                var bullet = BulletsPool[i].Pool.Get();
                bullet.Init(startPosition, forward);
                break;
            }
        }
    }
}
