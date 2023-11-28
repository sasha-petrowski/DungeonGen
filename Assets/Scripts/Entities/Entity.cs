using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public abstract class Entity : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    protected float _speed = 2;
    [SerializeField]
    protected float _drag = 10;
    [SerializeField]
    protected float _airDrag = 1;

    [Header("Refs")]
    [SerializeField]
    protected SpriteRenderer _spriteRenderer;
    [SerializeField]
    protected ParticleSystem _walkParticules;




    protected bool _grounded = true;
    protected Vector3 _srOffset;
    protected Rigidbody2D _rb;
    protected Vector3 _spawnPoint;

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _srOffset = _spriteRenderer.transform.localPosition;
        _spawnPoint = transform.position;
    }

    public void SpawnAt(Vector3 position)
    {
        _spawnPoint = position;

        Spawn();
    }
    public void Spawn()
    {
        transform.position = _spawnPoint;
    }
    protected Vector2 Move(Vector2 input)
    {
        float moveMag = input.magnitude;


        if (moveMag > 0)
        {
            if (input.x > 0)
            {
                _spriteRenderer.flipX = false;
            }
            else if (input.x < 0)
            {
                _spriteRenderer.flipX = true;
            }

            if (_walkParticules)
            {
                var shape = _walkParticules.shape;
                var emission = _walkParticules.emission;

                shape.rotation = new Vector3(0, 0, Mathf.Atan2(_rb.velocity.y, _rb.velocity.x) * Mathf.Rad2Deg + 90);
                
                emission.enabled = _grounded;
            }

            if (_grounded)
            {
                _rb.drag = 0;

                _rb.velocity = (Vector3)(input / moveMag * _speed);
            }
            else
            {
                _rb.drag = _airDrag;

                _rb.AddForce((input / moveMag * _speed * Time.deltaTime), ForceMode2D.Impulse);
            }
            return input / moveMag * _speed * Time.deltaTime;
        }
        else
        {
            Standing();
        }
        return Vector2.zero;
    }
    protected void Standing()
    {
        if (_walkParticules)
        {
            var emission = _walkParticules.emission;

            emission.enabled = false;
        }

        _rb.drag = _grounded ? _drag : _airDrag;
    }
}
