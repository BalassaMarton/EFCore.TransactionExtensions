# Provider-agnostic transaction sharing with Entity Framework Core



## Intro

Transactions in EF Core are easy to use and suitable for most simple scenarios when only a single `DbContext` is involved.
However, when we try to share a transaction between multiple `DbContexts`, the [official solution](https://docs.microsoft.com/en-us/ef/core/saving/transactions#cross-context-transaction-relational-databases-only)
ties our code to relational providers, or worse yet, to a specific provider. This helper library solves the problem
by introducting a new, provider-agnostic service that can be used to create `DbContext`s that share a single transaction.

## Basic usage

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

2. Use the provider-agnostic `IDbContextTransactionScope` interface in application code, where multiple db contexts or context types
are involved within a single transaction:

```cs
    public class CustomerImporter
    {
        private readonly IDbContextTransactionScope _transaction;

        public CustomerImporter(IDbContextTransactionScope transaction)
        {
            _transaction = transaction;
        }

        public void ImportCustomers(JObject json)
        {
            using (var db = _transaction.CreateDbContext<StoreContext>())
            {
                // Import customers
                db.SaveChanges();
            }
        }
    }

    public class OrderImporter
    {
        private readonly IDbContextTransactionScope _transaction;

        public OrderImporter(IDbContextTransactionScope transaction)
        {
            _transaction = transaction;
        }

        public void ImportOrders(JObject json)
        {
            using (var db = _transaction.CreateDbContext<StoreContext>())
            {
                // Import orders
                db.SaveChanges();
            }
        }
    }

```


3. Create the provider-specific implementation where the actual connection is known:

```cs
    using (var transaction =
        new SqlServerDbContextTransactionScope(connectionString: configuration.GetConnectionString("StoreDb")))
    {
        new CustomerImporter(transaction).ImportCustomers(customersJson);
        new OrderImporter(transaction).ImportOrders(ordersJson);

        transaction.Commit();
    }
```

Whenever there's a need to perform operations in a single transaction, we just create a scope and invoke any
components involved. Black box testing and replacing these components becomes easier as they can be completely agnostic about 
transaction handling and can create as many DbContexts as they like.

## Dependency injection

The package natively supports dependency injection with Microsoft's `IServiceProvider`. Transaction scopes are registered 
with configurable lifetime (scoped by default).

### Basic registration

```cs
services.AddDbContextTransactionScope(provider =>
    new SqlServerDbContextTransactionScope(connectionString: configuration.GetConnectionString("StoreDb")));
```

### Named scopes

When working with multiple databases, declaring a transaction scope as a constructor dependency would make it impossible for the container
to inject the correct instance. To solve this, we introduce named transaction scopes:

```cs
services.AddDbContextTransactionScope("Store", provider =>
    new SqlServerDbContextTransactionScope(connectionString: configuration.GetConnectionString("StoreDb")));
```

In application code, we have to replace any constructor dependencies referencing `IDbContextTransactionScope` 
with another service: `IDbContextTransactionScopeResolver`. This service can resolve the transaction scope by name:

```cs
    private readonly IDbContextTransactionScopeResolver _scopeResolver;
```
```cs
    using (var db = _resolver.Resolve("Store").CreateDbContext<StoreContext>()) {
        // do stuff
        db.SaveChanges();
    }
```

### Providing additional constructor parameters

Internally, the transaction scope will invoke the `IDbContextActivator` internal service to create the `DbContext`
instances. When using an `IServiceProvider`, this service is injected automatically, and resolves any constructor
dependencies of the context from the container, except `DbContextOptions` that must be set up for transaction handling.

When some constructor parameters cannot be resolved automatically, or we don't use the built-in container,
we can provide a factory for the scope:

```cs
    db = scope.CreateDbContext<MyContext>(options => new MyContext(options, "some parameter")))
```
