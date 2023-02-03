using System.Collections;

namespace SP.BPlusTreeCollection.Nodes
{
    public class Leaf<TKey, TValue> : INode<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
	{
		//List<TValue> Values = new List<TValue>();
		public List<TKey> Keys { get; private set; }
		public List<TValue> Values { get; private set; }
		public int Count { get { return Keys.Count; } }

		public string NodeType { get { return "leaf"; } }

		public Leaf(List<INode<TKey, TValue>> AllLeafNodes, int index = 0)
		{
			Values = new List<TValue>();
			Keys = new List<TKey>();
			AllLeafNodes.Insert(index, this);
		}
		public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> kvp, IComparer<TKey> comparer)
		{
			foreach (var k in kvp)
				Add(k.Key, k.Value, comparer);
		}
		public void AddRange(INode<TKey, TValue> node, IComparer<TKey> comparer)
		{
			AddRange(((Leaf<TKey, TValue>)node).AsEnumerable(), comparer);
		}
		public void AddFromLeft(INode<TKey, TValue> node, IComparer<TKey> comparer)
		{
			AddRange(((Leaf<TKey, TValue>)node).TakeLeft(), comparer);
		}
		public void AddFromRight(INode<TKey, TValue> node, IComparer<TKey> comparer)
		{
			AddRange(((Leaf<TKey, TValue>)node).TakeRight(), comparer);
		}

		public KeyValuePair<TKey, TValue>[] TakeLeft()
		{
			var count = Constants.TakeCount(this);
			var items = new KeyValuePair<TKey, TValue>[count];
			for (int i = 0; i < items.Length; i++)
				items[i] = new KeyValuePair<TKey, TValue>(Keys[i], Values[i]);
			Keys.RemoveRange(0, count);
			Values.RemoveRange(0, count);
			return items;
		}
		public KeyValuePair<TKey, TValue>[] TakeRight()
		{
			var count = Constants.TakeCount(this);
			var items = new KeyValuePair<TKey, TValue>[count];
			for (int i = 0; i < items.Length; i++)
			{
				var x = Keys.Count - count + i;
				items[i] = new KeyValuePair<TKey, TValue>(Keys[x], Values[x]);
			}
			Keys.RemoveRange(Keys.Count - count, count);
			Values.RemoveRange(Values.Count - count, count);
			return items;
		}

		public bool Add(TKey key, TValue value, IComparer<TKey> comparer)
		{
			var index = Keys.BinarySearch(key, comparer);
			if (index >= 0)
			{
				Keys[index] = key;
				Values[index] = value;
				return true;
			}

			if (Keys.Count == Constants.NodeSize) return false;
			index = ~index;

			Keys.Insert(index, key);
			Values.Insert(index, value);
			return true;
		}

		public INode<TKey, TValue> Split(List<INode<TKey, TValue>> AllLeafNodes)
		{
			var right = new Leaf<TKey, TValue>(AllLeafNodes, AllLeafNodes.IndexOf(this) + 1);
			var count = Constants.NodeSize / 2;
			right.Keys.AddRange(Keys.GetRange(count, count));
			Keys.RemoveRange(count, count);
			right.Values.AddRange(Values.GetRange(count, count));
			Values.RemoveRange(count, count);
			return right;
		}

		public bool Remove(TKey key, IComparer<TKey> comparer)
		{
			var index = Keys.BinarySearch(key, comparer);
			if (index < 0) return false;
			Keys.RemoveAt(index);
			Values.RemoveAt(index);
			return true;
		}

		public bool TryGetValue(TKey key, out TValue value, IComparer<TKey> comparer)
		{
			var index = Keys.BinarySearch(key, comparer);
			if (index >= 0)
			{
				value = Values[index];
				return true;
			}

			value = default(TValue);
			return false;
		}

		#region IEnumerable implementation


		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			for (int i = 0; i < Keys.Count; i++)
				yield return new KeyValuePair<TKey, TValue>(Keys[i], Values[i]);
		}


		#endregion


		#region IEnumerable implementation


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		#endregion
	}
}
