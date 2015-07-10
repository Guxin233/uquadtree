using UnityEngine;
using System.Collections;

delegate UQuadtreeNode UQuadtreeCreateNode(Rect bnd);

public static class UQuadtreeInternalUtil
{
    public static bool Intersects(Rect nodeBound, IQuadreeUserData userData)
    {
        Rect r = new Rect(
            userData.GetCenter().x - userData.GetExtends().x,
            userData.GetCenter().z - userData.GetExtends().z,
            userData.GetExtends().x * 2.0f,
            userData.GetExtends().z * 2.0f);
        return nodeBound.Overlaps(r);
    }

    public static void BuildRecursively(UQuadtreeNode node)
    {
        // parameters
        float subWidth = node.Bound.width * 0.5f;
        float subHeight = node.Bound.height * 0.5f;
        bool isPartible = subWidth >= UQuadtreeConfig.CellSizeThreshold && subHeight >= UQuadtreeConfig.CellSizeThreshold;

        // create subnodes
        UQuadtreeCreateNode _nodeCreator = (bnd) => { return new UQuadtreeNode(bnd); };
        UQuadtreeCreateNode _leafCreator = (bnd) => { return new UQuadtreeLeaf(bnd); };
        UQuadtreeCreateNode creator = isPartible ? _nodeCreator : _leafCreator;
        node.SetSubNodes(new UQuadtreeNode[UQuadtreeNode.SubCount] {
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

    public static UQuadtreeLeaf FindLeafRecursively(UQuadtreeNode node, Vector2 point)
    {
        if (!node.Bound.Contains(point))
            return null;

        foreach (var sub in node.SubNodes)
        {
            UQuadtreeLeaf leaf = FindLeafRecursively(sub, point);
            if (leaf != null)
                return leaf;
        }

        UCore.Assert(false);  // should never reaches here 
        return null;
    }
}
