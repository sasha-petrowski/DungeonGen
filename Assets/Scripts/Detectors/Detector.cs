using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    public List<Entity> Exclude;
    [HideInInspector]
    public List<Entity> Detected = new List<Entity>();

    public Action<Entity> OnDetect;
    public Action<Entity> OnRemove;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Entity other) && Detect(other))
        {
            Detected.Add(other);
            // callback
            OnDetect.Invoke(other);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Entity other) && Detected.Remove(other))
        {
            // callback
            OnRemove.Invoke(other);
        }
    }
    protected virtual bool Detect(Entity other)
    {
        return ! Exclude.Contains(other);
    }
}
