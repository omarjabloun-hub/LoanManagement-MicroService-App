namespace SharedLibrary;

public record LoanApplicationFullDto(Guid Id,float Income, string? FullName, bool HasDebt, List<Document> Documents);