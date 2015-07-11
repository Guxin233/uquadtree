using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// user data stored in quadtree leaves
public interface IQtUserData
{
    Vector3 GetCenter();
    Vector3 GetExtends();
}

public class UQtNode
{
    public UQtNode(Rect bound)
    {
        _bound = bound;
    }

    public Rect Bound { get { return _bound; } }
    protected Rect _bound;

    public virtual void SetSubNodes(UQtNode[] subNodes)
    {
        _subNodes = subNodes;
    }

    public virtual void Receive(IQtUserData userData)
    {
        if (!UQtInternalUtil.Intersects(Bound, userData))
        {
            return;
        }

        foreach (var sub in SubNodes)
        {
            Receive(userData);
        }
    }

    public UQtNode[] SubNodes { get { return _subNodes; } }
    public const int SubCount = 4;
    protected UQtNode[] _subNodes = null;
}

public class UQtLeaf : UQtNode
{
    public UQtLeaf(Rect bound) : base(bound)
    {
    }
    public override void SetSubNodes(UQtNode[] subNodes)
    {
        UCore.Assert(false);
    }

    public override void Receive(IQtUserData userData)
    {
        if (!UQtInternalUtil.Intersects(Bound, userData))
            return;

        if (Bound.Contains(new Vector2(userData.GetCenter().x, userData.GetCenter().z)))
        {
            if (_ownedObjects == null)
                _ownedObjects = new List<IQtUserData>();
            _ownedObjects.Add(userData);            
        }
        else
        {
            if (_affectedObjects == null)
                _affectedObjects = new List<IQtUserData>();
            _affectedObjects.Add(userData);
        }
    }

    private List<IQtUserData> _ownedObjects;
    private List<IQtUserData> _affectedObjects;
}

public class UQuadtree
{
    public UQuadtree(Rect bound)
    {
        _root = new UQtNode(bound);
        UQtInternalUtil.BuildRecursively(_root);
    }

    public void Update(Vector2 focusPoint)
    {
        if (EnableDebugLines)
        {
            DrawDebugLines();
        }

        if (Time.time - _lastSwapTime > UQtConfig.FocusUpdatingInterval)
        {
            if (UpdateFocus(focusPoint))
                ProcessSwapping(_focusLeaf);
            _lastSwapTime = Time.time;
        }

    }

    public event UQtCellChanged FocusCellChanged;

    public Rect SceneBound { get { return _root.Bound; } }
    public Vector3 FocusPoint { get { return _focusPoint; } }
    public bool EnableDebugLines { get; set; }

    private void DrawDebugLines()
    {
        UQtInternalUtil.TraverseAllLeaves(_root, (leaf) => {
            Color c = Color.gray;

            if (leaf == _focusLeaf)
            {
                c = Color.blue;
            }
            else if (_swapInLeaves.Contains(leaf))
            {
                c = Color.green;
            }
            else if (_swapOutLeaves.Contains(leaf))
            {
                c = Color.red;
            }
            UCore.DrawRect(leaf.Bound, 0.1f, c, 1.0f); 
        });
    }

    private bool UpdateFocus(Vector2 focusPoint)
    {
        _focusPoint = focusPoint;

        UQtLeaf newLeaf = UQtInternalUtil.FindLeafRecursively(_root, _focusPoint);
        if (newLeaf == _focusLeaf)
            return false;

        if (FocusCellChanged != null)
            FocusCellChanged(_focusLeaf, newLeaf);

        _focusLeaf = newLeaf;
        return true;
    }

    private void ProcessSwapping(UQtLeaf activeLeaf)
    {
        // refresh swapping lists
        UQtInternalUtil.GenerateSwappingLeaves(_root, activeLeaf, out _swapInLeaves, out _swapOutLeaves);
    }

    private UQtNode _root;

    private Vector3 _focusPoint;
    private UQtLeaf _focusLeaf;

    private List<UQtLeaf> _holdingLeaves = new List<UQtLeaf>();
    private List<UQtLeaf> _swapInLeaves = new List<UQtLeaf>();
    private List<UQtLeaf> _swapOutLeaves = new List<UQtLeaf>();

    private float _lastSwapTime = 0.0f;

}
