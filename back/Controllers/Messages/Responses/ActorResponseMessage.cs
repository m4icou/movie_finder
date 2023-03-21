using MovieFinder.Domain.Entities;

namespace MovieFinder.Controllers.Messages;

public class ActorResponseMessage
{
    public int id { get; set; }

    public string name { get; set; } = null!;

    public static implicit operator Ator(ActorResponseMessage message)
    {
        if (message == null)
            return null!;

        return new Ator("a" + message.id, message.name);
    }
}