using SP.BPlusTreeCollection.Nodes;
using System.Collections;

namespace SP.BPlusTreeCollection.BPlusTree
{
    public partial class BPlusTreeDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		internal List<INode<TKey, TValue>> _allLeafNodes = new List<INode<TKey, TValue>>();

		/// <summary>
		/// Initializes a new instance of BPlusTreeDictionary with default IComparer
		/// </summary>
		/// <param name="comparer">Instance of IComparer. Default set to null</param>
		public BPlusTreeDictionary(IComparer<TKey> comparer = null)
		{
			this.comparer = this.comparer ?? Comparer<TKey>.Default;
		}
		IComparer<TKey> comparer;
		INode<TKey, TValue> root;

		IEnumerable<KeyValuePair<TKey, TValue>> NodeItems(INode<TKey, TValue> node)
		{
			var n = node as Leaf<TKey, TValue>;
			if (n != null)
				foreach (var item in n)
					yield return item;
			else
				foreach (var x in (Internal<TKey, TValue>)node)
					foreach (var item in NodeItems(x))
						yield return item;
		}

		#region IEnumerable implementation
		/// <summary>
		/// This enumerates and returns the IEnumerable of KeyValuePair. This is EXTREMELY SLOW in terms of performance. 
		/// It's not recommended to use. 
		/// </summary>
		/// <returns>IEnumerator<KeyValuePair<TKey, TValue>></returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			if (root == null) yield break;
			foreach (var item in NodeItems(root))
				yield return item;
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
