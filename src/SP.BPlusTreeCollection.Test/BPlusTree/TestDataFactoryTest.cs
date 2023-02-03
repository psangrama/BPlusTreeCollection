using Microsoft.VisualStudio.TestTools.UnitTesting;
using SP.BPlusTreeCollection.Test.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.BPlusTreeCollection.Test.BPlusTree
{
	[TestClass]
	public class DecimalTestData
	{
		public DecimalDataFactory DecimalDataFactory { get; }

		public DecimalTestData()
		{
			DecimalDataFactory = new DecimalDataFactory();
		}
		[TestMethod]
		public void ValidateGetData()
		{
			Assert.IsTrue(this.GetTestData());
		}

		private bool GetTestData(
			int requiredLength = 4500,
			int numberofDuplicates = 400,
			int numberofNulls = 250,
			int minValue = 0,
			int maxValue = 72,
			double spreadDuplicateValue = 32.785489658,
			int spreadDuplicateNumber = 877)
		{
			var data = DecimalDataFactory.GetTestData(requiredLength, numberofDuplicates, numberofNulls, minValue, maxValue, spreadDuplicateValue, spreadDuplicateNumber);
			var duplicatesOnData = data.GroupBy(x => x.Key)
							  .Where(g => g.Count() > 1)
							  .Select(y => new { Key = y.Key, Count = y.Count() })
							  .ToList();
			var _spreadDupCount = data.Where(x => x.Key == 32.785489658);
			Assert.AreEqual(requiredLength + spreadDuplicateNumber, data.Count);
			var _duplicates = data.GroupBy(x => x.Key).Where(g => g.Count() > 2).ToList();
			Assert.IsTrue(duplicatesOnData.Count <= 400); // Chances of getting duplicates more or less.. but would be unique. 
			Assert.AreEqual(numberofNulls, data.Count(x => x.Key == 0));
			return true;
		}
	}
}
