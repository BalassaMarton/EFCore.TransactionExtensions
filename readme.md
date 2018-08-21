# Provider-agnostic transaction sharing with Entity Framework Core

## Intro

Transactions in EF Core are easy to use and suitable for most simple scenarios when only a single `DbContext` is involved.
Things get complicated and ugly, however, when we try to share a transaction between multiple `DbContexts`.

## The context

Let's assume, for the sake of this article (and because this real world problem inspired the article and the solution presented), 
that we are building some custom ETL solution that receives data from a sales system. Amongst other things, customers and orders
need to be processed and loaded into our data store, let's say, from JSON data. 
The relevant part of our model will look something like this:

```cs
public class Customer
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    // Identifier in the sales system
    public string CustomerCode { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public IList<Order> Orders { get;set; }
    // ...
}

public class Order
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    // Identifier in the sales system
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public Customer Customer { get; set; }
    public int CustomerId { get;set; }
    // ...
}

public class StoreContext : DbContext
{
    public DbSet<Customer> Customers { get;set; }
    public DbSet<Order> Orders {get; set; }

    // ...

    public StoreContext(DbContextOptions<StoreContext> options)
    {
    }
}
```

Now, since we love ~~to over-engineer~~ SOLID principles, we will have at least one class for processing each entity in our model:

```cs
public class CustomerLoader
{
    public async Task LoadCustomers(JsonReader source, StoreContext context)
    {
        // ...
    }
}

public class OrderLoader
{
    public async Task LoadOrders(JsonReader source, StoreContext context)
    {
        // ...
    }   
}
```

And there's going to be some orchestrator that calls each of them with the appropriate input:

```cs
public class LoaderOrchestrator
{
    private readonly DbContextOptions<StoreContext> _dbOptions;
    private readonly IFileProvider _fileProvider;
    private readonly CustomerLoader _customerLoader;
    private readonly OrderLoader _orderLoader;

    public LoaderOrchestrator(DbContextOptions<StoreContext> dbOptions, IFileProvider fileProvider,
        CustomerLoader customerLoader, OrderLoader orderLoader)
    {
        _dbOptions = dbOptions;
        _fileProvider = fileProvider;
        _customerLoader = customerLoader;
        _orderLoader = orderLoader;
    }

    public async Task Run()
    {
        using (var context = new StoreContext(_dbOptions))
        {
            ReadJsonFile(_fileProvider.GetFileInfo("customers.json"), async json =>
            {
                await _customerLoader.LoadCustomers(json, context);
            });
            ReadJsonFile(_fileProvider.GetFileInfo("orders.json"), async json =>
            {
                await _orderLoader.LoadOrders(json, context);
            });
        }
    }

    private static async void ReadJsonFile(IFileInfo fileInfo, Func<JsonReader, Task> callback)
    {
        using (var stream = fileInfo.CreateReadStream())
        using (var textReader = new StreamReader(stream))
        using (var jsonReader = new JsonTextReader(textReader))
            await callback(jsonReader);
    }
}
```

So far so good, except that we have to run this all in a transaction so that one failing file won't leave our data store in an inconsistent state.
Let's rewrite the `Run` method:

```cs
    public async Task Run()
    {
        using (var context = new StoreContext(_dbOptions))
        {
            using (var transaction = context.Database.BeginTransaction())
            {

                try
                {
                    ReadJsonFile(_fileProvider.GetFileInfo("customers.json"), async json =>
                    {
                        await _customerLoader.LoadCustomers(json, context);
                    });
                    ReadJsonFile(_fileProvider.GetFileInfo("orders.json"), async json =>
                    {
                        await _orderLoader.LoadOrders(json, context);
                    });
                    
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    // Log error, etc.
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
```

## The problem

It's all nice and simple. Except that it is never this simple. There will be dozens of loaders, we will introduce more abstractions, 
try to run things in parallel, etc. We do not want a single `DbContext`, but to be able to create contexts on demand (e.g. new context for each loader,
batch-inserting millions of records, or running queries in parallel).

