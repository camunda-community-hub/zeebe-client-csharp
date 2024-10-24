using System.Collections.Generic;

namespace Zeebe.Client.Api.Commands;

public interface ITenantIdsCommandStep<out T>
{
    /// <summary>
    /// Specifies the tenants that may own any entities (e.g. process definition, process instances, etc.) resulting
    /// from this command.
    /// </summary>
    ///
    /// This can be useful when requesting jobs for multiple tenants at once. Each of the activated 
    /// jobs will be owned by the tenant that owns the corresponding process instance.
    /// <param name="tenantIds">the identifiers of the tenants to specify for this command, e.g. ["ACME", "OTHER"].</param>
    /// <returns>The builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.</returns>
    T TenantIds(IList<string> tenantIds);

    /// <summary>
    /// Specifies the tenants that may own any entities (e.g. process definition, process instances, etc.) resulting from this command.
    /// </summary>
    ///
    /// This can be useful when requesting jobs for multiple tenants at once. Each of the activated
    /// jobs will be owned by the tenant that owns the corresponding process instance.
    /// <param name="tenantIds">the identifiers of the tenants to specify for this command, e.g. ["ACME", "OTHER"].</param>
    /// <returns>The builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.</returns>
    T TenantIds(params string[] tenantIds);
}