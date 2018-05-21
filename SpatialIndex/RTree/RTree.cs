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
        /// <summary>
        ///     构造一棵空 RTree
        /// </summary>
        /// <param name="maxEntries">每个结点的子树上限</param>
        /// <param name="minEntries">每个结点的子树下限</param>
        /// <exception cref="NotImplementedException"></exception>
        public RTree(int maxEntries, int minEntries)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        ///     从 R 树中移除指定的记录
        /// </summary>
        /// <param name="rectangle">移除记录对应的矩形</param>
        /// <param name="item">移除记录的元素</param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveRecord(Rectangle rectangle, TItem item)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        ///     获得与指定矩形有交集的所有元素
        /// </summary>
        /// <param name="rectangle">要查询的矩形</param>
        /// <returns>与 <c>rectangle</c> 有交集的所有元素的列表</returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<TItem> GetIntersectedItems(Rectangle rectangle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     获得位于指定矩形内部的所有元素
        /// </summary>
        /// <param name="rectangle">要查询的矩形</param>
        /// <returns>处于 <c>rectangle</c> 内部的所有元素的列表</returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<TItem> GetContainedItems(Rectangle rectangle)
        {
            throw new NotImplementedException();
        }
    }
}