namespace SharedLibrary;

public class LoanApplication
{
    public LoanApplication(Guid id, float income, string? fullName, bool hasDebt)
    {
        Id = id;
        Income = income;
        FullName = fullName;
        HasDebt = hasDebt;
    }
    public Guid Id { get; set; }
    public float Income { get; set; }
    public string? FullName { get; set; }
    public bool HasDebt { get; set; }
}