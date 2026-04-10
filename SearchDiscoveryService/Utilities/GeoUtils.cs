using System;

namespace SearchDiscoveryService.Utilities
{
	public static class GeoUtils
	{
		//  Haversine Formula → calculates distance in KM
		public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
		{
			double R = 6371; // Earth radius in KM

			var dLat = ToRadians(lat2 - lat1);
			var dLon = ToRadians(lon2 - lon1);

			var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
					Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
					Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

			var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

			return R * c;
		}

		private static double ToRadians(double angle)
		{
			return angle * (Math.PI / 180);
		}
	}
}