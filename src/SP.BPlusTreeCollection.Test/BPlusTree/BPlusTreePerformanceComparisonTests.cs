using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SP.BPlusTreeCollection.BPlusTree;
using SP.BPlusTreeCollection.Test.Common;
using SP.BPlusTreeCollection.Test.Extensions;
using SP.BPlusTreeCollection.Test.TestData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.BPlusTreeCollection.Test.BPlusTree
{
	[TestClass]
	public class BPlusTreePerformanceComparisonTests
	{
		// Initiate logger
		private static readonly ILog _log = LogManager.GetLogger("PerfStats");

		private static List<KeyValuePair<double, int>> _testData = new List<KeyValuePair<double, int>>();
		private static List<KeyValuePair<double, int>> _testDataClonedShuffled = new List<KeyValuePair<double, int>>();

		private static DecimalDataFactory _decimalDataFactory;

		// Required constants
		private const int _totalRows = 450000;
		private const int _totalDuplicates = 4000;
		private const int _totalNulls = 2000;
		private const int _minValue = 0;
		private const int _maxValue = 72;

		private const double _fixedDuplicateValue = 39.2373600900;
		private const int _fixedDuplicateInstances = 877;
		private const int _exactSearchIterations = 100000;

		private const double _range = 0.05;
		private const int _rangeSearchCount = 500;

		#region Private Variables
		// SortedList
		private static SortedList<double, List<int>> _sortedList = new SortedList<double, List<int>>();
		private TimeSpan _sortedListLoadTime = new TimeSpan();
		private long _memUtilizedSortedList = 0;
		private TimeSpan _sortedListReadTime = new TimeSpan();
		private TimeSpan _sortedListRemoveTime = new TimeSpan();

		// SortedDictionary
		private static SortedDictionary<double, List<int>> _sortedDictionary = new SortedDictionary<double, List<int>>();
		private TimeSpan _sortedDictionaryLoadTime = new TimeSpan();
		private long _memUtilizedSortedDictionary = 0;
		private TimeSpan _sortedDictionaryReadTime = new TimeSpan();
		private TimeSpan _sortedDictionaryRemoveTime = new TimeSpan();

		//  BPlusTree
		private static BPlusTreeDictionary<double, List<int>> _bplusTreeDictionary = new BPlusTreeDictionary<double, List<int>>();
		private TimeSpan _bplusTreeDictionaryLoadTime = new TimeSpan();
		private long _memUtilizedBPlusTreeDictionary = 0;
		private TimeSpan _bplusTreeDictionaryReadTime = new TimeSpan();
		private TimeSpan _bplusTreeDictionaryRemoveTime = new TimeSpan();

		// Search - Timings
		private TimeSpan _bplusTreeDictRangeSearchTime = new TimeSpan();
		private TimeSpan _sortedListRangeSearchTime = new TimeSpan();
		private TimeSpan _sortedDictRangeSearchTime = new TimeSpan();

		private Random _rnd = new Random();

		#endregion

		[ClassInitialize]
		public static void Initialize(TestContext tc)
		{
			SetupTestData();
		}

		[ClassCleanup()]
		public static void CleanupTestSuite()
		{
			_testData = null;
			_testDataClonedShuffled = null;
			_sortedDictionary = null;
			_sortedList = null;
			_bplusTreeDictionary = null;
			_log.Info("TestCleanup completed!\n");
		}

		#region Load Performance

		[TestMethod()]
		public void LoadPerformance_SortedList()
		{
			// Collect memory after GC ran
			var gc = GC.GetTotalMemory(true);
			// create a local sortedlist
			var _sortedListLocal = new SortedList<double, List<int>>();

			var stopWatch = Stopwatch.StartNew();

			// Adde items to SortedList
			foreach (var item in _testData)
			{
				double theValue = Convert.ToDouble(item.Key);
				List<int> entry;
				if (!_sortedListLocal.TryGetValue(theValue, out entry))
				{
					entry = new List<int>();
					_sortedListLocal.Add(theValue, entry);
				}
				entry.Add(item.Value);
			}

			_sortedListLoadTime = stopWatch.Elapsed;
			//var _x = _sortedListLocal.Where(x => x.Key == 39.2373600900);

			// Memory usage 
			// Keep the collection rooted: without this the JIT can treat it as dead
			// before the sample is taken, and the measured delta collapses to noise.
			_memUtilizedSortedList = GC.GetTotalMemory(true) - gc;
			GC.KeepAlive(_sortedListLocal);

			Console.WriteLine($"SortedList took {_sortedListLoadTime} to insert {_totalRows} records to memory and occupied memory: {(_memUtilizedSortedList / 1048576.0):#,#.##} MB");
			_log.Info($"SortedList took {_sortedListLoadTime} to insert {_totalRows} records to memory and occupied memory: {(_memUtilizedSortedList / 1048576.0):#,#.##} MB");
		}

		[TestMethod()]
		public void LoadPerformance_SortedDictionary()
		{
			// Collect memory after GC ran
			var gc = GC.GetTotalMemory(true);
			// create a local sorteddictionary
			var _sortedDictLocal = new SortedDictionary<double, List<int>>();

			var stopWatch = Stopwatch.StartNew();

			// Add items to SortedDictionary
			foreach (var item in _testData)
			{
				double theValue = Convert.ToDouble(item.Key);
				List<int> entry;
				if (!_sortedDictLocal.TryGetValue(theValue, out entry))
				{
					entry = new List<int>();
					_sortedDictLocal.Add(theValue, entry);
				}
				entry.Add(item.Value);
			}

			_sortedDictionaryLoadTime = stopWatch.Elapsed;

			// Memory usage 
			// Keep the collection rooted: without this the JIT can treat it as dead
			// before the sample is taken, and the measured delta collapses to noise.
			_memUtilizedSortedDictionary = GC.GetTotalMemory(true) - gc;
			GC.KeepAlive(_sortedDictLocal);

			Console.WriteLine($"SortedDictionary took {_sortedDictionaryLoadTime} to insert {_totalRows} records to memory and occupied memory: {(_memUtilizedSortedDictionary / 1048576.0):#,#.##} MB");
			_log.Info($"SortedDictionary took {_sortedDictionaryLoadTime} to insert {_totalRows} records to memory and occupied memory: {(_memUtilizedSortedDictionary / 1048576.0):#,#.##} MB");
		}

		[TestMethod()]
		public void LoadPerformance_BPlusTreeDictionary()
		{
			// Collect memory after GC ran
			var gc = GC.GetTotalMemory(true);
			// create a local bplusTree
			var _bplusTreeDictLocal = new BPlusTreeDictionary<double, List<int>>();

			var stopWatch = Stopwatch.StartNew();

			// Add items to SortedDictionary
			foreach (var item in _testData)
			{
				double theValue = Convert.ToDouble(item.Key);
				List<int> entry;
				if (!_bplusTreeDictLocal.TryGetValue(theValue, out entry))
				{
					entry = new List<int>();
					_bplusTreeDictLocal.Add(theValue, entry);
				}
				entry.Add(item.Value);
			}

			_bplusTreeDictionaryLoadTime = stopWatch.Elapsed;

			// Memory usage 
			// Keep the collection rooted: without this the JIT can treat it as dead
			// before the sample is taken, and the measured delta collapses to noise.
			_memUtilizedBPlusTreeDictionary = GC.GetTotalMemory(true) - gc;
			GC.KeepAlive(_bplusTreeDictLocal);

			Console.WriteLine($"BPlusTreeDictionary took {_bplusTreeDictionaryLoadTime} to insert {_totalRows} records to memory and occupied memory: {(_memUtilizedBPlusTreeDictionary / 1048576.0):#,#.##} MB");
			_log.Info($"BPlusTreeDictionary took {_bplusTreeDictionaryLoadTime} to insert {_totalRows} records to memory and occupied memory: {(_memUtilizedBPlusTreeDictionary / 1048576.0):#,#.##} MB");
		}

		#endregion

		#region ExactFind Performance (Single Search)

		[TestMethod()]
		public void ExactFindFromShuffled_SortedList()
		{
			var stopWatch = Stopwatch.StartNew();

			for (int i = 0; i < _exactSearchIterations; i++)
			{
				var j = i % _totalRows;
				var item = _testDataClonedShuffled[j];
				_sortedList.TryGetValue(item.Key, out var _value);
			}

			_sortedListReadTime = stopWatch.Elapsed;

			Console.WriteLine($"Exact find of single key from a shuffledList over {_exactSearchIterations} iterations on a SortedList took  {_sortedListReadTime}");
			_log.Info($"Exact find of single key from a shuffledList over {_exactSearchIterations} iterations on a SortedList took  {_sortedListReadTime}");
		}

		[TestMethod()]
		public void ExactFindFromShuffled_SortedDictionary()
		{
			var stopWatch = Stopwatch.StartNew();

			for (int i = 0; i < _exactSearchIterations; i++)
			{
				var j = i % _totalRows;
				var item = _testDataClonedShuffled[j];
				_sortedDictionary.TryGetValue(item.Key, out var _value);
			}

			_sortedDictionaryReadTime = stopWatch.Elapsed;

			Console.WriteLine($"Exact find of single key from a shuffledList over {_exactSearchIterations} iterations on a SortedDictionary took  {_sortedDictionaryReadTime}");
			_log.Info($"Exact find of single key from a shuffledList over {_exactSearchIterations} iterations on a SortedDictionary took  {_sortedDictionaryReadTime}");
		}

		[TestMethod()]
		public void ExactFindFromShuffled_BPlusTreeDictionary()
		{
			var stopWatch = Stopwatch.StartNew();

			for (int i = 0; i < _exactSearchIterations; i++)
			{
				var j = i % _totalRows;
				var item = _testDataClonedShuffled[j];
				_bplusTreeDictionary.TryGetValue(item.Key, out var _value);
			}

			_bplusTreeDictionaryReadTime = stopWatch.Elapsed;

			Console.WriteLine($"Exact find of single key from a shuffledList over {_exactSearchIterations} iterations on a BPlusTree took  {_bplusTreeDictionaryReadTime}");
			_log.Info($"Exact find of single key from a shuffledList over {_exactSearchIterations} iterations on a BPlusTree took  {_bplusTreeDictionaryReadTime}");
		}
		#endregion

		#region ExactFindRepeated (Spreaded duplicate values)

		[TestMethod]
		public void ExactFindRepeated_SortedList()
		{
			var stopWatch = Stopwatch.StartNew();

			for (int i = 0; i < _exactSearchIterations; i++)
			{
				_sortedList.TryGetValue(_fixedDuplicateValue, out var _value);
			}

			_sortedListReadTime = stopWatch.Elapsed;

			Console.WriteLine($"Exact find of {_fixedDuplicateValue} repeated with {_exactSearchIterations} iteration took {_sortedListReadTime} on a SortedList");
			_log.Info($"Exact find of {_fixedDuplicateValue} repeated with {_exactSearchIterations} iteration took {_sortedListReadTime} on a SortedList");
		}

		[TestMethod()]
		public void ExactFindRepeated_SortedDictionary()
		{
			var stopWatch = Stopwatch.StartNew();

			for (int i = 0; i < _exactSearchIterations; i++)
			{
				_sortedDictionary.TryGetValue(_fixedDuplicateValue, out var _value);
			}

			_sortedDictionaryReadTime = stopWatch.Elapsed;

			Console.WriteLine($"Exact find of {_fixedDuplicateValue} repeated with {_exactSearchIterations} iteration took {_sortedDictionaryReadTime} on a SortedDictionary");
			_log.Info($"Exact find of {_fixedDuplicateValue} repeated with {_exactSearchIterations} iteration took {_sortedDictionaryReadTime} on a SortedDictionary");

		}

		[TestMethod()]
		public void ExactFindRepeated_BPlusTreeDictionary()
		{
			var stopWatch = Stopwatch.StartNew();

			for (int i = 0; i < _exactSearchIterations; i++)
			{
				_bplusTreeDictionary.TryGetValue(_fixedDuplicateValue, out var _value);
			}

			_bplusTreeDictionaryReadTime = stopWatch.Elapsed;

			Console.WriteLine($"Exact find of {_fixedDuplicateValue} repeated with {_exactSearchIterations} iteration took {_bplusTreeDictionaryReadTime} on a BPlusTree");
			_log.Info($"Exact find of {_fixedDuplicateValue} repeated with {_exactSearchIterations} iteration took {_bplusTreeDictionaryReadTime} on a BPlusTree");
		}

		#endregion

		#region Remove Performance - Temporarely Disabled

		public void RemovePerformance_SortedList()
		{
			var stopWatch = Stopwatch.StartNew();

			foreach (var item in _testDataClonedShuffled)
				_sortedList.Remove(item.Key);

			_sortedListRemoveTime = stopWatch.Elapsed;
			Console.WriteLine($"Remove items one by one from SortedList completed in {_sortedListRemoveTime}");
			_log.Info($"Remove items one by one from SortedList completed in {_sortedListRemoveTime}");

			Assert.AreEqual(false, _sortedList.Any());
		}

		public void RemovePerformance_SortedDictionary()
		{
			var stopWatch = Stopwatch.StartNew();

			foreach (var item in _testDataClonedShuffled)
				_sortedDictionary.Remove(item.Key);

			_sortedDictionaryRemoveTime = stopWatch.Elapsed;
			Console.WriteLine($"Remove items one by one from SortedDictionary completed in {_sortedDictionaryRemoveTime}");
			_log.Info($"Remove items one by one from SortedDictionary completed in {_sortedDictionaryRemoveTime}");

			Assert.AreEqual(false, _sortedDictionary.Any());
		}

		public void RemovePerformance_BPlusTreeDictionary()
		{
			var stopWatch = Stopwatch.StartNew();
			foreach (var item in _testDataClonedShuffled)
				_bplusTreeDictionary.Remove(item.Key);

			_bplusTreeDictionaryRemoveTime = stopWatch.Elapsed;
			Console.WriteLine($"Remove items one by one from BPlusTreeDictionary completed in {_bplusTreeDictionaryRemoveTime}");
			_log.Info($"Remove items one by one from BPlusTreeDictionary completed in {_bplusTreeDictionaryRemoveTime}");

			Assert.AreEqual(false, _bplusTreeDictionary.Any());
		}

		#endregion

		#region Range Find Performance

		[TestMethod]
		public void RangeFindPerformance_SortedDictionary()
		{
			var stopWatch = Stopwatch.StartNew();

			// Range search - sorteddictionary			
			for (int i = 0; i < _rangeSearchCount; i++)
			{
				var index = _rnd.Next(0, _testDataClonedShuffled.Count - 1); // find any index with in list count
				var minToleranceVal = _testDataClonedShuffled[index].Key - _range; // tolerance min value
				var maxToleranceVal = _testDataClonedShuffled[index].Key + _range; // tolerance max value

				HashSet<int> matchesFoundInThisIndex = new HashSet<int>();

				// check if any key for look up does not fall in the range
				if (maxToleranceVal < _sortedDictionary.First().Key || minToleranceVal > _sortedDictionary.Last().Key)
				{
					var _val = new List<int>();
				}
				else
				{
					_sortedDictionary.Where(x => minToleranceVal <= x.Key && x.Key <= maxToleranceVal)
								  .Select(x => x.Value).ToList()
								  .ForEach(x =>
								  {
									  matchesFoundInThisIndex.UnionWith(x);
								  });
					matchesFoundInThisIndex.ToList();
				}
			}

			_sortedDictRangeSearchTime = stopWatch.Elapsed;
			Console.WriteLine($"SortedDictionary find range search completed in {_sortedDictRangeSearchTime}");
		}
		//[TestMethod] 
		public void RangeFindPerformance_BPlusTreeDictionary()
		{
			var stopWatch = Stopwatch.StartNew();

			// Range search - bplusTreeDictionary			
			for (int i = 0; i < _rangeSearchCount; i++)
			{
				var index = _rnd.Next(0, _testDataClonedShuffled.Count - 1); // find any index with in list count
				var minToleranceVal = _testDataClonedShuffled[index].Key - _range; // tolerance min value
				var maxToleranceVal = _testDataClonedShuffled[index].Key + _range; // tolerance max value

				if (_bplusTreeDictionary.Count > 0)
				{
					HashSet<int> matchesFoundInThisIndex = new HashSet<int>();

					_bplusTreeDictionary.Where(x => minToleranceVal <= x.Key && x.Key <= maxToleranceVal)
										  .Select(x => x.Value).ToList()
										  .ForEach(x =>
										  {
											  matchesFoundInThisIndex.UnionWith(x);
										  });

					matchesFoundInThisIndex.ToList();
				}
			}

			_bplusTreeDictRangeSearchTime = stopWatch.Elapsed;
			Console.WriteLine($"BPlusTree find range search completed in {_bplusTreeDictRangeSearchTime}");
		}
		[TestMethod]
		public void RangeFindPerformance_SortedList()
		{
			var stopWatch = Stopwatch.StartNew();

			// Range search - SortedList
			for (int iCount = 0; iCount < _rangeSearchCount; iCount++)
			{
				var index = _rnd.Next(0, _testDataClonedShuffled.Count - 1); // find any index with in list count
				var minTolernaceVal = _testDataClonedShuffled[index].Key - _range;  // tolerance min value
				var maxToleranceVal = _testDataClonedShuffled[index].Key + _range; // tolerance max value

				int minIndex = FindIndexOfValueOrLargerSortedList(minTolernaceVal);
				int maxIndex = FindIndexOfValueOrSmallerSortedList(maxToleranceVal);

				HashSet<int> matchesFoundInThisIndex = new HashSet<int>();

				if (_sortedList.Count > 0)
				{
					// Range search using index
					if (maxToleranceVal < _sortedList.Keys[0] || minTolernaceVal > _sortedList.Keys[_sortedList.Count - 1])
					{
						var _val = new List<int>();
					}
					else
					{
						for (int i = minIndex; i <= maxIndex; i++)
						{
							matchesFoundInThisIndex.UnionWith(_sortedList.Values[i]);
						}
					}

					matchesFoundInThisIndex.ToList();
				}
			}

			_sortedListRangeSearchTime = stopWatch.Elapsed;
			Console.WriteLine($"SortedList find range search completed in {_sortedListRangeSearchTime}");
			_log.Info($"SortedList find range search completed in {_sortedListRangeSearchTime}");
		}

		[TestMethod]
		public void RangeFindPerformance_BPlusTreeDictionary_IndexSearch()
		{
			var stopWatch = Stopwatch.StartNew();
			BPlusTreeIndexedRangeSearch<int> _bplusTreeIndexedRangeSearch = new BPlusTreeIndexedRangeSearch<int>();

			// Range search - bplusTreeDictionary			
			for (int i = 0; i < _rangeSearchCount; i++)
			{
				var index = _rnd.Next(0, _testDataClonedShuffled.Count - 1); // find any index with in list count
				var minToleranceVal = _testDataClonedShuffled[index].Key - _range; // tolerance min value
				var maxToleranceVal = _testDataClonedShuffled[index].Key + _range; // tolerance max value

				if (_bplusTreeDictionary.Count > 0)
				{
					HashSet<int> matchesFoundInThisIndex = new HashSet<int>();
					var _resultRange = _bplusTreeIndexedRangeSearch.RangeofKeys(_bplusTreeDictionary, minToleranceVal, maxToleranceVal);
					matchesFoundInThisIndex.UnionWith(_resultRange);
				}
			}

			_bplusTreeDictRangeSearchTime = stopWatch.Elapsed;
			Console.WriteLine($"BPlusTree find range search completed in {_bplusTreeDictRangeSearchTime}");
			_log.Info($"BPlusTree find range search completed in {_bplusTreeDictRangeSearchTime}");
		}

		#endregion

		#region Ensure Key and Value are same in all collections - Temporarely Disabled

		public void SearchofSameKeyInBothBPlussTreeSortedDictionary_ValuesExpectedToBeSameAlways()
		{
			var resultSame = true;

			// Act
			foreach (var item in _testData)
			{
				// TODO - Add a double check later
				//var bpt = bplusTreeDictionary[item];
				//var sd = sortedDictionary[item];
				//var sl = sortedDictionary[item];

				//if(!(bpt.All(sd.Contains) && bpt.Count == sd.Count) || !(sd.All(sl.Contains) && sd.Count == sl.Count))
				//	throw new Exception("mismatch " + item);


				_bplusTreeDictionary.TryGetValue(item.Key, out var valbpTree);
				_sortedDictionary.TryGetValue(item.Key, out var valSortedDict);
				_sortedList.TryGetValue(item.Key, out var valSortedList);

				if (!(valbpTree.All(valSortedDict.Contains) && valbpTree.Count == valSortedDict.Count)
					|| !(valSortedDict.All(valSortedList.Contains) && valSortedDict.Count == valSortedList.Count))
					resultSame = false; break;

			}
			Assert.AreEqual(true, resultSame);
			Console.WriteLine($"Searched same KEY in both BPlussTree and SortedDictionary - Both values are same. ");
		}

		#endregion

		#region Private Methods

		private static void LoadTestDataToCollection(CollectionType collectionType)
		{
			var _stopWatch = new Stopwatch();
			switch (collectionType)
			{
				case CollectionType.SortedList:
					_stopWatch.Start();
					foreach (var item in _testData)
					{
						double theValue = Convert.ToDouble(item.Key);
						List<int> entry;
						if (!_sortedList.TryGetValue(theValue, out entry))
						{
							entry = new List<int>();
							_sortedList.Add(theValue, entry);
						}
						entry.Add(item.Value);
					}
					_stopWatch.Stop();
					_log.Info($"Populated testdata on SortedList collection. Count - {_sortedList.Count}, Time Took = {_stopWatch.Elapsed}");
					break;
				case CollectionType.SortedDictionary:
					_stopWatch.Start();
					foreach (var item in _testData)
					{
						double theValue = Convert.ToDouble(item.Key);
						List<int> entry;
						if (!_sortedDictionary.TryGetValue(theValue, out entry))
						{
							entry = new List<int>();
							_sortedDictionary.Add(theValue, entry);
						}
						entry.Add(item.Value);
					}
					_stopWatch.Stop();
					_log.Info($"Populated testdata on SortedDictionary collection. Count - {_sortedDictionary.Count}, Time Took = {_stopWatch.Elapsed}");
					break;
				case CollectionType.BPlusTree:
					_stopWatch.Start();
					foreach (var item in _testData)
					{
						double theValue = Convert.ToDouble(item.Key);
						List<int> entry;
						if (!_bplusTreeDictionary.TryGetValue(theValue, out entry))
						{
							entry = new List<int>();
							_bplusTreeDictionary.Add(theValue, entry);
						}
						entry.Add(item.Value);
					}
					_stopWatch.Stop();
					_log.Info($"Populated testdata on BPlusTree collection. Count - {_bplusTreeDictionary.Count}, Time Took = {_stopWatch.Elapsed}");
					break;
				case CollectionType.All:
					foreach (var item in _testData)
					{
						double theValue = Convert.ToDouble(item.Key);

						// Add to SortedList
						List<int> entrySortedList;
						if (!_sortedList.TryGetValue(theValue, out entrySortedList))
						{
							entrySortedList = new List<int>();
							_sortedList.Add(theValue, entrySortedList);
						}
						entrySortedList.Add(item.Value);

						// Add to SortedDictionary
						List<int> entrySortedDict;
						if (!_sortedDictionary.TryGetValue(theValue, out entrySortedDict))
						{
							entrySortedDict = new List<int>();
							_sortedDictionary.Add(theValue, entrySortedDict);
						}
						entrySortedDict.Add(item.Value);

						// Add to BPlusTreeDictionary
						List<int> entryBPTree;
						if (!_bplusTreeDictionary.TryGetValue(theValue, out entryBPTree))
						{
							entryBPTree = new List<int>();
							_bplusTreeDictionary.Add(theValue, entryBPTree);
						}
						entryBPTree.Add(item.Value);
					}

					_log.Info("Populated testdata on SortedList, SortedDictionary, BPlusTree collections.");
					break;
			}
		}

		private int FindIndexOfValueOrLargerSortedList(double value)
		{
			int _minIndex = 0;
			int _maxIndex = _sortedList.Count - 1;

			while (_minIndex < _maxIndex)
			{
				int _midIndex = _minIndex + (_maxIndex - _minIndex) / 2;
				int comparisonResult = value.CompareTo(_sortedList.Keys[_midIndex]);
				if (comparisonResult == 0)
					return _midIndex;
				if (comparisonResult < 0)
					_maxIndex = _midIndex - 1;
				else
					_minIndex = _midIndex + 1;
			}

			if (_sortedList.Keys[_minIndex] >= value)
			{
				return _minIndex;
			}
			return _minIndex + 1;
		}

		private int FindIndexOfValueOrSmallerSortedList(double value)
		{
			int _minIndex = 0;
			int _maxIndex = _sortedList.Count - 1;

			while (_minIndex < _maxIndex)
			{
				int midIndex = _minIndex + (_maxIndex - _minIndex) / 2;
				int comparisonResult = value.CompareTo(_sortedList.Keys[midIndex]);
				if (comparisonResult == 0)
					return midIndex;
				if (comparisonResult < 0)
					_maxIndex = midIndex - 1;
				else
					_minIndex = midIndex + 1;
			}

			if (_sortedList.Keys[_minIndex] <= value)
			{
				return _minIndex;
			}
			return _minIndex - 1;
		}

		private static void SetupTestData()
		{
			_log.Info($"\n SetupTestData Initiated. Defined Constant for this test " +
			$"\n TotalRows={_totalRows}," +
			$"\n TotalDuplicates={_totalDuplicates}" +
			$",\n TotalNulls={_totalNulls}," +
			$"\n Minimum Value={_minValue}, " +
			$"\n Maximum Value={_maxValue}, " +
			$"\n FixedDuplicateValue={_fixedDuplicateValue}" +
			$"\n ExactSearchIterations={_exactSearchIterations}" +
			$"\n Range={_range}" +
			$"\n RangeSearchCount={_rangeSearchCount}");


			_decimalDataFactory = new DecimalDataFactory();
			_testData = _decimalDataFactory.GetTestData(_totalRows, _totalDuplicates, _totalNulls, _minValue, _maxValue, _fixedDuplicateValue, _fixedDuplicateInstances);
			_testDataClonedShuffled = _testData.ToList().GetRange(0, _testData.Count); // Create testdata deep cloned list 
			_testDataClonedShuffled.Shuffle(); // Shuffle the cloned list

			_log.Info("Test data collection populated.");

			var _stopWatch = Stopwatch.StartNew();
			// Load all data to collection
			LoadTestDataToCollection(CollectionType.All);

			// LoadTestDataToCollection(CollectionType.SortedDictionary);
			// LoadTestDataToCollection(CollectionType.BPlusTree);
			// LoadTestDataToCollection(CollectionType.SortedList);

			_stopWatch.Stop();

			_log.Info($"SetupTestData completed. Loaded test data to all required collections. Time Taken - {_stopWatch.Elapsed}");
		}		

		#endregion
	}
}
