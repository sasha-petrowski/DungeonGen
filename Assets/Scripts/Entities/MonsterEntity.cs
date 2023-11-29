using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MonsterEntity : Entity
{
    private const float IDLETIME = 1f;

    [Header("Monster Refs")]
    [SerializeField]
    protected Detector _agroDetector;
    [SerializeField]
    protected FadeBehaviour _agroFade;
    [SerializeField]
    protected FadeBehaviour _questionFade;

    [Header("Wander")]
    [SerializeField]
    protected WanderBehaviour _wanderBehaviour = WanderBehaviour.Directional;
    [SerializeField]
    protected float _wanderLength = 3;

    protected enum WanderBehaviour
    {
        Directional = 0,
        Straight = 1
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
                _questionFade.Finish();
                _agroFade.Play();

            }

            foreach (MonsterAbility ability in GetComponents<MonsterAbility>())
            {
                ability.OnState(_state);
            }
        }
    }
    protected float _timeAtStateChange;

    protected Vector2 _wanderVector;


    public Entity AgroTarget { get; private set; }

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
        if(entity != null & AgroTarget == entity)
        {
            AgroTarget = null;

            State = MonsterState.Idle;

            foreach (Entity other in _agroDetector.Detected)
            {
                if (TrySetTarget(other)) return;
            }
        }
    }
    public bool TrySetTarget(Entity entity)
    {
        if (entity != null & AgroTarget == null)
        {
            if (_spriteRenderer.flipX == (entity.transform.position.x - transform.position.x) > 0) return false;

            //raycast for any walls in the way
            if (!LineOfSight(entity.transform.position)) return false;

            AgroTarget = entity;
            State = MonsterState.Agro;

            return true;
        }
        return false;
    }
    private bool LineOfSight(Vector3 position)
    {
        Vector2 relative = position - transform.position;

        float distance = relative.magnitude;

        return !Physics2D.Raycast(transform.position, relative / distance, distance, (int)UnityLayerMask.Wall | (int)UnityLayerMask.Terrain);
    }

    protected override void Update()
    {
        base.Update();

        switch (_state)
        {
            case MonsterState.Idle:

                Standing();

                if(_timeAtStateChange + IDLETIME <= Time.time)
                {
                    State = MonsterState.Wander;

                    SetWanderVector();

                    Move(_wanderVector);
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

                Move(AgroTarget.transform.position - transform.position);

                if(! LineOfSight(AgroTarget.transform.position))
                {
                    Entity oldAgro = AgroTarget;
                    AgroTarget = null;
                    foreach (Entity other in _agroDetector.Detected)
                    {
                        if (oldAgro != other && TrySetTarget(other)) return;
                    }
                    _agroFade.Finish();
                    _questionFade.Play();

                    State = MonsterState.Wander;
                    SetWanderVector(oldAgro.transform.position);
                }

                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(State == MonsterState.Wander)
        {
            switch (_wanderBehaviour)
            {
                case WanderBehaviour.Directional:
                    _wanderVector = -_wanderVector;
                    break;

                case WanderBehaviour.Straight:
                    
                    _wanderVector = new Vector2(-_wanderVector.y, _wanderVector.x);

                    break;
                default:
                    break;
            }
        }
    }

    private void SetWanderVector()
    {
        switch (_wanderBehaviour)
        {
            case WanderBehaviour.Directional:
                _wanderVector = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * _speed * _wanderLength;

                break;
            case WanderBehaviour.Straight:
                switch (Random.Range(0, 3))
                {
                    case 0:
                        _wanderVector = Vector2.right;
                        break;
                    case 1:
                        _wanderVector = Vector2.left;
                        break;
                    case 2:
                        _wanderVector = Vector2.up;
                        break;
                    case 3:
                        _wanderVector = Vector2.down;
                        break;
                }

                _wanderVector *= _speed * _wanderLength;

                break;
            default:
                break;
        }

    }
    private void SetWanderVector(Vector3 target)
    {
        switch (_wanderBehaviour)
        {
            case WanderBehaviour.Directional:
                _wanderVector = target - transform.position;

                break;
            case WanderBehaviour.Straight:
                Vector2 relative = target - transform.position;

                if(Mathf.Abs(relative.x) > Mathf.Abs(relative.y))
                {
                    _wanderVector.x = relative.x;
                    _wanderVector.y = 0;
                }
                else
                {
                    _wanderVector.x = 0;
                    _wanderVector.y = relative.y;
                }

                break;
            default:
                break;
        }

    }
}
