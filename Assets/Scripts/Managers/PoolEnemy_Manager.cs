using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public enum Enemy
{
    None,
    Ghoul,
    GhoulBoss
}



public class PoolEnemy_Manager : Singleton<PoolEnemy_Manager>
{

    public List<PoolEnemy> EnemiesPool;

    protected override void Awake()
    {
        base.Awake();

        InitPools();       
    }

    public void InitPools()
    {
        int length = EnemiesPool.Count;
        for (int i = 0; i < length; ++i)
        {
            EnemiesPool[i].Pool = new ObjectPool<EnemyBehaviour>(EnemiesPool[i].CreateFunc,
                EnemiesPool[i].OnTakeFromPool,
                EnemiesPool[i].OnReturnToPool,
                EnemiesPool[i].OnDestroyEnemy,
                false, EnemiesPool[i].EnemyScriptable.MaxPool, EnemiesPool[i].EnemyScriptable.MaxPool);

            // Instantiate all of them
            List<EnemyBehaviour> tmp = new List<EnemyBehaviour>();
            int enemies = EnemiesPool[i].EnemyScriptable.MaxPool;
            for (int j = 0; j < enemies; ++j)
            {
                tmp.Add(EnemiesPool[i].Pool.Get());
                
            }
            for (int j = 0; j < enemies; ++j)
            {
                EnemiesPool[i].Pool.Release(tmp[j]);
            }
        }
    }

    public void SpawnEnemy(Enemy enemyType, Vector3 posTarget, Transform whereToSpawn)
    {
        int length = EnemiesPool.Count;
        for (int i = 0; i < length; ++i)
        {
            if (EnemiesPool[i].EnemyType == enemyType)
            {
                var enemy = EnemiesPool[i].Pool.Get();
                enemy.Init(posTarget, whereToSpawn);
                break;
            }
        }
    }

}

