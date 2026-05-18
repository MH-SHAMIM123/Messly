using Messly.Domain.Common;

namespace Messly.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Flat> CreatedFlats { get; set; } = new List<Flat>();
    public ICollection<FlatMember> FlatMemberships { get; set; } = new List<FlatMember>();
    public ICollection<Meal> Meals { get; set; } = new List<Meal>();
    public ICollection<Expense> PaidExpenses { get; set; } = new List<Expense>();
    public ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();
}
