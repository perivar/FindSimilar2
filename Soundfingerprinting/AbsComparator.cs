using System;
using System.Collections.Generic;

namespace Soundfingerprinting.Fingerprinting
{
	public class AbsComparator : IComparer<double>
	{
		#region IComparer<double> Members
		public int Compare(double x, double y)
		{
			return Math.Abs(y).CompareTo(Math.Abs(x));
		}
		#endregion
	}
}