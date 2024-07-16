namespace BaseLibrary.Responses
{
    public record ResponseList<T>(bool Flag, List<T> Items);
}
