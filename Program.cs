using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) 
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => 
    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseSerilogRequestLogging(); 
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors();

app.MapPost("/api/login", async (UserLogin login, AppDbContext db, ILogger<Program> logger) => {
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == login.Username);

    if (user == null || user.Password != login.Password) {
        logger.LogWarning("SECURITY: Failed login attempt for user: {User}", login.Username);
        return Results.Unauthorized(); 
    }

    logger.LogInformation("SECURITY: User {User} logged in as {Role}", user.Username, user.Role);
    return Results.Ok(new { user.FullName, user.Role });
});

app.MapGet("/api/admin/system-stats", (string role, ILogger<Program> logger) => {
    if (role != "Admin") {
        logger.LogError("ACCESS DENIED: Non-admin attempted to access system stats.");
        return Results.Forbid(); // Returns 403
    }
    return Results.Ok(new { status = "Healthy", server = "Local-IIS" });
});

app.MapGet("/api/crash", () => {
    throw new Exception("CRITICAL: Manual system crash triggered for log testing!");
});

app.MapGet("/api/maintenance", () => Results.StatusCode(503));
app.MapFallbackToFile("index.html");
app.Run();


public class User {
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class UserLogin {
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
}