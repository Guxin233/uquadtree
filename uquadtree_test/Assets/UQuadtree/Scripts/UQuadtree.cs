using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class UQuadtreeConfig
{
    // the cell would be created as leaf (stop dividing) if the size is smaller than this value
    public static float CellSizeThreshold = 5.0f;
    // swap-in distance of cells
    public static float CellSwapInDist = 10.0f;
    // swap-out distance of cells
    public static float CellSwapOutDist = 15.0f;
}

// user data stored in quadtree leaves
public interface IQuadreeUserData
{
    Vector3 GetCenter();
    Vector3 GetExtends();
}

public class UQuadtreeNode
{
    public UQuadtreeNode(Rect bound)
    {
        _bound = bound;
    }

    public Rect Bound { get { return _bound; } }
    protected Rect _bound;

    public virtual void SetSubNodes(UQuadtreeNode[] subNodes)
    {
        _subNodes = subNodes;
    }

    public virtual void Receive(IQuadreeUserData userData)
    {
        if (!UQuadtreeInternalUtil.Intersects(Bound, userData))
        {
            return;
        }

        foreach (var sub in SubNodes)
        {
            Receive(userData);
        }
    }

    public UQuadtreeNode[] SubNodes { get { return _subNodes; } }
    public const int SubCount = 4;
    protected UQuadtreeNode[] _subNodes = null;
}

public class UQuadtreeLeaf : UQuadtreeNode
{
    public UQuadtreeLeaf(Rect bound) : base(bound)
    {
    }
    public override void SetSubNodes(UQuadtreeNode[] subNodes)
    {
        UCore.Assert(false);
    }

    public override void Receive(IQuadreeUserData userData)
    {
        if (!UQuadtreeInternalUtil.Intersects(Bound, userData))
        {
            return;
        }

        if (Bound.Contains(new Vector2(userData.GetCenter().x, userData.GetCenter().z)))
        {
            _ownedObjects.Add(userData);            
        }
        else
        {
            _affectedObjects.Add(userData);
        }
    }

    private List<IQuadreeUserData> _ownedObjects;
    private List<IQuadreeUserData> _affectedObjects;
}

public class UQuadtree
{
    public UQuadtree(Rect bound)
    {
        _root = new UQuadtreeNode(bound);
        UQuadtreeInternalUtil.BuildRecursively(_root);
    }

    public void Update(Vector3 focusPoint)
    {
        _focusPoint = focusPoint;

        UQuadtreeLeaf newLeaf = UQuadtreeInternalUtil.FindLeafRecursively(_root, _focusPoint);
        if (newLeaf != _focusLeaf)
        {
            if (FocusCellChanged != null)
            {
                FocusCellChanged(_focusLeaf, newLeaf);
            }
            _focusLeaf = newLeaf;
        }
    }

    public event UQuadtreeCellChanged FocusCellChanged;

    public Rect SceneBound { get { return _root.Bound; } }
    public Vector3 FocusPoint { get { return _focusPoint; } }

    private UQuadtreeNode _root;
    private Vector3 _focusPoint;
    private UQuadtreeLeaf _focusLeaf;

    private List<UQuadtreeLeaf> _holdingLeaves = new List<UQuadtreeLeaf>();
    private List<UQuadtreeLeaf> _swapInLeaves = new List<UQuadtreeLeaf>();
    private List<UQuadtreeLeaf> _swapOutLeaves = new List<UQuadtreeLeaf>();
}
