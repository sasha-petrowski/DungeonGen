using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterJumpOnAgro: MonsterAbility
{
    public float Speed;

    public override void OnState(MonsterState state) 
    {
        if(state == MonsterState.Agro && _entity.Grounded)
        {
            _entity.RigidBody.velocity = (_entity.AgroTarget.transform.position - transform.position).normalized * Speed;
            _entity.Jump();
        }
    }
}
