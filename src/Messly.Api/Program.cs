using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Messly.Api.Services;
using Messly.Application.Interfaces.Security;
using Messly.Application.Interfaces.Services;
using Messly.Infrastructure;
using Messly.Infrastructure.Data;
using Messly.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException("Jwt:Key must be configured.");
var jwtIssuer = jwtSection["Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer must be configured.");
var jwtAudience = jwtSection["Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience must be configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<MesslyDbContext>("database");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MesslyDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await db.Database.MigrateAsync();
    await DevDataSeeder.SeedAsync(db, config);
    await IdentityDataSeeder.SeedAsync(scope.ServiceProvider, config);

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/v1/auth/login", async (
    LoginRequest request,
    UserManager<ApplicationUser> userManager,
    MesslyDbContext db,
    IConfiguration configuration,
    CancellationToken ct) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        return Results.Unauthorized();

    if (user.DomainUserId is null)
        return Results.Unauthorized();

    var membership = await db.FlatMembers
        .AsNoTracking()
        .Include(fm => fm.Role)
        .Where(fm => fm.UserId == user.DomainUserId && fm.IsActive)
        .OrderBy(fm => fm.JoinedAt)
        .FirstOrDefaultAsync(ct);

    if (membership is null)
        return Results.Unauthorized();

    var section = configuration.GetSection("Jwt");
    var key = section["Key"]!;
    var issuer = section["Issuer"]!;
    var audience = section["Audience"]!;
    var expiryMinutes = int.TryParse(section["ExpiryMinutes"], out var m) ? m : 60;

    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(TenantContext.DomainUserIdClaimType, user.DomainUserId.Value.ToString()),
        new(TenantContext.FlatIdClaimType, membership.FlatId.ToString()),
        new(ClaimTypes.Role, membership.Role?.RoleType.ToString() ?? "Member")
    };

    var token = new JwtSecurityToken(
        issuer,
        audience,
        claims,
        expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
        signingCredentials: new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256));

    return Results.Ok(new LoginResponse(
        new JwtSecurityTokenHandler().WriteToken(token),
        DateTime.UtcNow.AddMinutes(expiryMinutes)));
}).AllowAnonymous();

var api = app.MapGroup("/api/v1").RequireAuthorization();

api.MapGet("/me/members", async (IMemberService service, CancellationToken ct) =>
    Results.Ok(await service.GetMembersAsync(ct)));

api.MapGet("/me/expenses", async (IExpenseService service, CancellationToken ct) =>
    Results.Ok(await service.GetExpensesAsync(ct)));

api.MapGet("/me/deposits", async (IDepositService service, CancellationToken ct) =>
    Results.Ok(await service.GetDepositsAsync(ct)));

api.MapGet("/me/dashboard", async (IDashboardService service, CancellationToken ct) =>
    Results.Ok(await service.GetDashboardAsync(ct)));

app.MapHealthChecks("/health");
app.Run();

internal sealed record LoginRequest(string Email, string Password);
internal sealed record LoginResponse(string AccessToken, DateTime ExpiresAt);
