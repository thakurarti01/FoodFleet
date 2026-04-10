using System.Text;

namespace AdminService.Helpers
{
	public static class CsvHelper
	{
		public static byte[] GenerateCsv<T>(IEnumerable<T> data)
		{
			var csv = new StringBuilder();

			var properties = typeof(T).GetProperties();

			// Header
			csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

			// Rows
			foreach (var item in data)
			{
				var values = properties.Select(p => p.GetValue(item)?.ToString());
				csv.AppendLine(string.Join(",", values));
			}

			return Encoding.UTF8.GetBytes(csv.ToString());
		}
	}
}