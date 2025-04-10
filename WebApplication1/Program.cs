using Nethereum.Web3;

var builder = WebApplication.CreateBuilder(args);

// Get Alchemy URL from appsettings.json
var alchemyUrl = builder.Configuration["Ethereum:AlchemyUrl"];

// Register Web3 as a singleton service
builder.Services.AddSingleton<Web3>(provider =>
{
    return new Web3(alchemyUrl);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ CORS setup - must be BEFORE builder.Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ✅ Now build the app
var app = builder.Build();

app.UseCors("AllowAll");

// Enable serving static files (like index.html and JS)
app.UseDefaultFiles(); // Looks for index.html
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
