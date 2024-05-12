namespace SharedLibrary;

public record LoanApplication(Guid Id,float Income, string? FullName, bool HasDebt, Document[] Documents);