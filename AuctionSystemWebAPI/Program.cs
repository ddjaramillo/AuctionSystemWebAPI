using AuctionSystemWebAPI.Data;
using AuctionSystemWebAPI.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add Auth services to the container.
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.Authority = $"{builder.Configuration["AzureAd:Instance"]}{builder.Configuration["AzureAd:TenantId"]}/v2.0/";
//        options.Audience = builder.Configuration["AzureAd:ClientId"];
//    });

builder.Services.AddAuthentication("Bearer")
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
        options.TokenValidationParameters.NameClaimType = "name";
    },
    options => { builder.Configuration.Bind("AzureAdB2C", options); });


// Register configuration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
// Bind CosmosDbSettings
builder.Services.Configure<CosmosDbSettings>(builder.Configuration.GetSection("CosmosDb"));
var cosmosSettings = builder.Configuration.GetSection("CosmosDb").Get<CosmosDbSettings>();

// Register CosmosClient
#pragma warning disable CS8602 // Dereference of a possibly null reference.
builder.Services.AddSingleton(s => new CosmosClient(cosmosSettings.AccountEndpoint, cosmosSettings.AccountKey));


// Register CosmosDbContext
builder.Services.AddSingleton(provider =>
{
    var cosmosClient = provider.GetRequiredService<CosmosClient>();
    return new CosmosDbContext(cosmosClient, cosmosSettings.DatabaseName, cosmosSettings.ContainerName);
});

//builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddSingleton<IAuctionRepository>(provider =>
    new AuctionRepository(
        provider.GetRequiredService<CosmosClient>(),
        provider.GetRequiredService<IBidItemRepository>(),
        provider.GetRequiredService<IConfiguration>(),
        "AuctionDB", "Auctions"));

builder.Services.AddSingleton<IBidItemRepository>(provider =>
    new BidItemRepository(
        provider.GetRequiredService<CosmosClient>(),
        "AuctionDB", "Bids"));

builder.Services.AddSingleton<IUserItemRepository>(provider =>
    new UserItemRepository(
        provider.GetRequiredService<CosmosClient>(),
        "AuctionDB", "Users"));


builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});


var app = builder.Build();

// Initialize Cosmos DB
using (var scope = app.Services.CreateScope())
{
    var cosmosClient = scope.ServiceProvider.GetRequiredService<CosmosClient>();
    await CosmosDbContext.InitializeAsync(
        cosmosClient,
        cosmosSettings.DatabaseName,
        cosmosSettings.ContainerName
    );
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
