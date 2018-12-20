namespace EFCore.TransactionExtensions.Common
{
    public static class Messages
    {
        public const string CommitExternalTransaction = "Cannot commit an externally provided transaction";
        public const string RollbackExternalTransaction = "Cannot roll back an externally provided transaction";
        public const string AmbientTransactionsNotSupported = "The current provider does not support ambient transactions";
        public const string ServiceProviderDisposed = "Cannot create transaction scope. The service provider has been previously disposed.";
    }
}