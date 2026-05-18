namespace Messly.Application.Common;

public class NotFoundException : Exception
{
    public NotFoundException()
        : base("The requested resource was not found.") { }
}
