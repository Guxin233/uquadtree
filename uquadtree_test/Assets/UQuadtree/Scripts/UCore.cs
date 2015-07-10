using UnityEngine;
using System.Collections;

public class UCore 
{
    public static void Assert(bool expr)
    {
        System.Diagnostics.Debug.Assert(expr);
    }
}
