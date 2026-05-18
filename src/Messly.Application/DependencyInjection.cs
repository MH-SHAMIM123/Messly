using FluentValidation;
using Messly.Application.Interfaces.Security;
using Messly.Application.Interfaces.Services;
using Messly.Application.Services.Security;
using Messly.Application.Validators;
using Messly.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Messly.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFlatAuthorizationService, FlatAuthorizationService>();
        services.AddValidatorsFromAssemblyContaining<MemberUpsertDtoValidator>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IMealService, MealService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IDepositService, DepositService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IFlatService, FlatService>();
        services.AddScoped<IBillingCalculationService, BillingCalculationService>();

        return services;
    }
}
