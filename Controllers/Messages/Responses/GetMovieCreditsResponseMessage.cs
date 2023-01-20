namespace MovieFinder.Controllers.Messages;

public class GetMovieCreditsResponseMessage
{
    public int id { get; set; }

    public IEnumerable<ActorResponseMessage> cast { get; set; } = null!;
}