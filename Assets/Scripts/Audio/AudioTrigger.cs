using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    private List<GameObject> _enemyAIs = new List<GameObject>();

    private SphereCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy" && NewEnemy(other.gameObject))
            other.GetComponent<EnemyAI>().StartAlerted(transform.position);
    }

    private bool NewEnemy(GameObject newEnemy) 
    {
        foreach (GameObject enemy in _enemyAIs)
            if (enemy == newEnemy) return false;
        _enemyAIs.Add(newEnemy);
        return true;
    }

    private void OnDisable()
    {
        _enemyAIs.Clear();
    }

    public void SetColliderRadius(float radius) 
    {
        _collider.radius = radius;
    }
}
