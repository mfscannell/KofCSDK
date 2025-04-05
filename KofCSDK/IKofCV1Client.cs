using KofCSDK.Models.Requests;
using KofCSDK.Models.Responses;
using KofCSDK.Models;

namespace KofCSDK;

public interface IKofCV1Client
{
    Task<Result<LoginResponse>> LoginAsync(TenantInfo tenantInfo, LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<PasswordRequirements>> GetPasswordRequirementsAsync(TenantInfo tenantInfo, UserAuthentication userAuthentication, CancellationToken cancellationToken = default);
    Task<Result<List<Activity>>> GetAllActivities(TenantInfo tenantInfo, UserAuthentication userAuthentication, CancellationToken cancellationToken = default);
}
