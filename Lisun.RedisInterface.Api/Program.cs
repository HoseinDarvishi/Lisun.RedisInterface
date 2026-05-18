using Lisun.RedisInterface.Abstraction;
using Lisun.RedisInterface.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRedisInterface(option =>
{
    option.InstanceName = "TestRedis";
    option.Connection = builder.Configuration.GetConnectionString("Redis")!;
    option.RedisServiceLifeTime = ServiceLifetime.Scoped;
});

RedisInterface.RegisterConfig<Refer>(CacheSetting.Generate());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
} 

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();