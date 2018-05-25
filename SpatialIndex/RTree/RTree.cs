using System;
using System.Collections.Generic;

namespace SpatialIndex.RTree
{
    /// <summary>
    ///     R 树
    /// </summary>
    /// <typeparam name="TItem">RTree 中存放的元素的类型</typeparam>
    public class RTree<TItem>
    {
        private int minNodeEntries;
        private int maxNodeEntries;

        private const int DEFAULT_MAX_NODE_ENTRIES = 10;

        private byte[] initialEntryStatus;
        private byte[] entryStatus;

        private const int ENTRY_STATUS_ASSIGNED = 0;
        private const int ENTRY_STATUS_UNASSIGNED = 1;

        private volatile int idcounter = int.MinValue;

        private Dictionary<int, TItem> IdsToItems = new Dictionary<int, TItem>();
        private Dictionary<TItem, int> ItemsToIds = new Dictionary<TItem, int>();

        private List<int> nearestIds = new List<int>();

        public int Count;

        private int treeHeight = 1;

        private int rootNodeId;
        private Dictionary<int, Node> nodeMap = new Dictionary<int, Node>();

        private Stack<int> parents = new Stack<int>();
        private Stack<int> parentsEntry = new Stack<int>();

        private Stack<int> deletedNodeIds = new Stack<int>();

        private Rectangle oldRectangle;

        private int highestUsedNodeId;

        /// <summary>
        ///     构造一棵空 RTree
        /// </summary>
        /// <param name="maxEntries">每个结点的子树上限</param>
        /// <param name="minEntries">每个结点的子树下限</param>
        /// <exception cref="NotImplementedException"></exception>
        public RTree(int maxEntries, int minEntries)
        {
            maxNodeEntries = maxEntries;
            minNodeEntries = minEntries;

            if(maxNodeEntries < 2)
                maxNodeEntries = DEFAULT_MAX_NODE_ENTRIES;

            if(minNodeEntries < 1 || minNodeEntries > maxNodeEntries / 2)
                minNodeEntries = maxNodeEntries / 2;

            entryStatus = new byte[maxNodeEntries];
            initialEntryStatus = new byte[maxNodeEntries];

            for(int i = 0; i < maxNodeEntries; i++)
                initialEntryStatus[i] = ENTRY_STATUS_UNASSIGNED;

            Node root = new Node(rootNodeId, 1, maxNodeEntries);
            nodeMap.Add(rootNodeId, root);
        }

        /// <summary>
        ///     向 R 树中添加一条新的记录
        /// </summary>
        /// <param name="rectangle">新记录对应的矩形</param>
        /// <param name="item">新记录的元素</param>
        /// <exception cref="NotImplementedException"></exception>
        /// <remarks>如果希望只插入一个点，可以使用所有边长都为 0 的矩形</remarks>
        public void AddRecord(Rectangle rectangle, TItem item)
        {
            idcounter++;
            
            IdsToItems.Add(idcounter, item);
            ItemsToIds.Add(item, idcounter);

            Rectangle r = rectangle.Copy();

            add(r, idcounter, 1);
            
            Count++;
        }

        private void add(Rectangle r, int id, int level)
        {
            Node n = ChooseNode(r, 1);
            Node newLeaf = null;

            if (n.EntryCount < maxNodeEntries)
                n.AddEntry(r, id);
            else
                newLeaf = SplitNode(n, r, id);

            Node newNode = AdjustTree(n, newLeaf);
            if (newNode != null)
            {
                int oldRootNodeId = rootNodeId;
                Node oldRoot = GetNode(oldRootNodeId);

                rootNodeId = GetNextNodeId();
                treeHeight++;
                Node root = new Node(rootNodeId, treeHeight, maxNodeEntries);
                root.AddEntry(newNode.Mbr, newNode.Id);
                root.AddEntry(oldRoot.Mbr, oldRoot.Id);
                nodeMap.Add(rootNodeId, root);
            }
        }

