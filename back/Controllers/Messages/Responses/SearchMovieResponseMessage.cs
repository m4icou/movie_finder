namespace MovieFinder.Controllers.Messages;

public class SearchMovieResponseMessage
{
    public int page { get; set; }

    public IEnumerable<MovieResponseMessage> results { get; set; } = null!;
}