using System.Collections.Generic;

namespace Zeebe.Client.Api.Commands
{
    public interface ITenantIdsCommandStep<out T>
    {
        /// <summary>
        /// Set a lit of tenantIds to associate to this resource.
        /// </summary>
        /// <param name="tenantIds">the tenant to associate to this resource.</param>
        /// <returns>The builder for this command.  Call <see cref="IFinalCommandStep{T}.Send"/> to complete the command and send it
        ///     to the broker.</returns>
        T AddTenantIds(IList<string> tenantIds);

        /// <summary>
        /// Set a list of tenantIds to associate to this resource.
        /// </summary>
        /// <param name="tenantIds">the tenant to associate to this resource.</param>
        /// <returns>The builder for this command.  Call <see cref="IFinalCommandStep{T}.Send"/> to complete the command and send it
        ///     to the broker.</returns>
        T AddTenantIds(params string[] tenantIds);
    }
}