        /// <summary>
        ///     从 R 树中移除指定的记录
        /// </summary>
        /// <param name="rectangle">移除记录对应的矩形</param>
        /// <param name="item">移除记录的元素</param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveRecord(Rectangle rectangle, TItem item)
        {
            parents.Clear();
            parents.Push(rootNodeId);

            parentsEntry.Clear();
            parentsEntry.Push(-1);
            Node n = null;
            int foundIndex = -1;

            while (foundIndex == -1 && parents.Count > 0)
            {
                n = GetNode(parents.Peek());
                int startIndex = parentsEntry.Peek() + 1;

                if (!n.IsLeaf())
                {
                    bool contains = false;
                    for (int i = startIndex; i < n.EntryCount; i++)
                        if (n.Entries[i].Contains(rectangle))
                        {
                            parents.Push(n.Ids[i]);
                            parentsEntry.Pop();
                            parentsEntry.Push(i); 
                            parentsEntry.Push(-1);
                            contains = true;
                            break; 
                        }

                    if (contains) continue;
                }
                else
                {
                    foundIndex = n.LocateEntry(rectangle, ItemsToIds[item]);
                }

                parents.Pop();
                parentsEntry.Pop();
            } 

            if (foundIndex != -1)
            {
                n.RemoveEntry(foundIndex, minNodeEntries);
                CondenseTree(n);
                Count--;
            }

            Node root = GetNode(rootNodeId);
            while (root.EntryCount == 1 && treeHeight > 1)
            {
                root.EntryCount = 0;
                rootNodeId = root.Ids[0];
                treeHeight--;
                root = GetNode(rootNodeId);
            }

            IdsToItems.Remove(ItemsToIds[item]);
            ItemsToIds.Remove(item);
        }

        private void CondenseTree(Node l)
        {
            Node n = l;
            Node parent = null;
            int parentEntry = 0;

            Stack<int> eliminatedNodeIds = new Stack<int>();

            while (n.Level != treeHeight)
            {
                parent = GetNode(parents.Pop());
                parentEntry = parentsEntry.Pop();

                if (n.EntryCount < minNodeEntries)
                {
                    parent.RemoveEntry(parentEntry, minNodeEntries);
                    eliminatedNodeIds.Push(n.Id);
                }
                else
                {
                    if (!n.Mbr.Equals(parent.Entries[parentEntry]))
                    {
                        oldRectangle.setDimension(parent.Entries[parentEntry].Dimension);
                        oldRectangle.set(parent.Entries[parentEntry].MinBoundries, parent.Entries[parentEntry].MaxBoundries);
                        parent.Entries[parentEntry].set(n.Mbr.MinBoundries, n.Mbr.MaxBoundries);
                        parent.recalculateMBR(oldRectangle);
                    }
                }

                n = parent;
            }

            while (eliminatedNodeIds.Count > 0)
            {
                Node e = GetNode(eliminatedNodeIds.Pop());
                for (int j = 0; j < e.EntryCount; j++)
                {
                    add(e.Entries[j], e.Ids[j], e.Level);
                    e.Entries[j] = null;
                }

                e.EntryCount = 0;
                deletedNodeIds.Push(e.Id);
            }
        }

        /// <summary>
        ///     获得距离指定位置最近的所有元素
        /// </summary>
        /// <param name="point">指定位置</param>
        /// <param name="distanceLimit">查找时的距离上限</param>
        /// <returns>距离 <c>point</c> 最近的元素构成的列表，如果有多个元素与 <c>point</c> 的距离一致才会同时处于列表中</returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<TItem> GetNearestItems(Point point, float distanceLimit)
        {
            List<TItem> retval = new List<TItem>();

            Node rootNode = GetNode(rootNodeId);
            nearest(point, rootNode, distanceLimit);
            foreach (int id in nearestIds)
                retval.Add(IdsToItems[id]);
            nearestIds.Clear();

            return retval;
        }

