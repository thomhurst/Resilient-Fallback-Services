namespace TomLonghurst.Services.Resilient.Fallback.Models;

public static class Argument
{
    public static Argument<T> Of<T>() => new();
}

public struct Argument<T>
{
    public Argument()
    {
    }

    public static implicit operator T(Argument<T> argument) => default!;
}