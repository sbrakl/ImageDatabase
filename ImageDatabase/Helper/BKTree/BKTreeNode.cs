using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageDatabase.Helper.Tree
{
    [Serializable]
    public abstract class BKTreeNode
    {
        public SortedDictionary<Int32, BKTreeNode> _children { get; set; }

        public BKTreeNode()
        {
            
        }

        public virtual void add(BKTreeNode node)
        {
            if (_children == null)
                _children = new SortedDictionary<Int32, BKTreeNode>();

            Int32 distance = calculateDistance(node);

            if (_children.ContainsKey(distance))
            {
                _children[distance].add(node);
            }
            else
            {
                _children.Add(distance, node);
            }
        }

        public virtual Int32 findBestMatch(BKTreeNode node, Int32 bestDistance, out BKTreeNode bestNode)
        {
            Int32 distanceAtNode = calculateDistance(node);

            bestNode = node;

            if(distanceAtNode < bestDistance)
            {
                bestDistance = distanceAtNode;
                bestNode = this;
            }

            Int32 possibleBest = bestDistance;

            foreach (Int32 distance in _children.Keys)
            {
                if (distance < distanceAtNode + bestDistance)
                {
                    possibleBest = _children[distance].findBestMatch(node, bestDistance, out bestNode);
                    if (possibleBest < bestDistance)
                    {
                        bestDistance = possibleBest;
                    }
                }
            }

            return bestDistance;
        }

        public virtual void query(BKTreeNode node, Int32 threshold, Dictionary<BKTreeNode, Int32> collected)
        {
            Int32 distanceAtNode = calculateDistance(node);

            if (distanceAtNode == threshold)
            {
                collected.Add(this, distanceAtNode);
                return;
            }

            if (distanceAtNode < threshold)
            {
                collected.Add(this, distanceAtNode);
            }

            for (Int32 distance = (distanceAtNode - threshold); distance <= (threshold + distanceAtNode); distance++)
            {
                if (_children != null)
                {
                    if (_children.ContainsKey(distance))
                    {
                        _children[distance].query(node, threshold, collected);
                    }
                }                
            }
        }

        protected abstract int calculateDistance(BKTreeNode node);
    }
}
