using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolEnemy : MonoBehaviour
{
    public DataEnemy EnemyScriptable;
    public Transform ParentContainer;
    public IObjectPool<EnemyBehaviour> Pool;
    public Enemy EnemyType { get => EnemyScriptable.EnemyType; }

    public EnemyBehaviour CreateFunc()
    {
        var go = Instantiate(EnemyScriptable.Prefab, ParentContainer);
        go.Data = EnemyScriptable;
        go.PoolReference = this;
        go.InstanceEnemy(Pool, go.gameObject);
        return go;
    }

    public void OnTakeFromPool(EnemyBehaviour enemy) => enemy.TakeFromPool();

    public void OnReturnToPool(EnemyBehaviour enemy)
    {
        enemy.ReturnToPool();
    }

    public void OnDestroyEnemy(EnemyBehaviour enemy) => Destroy(enemy);
}
