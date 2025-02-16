using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AnimaorHandler : MonoBehaviour
{
    private Animator _animator;
    private float _speed;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetSpeed(NavMeshAgent agent)
    {
        float newSpeed;

        if (agent.velocity == Vector3.zero)
             newSpeed = 0;
        else newSpeed = 1;

        if (newSpeed != _speed) 
        {
            _speed = newSpeed;
            _animator.SetFloat("Speed", _speed);
        }
    }

    public void SetState(EEnemyState state)
    {
        switch (state)
        {
            case EEnemyState.Alerted:
            case EEnemyState.WaitAlert:
                _animator.SetInteger("State", 2);
                break;
            case EEnemyState.Patrolling:
                _animator.SetInteger("State", 0);
                break;
            case EEnemyState.Chasing:
            case EEnemyState.WaitChasing:
                _animator.SetInteger("State", 1);
                break;
            default:
                _animator.SetInteger("State", 0);
                break;
        }
    }

    public void Attack() 
    {
        _animator.SetTrigger("Attack");
    }
}
