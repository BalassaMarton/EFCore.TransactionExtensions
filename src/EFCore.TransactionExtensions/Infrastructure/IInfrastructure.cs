namespace EFCore.TransactionExtensions.Infrastructure
{
    /// <summary>
    /// Provides access to otherwise hidden properties of a <see cref="IDbContextTransactionScope"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IInfrastructure<T>
    {
        T Instance { get; set; }
    }
}