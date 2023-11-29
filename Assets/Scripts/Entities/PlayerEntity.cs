using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntity : Entity
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out Entity other))
        {
            Vector2 direction = (transform.position - other.transform.position).normalized;
            if(_grounded)
            {
                _rb.velocity = direction * other.PushForce;
            }
            else
            {
                _rb.velocity += direction * other.PushForce;
            }
            if(other.Grounded)
            {
                other.RigidBody.velocity = -direction * PushForce;
            }
            else
            {
                other.RigidBody.velocity += -direction * PushForce;
            }

            if (other is MonsterEntity monster)
            {
                monster.TrySetTarget(this);
            }

            Jump();
            other.Jump();
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        Move(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        base.Update();
    }

}
