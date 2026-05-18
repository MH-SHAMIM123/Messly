using System.Text;
using Messly.Application.Interfaces.Services;
using Messly.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "Messly_Dev_Signing_Key_Change_In_Production_32chars!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api/v1").RequireAuthorization();

api.MapGet("/members/{flatId:guid}", async (Guid flatId, IMemberService service, CancellationToken ct) =>
    Results.Ok(await service.GetMembersAsync(flatId, ct)));

api.MapGet("/expenses/{flatId:guid}", async (Guid flatId, IExpenseService service, CancellationToken ct) =>
    Results.Ok(await service.GetExpensesAsync(flatId, ct)));

api.MapGet("/deposits/{flatId:guid}", async (Guid flatId, IDepositService service, CancellationToken ct) =>
    Results.Ok(await service.GetDepositsAsync(flatId, ct)));

api.MapGet("/dashboard/{flatId:guid}", async (Guid flatId, IDashboardService service, CancellationToken ct) =>
    Results.Ok(await service.GetDashboardAsync(flatId, ct)));

app.MapHealthChecks("/health");
app.Run();
