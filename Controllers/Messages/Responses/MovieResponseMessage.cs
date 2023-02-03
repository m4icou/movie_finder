using MovieFinder.Domain.Entities;

namespace MovieFinder.Controllers.Messages;

public class MovieResponseMessage
{
    public int id { get; set; }

    public string title { get; set; } = null!;

    public decimal popularity { get; set; }

    public static implicit operator Filme(MovieResponseMessage message)
    {
        if (message == null)
            return null!;

        return new Filme("f" + message.id, message.title, message.popularity.ToString());
    }
}