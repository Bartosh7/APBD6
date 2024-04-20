namespace APBD6.DTOs;

public record GetAllAnimalsResponse(int IdAnimal, string Name, string Description, string Category, string Area);
public record CreateAnimalsRequest(string Name, string Description, string Category, string Area);
