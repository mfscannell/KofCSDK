using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KofCSDK
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKofC(this IServiceCollection services, KofCApiConfig kofcApiConfig)
        {
            services.AddOptions<KofCApiConfig>()
                .Configure(c =>
                {
                    c.HttpClientName = kofcApiConfig.HttpClientName;
                    c.BaseUrl = kofcApiConfig.BaseUrl;
                });
            //services.AddHttpClient<IKofCV1Client, KofCV1Client>((sp, httpClient) =>
            //{
            //    var kofcApiConfigOptions = sp.GetRequiredService<IOptionsMonitor<KofCApiConfig>>();
            //    httpClient.BaseAddress = new Uri(kofcApiConfigOptions.CurrentValue.BaseUrl, UriKind.Absolute);
            //});
            services.AddHttpClient(kofcApiConfig.HttpClientName, (sp, httpClient) =>
            {
                httpClient.BaseAddress = new Uri(kofcApiConfig.BaseUrl, UriKind.Absolute);
            });
            services.AddTransient<IKofCV1Client, KofCV1Client>(serviceProvider =>
            {
                var kofcApiConfigOptions = serviceProvider.GetRequiredService<IOptionsMonitor<KofCApiConfig>>();
                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                return new KofCV1Client(kofcApiConfigOptions, httpClientFactory, kofcApiConfig.HttpClientName);
            });

            return services;
        }
    }
}
