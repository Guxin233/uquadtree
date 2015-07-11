using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate UQtNode UQtCreateNode(Rect bnd);
public delegate void UQtForeachLeaf(UQtLeaf leaf);

public static class UQtInternalUtil
{
    public static bool Intersects(Rect nodeBound, IQtUserData userData)
    {
        Rect r = new Rect(
            userData.GetCenter().x - userData.GetExtends().x,
            userData.GetCenter().z - userData.GetExtends().z,
            userData.GetExtends().x * 2.0f,
            userData.GetExtends().z * 2.0f);
        return nodeBound.Overlaps(r);
    }

    public static bool Intersects(Rect nodeBound, Vector2 targetCenter, float targetRadius)
    {
        bool xOutside = targetCenter.x + targetRadius < nodeBound.xMin || targetCenter.x - targetRadius > nodeBound.xMax;
        bool yOutside = targetCenter.y + targetRadius < nodeBound.yMin || targetCenter.y - targetRadius > nodeBound.yMax;
        bool outside = xOutside || yOutside;
        return !outside;
    }

    public static void BuildRecursively(UQtNode node)
    {
        // parameters
        float subWidth = node.Bound.width * 0.5f;
        float subHeight = node.Bound.height * 0.5f;
        bool isPartible = subWidth >= UQtConfig.CellSizeThreshold && subHeight >= UQtConfig.CellSizeThreshold;

        // create subnodes
        UQtCreateNode _nodeCreator = (bnd) => { return new UQtNode(bnd); };
        UQtCreateNode _leafCreator = (bnd) => { return new UQtLeaf(bnd); };
        UQtCreateNode creator = isPartible ? _nodeCreator : _leafCreator;
        node.SetSubNodes(new UQtNode[UQtNode.SubCount] {
            creator(new Rect(node.Bound.xMin,             node.Bound.yMin,                subWidth, subHeight)),
            creator(new Rect(node.Bound.xMin + subWidth,  node.Bound.yMin,                subWidth, subHeight)),
            creator(new Rect(node.Bound.xMin,             node.Bound.yMin + subHeight,    subWidth, subHeight)),
            creator(new Rect(node.Bound.xMin + subWidth,  node.Bound.yMin + subHeight,    subWidth, subHeight)),
        });

        // do it recursively
        if (isPartible)
        {
            foreach (var sub in node.SubNodes)
            {
                BuildRecursively(sub);
            }
        }
    }

    public static void TraverseAllLeaves(UQtNode node, UQtForeachLeaf func)
    {
        if (node is UQtLeaf)
            func(node as UQtLeaf);
        else
            foreach (var sub in node.SubNodes)
                TraverseAllLeaves(sub, func);
    }

    public static UQtLeaf FindLeafRecursively(UQtNode node, Vector2 point)
    {
        if (!node.Bound.Contains(point))
            return null;

        if (node is UQtLeaf)
            return node as UQtLeaf;

        foreach (var sub in node.SubNodes)
        {
            UQtLeaf leaf = FindLeafRecursively(sub, point);
            if (leaf != null)
                return leaf;
        }

        UCore.Assert(false);  // should never reaches here 
        return null;
    }

    public static void GenerateSwappingLeaves(UQtNode node, UQtLeaf active, out List<UQtLeaf> inLeaves, out List<UQtLeaf> outLeaves)
    {
        List<UQtLeaf> inList = new List<UQtLeaf>();
        GenerateLeavesByDist(node, active, UQtConfig.CellSwapInDist, ref inList);
        List<UQtLeaf> outList = new List<UQtLeaf>();
        GenerateLeavesByDist(node, active, UQtConfig.CellSwapOutDist, ref outList);
        
        outList.RemoveAll((item) => inList.Contains(item));

        inLeaves = inList;
        outLeaves = outList;
    }

    private static void GenerateLeavesByDist(UQtNode node, UQtLeaf active, float dist, ref List<UQtLeaf> leaves)
    {
        if (!Intersects(node.Bound, active.Bound.center, dist))
            return;

        if (node is UQtLeaf)
            leaves.Add(node as UQtLeaf);
        else
            foreach (var sub in node.SubNodes)
                GenerateLeavesByDist(sub, active, dist, ref leaves);
    }
}
