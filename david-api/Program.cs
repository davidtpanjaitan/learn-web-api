using Microsoft.Azure.Cosmos;
using webapp.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using webapp.DAL.Tools;
using david_api.Middlewares;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables();
        var config = configBuilder.Build();

        string cosmosDbConnectionString = config["CosmosDbConnectionString"];
        string databaseName = config["CosmosDbDatabaseName"];
        string key = config["jwtkey"];
        string issuer = config["jwtissuer"];
        string audience = config["jwtaudience"];


        CosmosClient cosmosClient = new CosmosClient(cosmosDbConnectionString);


        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped((service) => new LokasiRepository(cosmosClient, databaseName));
        builder.Services.AddScoped((service) => new UserRepository(cosmosClient, databaseName));
        builder.Services.AddScoped((service) => new PanenRepository(cosmosClient, databaseName));
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigin",
                builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        });


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
            app.UseSwagger();
            app.UseSwaggerUI();
        //}
        app.UseCors("AllowAllOrigin");

        app.UseMiddleware<TokenMiddleware>(key, issuer, audience);

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}