        private float nearest(Point p, Node n, float nearestDistance)
        {
            for (int i = 0; i < n.EntryCount; i++)
            {
                float tempDistance = n.Entries[i].MinimalDistanceTo(p);
                if (n.IsLeaf())
                {
                    if (tempDistance < nearestDistance)
                    {
                        nearestDistance = tempDistance;
                        nearestIds.Clear();
                    }

                    if (tempDistance <= nearestDistance) nearestIds.Add(n.Ids[i]);
                }
                else
                {
                    if (tempDistance <= nearestDistance)
                        nearestDistance = nearest(p, GetNode(n.Ids[i]), nearestDistance);
                }
            }

            return nearestDistance;
        }

        /// <summary>
        ///     获得最近邻的 k 个元素
        /// </summary>
        /// <param name="point">要查询位置</param>
        /// <param name="k">要查询的元素数量</param>
        /// <returns>距离 <c>point</c> 最近的 <c>k</c> 个元素，按照距离降序排列</returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<TItem> GetKNearestItems(Point point, int k)
        {
            List<TItem> retval = new List<TItem>();
            int retnum = 0;
            float lowerBound = -1.0f;
            while(retnum < k)
            {
                Node rootNode = GetNode(rootNodeId);

                
                lowerBound = nearestK(point, rootNode, float.MaxValue, lowerBound);

                foreach (int id in nearestIds)
                    if (retnum < k)
                    {
                        retnum++;
                        retval.Add(IdsToItems[id]);
                    }
                    else
                        break;
                nearestIds.Clear();
            }
            return retval;
        }

        private float nearestK(Point p, Node n, float nearestDistance, float lowerBound)
        {
            for (int i = 0; i < n.EntryCount; i++)
            {
                float tempDistance = n.Entries[i].MinimalDistanceTo(p);
                if (n.IsLeaf())
                {
                    if (tempDistance < nearestDistance && tempDistance > lowerBound)
                    {
                        nearestDistance = tempDistance;
                        nearestIds.Clear();
                    }

                    if (tempDistance <= nearestDistance && tempDistance > lowerBound) nearestIds.Add(n.Ids[i]);
                }
                else
                {
                    if (tempDistance <= nearestDistance)
                        nearestDistance = nearest(p, GetNode(n.Ids[i]), nearestDistance);
                }
            }

            return nearestDistance;
        }

        /// <summary>
        ///     获得与指定矩形有交集的所有元素
        /// </summary>
        /// <param name="rectangle">要查询的矩形</param>
        /// <returns>与 <c>rectangle</c> 有交集的所有元素的列表</returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<TItem> GetIntersectedItems(Rectangle rectangle)
        {
            List<TItem> retval = new List<TItem>();
            Node rootNode = GetNode(rootNodeId);
            intersects(rectangle, delegate (int id) { retval.Add(IdsToItems[id]); }, rootNode);
            return retval;
        }

        private void intersects(Rectangle r, intproc v, Node n)
        {
            for (int i = 0; i < n.EntryCount; i++)
                if (r.IntersectsWith(n.Entries[i]))
                {
                    if (n.IsLeaf())
                    {
                        v(n.Ids[i]);
                    }
                    else
                    {
                        Node childNode = GetNode(n.Ids[i]);
                        intersects(r, v, childNode);
                    }
                }
        }

        /// <summary>
        ///     获得位于指定矩形内部的所有元素
        /// </summary>
        /// <param name="rectangle">要查询的矩形</param>
        /// <returns>处于 <c>rectangle</c> 内部的所有元素的列表</returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<TItem> GetContainedItems(Rectangle rectangle)
        {
            List<TItem> retval = new List<TItem>();
            contains(rectangle, delegate (int id) { retval.Add(IdsToItems[id]); });
            return retval;
        }

