using Microsoft.VisualStudio.TestTools.UnitTesting;
using SP.BPlusTreeCollection.BPlusTree;
using SP.BPlusTreeCollection.Nodes;
using SP.BPlusTreeCollection.Test.Common;
using SP.BPlusTreeCollection.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.BPlusTreeCollection.Test.BPlusTree
{
	[TestClass()]
	public class BPlusTreeDictionaryTests
	{
		[TestMethod()]
		public void BPlusTreeDictionaryInteger_Basic()
		{
			// should run setting degree to 4
			BPlusTreeDictionary<int, string> _intbpTreeDictionary = new BPlusTreeDictionary<int, string>();
			List<int> _items = new List<int>() { 7, 8, 4, 9, 2, 5, 3, 1, 12, 14, 16 };

			for (int i = 0; i < _items.Count; i++)
			{
				_intbpTreeDictionary.Add(_items[i], $"{_items[i]}v");
			}

			Assert.AreEqual(1, 1);
		}

		[TestMethod()]
		public void BPlusTreeDictionaryInteger_LeafNodeValidation()
		{
			BPlusTreeDictionary<int, string> _intbpTreeDictionary = new BPlusTreeDictionary<int, string>();
			List<int> _items = Enumerable.Range(0, 10000).ToList();
			_items.Shuffle();

			foreach (var item in _items)
				_intbpTreeDictionary.Add(item, $"{item}v");

			var _leafNodes = _intbpTreeDictionary.Nodes.Where(x => x.GetType().FullName.Contains(typeof(Leaf<,>).FullName)).ToList();
			var _internalNodes = _intbpTreeDictionary.Nodes.Where(x => x.GetType().FullName.Contains(typeof(Internal<,>).FullName)).ToList();

			Assert.AreEqual(17, _intbpTreeDictionary.Nodes.Count()); // Total Nodes, considering max count set to 1024, total items set to 10k
			Assert.AreEqual(16, _leafNodes.Count); // Total Leaf Nodes, considering max count set to 1024, total items set to 10k
			Assert.AreEqual(1, _internalNodes.Count); // Total Internal Nodes, considering max count set to 1024, total items set to 10k
		}

		[TestMethod()]
		public void PerfectBPlusTree_AllKeysMustPresentinLeafNodes_Validation()
		{
			BPlusTreeDictionary<int, string> _intbpTreeDictionary = new BPlusTreeDictionary<int, string>();
			List<int> _keyValuePair = Enumerable.Range(0, 10000).ToList();
			_keyValuePair.Shuffle();

			foreach (var key in _keyValuePair)
				_intbpTreeDictionary[key] = $"{key}v";

			var _leafNodes = _intbpTreeDictionary.Nodes.Where(x => x.GetType().FullName.Contains(typeof(Leaf<,>).FullName)).ToList();

			// Ensure all keys are in leaf nodes
			var _keyNotFoundInLeafNodes = false;

			foreach (var key in _keyValuePair)
			{
				for (int i = 0; i < _leafNodes.Count; i++)
				{
					var _keyAlreadyFoundInPrevNodes = false;
					var hasKey = _leafNodes[i].Keys.BinarySearch(key);
					if (hasKey < 0 && i == _leafNodes.Count - 1 && _keyAlreadyFoundInPrevNodes)
					{
						_keyNotFoundInLeafNodes = true;
						break;
					}
					else if (hasKey >= 0)
					{
						_keyAlreadyFoundInPrevNodes = true;
					}
				}
			}

			Assert.AreEqual(false, _keyNotFoundInLeafNodes);  // Ensure all keys present in leaf nodes (First condition of BPulsTree
		}

		[TestMethod()]
		public void BPlusTreeDictionaryInteger_AddRemoveSearhCheck()
		{
			int _startRange = 0;
			int _endRange = 10000;
			BPlusTreeDictionary<int, string> _bpTreeInt = new BPlusTreeDictionary<int, string>();
			List<int> _items = Enumerable.Range(_startRange, _endRange).ToList();
			_items.Shuffle();

			foreach (var item in _items)
				_bpTreeInt.Add(item, $"{item}v");

			// Check if the total item count is 50
			Assert.AreEqual(_bpTreeInt.Count, _endRange);

			// Search 49th item, value should be 49
			var searchResult = _bpTreeInt.TryGetValue(49, out var value);
			Assert.AreEqual(true, searchResult);
			Assert.AreEqual("49v", value);

			// Remove 49th Item
			_bpTreeInt.Remove(49);
			Assert.AreEqual(false, _bpTreeInt.TryGetValue(49, out value));
			Assert.AreEqual(null, value);

			Assert.AreEqual(false, _bpTreeInt.ContainsKey(49)); // Keys already removed, so should return false
			Assert.AreEqual(true, _bpTreeInt.ContainsKey(48));  // Keys present, hence should return true

			Assert.AreEqual(true, _bpTreeInt.Contains(new KeyValuePair<int, string>(_items[48], $"{_items[48]}v"))); // Keys present, hence should return true
			Assert.AreEqual(false, _bpTreeInt.Contains(new KeyValuePair<int, string>(49, $"49v"))); // Keys already removed, so should return false

			Assert.AreEqual(true, _bpTreeInt.Remove(new KeyValuePair<int, string>(48, $"48v"))); // Keys already removed, so should return false
		}

		[TestMethod()]
		public void AddToBPlusTreeDictionaryIntegerDuplicateKeys()
		{
			BPlusTreeDictionary<int, int> _bpTreeinteger = new BPlusTreeDictionary<int, int>();
			List<int> _items = new List<int>() { 5, 5, 5, 5, 5 };
			foreach (var item in _items)
				_bpTreeinteger.Add(item, item);

			Assert.AreEqual(1, _bpTreeinteger.Count);
		}

		[TestMethod()]
		public void RetrieveValuefromanEmptyBPlusTreeDictionary()
		{
			BPlusTreeDictionary<int, int> _bpTreeinteger = new BPlusTreeDictionary<int, int>();
			_bpTreeinteger.TryGetValue(5, out var outVal);

			Assert.AreEqual(0, outVal);
		}

		[TestMethod()]
		public void AddToBPlusTreeDictionaryDouble_VerifyCount()
		{
			BPlusTreeDictionary<double, List<double>> _bpTreeDouble = new BPlusTreeDictionary<double, List<double>>();
			List<int> items = Enumerable.Range(0, 50).ToList();
			items.Shuffle();

			foreach (var item in items)
				_bpTreeDouble.Add(item, new List<double> { (double)item });

			Assert.AreEqual(50, _bpTreeDouble.Count);
		}

		[TestMethod()]
		public void AddToBPlusTreeDictionaryDate_VerifyCount()
		{
			DateTime _startDate = new DateTime(1984, 07, 20);
			DateTime _endDate = new DateTime(2020, 07, 21);

			var _dateList = Enumerable.Range(0, (_endDate - _startDate).Days + 1).Select(day => _startDate.AddDays(day)).ToList();
			_dateList.Shuffle();

			BPlusTreeDictionary<DateTime, List<DateTime>> _bpTreeDate = new BPlusTreeDictionary<DateTime, List<DateTime>>();

			foreach (var date in _dateList)
				_bpTreeDate.Add(date, new List<DateTime> { date });

			Assert.AreEqual(13151, _bpTreeDate.Count);
		}

		[TestMethod()]
		public void AddToBPlusTreeDictionaryDouble_VerifyFirstandLast()
		{
			int _rangeStart = 0;
			int _rangeEnd = 500000;
			BPlusTreeDictionary<double, List<double>> _bpTreeDouble = new BPlusTreeDictionary<double, List<double>>();
			List<int> _items = Enumerable.Range(_rangeStart, _rangeEnd).ToList();
			_items.Shuffle();

			// Add items to tree
			foreach (var item in _items)
				_bpTreeDouble.Add(item, new List<double> { (double)item });

			Assert.AreEqual(_bpTreeDouble.First(), _rangeStart);
			Assert.AreEqual(_bpTreeDouble.Last(), _rangeEnd - 1);
		}

		[TestMethod]
		public void VerfiyCorrectness_RangeSearch_SingleNode()
		{
			int _rangeStart = 0;
			int _rangeEnd = 1000; // This will create a single leaf
			BPlusTreeDictionary<double, List<int>> _bpTreeDouble = new BPlusTreeDictionary<double, List<int>>();
			List<int> items = Enumerable.Range(_rangeStart, _rangeEnd).ToList();
			items.Shuffle();

			// Add items to tree
			foreach (var item in items)
				_bpTreeDouble.Add(item, new List<int> { item });

			// Object of IndexedRangeSearch
			BPlusTreeIndexedRangeSearch<int> _bplusTreeIndexedRangeSearch = new BPlusTreeIndexedRangeSearch<int>();

			// Range search result comparison
			Assert.AreEqual(6, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 65, 70).Count); // smaller numbers
			Assert.AreEqual(401, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 300, 700).Count); // medium numbers
			Assert.AreEqual(50, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 950, 1000).Count); //larger numbers (This should 51-1 = 50), as the max value would be 999
			Assert.AreEqual(1, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 999, 1005).Count); // One item within range, rest out of range
			Assert.AreEqual(0, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 1001, 1005).Count); // None of the items in the range
			Assert.AreEqual(0, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, -999, -1).Count); // None of the items in the range
			Assert.AreEqual(_bpTreeDouble.Count, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, -9999, 1000000).Count); // outside range

			Assert.AreEqual(_bpTreeDouble.First(), _rangeStart);
			Assert.AreEqual(_bpTreeDouble.Last(), _rangeEnd - 1); // Last value would be the RangeEnd - 1
		}

		[TestMethod]
		public void VerfiyCorrectness_RangeSearch_MultipleNode()
		{
			int _rangeStart = 0;
			int _rangeEnd = 10000; //This will create atleast 15 leaf nodes
			BPlusTreeDictionary<double, List<int>> _bpTreeDouble = new BPlusTreeDictionary<double, List<int>>();
			List<int> _items = Enumerable.Range(_rangeStart, _rangeEnd).ToList();
			_items.Shuffle();

			// Add items to tree
			foreach (var item in _items)
				_bpTreeDouble.Add(item, new List<int> { item });

			// Object of IndexedRangeSearch
			BPlusTreeIndexedRangeSearch<int> _bplusTreeIndexedRangeSearch = new BPlusTreeIndexedRangeSearch<int>();

			// Range search result comparison
			Assert.AreEqual(6, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 65, 70).Count); // smaller numbers
			Assert.AreEqual(401, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 300, 700).Count); // medium numbers
			Assert.AreEqual(51, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 950, 1000).Count);
			Assert.AreEqual(1551, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 950, 2500).Count);
			Assert.AreEqual(9050, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 950, 9999).Count);
			Assert.AreEqual(3, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 9500, 9502).Count);
			Assert.AreEqual(0, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 9503, 9502).Count);
			Assert.AreEqual(3, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 6797, 6799).Count);
			Assert.AreEqual(1, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 9999, 10005).Count); // One item within range, rest out of range
			Assert.AreEqual(0, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, 10001, 10005).Count); // None of the items in the range
			Assert.AreEqual(0, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, -9999, -1).Count); // None of the items in the range
			Assert.AreEqual(_bpTreeDouble.Count, _bplusTreeIndexedRangeSearch.RangeofKeys(_bpTreeDouble, -9999, 1000000).Count); // outside range 

			Assert.AreEqual(_bpTreeDouble.First(), _rangeStart);
			Assert.AreEqual(_bpTreeDouble.Last(), _rangeEnd - 1); // Last value would be the RangeEnd - 1
		}

		[TestMethod]
		public void VerfiyCorrectness_LeafNodes_MultipleNodes_OnRemovalItems()
		{
			int _rangeStart = 0;
			int _rangeEnd = 10000; //This will create atleast 15 leaf nodes
			BPlusTreeDictionary<double, List<int>> _bpTreeDouble = new BPlusTreeDictionary<double, List<int>>();
			List<int> _items = Enumerable.Range(_rangeStart, _rangeEnd).ToList();
			_items.Shuffle();

			// Add items to tree
			foreach (var item in _items)
				_bpTreeDouble.Add(item, new List<int> { item });

			// Shuffle the list and remove 5000 items from the collection - number of leaf nodes will reduce to around 9
			_items.Shuffle();
			for (int i = 0; i < 5000; i++)
			{
				_bpTreeDouble.Remove(_items[i]);
			}

			Assert.AreEqual(_bpTreeDouble.LeafNodes.Count(), _bpTreeDouble.Nodes.Where(x => x.NodeType == "leaf").ToList().Count());
			Assert.AreEqual(_bpTreeDouble.Nodes.Where(x => x.GetType().FullName.Contains(typeof(Leaf<,>).FullName)).Count(), _bpTreeDouble.Nodes.Where(x => x.NodeType == "leaf").ToList().Count());
		}

		[TestMethod()]
		public void BPlusTreeDictionary_Validate_CountAndClear()
		{
			BPlusTreeDictionary<int, string> _intbpTreeDictionary = new BPlusTreeDictionary<int, string>();
			List<int> _items = Enumerable.Range(0, 10000).ToList();
			_items.Shuffle();

			foreach (var item in _items)
				_intbpTreeDictionary.Add(item, $"{item}v");

			// Validation of leaf and internal nodes using enumeration
			var _leafNodes = _intbpTreeDictionary.Nodes.Where(x => x.GetType().FullName.Contains(typeof(Leaf<,>).FullName)).ToList();
			var _internalNodes = _intbpTreeDictionary.Nodes.Where(x => x.GetType().FullName.Contains(typeof(Internal<,>).FullName)).ToList();
			Assert.AreEqual(17, _intbpTreeDictionary.Nodes.Count()); // Total Nodes, considering max count set to 1024, total items set to 10k
			Assert.AreEqual(16, _leafNodes.Count); // Total Leaf Nodes, considering max count set to 1024, total items set to 10k
			Assert.AreEqual(1, _internalNodes.Count); // Total Internal Nodes, considering max count set to 1024, total items set to 10k
			Assert.AreEqual(_intbpTreeDictionary.LeafNodes.Count, _leafNodes.Count); // Validataion of leaf nodes using LeafNode property

			// Clear the collection
			_intbpTreeDictionary.Clear();

			_leafNodes = _intbpTreeDictionary.Nodes.Where(x => x.GetType().FullName.Contains(typeof(Leaf<,>).FullName)).ToList();
			_internalNodes = _intbpTreeDictionary.Nodes.Where(x => x.GetType().FullName.Contains(typeof(Internal<,>).FullName)).ToList();

			Assert.AreEqual(0, _intbpTreeDictionary.Nodes.Count()); // Count should be zero as collection already cleared
			Assert.AreEqual(0, _leafNodes.Count); // Count should be zero as collection already cleared
			Assert.AreEqual(0, _internalNodes.Count); // Count should be zero as collection already cleared

			Assert.AreEqual(0, _intbpTreeDictionary.First()); // As the collection is cleared, it should return default
			Assert.AreEqual(0, _intbpTreeDictionary.Last()); // As the collection is cleared, it should return default
		}
		#region Multi-level tree (depth 3+)

		// A root internal node holds up to Constants.NodeSize children, so a tree only
		// grows past two levels at roughly 1.5M keys. Every test below 1024 leaves
		// therefore never exercises internal-node splitting, merging or borrowing.
		private const int _multiLevelItemCount = 1_500_000;

		[TestMethod]
		public void MultiLevelTree_LoadRemoveAndVerify_KeepsTreeConsistent()
		{
			var _tree = new BPlusTreeDictionary<int, int>();
			var _keys = Enumerable.Range(0, _multiLevelItemCount).ToList();
			_keys.Shuffle();

			foreach (var _key in _keys)
				_tree.Add(_key, _key);

			// Guard the premise of this test: the tree must actually be more than two levels deep.
			var _internalNodes = _tree.Nodes.Count(x => x.NodeType != "leaf");
			Assert.IsTrue(_internalNodes > 1, $"Expected a multi-level tree, but found {_internalNodes} internal node(s).");
			Assert.AreEqual(_multiLevelItemCount, _tree.Count);

			_tree.Verify();

			foreach (var _key in _keys)
			{
				Assert.IsTrue(_tree.TryGetValue(_key, out var _value), $"Key {_key} missing after load.");
				Assert.AreEqual(_key, _value);
			}

			// Remove 60% of the keys to force merges and borrows at the internal level.
			var _removeCount = (int)(_multiLevelItemCount * 0.6);
			for (int i = 0; i < _removeCount; i++)
				_tree.Remove(_keys[i]);

			Assert.AreEqual(_multiLevelItemCount - _removeCount, _tree.Count);

			_tree.Verify();

			for (int i = 0; i < _removeCount; i++)
				Assert.IsFalse(_tree.TryGetValue(_keys[i], out _), $"Key {_keys[i]} still present after removal.");

			for (int i = _removeCount; i < _multiLevelItemCount; i++)
				Assert.IsTrue(_tree.TryGetValue(_keys[i], out _), $"Key {_keys[i]} lost during removal of other keys.");
		}

		[TestMethod]
		public void MultiLevelTree_LeafNodesRemainInKeyOrder()
		{
			var _tree = new BPlusTreeDictionary<int, int>();
			var _keys = Enumerable.Range(0, _multiLevelItemCount).ToList();
			_keys.Shuffle();

			foreach (var _key in _keys)
				_tree.Add(_key, _key);

			AssertLeavesAreOrdered(_tree);

			_keys.Shuffle();
			for (int i = 0; i < _multiLevelItemCount / 2; i++)
				_tree.Remove(_keys[i]);

			AssertLeavesAreOrdered(_tree);
		}

		private static void AssertLeavesAreOrdered(BPlusTreeDictionary<int, int> tree)
		{
			var _leaves = tree.LeafNodes;
			for (int i = 1; i < _leaves.Count; i++)
			{
				if (_leaves[i - 1].Count == 0 || _leaves[i].Count == 0)
					continue;

				var _previousLast = _leaves[i - 1].Keys[_leaves[i - 1].Count - 1];
				var _currentFirst = _leaves[i].Keys[0];
				Assert.IsTrue(_previousLast < _currentFirst,
					$"Leaf {i - 1} ends at {_previousLast} but leaf {i} starts at {_currentFirst}.");
			}
		}

		#endregion

		#region IDictionary contract

		[TestMethod]
		public void Keys_And_Values_AreDeliberatelyNotImplemented()
		{
			var _tree = new BPlusTreeDictionary<int, int>();
			_tree.Add(1, 1);

			Assert.Throws<NotImplementedException>(() => _ = _tree.Keys);
			Assert.Throws<NotImplementedException>(() => _ = _tree.Values);
		}

		[TestMethod]
		public void Indexer_Get_ReturnsValue_AndDefaultWhenKeyIsAbsent()
		{
			var _tree = new BPlusTreeDictionary<int, string>();
			_tree[7] = "seven"; // setter routes to Add

			Assert.AreEqual("seven", _tree[7]);
			Assert.IsNull(_tree[42]); // absent key yields default, it does not throw
		}

		[TestMethod]
		public void AddKeyValuePair_And_Contains_And_RemoveKeyValuePair()
		{
			var _tree = new BPlusTreeDictionary<int, string>();
			_tree.Add(new KeyValuePair<int, string>(1, "one"));
			_tree.Add(new KeyValuePair<int, string>(2, "two"));

			Assert.IsTrue(_tree.Contains(new KeyValuePair<int, string>(1, "one")));
			Assert.IsFalse(_tree.Contains(new KeyValuePair<int, string>(3, "three")));
			Assert.IsFalse(_tree.IsReadOnly);

			Assert.IsTrue(_tree.Remove(new KeyValuePair<int, string>(1, "one")));
			Assert.IsFalse(_tree.ContainsKey(1));
			Assert.IsTrue(_tree.ContainsKey(2));
		}

		[TestMethod]
		public void CopyTo_WritesPairsIntoTargetArrayAtOffset()
		{
			var _tree = new BPlusTreeDictionary<int, string>();
			for (int i = 0; i < 5; i++)
				_tree.Add(i, $"{i}v");

			var _target = new KeyValuePair<int, string>[7];
			_tree.CopyTo(_target, 2);

			Assert.AreEqual(default, _target[0]);
			Assert.AreEqual(default, _target[1]);
			for (int i = 0; i < 5; i++)
				Assert.AreEqual(new KeyValuePair<int, string>(i, $"{i}v"), _target[i + 2]);
		}

		[TestMethod]
		public void Enumerator_YieldsEveryPairInKeyOrder()
		{
			var _tree = new BPlusTreeDictionary<int, int>();
			var _keys = Enumerable.Range(0, 5000).ToList();
			_keys.Shuffle();

			foreach (var _key in _keys)
				_tree.Add(_key, _key * 2);

			var _enumerated = _tree.ToList(); // exercises the generic enumerator
			Assert.AreEqual(5000, _enumerated.Count);
			for (int i = 0; i < 5000; i++)
			{
				Assert.AreEqual(i, _enumerated[i].Key);
				Assert.AreEqual(i * 2, _enumerated[i].Value);
			}

			// non-generic IEnumerable path
			var _nonGenericCount = 0;
			foreach (var _ in (System.Collections.IEnumerable)_tree)
				_nonGenericCount++;
			Assert.AreEqual(5000, _nonGenericCount);
		}

		[TestMethod]
		public void RemoveLastKey_FromRootLeaf_EmptiesTheTree()
		{
			var _tree = new BPlusTreeDictionary<int, int>();
			_tree.Add(1, 1);
			_tree.Add(2, 2);

			_tree.Remove(1);
			_tree.Remove(2);

			Assert.AreEqual(0, _tree.Count);
			Assert.IsFalse(_tree.TryGetValue(1, out _));
			Assert.IsFalse(_tree.TryGetValue(2, out _));
		}

		#endregion
	}
}
