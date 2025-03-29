using KofCSDK.Models;
using KofCSDK.Models.Requests;
using KofCSDK.Models.Responses;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KofCSDK;

public class KofCV1Client : IKofCV1Client
{
    private readonly KofCApiConfig _kofcApiConfig;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _httpClientName;

    public KofCV1Client(
        IOptionsMonitor<KofCApiConfig> kofcApiConfig,
        IHttpClientFactory httpClientFactory,
        string httpClientName)
    {
        _kofcApiConfig = kofcApiConfig.CurrentValue;
        _httpClientFactory = httpClientFactory;
        _httpClientName = httpClientName;
    }

    public async Task<Result<LoginResponse>> LoginAsync(
        TenantInfo tenantInfo,
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var content = SerializeRequest(request))
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"api/{tenantInfo.TenantId}/accounts/login"))
            {
                httpRequest.Content = content;

                using (var httpClient = _httpClientFactory.CreateClient(_httpClientName))
                using (var httpResponseMesage = await httpClient.SendAsync(httpRequest, cancellationToken))
                {
                    return await ProcessResponse<LoginResponse>(httpResponseMesage);
                }
            }
        }
        catch (Exception exception)
        {
            return HandleError<LoginResponse>(exception);
        }
    }

    public async Task<Result<List<Activity>>> GetAllActivities(
        TenantInfo tenantInfo,
        UserAuthentication userAuthentication,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"api/{tenantInfo.TenantId}/activities"))
            {
                SetAuthorizationHeader(httpRequest, userAuthentication.WebToken);

                using (var httpClient = _httpClientFactory.CreateClient(_httpClientName))
                using (var httpResponseMesage = await httpClient.SendAsync(httpRequest, cancellationToken))
                {
                    return await ProcessResponse<List<Activity>>(httpResponseMesage);
                }
            }
        }
        catch (Exception exception)
        {
            return HandleError<List<Activity>>(exception);
        }
    }

    private void SetAuthorizationHeader(HttpRequestMessage httpRequestMessage, string authorizationHeader)
    {
        httpRequestMessage.Headers.Add("Authorization", $"Bearer {authorizationHeader}");
    }

    private StringContent SerializeRequest<TRequest>(TRequest request)
    {
        var requestJson = JsonSerializer.Serialize(request, _jsonSerializerOptions);

        return new StringContent(requestJson, Encoding.UTF8, "application/json");
    }

    private async Task<Result<TResponse>> ProcessResponse<TResponse>(HttpResponseMessage httpResponseMessage)
    {
        var result = new Result<TResponse>();

        try
        {
            var rawContent = await httpResponseMessage.Content.ReadAsStringAsync();
            result.Success = httpResponseMessage.IsSuccessStatusCode;
            result.RawContent = rawContent;
            result.StatusCode = httpResponseMessage.StatusCode;

            if (result.Success)
            {
                result.Data = JsonSerializer.Deserialize<TResponse>(rawContent, _jsonSerializerOptions);
            }
            else
            {
                result.Error = await DeserializeErrorResponse(httpResponseMessage);
            }
        }
        catch (Exception exception)
        {
            result.Success = false;
            result.Error = new ProblemDetails
            {
                Title = "Request failed",
                Detail = exception.Message
            };
        }

        return result;
    }

    private async Task<ProblemDetails> DeserializeErrorResponse(HttpResponseMessage httpResponseMessage)
    {
        var rawContent = await httpResponseMessage.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(rawContent))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ProblemDetails>(rawContent, _jsonSerializerOptions);
    }

    private Result<T> HandleError<T>(Exception exception)
    {
        return new Result<T>
        {
            Error = new ProblemDetails
            {
                Title = "Request failed",
                Detail = exception.Message
            }
        };
    }
}
