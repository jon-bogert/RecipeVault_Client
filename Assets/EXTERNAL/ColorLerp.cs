using XephTools;
using UnityEngine;
using System;

public class ColorLerp : OverTime.ModuleBase
{
    private readonly Color start;
    private readonly Color end;
    private readonly Action<Color> setter;

    public ColorLerp(Color start, Color end, float length, Action<Color> setter)
    {
        this.start = start;
        this.end = end;
        this.setter = setter;
        Init(length);
    }

    internal override void Update()
    {
        setter(Color.Lerp(start, end, Progress));
    }
}

public class Vector3Lerp : OverTime.ModuleBase
{
    private readonly Vector3 start;
    private readonly Vector3 end;
    private readonly Action<Vector3> setter;

    public Vector3Lerp(Vector3 start, Vector3 end, float length, Action<Vector3> setter)
    {
        this.start = start;
        this.end = end;
        this.setter = setter;
        Init(length);
    }

    internal override void Update()
    {
        setter(Vector3.Lerp(start, end, Progress));
    }
}
