using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions.Tests.Model
{
    public class ContextWithDependency : DbContext
    {
        public ContextWithDependency(DbContextOptions<ContextWithDependency> options, Dependency dependency) : base(options)
        {
            Dependency = dependency;
        }

        public Dependency Dependency { get; }
    }
}