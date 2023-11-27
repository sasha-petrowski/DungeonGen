using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _drag = 10;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float moveMag = move.magnitude;
        if(moveMag > 0)
        {
            _rb.drag = 0;
            _rb.velocity = (Vector3)(move / moveMag * _speed);

            if(move.x > 0)
            {
                _spriteRenderer.flipX = false;
            }
            else if (move.x < 0)
            {
                _spriteRenderer.flipX = true;
            }
        }
        else
        {
            _rb.drag = _drag;
        }
    }
}
