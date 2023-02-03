using SP.BPlusTreeCollection.BPlusTree;
using SP.BPlusTreeCollection.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.BPlusTreeCollection.Test.Common
{
	public class BPlusTreeIndexedRangeSearch<T>
	{
		#region BPlusTree Index Based Search

		public List<T> RangeofKeys(BPlusTreeDictionary<double, List<T>> bPlusDictionary, double minValue, double maxValue)
		{
			return this.RangeofKeys(bPlusDictionary.LeafNodes, minValue, maxValue);
		}

		public List<T> RangeofKeys(IList<INode<double, List<T>>> leafNodes, double minValue, double maxValue)
		{
			var _leafNodeContainsMinValue = FindLeafNodeofMinValueOrLarger(leafNodes, minValue); // Identify the leafNode of minValue
			var _leafNodeContainsMaxValue = FindLeafNodeofMaxValueOrSmaller(leafNodes, maxValue); // Identify the leafNode of maxValue

			var _nodeIndexOfMinValueInLeaf = FindIndexOfMinValueOrLarger(leafNodes, _leafNodeContainsMinValue, minValue); // Identify the node index of minValue in leaf node contains minValue
			var _nodeIndexOfMaxValueInLeaf = FindIndexOfMaxValueOrSmaller(leafNodes, _leafNodeContainsMaxValue, maxValue); // Identify the node index of maxValue in leaf node contains maxValue

			List<T> _matchesFoundInThisIndex = new List<T>();

			for (int i = _leafNodeContainsMinValue; i <= _leafNodeContainsMaxValue; i++) // loop thru leafNodes contains minValue to maxValue
			{
				if (leafNodes[i] != null && leafNodes[i].Count > 0)
				{
					int _indexLoopCountStart;
					int _indexLoopCountEnd;

					// if min and max values present in the same leaf node 
					if (_leafNodeContainsMinValue == _leafNodeContainsMaxValue)
					{
						_indexLoopCountStart = _nodeIndexOfMinValueInLeaf;
						_indexLoopCountEnd = _nodeIndexOfMaxValueInLeaf;
					}
					else
					{
						// Set the loopStartCounter
						if (i == _leafNodeContainsMinValue)
							_indexLoopCountStart = _nodeIndexOfMinValueInLeaf;
						else
							_indexLoopCountStart = 0;

						// Set the loop end counter
						if (i == _leafNodeContainsMaxValue)
							_indexLoopCountEnd = _nodeIndexOfMaxValueInLeaf;
						else
							_indexLoopCountEnd = leafNodes[i].Count - 1;
					}

					for (int j = _indexLoopCountStart; j <= _indexLoopCountEnd; j++)
					{
						var _leaflcl = leafNodes[i] as Leaf<double, List<T>>;
						_matchesFoundInThisIndex.AddRange(_leaflcl.Values[j]);
					}
				}
			}

			return _matchesFoundInThisIndex;
		}

		private int FindLeafNodeofMinValueOrLarger(IList<INode<double, List<T>>> leafNodes, double value)
		{
			int _minIndex = 0;
			int _maxIndex = leafNodes.Count - 1;

			while (_minIndex < _maxIndex)
			{
				int _midIndex = _minIndex + (_maxIndex - _minIndex) / 2;
				int _comparisonResult = value.CompareTo(leafNodes[_midIndex].Keys[0]);

				if (_comparisonResult == 0)
					return _midIndex;
				if (_comparisonResult < 0)
					_maxIndex = _midIndex - 1;
				else
					_minIndex = _midIndex + 1;
			}

			if (leafNodes.Count == 1) // If has only one leaf node
				return _minIndex;
			else if (_minIndex >= 1 && leafNodes[_minIndex - 1].Keys[0] <= value && value <= leafNodes[_minIndex].Keys[0])
				return _minIndex - 1;
			return _minIndex;
		}

		private int FindLeafNodeofMaxValueOrSmaller(IList<INode<double, List<T>>> leafNodes, double value)
		{
			int _minIndex = 0;
			int _maxIndex = leafNodes.Count - 1;

			while (_minIndex < _maxIndex)
			{
				int _midIndex = _minIndex + (_maxIndex - _minIndex) / 2;
				int _comparisonResult = value.CompareTo(leafNodes[_midIndex].Keys[0]);

				if (_comparisonResult == 0)
					return _midIndex;
				if (_comparisonResult < 0)
					_maxIndex = _midIndex - 1;
				else
					_minIndex = _midIndex + 1;
			}

			if (leafNodes[_minIndex].Keys[0] <= value)
				return _minIndex;

			if (_minIndex - 1 <= 0)
				return _minIndex;

			return _minIndex - 1;
		}

		private int FindIndexOfMinValueOrLarger(IList<INode<double, List<T>>> leafNodes, int leafNodeIndexofMinValue, double value)
		{
			var _leafNodeHavingMinValue = leafNodes[leafNodeIndexofMinValue];
			int _minIndex = 0;
			int _maxIndex = _leafNodeHavingMinValue.Count - 1;

			while (_minIndex < _maxIndex)
			{
				int _midIndex = _minIndex + (_maxIndex - _minIndex) / 2;
				int _comparisonResult = value.CompareTo(_leafNodeHavingMinValue.Keys[_midIndex]);
				if (_comparisonResult == 0)
					return _midIndex;
				if (_comparisonResult < 0)
					_maxIndex = _midIndex - 1;
				else
					_minIndex = _midIndex + 1;
			}

			if (_leafNodeHavingMinValue.Keys[_minIndex] >= value)
				return _minIndex;

			return _minIndex + 1;
		}

		private int FindIndexOfMaxValueOrSmaller(IList<INode<double, List<T>>> leafNodes, int leafNodeIndexofMaxValue, double value)
		{
			var _leafNodeHavingMaxValue = leafNodes[leafNodeIndexofMaxValue];
			int _minIndex = 0;
			int _maxIndex = _leafNodeHavingMaxValue.Count - 1;

			while (_minIndex < _maxIndex)
			{
				int _midIndex = _minIndex + (_maxIndex - _minIndex) / 2;
				int _comparisonResult = value.CompareTo(_leafNodeHavingMaxValue.Keys[_midIndex]);
				if (_comparisonResult == 0)
					return _midIndex;
				if (_comparisonResult < 0)
					_maxIndex = _midIndex - 1;
				else
					_minIndex = _midIndex + 1;
			}

			if (_leafNodeHavingMaxValue.Keys[_minIndex] <= value)
				return _minIndex;

			return _minIndex - 1;
		}

		#endregion
	}
}
