namespace SP.BPlusTreeCollection.BPlusTree
{
    public partial class BPlusTreeDictionary<TKey, TValue>
	{
		public ICollection<TKey> Keys
		{
			get { throw new NotImplementedException("Not implemented as might cause performance issue. "); }
		}

		public ICollection<TValue> Values
		{
			get { throw new NotImplementedException("Not implemented as might cause performance issue. "); }
		}

		public bool ContainsKey(TKey key)
		{
			TValue value;
			return TryGetValue(key, out value);
		}

		// todo - This has to be revisited
		/// <summary>
		/// Retrieves the value associated with the specified key.
		/// </summary>		
		public TValue this[TKey key]
		{
			get
			{
				TValue value;
				TryGetValue(key, out value);
				return value;
			}
			set
			{
				Add(key, value);
			}
		}

		/// <summary>
		/// Returns the number of Key, Value pairs present in this BPlusTree dictionary
		/// </summary>
		public int Count
		{
			get
			{
				int iCount = 0;
				if (this._allLeafNodes == null)
					return 0;
				_allLeafNodes.ForEach(leafNode => { iCount += leafNode.Count; });
				return iCount;
			}
			//get; private set; ////TODO - Set it on Add and Remove
		}

		/// <summary>
		/// Add a KeyValuePair to the BPlusDictionary  
		/// </summary>
		/// <param name="item">Generic KeyValuePair</param>
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			this.Add(item.Key, item.Value);
		}

		/// <summary>
		/// Clears the current BPlusDictionary 
		/// </summary>
		public void Clear()
		{
			_allLeafNodes = null;
			root = null;
		}

		/// <summary>
		/// Determines whether a BPTreeDictionary containst an element of the KeyValuePair
		/// </summary>
		/// <param name="item">KeyValuePair to locate in the BPTreeDictionary</param>
		/// <returns></returns>
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return ContainsKey(item.Key);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			foreach (var item in this)
				array[arrayIndex++] = item;
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Removes the element of the KeyValuePair in the BPTreeDictionary
		/// </summary>
		/// <param name="item">KeyValuePair to delete from the BPTreeDictionary</param>
		/// <returns>Returns true if deleted else false</returns>
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return ((IDictionary<TKey, TValue>)this).Remove(item.Key);
		}

		bool IDictionary<TKey, TValue>.Remove(TKey key)
		{
			bool contains = ContainsKey(key);
			Remove(key);
			return contains;
		}
	}
}