        private void contains(Rectangle r, intproc v)
        {
            parents.Clear();
            parents.Push(rootNodeId);

            parentsEntry.Clear();
            parentsEntry.Push(-1);

            while (parents.Count > 0)
            {
                Node n = GetNode(parents.Peek());
                int startIndex = parentsEntry.Peek() + 1;

                if (!n.IsLeaf())
                {
                    bool intersects = false;
                    for (int i = startIndex; i < n.EntryCount; i++)
                        if (r.IntersectsWith(n.Entries[i]))
                        {
                            parents.Push(n.Ids[i]);
                            parentsEntry.Pop();
                            parentsEntry.Push(i); 
                            parentsEntry.Push(-1);
                            intersects = true;
                            break; 
                        }

                    if (intersects) continue;
                }
                else
                {
                    for (int i = 0; i < n.EntryCount; i++)
                        if (r.Contains(n.Entries[i]))
                            v(n.Ids[i]);
                }

                parents.Pop();
                parentsEntry.Pop();
            }
        }

        private Node ChooseNode(Rectangle r, int level)
        {
            Node n = GetNode(rootNodeId);
            parents.Clear();
            parentsEntry.Clear();

            while (true)
            {
                if (n.Level == level) return n;

                float leastEnlargement = n.GetEntryAt(0).GetEnlargement(r);
                int index = 0; 
                for (int i = 1; i < n.EntryCount; i++)
                {
                    Rectangle tempRectangle = n.GetEntryAt(i);
                    float tempEnlargement = tempRectangle.GetEnlargement(r);
                    if (tempEnlargement < leastEnlargement ||
                        tempEnlargement == leastEnlargement &&
                        tempRectangle.GetArea() < n.GetEntryAt(index).GetArea())
                    {
                        index = i;
                        leastEnlargement = tempEnlargement;
                    }
                }

                parents.Push(n.Id);
                parentsEntry.Push(index);

                n = GetNode(n.Ids[index]);
            }
        }

