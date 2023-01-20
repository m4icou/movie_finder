namespace MovieFinder.Controllers.Messages;

public class MovieResponseMessage
{
    public int id { get; set; }

    public string title { get; set; } = null!;

    public decimal popularity { get; set; }
}