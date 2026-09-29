namespace KittenClaws.Api.Dtos;

public class CatDto
{
    public string Id { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string UpdatedBy { get; set; } = default!;
}

public class DogDto
{
    public string Id { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string UpdatedBy { get; set; } = default!;
}
