using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SpatialIndex.RTree
{
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public class RTree<TItem>
    {
        private readonly Stack<int> _deletedNodeIds = new Stack<int>();
        private readonly Dictionary<int, TItem> _idsToItems = new Dictionary<int, TItem>();
        private readonly bool[] _isEntryAssigned;
        private readonly bool[] _isInitEntryAssigned;
        private readonly Dictionary<TItem, int> _itemsToIds = new Dictionary<TItem, int>();
        private readonly int _maxNodeEntries;
        private readonly int _minNodeEntries;
        private readonly List<int> _nearestIds = new List<int>();
        private readonly Dictionary<int, Node> _nodeMap = new Dictionary<int, Node>();
        private readonly Stack<int> _parents = new Stack<int>();
        private readonly Stack<int> _parentsEntry = new Stack<int>();
        private int _highestUsedNodeId;
        private volatile int _idCounter = int.MinValue;
        private int _rootNodeId;
        private int _treeHeight = 1;

        public int Count;

        private int _countSum;

        public RTree(int maxEntries, int minEntries)
        {
            Debug.Assert(maxEntries >= 2 && minEntries <= maxEntries / 2 && minEntries >= 1);

            _maxNodeEntries = maxEntries;
            _minNodeEntries = minEntries;

            _isEntryAssigned = new bool[_maxNodeEntries];
            _isInitEntryAssigned = new bool[_maxNodeEntries];

            for (var i = 0; i < _maxNodeEntries; i++)
                _isInitEntryAssigned[i] = false;

            var root = new Node(_rootNodeId, 1, _maxNodeEntries);
            _nodeMap.Add(_rootNodeId, root);
        }

        public void AddRecord(Rectangle rectangle, TItem item)
        {
            _idCounter++;
            _idsToItems.Add(_idCounter, item);
            _itemsToIds.Add(item, _idCounter);
            Add(rectangle.Copy(), _idCounter, 1);
            Count++;
        }

        private void Add(Rectangle r, int id, int level)
        {
            var n = ChooseNode(r, level);
            Node newLeaf = null;

            if (n.EntryCount < _maxNodeEntries)
                n.AddEntry(r, id);
            else
                newLeaf = SplitNode(n, r, id);

            var newNode = AdjustTree(n, newLeaf);

            if (newNode == null) return;

            var oldRootNodeId = _rootNodeId;
            var oldRoot = GetNode(oldRootNodeId);
            _rootNodeId = GetNextNodeId();
            _treeHeight++;
            var root = new Node(_rootNodeId, _treeHeight, _maxNodeEntries);
            root.AddEntry(newNode.Mbr, newNode.Id);
            root.AddEntry(oldRoot.Mbr, oldRoot.Id);
            _nodeMap.Add(_rootNodeId, root);
        }

        public void RemoveRecord(Rectangle rectangle, TItem item)
        {
            _parents.Clear();
            _parents.Push(_rootNodeId);

            _parentsEntry.Clear();
            _parentsEntry.Push(-1);
            Node n = null;
            var foundIndex = -1;

            while (foundIndex == -1 && _parents.Count > 0)
            {
                n = GetNode(_parents.Peek());
                var startIndex = _parentsEntry.Peek() + 1;

                if (!n.IsLeaf())
                {
                    var contains = false;
                    for (var i = startIndex; i < n.EntryCount; i++)
                        if (n.Entries[i].Contains(rectangle))
                        {
                            _parents.Push(n.Ids[i]);
                            _parentsEntry.Pop();
                            _parentsEntry.Push(i);
                            _parentsEntry.Push(-1);
                            contains = true;
                            break;
                        }

                    if (contains) continue;
                }
                else
                {
                    foundIndex = n.LocateEntry(rectangle, _itemsToIds[item]);
                }

                _parents.Pop();
                _parentsEntry.Pop();
            }

            if (foundIndex != -1)
            {
                Debug.Assert(n != null);

                n.RemoveEntry(foundIndex, _minNodeEntries);
                CondenseTree(n);
                Count--;
            }

            var root = GetNode(_rootNodeId);
            while (root.EntryCount == 1 && _treeHeight > 1)
            {
                root.EntryCount = 0;
                _rootNodeId = root.Ids[0];
                _treeHeight--;
                root = GetNode(_rootNodeId);
            }

            if (foundIndex == -1) return;
            _idsToItems.Remove(_itemsToIds[item]);
            _itemsToIds.Remove(item);
        }

        private void CondenseTree(Node l)
        {
            var n = l;
            var eliminatedNodeIds = new Stack<int>();

            while (n.Level != _treeHeight)
            {
                var parent = GetNode(_parents.Pop());
                var parentEntry = _parentsEntry.Pop();

                if (n.EntryCount < _minNodeEntries)
                {
                    parent.RemoveEntry(parentEntry, _minNodeEntries);
                    eliminatedNodeIds.Push(n.Id);
                }
                else if (!n.Mbr.Equals(parent.Entries[parentEntry]))
                {
                    var oldRect = new Rectangle(
                        parent.Entries[parentEntry].Dimension,
                        parent.Entries[parentEntry].MinBoundries,
                        parent.Entries[parentEntry].MaxBoundries
                    );
                    parent.Entries[parentEntry].MinBoundries = n.Mbr.MinBoundries.ToList();
                    parent.Entries[parentEntry].MaxBoundries = n.Mbr.MaxBoundries.ToList();
                    parent.MbrRemovalUpdate(oldRect);
                }

                n = parent;
            }

            while (eliminatedNodeIds.Count > 0)
            {
                var e = GetNode(eliminatedNodeIds.Pop());
                for (var j = 0; j < e.EntryCount; j++)
                {
                    Add(e.Entries[j], e.Ids[j], e.Level);
                    e.Entries[j] = null;
                }

                e.EntryCount = 0;
                _deletedNodeIds.Push(e.Id);
            }
        }

        public List<TItem> GetNearestItems(Point point, float distanceLimit)
        {
            var retval = new List<TItem>();
            var rootNode = GetNode(_rootNodeId);
            Nearest(point, rootNode, distanceLimit);
            foreach (var id in _nearestIds)
                retval.Add(_idsToItems[id]);
            _nearestIds.Clear();

            return retval;
        }

        private float Nearest(Point p, Node n, float nearestDistance)
        {
            for (var i = 0; i < n.EntryCount; i++)
            {
                var tempDistance = n.Entries[i].MinimalDistanceTo(p);
                if (n.IsLeaf())
                {
                    if (tempDistance < nearestDistance)
                    {
                        nearestDistance = tempDistance;
                        _nearestIds.Clear();
                    }

                    if (tempDistance <= nearestDistance)
                        _nearestIds.Add(n.Ids[i]);
                }
                else if (tempDistance <= nearestDistance)
                {
                    nearestDistance = Nearest(p, GetNode(n.Ids[i]), nearestDistance);
                }
            }

            return nearestDistance;
        }

        public List<TItem> GetKNearestItems(Point point, int k)
        {
            var ret = new List<TItem>();
            var retnum = 0;
            var lowerBound = -1.0f;
            while (retnum < k)
            {
                var rootNode = GetNode(_rootNodeId);

                lowerBound = NearestK(point, rootNode, float.MaxValue, lowerBound);

                foreach (var id in _nearestIds)
                    if (retnum < k)
                    {
                        retnum++;
                        ret.Add(_idsToItems[id]);
                    }
                    else
                    {
                        break;
                    }


                _nearestIds.Clear();
            }

            return ret;
        }

        private float NearestK(Point p, Node n, float nearestDistance, float lowerBound)
        {
            for (var i = 0; i < n.EntryCount; i++)
            {
                var tempDistance = n.Entries[i].MinimalDistanceTo(p);
                if (n.IsLeaf())
                {
                    if (tempDistance < nearestDistance && tempDistance > lowerBound)
                    {
                        nearestDistance = tempDistance;
                        _nearestIds.Clear();
                    }

                    if (tempDistance <= nearestDistance && tempDistance > lowerBound)
                        _nearestIds.Add(n.Ids[i]);
                }
                else if (tempDistance <= nearestDistance)
                {
                    nearestDistance = NearestK(p, GetNode(n.Ids[i]), nearestDistance, lowerBound);
                }
            }

            return nearestDistance;
        }

        public List<TItem> GetIntersectedItems(Rectangle rectangle)
        {
            var retval = new List<TItem>();
            var rootNode = GetNode(_rootNodeId);
            Intersects(rectangle, delegate(int id) { retval.Add(_idsToItems[id]); }, rootNode);
            return retval;
        }

        private void Intersects(Rectangle r, IntProc v, Node n)
        {
            for (var i = 0; i < n.EntryCount; i++)
                if (r.IntersectsWith(n.Entries[i]))
                {
                    if (n.IsLeaf())
                    {
                        v(n.Ids[i]);
                    }
                    else
                    {
                        var childNode = GetNode(n.Ids[i]);
                        Intersects(r, v, childNode);
                    }
                }
        }

        public List<TItem> GetContainedItems(Rectangle rectangle, out int countSum)
        {
            _countSum = 0;
            var retval = new List<TItem>();
            Contains(rectangle, delegate(int id) { retval.Add(_idsToItems[id]); });
            countSum = _countSum;
            return retval;
        }

        private void Contains(Rectangle r, IntProc v)
        {
            _parents.Clear();
            _parents.Push(_rootNodeId);
            _countSum++;

            _parentsEntry.Clear();
            _parentsEntry.Push(-1);

            while (_parents.Count > 0)
            {
                var n = GetNode(_parents.Peek());
                var startIndex = _parentsEntry.Peek() + 1;
                _countSum++;
                if (!n.IsLeaf())
                {
                    var intersects = false;
                    for (var i = startIndex; i < n.EntryCount; i++)
                        if (r.IntersectsWith(n.Entries[i]))
                        {
                            _parents.Push(n.Ids[i]);
                            _countSum++;
                            _parentsEntry.Pop();
                            _parentsEntry.Push(i);
                            _parentsEntry.Push(-1);
                            intersects = true;
                            break;
                        }

                    if (intersects) continue;
                }
                else
                {
                    for (var i = 0; i < n.EntryCount; i++)
                        if (r.Contains(n.Entries[i]))
                            v(n.Ids[i]);
                }

                _parents.Pop();
                _parentsEntry.Pop();
            }
        }

        private Node ChooseNode(Rectangle r, int level)
        {
            var n = GetNode(_rootNodeId);
            _parents.Clear();
            _parentsEntry.Clear();

            while (true)
            {
                if (n.Level == level) return n;

                var leastEnlargement = n.GetEntryAt(0).GetEnlargement(r);
                var index = 0;
                for (var i = 1; i < n.EntryCount; i++)
                {
                    var tempRectangle = n.GetEntryAt(i);
                    var tempEnlargement = tempRectangle.GetEnlargement(r);
                    if (!(tempEnlargement < leastEnlargement) &&
                        (tempEnlargement != leastEnlargement ||
                         !(tempRectangle.GetArea() < n.GetEntryAt(index).GetArea()))) continue;
                    index = i;
                    leastEnlargement = tempEnlargement;
                }

                _parents.Push(n.Id);
                _parentsEntry.Push(index);

                n = GetNode(n.Ids[index]);
            }
        }

        private Node SplitNode(Node n, Rectangle newRect, int newId)
        {
            Array.Copy(_isInitEntryAssigned, 0, _isEntryAssigned, 0, _maxNodeEntries);
            var newNode = new Node(GetNextNodeId(), n.Level, _maxNodeEntries);
            _nodeMap.Add(newNode.Id, newNode);

            PickSeeds(n, newRect, newId, newNode);

            while (n.EntryCount + newNode.EntryCount < _maxNodeEntries + 1)
            {
                if (_maxNodeEntries + 1 - newNode.EntryCount == _minNodeEntries)
                {
                    for (var i = 0; i < _maxNodeEntries; i++)
                        if (!_isEntryAssigned[i])
                        {
                            _isEntryAssigned[i] = true;
                            n.Mbr.AddRectangle(n.Entries[i]);
                            n.EntryCount++;
                        }

                    break;
                }

                if (_maxNodeEntries + 1 - n.EntryCount == _minNodeEntries)
                {
                    for (var i = 0; i < _maxNodeEntries; i++)
                        if (!_isEntryAssigned[i])
                        {
                            _isEntryAssigned[i] = true;
                            newNode.AddEntry(n.Entries[i], n.Ids[i]);
                            n.Entries[i] = null;
                        }

                    break;
                }

                PickNext(n, newNode);
            }

            n.Shrink();

            return newNode;
        }

        private void PickSeeds(Node n, Rectangle newRect, int newId, Node newNode)
        {
            var maxNormalizedSeparation = 0f;
            var highestLowIndex = 0;
            var lowestHighIndex = 0;

            n.Mbr.AddRectangle(newRect);

            for (var d = 0; d < newRect.Dimension; d++)
            {
                var tempHighestLow = newRect.MinBoundries[d];
                var tempHighestLowIndex = -1;

                var tempLowestHigh = newRect.MaxBoundries[d];
                var tempLowestHighIndex = -1;

                for (var i = 0; i < n.EntryCount; i++)
                {
                    var tempLow = n.Entries[i].MinBoundries[d];
                    if (tempLow >= tempHighestLow)
                    {
                        tempHighestLow = tempLow;
                        tempHighestLowIndex = i;
                    }
                    else
                    {
                        var tempHigh = n.Entries[i].MaxBoundries[d];
                        if (tempHigh <= tempLowestHigh)
                        {
                            tempLowestHigh = tempHigh;
                            tempLowestHighIndex = i;
                        }
                    }

                    var normalizedSeparation = (tempHighestLow - tempLowestHigh) /
                                               (n.Mbr.MaxBoundries[d] - n.Mbr.MinBoundries[d]);

                    if (normalizedSeparation <= maxNormalizedSeparation) continue;
                    maxNormalizedSeparation = normalizedSeparation;
                    highestLowIndex = tempHighestLowIndex;
                    lowestHighIndex = tempLowestHighIndex;
                }
            }

            if (highestLowIndex == -1)
            {
                newNode.AddEntry(newRect, newId);
            }
            else
            {
                newNode.AddEntry(n.Entries[highestLowIndex], n.Ids[highestLowIndex]);
                n.Entries[highestLowIndex] = newRect;
                n.Ids[highestLowIndex] = newId;
            }

            if (lowestHighIndex == -1)
                lowestHighIndex = highestLowIndex;

            _isEntryAssigned[lowestHighIndex] = true;
            n.EntryCount = 1;

            n.Mbr.MinBoundries = n.Entries[lowestHighIndex].MinBoundries.ToList();
            n.Mbr.MaxBoundries = n.Entries[lowestHighIndex].MaxBoundries.ToList();
        }

        private void PickNext(Node n, Node newNode)
        {
            var maxDifference = float.NegativeInfinity;
            var next = 0;
            var nextGroup = 0;

            for (var i = 0; i < _maxNodeEntries; i++)
                if (!_isEntryAssigned[i])
                {
                    var nIncrease = n.Mbr.GetEnlargement(n.Entries[i]);
                    var newNodeIncrease = newNode.Mbr.GetEnlargement(n.Entries[i]);
                    var difference = Math.Abs(nIncrease - newNodeIncrease);

                    if (!(difference > maxDifference)) continue;

                    next = i;
                    if (nIncrease < newNodeIncrease)
                        nextGroup = 0;
                    else if (newNodeIncrease < nIncrease)
                        nextGroup = 1;
                    else if (n.Mbr.GetArea() < newNode.Mbr.GetArea())
                        nextGroup = 0;
                    else if (newNode.Mbr.GetArea() < n.Mbr.GetArea())
                        nextGroup = 1;
                    else if (newNode.EntryCount < _maxNodeEntries / 2)
                        nextGroup = 0;
                    else
                        nextGroup = 1;
                    maxDifference = difference;
                }

            _isEntryAssigned[next] = true;

            if (nextGroup == 0)
            {
                n.Mbr.AddRectangle(n.Entries[next]);
                n.EntryCount++;
            }
            else
            {
                newNode.AddEntry(n.Entries[next], n.Ids[next]);
                n.Entries[next] = null;
            }
        }

        public Rectangle GetRootMbr()
        {
            return GetNode(_rootNodeId).Mbr;
        }

        private Node AdjustTree(Node n, Node nn)
        {
            while (n.Level != _treeHeight)
            {
                var parent = GetNode(_parents.Pop());
                var entry = _parentsEntry.Pop();

                if (!parent.Entries[entry].ContentEquals(n.Mbr))
                {
                    parent.Entries[entry].MinBoundries = n.Mbr.MinBoundries.ToList();
                    parent.Entries[entry].MaxBoundries = n.Mbr.MaxBoundries.ToList();
                    parent.Mbr.MinBoundries = parent.Entries[0].MinBoundries;
                    parent.Mbr.MaxBoundries = parent.Entries[0].MaxBoundries;
                    for (var i = 1; i < parent.EntryCount; i++)
                        parent.Mbr.AddRectangle(parent.Entries[i]);
                }

                Node newNode = null;
                if (nn != null)
                {
                    if (parent.EntryCount < _maxNodeEntries)
                        parent.AddEntry(nn.Mbr, nn.Id);
                    else
                        newNode = SplitNode(parent, nn.Mbr.Copy(), nn.Id);
                }

                n = parent;
                nn = newNode;
            }

            return nn;
        }

        private Node GetNode(int index)
        {
            return _nodeMap[index];
        }

        private int GetNextNodeId()
        {
            int nextNodeId;
            if (_deletedNodeIds.Count > 0)
                nextNodeId = _deletedNodeIds.Pop();
            else
                nextNodeId = 1 + _highestUsedNodeId++;
            return nextNodeId;
        }

        private delegate void IntProc(int x);
    }
}