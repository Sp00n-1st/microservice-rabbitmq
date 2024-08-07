using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using PostAppService.Data;
using PostAppService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<PostAppServiceContext>(o =>
    o.UseSqlite(@"Data Source=post.db"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PostAppServiceContext>();
        dbContext.Database.EnsureCreated();
    }
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

ListenForIntegrationEvents();

app.Run();

static void ListenForIntegrationEvents()
{
    var factory = new ConnectionFactory();
    var connection = factory.CreateConnection();
    var channel = connection.CreateModel();
    var consumer = new EventingBasicConsumer(channel);

    consumer.Received += (model, ea) =>
    {
        var contextOptions = new DbContextOptionsBuilder<PostAppServiceContext>()
            .UseSqlite(@"Data Source=post.db")
            .Options;
        var dbContext = new PostAppServiceContext(contextOptions);
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine(" [x] Received From {0} {1}",ea.RoutingKey, message);
        var data = JObject.Parse(message);
        var type = ea.RoutingKey;
        if (type == "user.add")
        {
            Console.WriteLine("Adding User");
            if (dbContext.Users.Any(a => a.ID == data["id"].Value<int>()))
            {
                Console.WriteLine("Ignoring old/duplicate entity");
            }
            else
            {
                Console.WriteLine("Save User");
                dbContext.Users.Add(new User()
                {
                    ID = data["id"].Value<int>(),
                    Name = data["name"].Value<string>(),
                    Version = data["version"].Value<int>()
                });
                dbContext.SaveChanges();
            }
        }
        else if (type == "user.update")
        {
            int newVersion = data["version"].Value<int>();
            var user = dbContext.Users.First(a => a.ID == data["id"].Value<int>());
            if (user.Version >= newVersion)
            {
                Console.WriteLine("Ignoring old/duplicate entity");
            }
            else
            {
                user.Name = data["newname"].Value<string>();
                user.Version = newVersion;
                dbContext.SaveChanges();
            }
        }
        channel.BasicAck(ea.DeliveryTag, false);
    };
    channel.BasicConsume(queue: "user.postservice",
                             autoAck: false,
                             consumer: consumer);
}
