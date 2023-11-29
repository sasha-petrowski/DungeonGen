using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterJumpOnAgro: MonsterAbility
{
    public float Speed;

    public override void OnState(MonsterState state) 
    {
        if(state == MonsterState.Agro)
        {
            Debug.Log("Ability Jump on agro");

            _entity.RigidBody.velocity = (_entity.AgroTarget.transform.position - transform.position).normalized * Speed;
            Debug.Log(_entity.RigidBody.velocity);
            _entity.Jump();
        }
    }
}
