using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.BPlusTreeCollection.Nodes
{
	public static class Constants
	{
		public const int NodeSize = 1024;
		public const int MinNodeSize = NodeSize / 2;

		public static int TakeCount<TKey, TValue>(INode<TKey, TValue> node)
		{
			return Math.Max((node.Count - Constants.MinNodeSize) / 2, 1);
		}
	}
}
