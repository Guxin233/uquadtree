using UnityEngine;
using System.Collections;

public enum UQuad
{
    LowerLeft,
    LowerRight,
    UpperLeft,
    UpperRight,
}

public enum UQtLeafActions
{
    SwappingIn,
    SwappedIn,

    SwappingOut,
    SwappedOut,
}

public delegate void UQtCellChanged(UQtLeaf left, UQtLeaf entered);
public delegate void UQtCellSwapInOut(UQtLeaf leaf, UQtLeafActions action);

public static class UQtConfig
{
    // the cell would be created as leaf (stop dividing) if the size is smaller than this value
    public static float CellSizeThreshold = 50.0f;
    // swap-in distance of cells
    public static float CellSwapInDist = 100.0f;
    // swap-out distance of cells
    public static float CellSwapOutDist = 150.0f;
    // time interval to update the focus point (in seconds)
    public static float FocusUpdatingInterval = 0.1f;
}

