namespace MoviesAppSample;

public record MovieDto(long Id, string Title, string? Description, DateTime DateReleased);
public record PersonDto(long Id, string Name);