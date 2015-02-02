using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/*
 * This class is an implementation of a Burkhard-Keller tree.  
 * The BK-Tree is a tree structure used to quickly find close matches to
 * any defined object.
 * 
 * The BK-Tree was first described in the paper:
 * "Some Approaches to Best-Match File Searching" by W. A. Burkhard and R. M. Keller
 * It is available in the ACM archives.
 * 
 * Another good explanation can be found here:
 * http://blog.notdot.net/2007/4/Damn-Cool-Algorithms-Part-1-BK-Trees
 * 
 * Searching the tree yields O(logn), which is a huge upgrade over brute force.
 * 
 * The original author of this code in Java is Josh Clemm
 * (The preceding comment block is his with a handful of edits)
 * http://code.google.com/p/java-bk-tree
 *
 * Ported to C# with generic tree nodes + three example distance metrics
 * by Mike Karlesky.
 * See readme for more on specific individual changes.
 */

namespace ImageDatabase.Helper.Tree
{
    [Serializable]
    public class BKTree<T> where T : BKTreeNode
    {
        public T _root { get; set; }

        private readonly Dictionary<T, Int32> _matches;

        public BKTree()
        {
            //_root    = null;
            _matches = new Dictionary<T, Int32>();
        }

        public void add(T node)
        {
            if (_root != null)
            {
                _root.add(node);
            }
            else
            {
                _root = node;
            }
        }

        /**
         * This method will find all the close matching Nodes within
         * a certain threshold.  For instance, to search for similar
         * strings, threshold set to 1 will return all the strings that
         * are off by 1 edit distance.
         * @param searchNode
         * @param threshold
         * @return
         */
        public Dictionary<T, Int32> query(BKTreeNode searchNode, Int32 threshold)
        {
            Dictionary<BKTreeNode, Int32> matches = new Dictionary<BKTreeNode, Int32>();

            _root.query(searchNode, threshold, matches);

            return copyMatches(matches);
        }

        /**
         * Attempts to find the closest match to the search node.
         * @param node 
         * @return The edit distance of the best match
         */
        public Int32 findBestDistance(BKTreeNode node)
        {
            BKTreeNode bestNode;
            return _root.findBestMatch(node, Int32.MaxValue, out bestNode);
        }

        /**
         * Attempts to find the closest match to the search node.
         * @param node
         * @return A match that is within the best edit distance of the search node.
         */
        public T findBestNode(BKTreeNode node)
        {
            BKTreeNode bestNode;
            _root.findBestMatch(node, Int32.MaxValue, out bestNode);
            return (T)bestNode;
        }

        /**
         * Attempts to find the closest match to the search node.
         * @param node
         * @return A match that is within the best edit distance of the search node.
         */
        public Dictionary<T, Int32> findBestNodeWithDistance(BKTreeNode node)
        {
            BKTreeNode bestNode;
            Int32 distance = _root.findBestMatch(node, Int32.MaxValue, out bestNode);
            _matches.Clear();
            _matches.Add((T)bestNode, distance);
            return _matches;
        }

        private Dictionary<T, Int32> copyMatches(Dictionary<BKTreeNode, Int32> source)
        {
            _matches.Clear();

            foreach (KeyValuePair<BKTreeNode, Int32> pair in source)
            {
                _matches.Add((T)pair.Key, pair.Value);
            }

            return _matches;
        }
    }
}
