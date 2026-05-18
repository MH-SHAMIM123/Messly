namespace Messly.Application.Common;

public class ForbiddenException : Exception
{
    public ForbiddenException()
        : base("You are not authorized to perform this action.") { }

    public ForbiddenException(string message)
        : base(message) { }
}
