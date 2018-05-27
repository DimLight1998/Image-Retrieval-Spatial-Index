using System.Diagnostics;

namespace SpatialIndex.RTree
{
    public class Node
    {
        internal readonly Rectangle[] Entries;
        internal readonly int[] Ids;

        internal Node(int id, int level, int maxEntries)
        {
            Debug.Assert(id >= 0);
            Id = id;
            Level = level;

            Entries = new Rectangle[maxEntries];
            Ids = new int[maxEntries];
            EntryCount = 0;
        }

        internal int Id { get; }
        internal int Level { get; }
        internal Rectangle Mbr { get; private set; }
        internal int EntryCount { get; set; }

        internal void AddEntry(Rectangle rectangle, int id)
        {
            Debug.Assert(EntryCount < Entries.Length);

            Entries[EntryCount] = rectangle.Copy();
            Ids[EntryCount] = id;
            EntryCount++;

            if (Mbr == null)
                Mbr = rectangle.Copy();
            else
                Mbr.AddRectangle(rectangle);
        }

        internal int LocateEntry(Rectangle rectangle, int id)
        {
            for (var i = 0; i < EntryCount; i++)
                if (rectangle.ContentEquals(Entries[i]) && id == Ids[i])
                    return i;
            return -1;
        }

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

        internal Rectangle GetEntryAt(int i)
        {
            return Entries[i];
        }

        internal int GetIdAt(int i)
        {
            return Ids[i];
        }

        internal void Shrink()
        {
            var lastNotEmpty = Entries.Length - 1;
            for (var i = 0; i < EntryCount; i++)
            {
                if (Entries[i] != null) continue;
                while (Entries[lastNotEmpty] == null && lastNotEmpty > i) lastNotEmpty--;
                Entries[i] = Entries[lastNotEmpty];
                Ids[i] = Ids[lastNotEmpty];
                Entries[lastNotEmpty] = null;
            }
        }

        internal bool IsLeaf()
        {
            return Level == 1;
        }

        internal void MbrRemovalUpdate(Rectangle removed)
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
    }
}