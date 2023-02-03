using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.BPlusTreeCollection.Test.TestData
{
	public interface IDataFactory
	{
		int RequiredLength { get; }
		int NumberofDuplicates { get; }
		int NumberofNulls { get; }
		int MinValue { get; }
		int MaxValue { get; }
		int SpreadDuplicateTimes { get; set; }
		double SpreadDuplicateValue { get; set; }
		List<KeyValuePair<double, int>> GetTestData(int requiredLengh, int numberofDuplicates, int numberofNulls, int minValue, int maxValue, double spreadDuplicateValue, int spreadDuplicateNumber);
	}
}
