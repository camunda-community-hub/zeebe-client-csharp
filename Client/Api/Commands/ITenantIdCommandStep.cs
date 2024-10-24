namespace Zeebe.Client.Api.Commands;

public interface ITenantIdCommandStep<out T>
{
    /// <summary>
    /// Set the tenantId for the resource.
    /// </summary>
    /// <param name="tenantId">the tenantId to associate to this resource</param>
    /// <returns>The builder for this command.  Call <see cref="IFinalCommandStep{T}.Send"/> to complete the command and send it
    ///     to the broker.</returns>
    T AddTenantId(string tenantId);
}