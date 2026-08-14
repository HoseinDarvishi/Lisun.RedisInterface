# Lisun.RedisInterface

A lightweight and developer-friendly .NET library that simplifies working with Redis through a strongly typed, object-oriented API.

Instead of working directly with Redis commands, keys, serialization, and low-level data structures, **Lisun.RedisInterface** allows you to work with Redis using familiar C# types and objects.

## Features

* Strongly typed Redis operations
* Object-oriented API
* Generic support for C# types
* Simplified Redis data access
* Reduced serialization boilerplate
* Easy integration with .NET applications
* Built on top of Microsoft's Redis caching infrastructure
* Designed for clean and maintainable application code

## Requirements

* .NET 10.0 or later
* Redis server
* Microsoft.Extensions.Caching.StackExchangeRedis

## Installation

Install the package using the .NET CLI:

```bash
dotnet add package Lisun.RedisInterface
```

Or using the NuGet Package Manager:

```powershell
Install-Package Lisun.RedisInterface
```

## Usage 

1. Set configurations in program.cs :
```csharp
using Lisun.RedisInterface.Abstraction;

builder.Services.AddRedisInterface(option =>
{
    option.InstanceName = "TestRedis";
    option.Connection = builder.Configuration.GetConnectionString("Redis")!;
    option.RedisServiceLifeTime = ServiceLifetime.Scoped;
});
```

2. Make your C# object 'Cacheable' : 
this setting can be applied to types in two ways

------------

* "ICacheable" :

```csharp
using Lisun.RedisInterface.Abstraction;

public class Person : ICacheable
{
   public int Id { get; set; }
   public string Name { get; set; }
   public string PostalCode { get; set; }
}
```
then in program.cs : 
```csharp
RedisInterface.RegisterConfig<Refer>(CacheSetting.Generate());
```
------------

* "IConfigedCacheable" : 

```csharp
public class User : IConfigedCacheable
{
   public static CacheSetting CacheSetting 
      => new CacheSetting("User").ExpireAfter(TimeSpan.FromMinutes(2));

   public int Id { get; set; }
   public string Name { get; set; }
   public string PostalCode { get; set; }
}
```
------------

3. Inject 'IRedisService' to manage cached data : 

```csharp
public class BaseService(
    IRedisService<Person> personCacheService,
    IRedisService<User> userCacheService)
{
   public async Task Do(int personId)
   {
      var user = new User();
      await userCacheService.SetAsync(user.Id.ToString(),user);
      await personCacheService.GetAsync(personId.ToString());
   }
}
```

This approach keeps Redis-related code simple, readable, and strongly typed.

## License

This project is licensed under the [MIT License](LICENSE).

## Contributing

Contributions, issues, and feature requests are welcome.

Feel free to open an issue or submit a pull request on the project's GitHub repository.

## ⭐ Support the Project

If you find `Lisun.RedisInterface` useful, consider giving the project a ⭐ on GitHub.