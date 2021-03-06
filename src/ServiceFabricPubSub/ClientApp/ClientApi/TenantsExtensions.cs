// Code generated by Microsoft (R) AutoRest Code Generator 1.0.1.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace ClientApi.Admin
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for Tenants.
    /// </summary>
    public static partial class TenantsExtensions
    {
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='tenantName'>
            /// </param>
            /// <param name='appVersion'>
            /// </param>
            public static string CreateTenant(this ITenants operations, string tenantName, string appVersion)
            {
                return operations.CreateTenantAsync(tenantName, appVersion).GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='tenantName'>
            /// </param>
            /// <param name='appVersion'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<string> CreateTenantAsync(this ITenants operations, string tenantName, string appVersion, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.CreateTenantWithHttpMessagesAsync(tenantName, appVersion, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

    }
}
