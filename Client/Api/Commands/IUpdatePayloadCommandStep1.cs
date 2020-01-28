using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands
{
    public interface ISetVariablesCommandStep1
    {
        /// <summary>
        /// Set the new variables of the element instance.
        /// </summary>
        ///
        /// <param name="variables">the variables (JSON) as String</param>
        /// <returns>the builder for this command. Call <see cref="IFinalCommandStep{T}.Send"/> to complete the command and send it
        ///     to the broker.</returns>
        ISetVariablesCommandStep2 Variables(string variables);
    }

    public interface ISetVariablesCommandStep2 : IFinalCommandStep<ISetVariablesResponse>
    {
        /// <summary>
        /// The variables will be merged strictly into the local scope (as indicated by the given elementInstanceKey);
        /// this means the variables is not propagated to upper scopes.
        /// </summary>
        ///
        /// <p>For example, let's say we have two scopes, '1' and '2', with each having effective
        /// variables as:
        ///
        /// <ol>
        ///   <li>1 => `{ "foo" : 2 }`
        ///   <li>2 => `{ "bar" : 1 }`
        /// </ol>
        ///
        /// <p>If we send an update request with elementInstanceKey = 2, a new document of `{ "foo" : 5
        /// }`, and local is true, then scope 1 will be unchanged, and scope 2 will now be `{ "bar" : 1,
        /// "foo" 5 }`.
        ///
        /// <p>If local was false, however, then scope 1 would be `{ "foo": 5 }`, and scope 2 would be `{
        /// "bar" : 1 }`.
        ///
        /// <returns> the builder for this command. Call <see cref="IFinalCommandStep{T}.Send"/> to complete the command and send
        ///     it to the broker.
        ///     </returns>
        ISetVariablesCommandStep2 Local();
    }
}