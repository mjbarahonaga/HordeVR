using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGoal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        
        if(other.TryGetComponent(out EnemyBehaviour enemy))
        {
            GameManager.Instance.EnemyReachedGoal();
            enemy.PoolReference.OnReturnToPool(enemy);
        }
    }
}
