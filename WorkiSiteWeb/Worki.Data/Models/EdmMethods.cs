using System;
using System.Data.Objects.DataClasses;

namespace Worki.Data
{
	public static class EdmMethods
	{
		[EdmFunction("WorkiDBModel.Store", "DistanceBetween")]
		public static float? DistanceBetween(float Lat1, float Long1, float Lat2, float Long2, float Radius)
		{
			throw new NotSupportedException("This function is only for L2E query.");
		}
	}
}
