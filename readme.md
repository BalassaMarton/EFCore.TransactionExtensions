# Provider-agnostic transaction sharing with Entity Framework Core

## Intro

Transactions in EF Core are easy to use and suitable for most simple scenarios when only a single `DbContext` is involved.
However, when we try to share a transaction between multiple `DbContexts`, the [official solution](https://docs.microsoft.com/en-us/ef/core/saving/transactions#cross-context-transaction-relational-databases-only)
ties our code to relational providers, or worse yet, to a specific provider. This helper library solves the problem
by introducting a new, provider-agnostic service that can be used to create `DbContext`s that share a single transaction.

## Usage

1. Your DbContext should have a constructor with a `DbContextOptions` parameter.

```cs
    public class StoreContext : DbContext
    {
        public StoreContext(DbContextOptions<StoreContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
```

2. Use the provider-agnostic `IDbContextTransactionScope` interface in application code:

```cs
        public void UpdateProducts(IDbContextTransactionScope transaction)
        {
            using (var db = transaction.CreateDbContext<StoreContext>())
            {
                foreach (var prod in db.Products)
                {
                    // ...
                }
                db.SaveChanges();
            }
        }
```

3. Create the provider-specific object where the actual connection is known:

```cs
            using (var transaction =
                new SqlServerDbContextTransactionScope(configuration.GetConnectionString("StoreDb")))
            {
                var products = new ProductService();
                products.UpdateProducts(transaction);

                transaction.Complete();
            }  

```

### Dependency injection

In most cases, your application code will not expect an externally provided transaction scope, but a factory for creating
new transaction scopes. This is analogous with the injected `DbContextOptions` (instead of providing a `DbContext`,
provide the means of connecting to the database, and let the consumer create instances of the context).
The simplest way of injecting a 'transaction scope factory' is by registering a `Func<IDbContextTransactionScope>`:

```cs
        serviceCollection.AddSingleton<Func<IDbContextTransactionScope>>(
            () => new SqlServerDbContextTransactionScope(configuration.GetConnectionString("StoreDb")));
```

...and injecting it into the consumer:

```cs
    public class ProductService
    {
        private Func<IDbContextTransactionScope> _transactionFactory;

        public ProductService(Func<IDbContextTransactionScope> transactionFactory)
        {
            _transactionFactory = transactionFactory;
        }

        public void UpdateProducts()
        {
            using (var transaction = _transactionFactory())
            {
                using (var db = transaction.CreateDbContext<StoreContext>())
                {
                    // ...
                    db.SaveChanges();
                }
                transaction.Complete();
            }
        }
    }
```
