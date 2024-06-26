using Common.Dapr;
using Common.Models;
using Serilog;
using System.Collections;
using System.Net;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// remove default logging providers
builder.Logging.ClearProviders();
// Serilog configuration        
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
// Register Serilog
builder.Logging.AddSerilog(logger);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDaprClient();
builder.Services.AddHttpClient("attack-goku", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["GokuUrl"]);
});

builder.Services.AddSingleton<IDaprOutputBindingService, DaprOutputBindingService>();

// Add OpenTelemetry Observability
builder.Services.AddObservability(Assembly.GetExecutingAssembly().GetName().Name!, builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapObservability();

Hashtable results = [];

app.MapPost("/freezer", async (Attack attack, HttpContext context, IDaprOutputBindingService daprOutputBindingService,
                              ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
{
    const string retriableHeader = "x-ms-retriable-status-code";

    var logger = loggerFactory.CreateLogger("freeze");
    logger.LogInformation("Receiving an attack {Energy} energy and hit number {Number}", attack.Energy, attack.NumberOfAttack);
    await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);

    switch (attack.Energy)
    {
        case >= 500:
            logger.LogInformation("A perfect attack! [200]");
            // signalr to client
            await SignalRToClient("A perfect attack! [200]");
            SetState(results, attack.NumberOfAttack, 200);
            return Results.Ok();
        case < 300:
            // circuit breaker
            logger.LogInformation("You're tired, your punch is almost bad [503]");
            // signalr to client
            await SignalRToClient("You're tired, your punch is almost bad [503]");
            context.Response.Headers.Append(retriableHeader, "true");
            return Results.StatusCode(SetState(results, attack.NumberOfAttack, 503));
        case >= 300 and < 500:
            if (!results.ContainsKey(attack.NumberOfAttack))
            {
                logger.LogInformation("You've failed by a lot. Try AGAIN [429]");
                // signalr to client
                await SignalRToClient("You've failed by a lot. Try AGAIN [429]");
            }
            else
            {
                logger.LogInformation("Now you've hit him the retry [429]");

                // 70 chance to recover 
                if (isOK())
                {
                    logger.LogInformation("Attack completed!! [200]");
                    // signalr to client
                    await SignalRToClient("Attack completed!! [200]");
                    results[attack.NumberOfAttack] = 200;
                    return Results.Ok();
                }
            }
            context.Response.Headers.Append(retriableHeader, "true");
            // signalr to client
            await SignalRToClient("Now you've hit him the retry [429]");
            return Results.StatusCode(429);
    }


    bool isOK()
    {
        Random random = new();
        int randomInt = random.Next(1, 100);
        logger.LogInformation("... ... --> retry [ % of success {0}]", randomInt);
        return randomInt >= 30;
    }

    // Method to save state before retries
    int SetState(Hashtable results, Guid? attackNumber, int statusCode)
    {
        if (!results.ContainsKey(attackNumber)) { results.Add(attackNumber, 503); }
        return statusCode;
    }

    async Task SignalRToClient(string message)
    {
        const string tenkaichibudokai = "tenkaichibudokai";
        const string target = "tenkaichibudokai-goku";

        // SignalR to client
        var metadata = new Dictionary<string, string>()
        {
            { "hub", tenkaichibudokai }
        };

        var messageText = new
        {
            message
        };

        DaprPayloadMessageSignalR messageSignalR = new()
        {
            Target = target,
            Arguments =
            [
                new()
                {
                    Sender = tenkaichibudokai,
                    Text = System.Text.Json.JsonSerializer.Serialize(messageText)
                }
            ]
        };

        await daprOutputBindingService.PublishMessageAsync(messageSignalR, metadata, tenkaichibudokai, cancellationToken);
    }
})
.WithName("Freezer")
.WithOpenApi();

app.MapPost("/attack-goku", async (Attack attack, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory) =>
{
    // preparing strike energy
    Random random = new();
    int randomNumber = random.Next(1, 1001);

    var strike = attack ?? new Attack(randomNumber, Guid.NewGuid());
    strike.NumberOfAttack = attack.NumberOfAttack ?? Guid.NewGuid();
    var logger = loggerFactory.CreateLogger("attack-goku");
    logger.LogInformation("Attack Goku with {Energy} energy", strike.Energy);
    var client = httpClientFactory.CreateClient("attack-goku");
    var response = await client.PostAsJsonAsync("/goku", attack);
    return response.StatusCode switch
    {
        HttpStatusCode.OK => Results.Ok(),
        HttpStatusCode.ServiceUnavailable => Results.StatusCode(503),
        HttpStatusCode.TooManyRequests => Results.StatusCode(429),
        _ => Results.Ok()
    };
})
.WithName("AttackGoku")
.WithOpenApi();


await app.RunAsync();

