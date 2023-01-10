namespace TomLonghurst.Services.Resilient.Fallback.Builders;

internal interface IIncompleteResilientServiceBuilder
{
    Type Type { get; }
}