using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FadeBehaviour : MonoBehaviour
{
    public enum FadeAction
    {
        Out,
        In,
        Flash
    }

    private SpriteRenderer _sr;

    private FadeAction _action = FadeAction.Out;
    private float _timeSpent;
    private float _timer;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    public void Play(FadeAction action = FadeAction.Flash, float timer = 0.5f)
    {
        _timer = timer;
        _action = action;
        enabled = true;

        if (_action == FadeAction.Out)
        {
            _timeSpent = (1 - _sr.color.a) * _timer;
        }
        else if (_action == FadeAction.In | _action == FadeAction.Flash)
        {
            _timeSpent = _sr.color.a * _timer;
        }
    }

    // Update is called once per frame
    void Update()
    {
        _timeSpent += Time.deltaTime;

        if(_timeSpent > _timer)
        {
            if (_action == FadeAction.Out)
            {
                _sr.color = new Color(1, 1, 1, 0);

                enabled = false;
            }
            else if(_action == FadeAction.In | _action == FadeAction.Flash)
            {
                _sr.color = new Color(1, 1, 1, 1);

                enabled = false;

                if (_action == FadeAction.Flash) Play(FadeAction.Out, _timer * 2f);
            }
        }
        else
        {
            if (_action == FadeAction.Out)
            {
                _sr.color = new Color(1, 1, 1, 1 - _timeSpent / _timer);
            }
            else if (_action == FadeAction.In | _action == FadeAction.Flash)
            {
                _sr.color = new Color(1, 1, 1, _timeSpent / _timer);
            }
        }
    }
}
