namespace AdminService.Config
{
	public static class HttpClientConfig
	{
		public static void RegisterHttpClients(IServiceCollection services, IConfiguration configuration)
		{
			//  Order Service
			services.AddHttpClient("OrderService", client =>
			{
				client.BaseAddress = new Uri(configuration["Services:OrderService"]);
				client.Timeout = TimeSpan.FromSeconds(30);
			});

			//  Payment Service
			services.AddHttpClient("PaymentService", client =>
			{
				client.BaseAddress = new Uri(configuration["Services:PaymentService"]);
				client.Timeout = TimeSpan.FromSeconds(30);
			});

			//  User Service
			services.AddHttpClient("UserService", client =>
			{
				client.BaseAddress = new Uri(configuration["Services:UserService"]);
				client.Timeout = TimeSpan.FromSeconds(30);
			});
		}
	}
}