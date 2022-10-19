using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGoal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameManager.Instance.EnemyReachedGoal();
        EnemyBehaviour enemyBehaviour = other.GetComponent<EnemyBehaviour>();
        enemyBehaviour.PoolReference.OnReturnToPool(enemyBehaviour);
    }
}
