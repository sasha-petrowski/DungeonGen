using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MonsterEntity))]
public abstract class MonsterAbility : MonoBehaviour
{
    protected MonsterEntity _entity;

    private void Awake()
    {
        _entity = GetComponent<MonsterEntity>();
    }

    public virtual void OnState(MonsterState state) { }
}
