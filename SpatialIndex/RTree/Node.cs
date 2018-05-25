using System.Diagnostics;

namespace SpatialIndex.RTree
{
    /// <summary>
    ///     RTree 中的一个结点
    /// </summary>
    public class Node
    {
        /// <summary>
        ///     结点子树对应的矩形
        /// </summary>
        internal readonly Rectangle[] Entries;

        /// <summary>
        ///     结点子树对应的结点 ID
        /// </summary>
        internal readonly int[] Ids;

        /// <summary>
        ///     构造一个指定的结点
        /// </summary>
        /// <param name="id">结点的 id</param>
        /// <param name="level">结点的层数（叶结点层数为 0）</param>
        /// <param name="maxEntries">结点具有子树容量</param>
        internal Node(int id, int level, int maxEntries)
        {
            Id = id;
            Level = level;

            Entries = new Rectangle[maxEntries];
            Ids = new int[maxEntries];
            EntryCount = 0;
        }

        /// <summary>
        ///     结点的 ID
        /// </summary>
        internal int Id { get; }

        /// <summary>
        ///     结点的层数（规定叶子结点层数为 0）
        /// </summary>
        internal int Level { get; }

        /// <summary>
        ///     这个结点对应的最小包络矩形
        /// </summary>
        internal Rectangle Mbr { get; private set; }

        /// <summary>
        ///     结点子树数目
        /// </summary>
        internal int EntryCount { get; set; }

        /// <summary>
        ///     向结点中加入一个矩形（以及它的 ID）
        /// </summary>
        internal void AddEntry(Rectangle rectangle, int id)
        {
            Debug.Assert(EntryCount < Entries.Length - 1);

            Entries[EntryCount] = rectangle.Copy();
            Ids[EntryCount] = id;
            EntryCount++;

            if (Mbr == null)
                Mbr = rectangle.Copy();
            else
                Mbr.AddRectangle(rectangle);
        }

        /// <summary>
        ///     在结点中找到一个特定的矩形（需要同时提供矩形和 id，两者都需要匹配）
        /// </summary>
        /// <returns>矩形在节点中的位置。-1 表示没找到</returns>
        internal int LocateEntry(Rectangle rectangle, int id)
        {
            for (var i = 0; i < EntryCount; i++)
                if (rectangle.ContentEquals(Entries[i]) && id == Ids[i])
                    return i;
            return -1;
        }

        /// <summary>
        ///     在结点中移除特定位置上的记录，需要提供结点子结点数下界来指导其行为
        /// </summary>
        internal void RemoveEntry(int i, int minEntries)
        {
            Debug.Assert(0 < i && i < EntryCount);

            var removed = Entries[i];
            EntryCount--;
            Entries[i] = Entries[EntryCount];
            Ids[i] = Ids[EntryCount];
            Entries[EntryCount] = null;

            if (EntryCount >= minEntries)
                MbrRemovalUpdate(removed);
        }

        /// <summary>
        ///     得到在给定位置上的矩形
        /// </summary>
        internal Rectangle GetEntryAt(int i)
        {
            return Entries[i];
        }

        /// <summary>
        ///     得到在给定位置上的 ID
        /// </summary>
        internal int GetIdAt(int i)
        {
            return Ids[i];
        }

        /// <summary>
        ///     让结点重排子结点在数组中的分布情况，使其集中在数组低下标端
        /// </summary>
        internal void Shrink()
        {
            var lastNotEmpty = Entries.Length - 1;
            for (var i = 0; i < EntryCount; i++)
            {
                if (Entries[i] != null) continue;
                while (Entries[lastNotEmpty] == null) lastNotEmpty--;
                Entries[i] = Entries[lastNotEmpty];
                Ids[i] = Ids[lastNotEmpty];
                Entries[lastNotEmpty] = null;
            }
        }

        /// <summary>
        ///     检查这个结点是否为叶节点
        /// </summary>
        internal bool IsLeaf()
        {
            return Level == 0;
        }

        private void MbrRemovalUpdate(Rectangle removed)
        {
            if (EntryCount == 0)
            {
                Mbr = null;
            }
            else if (Mbr.OverlapsWith(removed))
            {
                Mbr = Entries[0].Copy();
                for (var i = 1; i < EntryCount; i++)
                    Mbr.AddRectangle(Entries[i]);
            }
        }

        internal void recalculateMBR(Rectangle deletedRectangle)
        {
            if (Mbr.edgeOverlaps(deletedRectangle))
            {
                Mbr.set(Entries[0].MinBoundries, Entries[0].MaxBoundries);

                for (int i = 1; i < EntryCount; i++) Mbr.AddRectangle(Entries[i]);
            }
        }
    }
}