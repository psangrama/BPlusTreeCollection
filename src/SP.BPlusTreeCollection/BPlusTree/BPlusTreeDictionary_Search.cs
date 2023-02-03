using SP.BPlusTreeCollection.Nodes;

namespace SP.BPlusTreeCollection.BPlusTree
{
    public partial class BPlusTreeDictionary<TKey, TValue>
	{
		/// <summary>
		/// Get the first key of the BPlusTree 
		/// </summary>
		/// <returns>Key of first key in the BPlusTree</returns>
		public TKey First()
		{
			if (_allLeafNodes == null)
				return default(TKey);

			return _allLeafNodes.FirstOrDefault().Keys[0];
		}

		/// <summary>
		/// Get the last key of the BPlusTree
		/// </summary>
		/// <returns>Key of first key in the BPlusTree</returns>
		public TKey Last()
		{
			if (_allLeafNodes == null)
				return default(TKey);

			return _allLeafNodes.LastOrDefault().Keys[_allLeafNodes.LastOrDefault().Keys.Count - 1];
		}

		/// <summary>
		/// Returns the value associated to a specified key
		/// </summary>
		/// <param name="key">Key of the value to retrieve</param>
		/// <param name="value">Out parameter, returns the value of key</param>
		/// <returns>Returns true if key exists</returns>
		public bool TryGetValue(TKey key, out TValue value)
		{
			if (root == null)
			{
				value = default(TValue);
				return false;
			}

			return TryGetValue(key, out value, root);
		}

		bool TryGetValue(TKey key, out TValue value, INode<TKey, TValue> node)
		{
			var leaf = node as Leaf<TKey, TValue>;
			if (leaf != null)
				return leaf.TryGetValue(key, out value, comparer);
			var i = (Internal<TKey, TValue>)node;
			return TryGetValue(key, out value, i.GetNode(key, comparer).Node);
		}
	}
}
