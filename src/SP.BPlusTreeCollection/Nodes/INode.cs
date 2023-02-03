using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.BPlusTreeCollection.Nodes
{
	public interface INode<TKey, TValue>
	{
		List<TKey> Keys { get; }
		INode<TKey, TValue> Split(List<INode<TKey, TValue>> AllLeafNodes);
		int Count { get; }
		void AddRange(INode<TKey, TValue> node, IComparer<TKey> comparer);
		void AddFromLeft(INode<TKey, TValue> node, IComparer<TKey> comparer);
		void AddFromRight(INode<TKey, TValue> node, IComparer<TKey> comparer);
		string NodeType { get; }
	}
}
