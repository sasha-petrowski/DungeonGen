using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetector : Detector
{
    protected override bool Detect(Entity other)
    {
        return other is PlayerEntity && base.Detect(other);
    }
}
