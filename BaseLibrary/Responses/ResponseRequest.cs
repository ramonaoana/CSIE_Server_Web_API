namespace BaseLibrary.Responses
{
    public record ResponseRequest<T>(bool Flag, T obj);
}
