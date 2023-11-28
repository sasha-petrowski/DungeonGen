using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MonsterEntity : Entity
{
    private const float IDLETIME = 0.5f;

    [Header("Monster Refs")]
    [SerializeField]
    protected Detector _agroDetector;
    [SerializeField]
    protected FadeBehaviour _agroFade;

    [Header("Wander")]
    [SerializeField]
    protected float _wanderLength = 3;

    protected enum MonsterState
    {
        Idle = 0,
        Wander = 1,
        Agro = 2,
    }

    private MonsterState _state = MonsterState.Idle;
    protected MonsterState State 
    {
        get { return _state; }
        set
        {
            Standing();
            _timeAtStateChange = Time.time;
            _state = value;


            if(_state == MonsterState.Agro)
            {
                _agroFade.Play();
            }
        }
    }
    protected float _timeAtStateChange;

    protected Vector2 _wanderVector;
    protected Entity _agroTarget;

    protected override void Awake()
    {
        base.Awake();

        _agroDetector.OnDetect += AgroDetect;
        _agroDetector.OnRemove += AgroRemove;
    }

    private void AgroDetect(Entity entity)
    {
        TrySetTarget(entity);
    }
    private void AgroRemove(Entity entity)
    {
        if(entity != null & _agroTarget == entity)
        {
            _agroTarget = null;

            State = MonsterState.Idle;

            foreach (Entity other in _agroDetector.Detected)
            {
                if (TrySetTarget(other)) return;
            }
        }
    }
    private bool TrySetTarget(Entity entity)
    {
        if (entity != null & _agroTarget == null)
        {
            Vector2 relative = entity.transform.position - transform.position;
            float distance = relative.magnitude;

            //raycast for any walls in the way
            if (Physics2D.Raycast(transform.position, relative / distance, distance, (int)UnityLayerMask.Wall)) return false;
            
            _agroTarget = entity;
            State = MonsterState.Agro;

            return true;
        }
        return false;
    }

    private void Update()
    {
        switch (_state)
        {
            case MonsterState.Idle:

                Standing();

                if(_timeAtStateChange + IDLETIME <= Time.time)
                {
                    State = MonsterState.Wander;

                    _wanderVector = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f)).normalized * _speed * _wanderLength;
                }
                else
                {
                    foreach (Entity other in _agroDetector.Detected)
                    {
                        if (TrySetTarget(other)) return;
                    }
                }

                break;
            case MonsterState.Wander:

                Vector2 step = Move(_wanderVector);

                if(_wanderVector.sqrMagnitude <= step.sqrMagnitude)
                {
                    State = MonsterState.Idle;
                }
                else
                {
                    _wanderVector -= step;

                    foreach (Entity other in _agroDetector.Detected)
                    {
                        if (TrySetTarget(other)) return;
                    }
                }

                break;
            case MonsterState.Agro:

                Move(_agroTarget.transform.position - transform.position);

                break;
            default:
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(State == MonsterState.Wander) 
        {
            _wanderVector = -_wanderVector;
        }
    }
}