        private Node SplitNode(Node n, Rectangle newRect, int newID)
        {
            Array.Copy(initialEntryStatus, 0, entryStatus, 0, maxNodeEntries);

            Node newNode = null;
            newNode = new Node(GetNextNodeId(), n.Level, maxNodeEntries);
            nodeMap.Add(newNode.Id, newNode);

            PickSeeds(n, newRect, newID, newNode);

            while (n.EntryCount + newNode.EntryCount < maxNodeEntries + 1)
            {
                if (maxNodeEntries + 1 - newNode.EntryCount == minNodeEntries)
                {
                    for (int i = 0; i < maxNodeEntries; i++)
                        if (entryStatus[i] == ENTRY_STATUS_UNASSIGNED)
                        {
                            entryStatus[i] = ENTRY_STATUS_ASSIGNED;
                            n.Mbr.AddRectangle(n.Entries[i]);
                            n.EntryCount++;
                        }

                    break;
                }

                if (maxNodeEntries + 1 - n.EntryCount == minNodeEntries)
                {
                    for (int i = 0; i < maxNodeEntries; i++)
                        if (entryStatus[i] == ENTRY_STATUS_UNASSIGNED)
                        {
                            entryStatus[i] = ENTRY_STATUS_ASSIGNED;
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
            float maxNormalizedSeparation = 0;
            int highestLowIndex = 0;
            int lowestHighIndex = 0;

            n.Mbr.AddRectangle(newRect);

            for (int d = 0; d < newRect.Dimension; d++)
            {
                float tempHighestLow = newRect.MinBoundries[d];
                int tempHighestLowIndex = -1; 

                float tempLowestHigh = newRect.MaxBoundries[d];
                int tempLowestHighIndex = -1;

                for (int i = 0; i < n.EntryCount; i++)
                {
                    float tempLow = n.Entries[i].MinBoundries[d];
                    if (tempLow >= tempHighestLow)
                    {
                        tempHighestLow = tempLow;
                        tempHighestLowIndex = i;
                    }
                    else
                    {
                        float tempHigh = n.Entries[i].MaxBoundries[d];
                        if (tempHigh <= tempLowestHigh)
                        {
                            tempLowestHigh = tempHigh;
                            tempLowestHighIndex = i;
                        }
                    }

                    float normalizedSeparation = (tempHighestLow - tempLowestHigh) / (n.Mbr.MaxBoundries[d] - n.Mbr.MinBoundries[d]);

                    if (normalizedSeparation > maxNormalizedSeparation)
                    {
                        maxNormalizedSeparation = normalizedSeparation;
                        highestLowIndex = tempHighestLowIndex;
                        lowestHighIndex = tempLowestHighIndex;
                    }
                }
            }

            if (highestLowIndex == -1)
            {
                newNode.AddEntry(newRect, newId);
            }
            else
            {
                newNode.AddEntry(n.Entries[highestLowIndex], n.Ids[highestLowIndex]);
                n.Entries[highestLowIndex] = null;

                n.Entries[highestLowIndex] = newRect;
                n.Ids[highestLowIndex] = newId;
            }

            if (lowestHighIndex == -1) lowestHighIndex = highestLowIndex;

            entryStatus[lowestHighIndex] = ENTRY_STATUS_ASSIGNED;
            n.EntryCount = 1;
            n.Mbr.set(n.Entries[lowestHighIndex].MinBoundries, n.Entries[lowestHighIndex].MaxBoundries);
        }

        private int PickNext(Node n, Node newNode)
        {
            float maxDifference = float.NegativeInfinity;
            int next = 0;
            int nextGroup = 0;

            maxDifference = float.NegativeInfinity;

            for (int i = 0; i < maxNodeEntries; i++)
                if (entryStatus[i] == ENTRY_STATUS_UNASSIGNED)
                {
                    float nIncrease = n.Mbr.GetEnlargement(n.Entries[i]);
                    float newNodeIncrease = newNode.Mbr.GetEnlargement(n.Entries[i]);
                    float difference = Math.Abs(nIncrease - newNodeIncrease);

                    if (difference > maxDifference)
                    {
                        next = i;

                        if (nIncrease < newNodeIncrease)
                            nextGroup = 0;
                        else if (newNodeIncrease < nIncrease)
                            nextGroup = 1;
                        else if (n.Mbr.GetArea() < newNode.Mbr.GetArea())
                            nextGroup = 0;
                        else if (newNode.Mbr.GetArea() < n.Mbr.GetArea())
                            nextGroup = 1;
                        else if (newNode.EntryCount < maxNodeEntries / 2)
                            nextGroup = 0;
                        else
                            nextGroup = 1;
                        maxDifference = difference;
                    }

                }

            entryStatus[next] = ENTRY_STATUS_ASSIGNED;

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

            return next;
        }



        private Node AdjustTree(Node n, Node nn)
        {
            while (n.Level != treeHeight)
            {
                Node parent = GetNode(parents.Pop());
                int entry = parentsEntry.Pop();

                if (!parent.Entries[entry].Equals(n.Mbr))
                {
                    parent.Entries[entry].set(n.Mbr.MinBoundries, n.Mbr.MaxBoundries);
                    parent.Mbr.set(parent.Entries[0].MinBoundries, parent.Entries[0].MaxBoundries);
                    for (int i = 1; i < parent.EntryCount; i++) parent.Mbr.AddRectangle(parent.Entries[i]);
                }

                Node newNode = null;
                if (nn != null)
                {
                    if (parent.EntryCount < maxNodeEntries)
                        parent.AddEntry(nn.Mbr, nn.Id);
                    else
                        newNode = SplitNode(parent, nn.Mbr.Copy(), nn.Id);
                }

                n = parent;
                nn = newNode;

                parent = null;
                newNode = null;
            }

            return nn;
        }

        private Node GetNode(int index)
        {
            return nodeMap[index];
        }

        private int GetNextNodeId()
        {
            int nextNodeId = 0;
            if (deletedNodeIds.Count > 0)
                nextNodeId = deletedNodeIds.Pop();
            else
                nextNodeId = 1 + highestUsedNodeId++;
            return nextNodeId;
        }

        private delegate void intproc(int x);
    }
}
