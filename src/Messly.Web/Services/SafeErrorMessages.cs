using Messly.Application.Common;

namespace Messly.Web.Services;

public static class SafeErrorMessages
{
    public static string FromException(Exception ex) => ex switch
    {
        ForbiddenException => "You are not authorized to perform this action.",
        NotFoundException => "The requested resource was not found.",
        BusinessException business => business.Message,
        _ => "An unexpected error occurred. Please try again."
    };
}
