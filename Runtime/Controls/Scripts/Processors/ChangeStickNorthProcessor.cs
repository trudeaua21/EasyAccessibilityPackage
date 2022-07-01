using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class ChangeStickNorthProcessor : InputProcessor<Vector2>
{

#if UNITY_EDITOR
    static ChangeStickNorthProcessor()
    {
        Initialize();
    }

#endif

    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<ChangeStickNorthProcessor>();
    }

    public int num45ToRight;

    public override Vector2 Process(Vector2 value, InputControl control)
    {
        float rotationDegrees = 45f * num45ToRight;

        return rotateVector(value, rotationDegrees); 
    }

    /// <summary>
    /// Rotates a given 2D vector by the given angle. Degrees will be clamped to be within 0 to 360.
    /// </summary>
    /// <param name="input">The vector to be rotated.</param>
    /// <param name="degrees">The number of degrees to rotate the vector.</param>
    /// <returns>The resulting vector after the given rotation is applied.</returns>
    private Vector2 rotateVector(Vector2 input, float degrees)
    {
        // clamp degrees to desired range
        degrees = Mathf.Clamp(degrees, 0f, 360f);

        // change the degree to radians
        float rads = degrees * Mathf.Deg2Rad;

        float x = input.x;
        float y = input.y;

        // in 2d vector rotation, rotation by d degrees is:
        // x' = x cos d - y sin d
        // y' = x sin d - y cos d
        float newX = (x * Mathf.Cos(rads)) - (y * Mathf.Sin(rads));
        float newY = (x * Mathf.Sin(rads)) + (y * Mathf.Cos(rads));

        Vector2 result = new Vector2(newX, newY);
        return result;
    }
}