According to the EF Core  [documentation](https://docs.microsoft.com/en-us/ef/core/saving/transactions#cross-context-transaction-relational-databases-only), 
what we should do, is 
1. Build `dbOptions` from an externally provided `SqlConnection`;
2. Create the first context, start a transaction;
3. Create all other contexts and call `DbContext.Database.UseTransaction` to enlist them in the same transaction.

And this is where things get ugly. Did I ever mention SQL Server? No. `dbOptions` was provided externally, and we didn't assume for a second
that a specific provider will be used. One reason I love EF Core is the ability to decouple my business logic from any specific DB provider.
Steps 1 and 2 not only force us to put provider-specific code into our otherwise povider-agnostic classes, but also break our design where
`DbContextOptions` is an injected dependency. 

The next thing we naturally come up with, is to still provide `dbOptions` externally, but with a `SqlConnection` instead of a connection string.
But this is still bad design. A dependency like `dbOptions` says _'To do my job, I need to know how to connect to the database'_. 
Looking at this dependency from the outside, we have no way of knowing that we MUST provide a `DbContextOptions` object that is suitable for
transaction sharing. We can document this requirement, but that would kind of break the purpose of DI - we shouldn't assume that some dependency 
will have any characteristic other than what it's contract implies. If we need a `SqlConnection` for our class to work, we must declare that as a dependency,
tying our code to SQL Server. 

But we are problem solvers, and already 3 months into the project after convincing the architecture team that EF Core is the way to go.
So let's just forget our purist design, and provide `dbOptions` with a connection, just to see if it works. Our registration code will look
something like this:

```cs
    services.AddScoped<DbContextOptions<StoreContext>>(sp =>
    {
        var conn = new SqlConnection(Configuration.GetConnectionString("StoreDB"));
        return new DbContextOptionsBuilder<StoreContext>().UseSqlServer(conn).Options;
    });
```

We go back to our orchestrator code to implement step 3:

```cs
    public async Task Run()
    {
        using (var context = new StoreContext(_dbOptions))
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    using (var db = new StoreContext(_dbOptions))
                    {
                        db.Database.UseTransaction(transaction.GetDbTransaction());
                        ReadJsonFile(_fileProvider.GetFileInfo("customers.json"), async json =>
                        {
                            await _customerLoader.LoadCustomers(json, db);
                        });
                    }
                        
                    using (var db = new StoreContext(_dbOptions))
                    {
                        db.Database.UseTransaction(transaction.GetDbTransaction());
                        ReadJsonFile(_fileProvider.GetFileInfo("orders.json"), async json =>
                        {
                            await _orderLoader.LoadOrders(json, db);
                        });
                    }
                    
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    // Log error, etc.
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
```

It's 8 PM on Friday, all I need before going home is one lousy green tick (because TDD is just as addictive as social media), I click on
'Run All' while stuffing my phone into my pocket and clearing my desk, and then I'm greeted with a nice big `InvalidOperationException` saying
_'blah blah blah Transactions are not supported by the in-memory store. blah blah blah'_. Yep, our unit tests are using the in-memory provider.
After configuring warnings as instructed by the exception message (thanks for that info by the way), we run the tests once again, only to
run into another `InvalidOperationException`, this time saying _'Relational-specific methods can only be used when the context is using a relational database provider.'_.
And that, my friend, is a brick wall we just hit. You see, there is absolutely no way to configure EF to accept the
`UseTransaction` call, because that method itself is in the `RelationalDatabaseFacadeExtensions` class and will throw the exception for
non-relational providers. We have to develop something.

## The workaround

To sum up what we want to accomplish at this point:
1. Our code to work with at least SQL Server and the in-memory provider;
2. To be able to provide the database options with DI, with no hidden requirements;
3. To be able to create an arbitrary number of db contexts that use the same transaction.

We obviously need to get rid of the `UseTransaction` call, and replace or wrap it so that it gets called only when applicable.
So what we really need as an injected dependency is something that can give us a transaction and also create db contexts that use that transaction.
And because naming things is really hard, but I'm also lazy, and all the other names are taken, I'll just call this thing 
`IDbContextTransactionScope`, and it will look like this:

```cs
public interface IDbContextTransactionScope<out TContext> : IDisposable where TContext : DbContext
{
    TContext CreateDbContext();
    void Complete();
}
```

`Dispose` and `Complete` has the same semantics as in a real `TransactionScope` - the transaction should be committed when `Complete` is called,
and rolled back on `Dispose` otherwise. `CreateDbContext` should create a context within the same transaction.

Let's go back to our orchestrator and change it accordingly:

```cs
public class LoaderOrchestrator
{
    private readonly Func<IDbContextTransactionScope<StoreContext>> _transactionFactory;
    private readonly IFileProvider _fileProvider;
    private readonly CustomerLoader _customerLoader;
    private readonly OrderLoader _orderLoader;

    public LoaderOrchestrator(Func<IDbContextTransactionScope<StoreContext>> transactionFactory, IFileProvider fileProvider, CustomerLoader customerLoader, OrderLoader orderLoader)
    {
        _transactionFactory = transactionFactory;
        _fileProvider = fileProvider;
        _customerLoader = customerLoader;
        _orderLoader = orderLoader;
    }

    public async Task Run()
    {
        using (var transaction = _transactionFactory())
        {
            try
            {
                using (var db = transaction.CreateDbContext())
                {
                    ReadJsonFile(_fileProvider.GetFileInfo("customers.json"), async json =>
                    {
                        await _customerLoader.LoadCustomers(json, db);
                    });
                }
                        
                using (var db = transaction.CreateDbContext())
                {
                    ReadJsonFile(_fileProvider.GetFileInfo("orders.json"), async json =>
                    {
                        await _orderLoader.LoadOrders(json, db);
                    });
                }
                transaction.Complete();
            }
            catch (Exception e)
            {
                // Log error, etc.
                throw;
            }
        }
    }

    // ...
}
```

The factory pattern here is of course optional, but it removes the possible uncertainty about the injected transaction's supposed life cycle, explicitly
saying: _'Give me something that I can use to start a transaction and create an arbitrary number of StoreContexts using that transaction'._

Registering the service will look something like this, provided that we have an implementation (spoiler alert: we sort of have):

```cs
    services.AddScoped<Func<IDbContextTransactionScope<StoreContext>>>(sp =>
        {
            return () =>
                new SqlServerDbContextTransactionScope<StoreContext>(sp.GetRequiredService<IConfigurationRoot>()
                    .GetConnectionString("StoreDB"));
        }
    );
```

You can find the implementation for the SqlServer and InMemory providers at
https://github.com/BalassaMarton/EFCore.TransactionExtensions

To see how to use with an ambient `TransactionScope`, take a look at the unit tests.

## In closing

This is just a proof of concept. It is far from complete, poorly tested and possibly full of hazardous code or even anti-patterns.
Also, this was my first coding article ever, so I'd appreciate any ~~criticism~~ input from the community.

