using UnityEngine;
using System.Collections;

public enum UQuad
{
    LowerLeft,
    LowerRight,
    UpperLeft,
    UpperRight,
}

public enum UQLeafActions
{
    SwappingIn,
    SwappedIn,

    SwappingOut,
    SwappedOut,
}

public delegate void UQuadtreeCellChanged(UQuadtreeLeaf left, UQuadtreeLeaf entered);
public delegate void UQuadtreeCellSwapInOut(UQuadtreeLeaf leaf, UQLeafActions action);

