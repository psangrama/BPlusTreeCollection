using SP.BPlusTreeCollection.Nodes;

namespace SP.BPlusTreeCollection.BPlusTree
{
    public partial class BPlusTreeDictionary<TKey, TValue>
	{
		/// <summary>
		/// Verify if the BPlusTree is accurate. Ensures, number of keys in a node should be less than the minimum node size
		/// </summary>
		public void Verify()
		{
			if (root == null) return;
			foreach (var node in this.Nodes)
				if (node != root && node.Keys.Count < Constants.MinNodeSize)
					throw new Exception("Verify failed");
		}

		/// <summary>
		/// Returns the list of Internal and Leaf nodes in the BPlusDictionary
		/// </summary>
		public IEnumerable<INode<TKey, TValue>> Nodes
		{
			get
			{
				if (root == null)
					return Enumerable.Empty<INode<TKey, TValue>>();
				return NodesOf(root);
			}
		}

		public List<INode<TKey, TValue>> LeafNodes
		{
			get
			{
				return _allLeafNodes;
			}
		}

		IEnumerable<INode<TKey, TValue>> NodesOf(INode<TKey, TValue> node)
		{
			var n = node as Leaf<TKey, TValue>;
			if (n != null)
				yield return n;
			else
			{
				yield return node;
				var i = ((Internal<TKey, TValue>)node);
				int j = 0;
				foreach (var x in i.Nodes)
				{
					if (comparer.Compare(x.Keys[0], i.Keys[j++]) != 0)
						throw new Exception("Mismatch " + x.Keys[0] + " " + i.Keys[j - 1]);
					foreach (var item in NodesOf(x))
						yield return item;
				}
			}
		}
	}
}
