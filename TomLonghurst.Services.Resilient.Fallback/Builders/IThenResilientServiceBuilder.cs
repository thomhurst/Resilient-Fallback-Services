namespace TomLonghurst.Services.Resilient.Fallback.Builders;

internal interface IThenResilientServiceBuilder
{
    int ImplementationCount { get; }
    Type Type { get; }
}