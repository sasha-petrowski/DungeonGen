using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntity : Entity
{
    [SerializeField]
    private ParticleSystem _jumpParticules;

    [Header("Jump")]
    [SerializeField]
    private float _jumpTime = 1;
    private float _timeAtJump;



    // Update is called once per frame
    void Update()
    {
        Move(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));

        #region Jump
        if (_grounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                gameObject.layer = (int)UnityLayer.AirEntity;
                _grounded = false;
                _rb.drag = _airDrag;
                _timeAtJump = Time.time;

                _jumpParticules.Play();
            }
        }
        else
        {
            float stage = (Time.time - _timeAtJump) / _jumpTime;
            if (stage >= 1)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero, 0, (int)UnityLayerMask.Void);
                if(hit)
                {
                    SpawnAt(_spawnPoint);
                }

                gameObject.layer = (int)UnityLayer.Entity;

                _jumpParticules.Play();
                _grounded = true;
                _spriteRenderer.transform.position = _srOffset + transform.position;
            }
            else
            {
                _spriteRenderer.transform.position = new Vector3(0, Mathf.Sin(stage * Mathf.PI), 0) + transform.position + _srOffset;
            }
        }
        #endregion
    }
}
