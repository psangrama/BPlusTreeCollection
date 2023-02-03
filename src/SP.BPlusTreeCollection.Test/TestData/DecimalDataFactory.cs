using SP.BPlusTreeCollection.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.BPlusTreeCollection.Test.TestData
{
	public class DecimalDataFactory : IDataFactory
	{
		public int RequiredLength { get; set; }
		public int NumberofDuplicates { get; set; }
		public int NumberofNulls { get; set; }
		public int MinValue { get; set; }
		public int MaxValue { get; set; }
		public double SpreadDuplicateValue { get; set; }
		public int SpreadDuplicateTimes { get; set; }

		private int ActualRequiredLength { get; set; }
		private const Int32 precession = 1000000000;

		public List<KeyValuePair<double, int>> GetTestData(
			int requiredLengh,
			int numberofDuplicates,
			int numberofNulls,
			int minValue,
			int maxValue,
			double spreadDuplicateValue,
			int spreadDuplicateNumber
			)
		{
			this.RequiredLength = requiredLengh;
			this.NumberofDuplicates = numberofDuplicates;
			this.NumberofNulls = numberofNulls;
			this.MinValue = minValue;
			this.MaxValue = maxValue;
			this.SpreadDuplicateValue = spreadDuplicateValue;
			this.SpreadDuplicateTimes = spreadDuplicateNumber;
			this.ActualRequiredLength = requiredLengh - numberofNulls - numberofDuplicates;

			return GenerateRangeRandomNumber();
		}

		private List<KeyValuePair<double, int>> GenerateRangeRandomNumber()
		{
			var _kvPairList = new List<KeyValuePair<double, int>>();
			Random _rnd = new Random();
			for (int i = 0; i <= ActualRequiredLength; i++)
			{
				var _val = Convert.ToDouble(_rnd.Next(MinValue, MaxValue) + "." + _rnd.Next(MinValue, precession) + _rnd.Next(0, 9));
				_kvPairList.Add(new KeyValuePair<double, int>(_val, _rnd.Next(0, ActualRequiredLength)));
			}
			var iCount = 0;
			var _indexes = new List<int>();

			// Add duplicates
			for (int i = 0; i < NumberofDuplicates - 1; i++)
			{
				var _rndIndex = _rnd.Next(0, _kvPairList.Count - 1);
				_kvPairList.Add(new KeyValuePair<double, int>(_kvPairList[_rndIndex].Key, _kvPairList[_rndIndex + 1].Value));
				iCount++;
				_indexes.Add(_rndIndex);
			}

			// Add converted nulls 
			_kvPairList.AddRange(RequiredSetofNullKVP(ActualRequiredLength));

			// Spread duplicate's (Add the desired number to the collection as specified)
			for (int i = 0; i < SpreadDuplicateTimes; i++)
			{
				_kvPairList.Add(new KeyValuePair<double, int>(SpreadDuplicateValue, ActualRequiredLength + i));
			}

			// Shuffle the numbers
			_kvPairList.Shuffle();

			return _kvPairList;
		}

		private List<KeyValuePair<double, int>> RequiredSetofNullKVP(int actualRequiredLength)
		{
			Random _rnd = new Random();

			var _requiredNulls = new List<KeyValuePair<double, int>>();
			for (int i = 0; i < NumberofNulls; i++)
			{
				_requiredNulls.Add(new KeyValuePair<double, int>(Convert.ToDouble(null), _rnd.Next(0, actualRequiredLength)));
			}

			return _requiredNulls;
		}
	}
}
