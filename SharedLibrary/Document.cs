namespace SharedLibrary;

public record Document(int Id, string FileName, string ContentType, long Size, byte[] Content);