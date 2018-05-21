using System;

namespace SpatialIndex.RTree
{
    /// <summary>
    /// RTree 中的一个结点
    /// </summary>
    public class Node
    {
        /// <summary>
        /// 构造一个指定的结点
        /// </summary>
        /// <param name="id">结点的 id</param>
        /// <param name="level">结点的深度（叶节点深度为 0）</param>
        /// <param name="maxEntries">结点具有子树容量</param>
        /// <exception cref="NotImplementedException"></exception>
        public Node(int id, int level, int maxEntries)
        {
            throw new NotImplementedException();
        }
    }
}