using Cashlog.Data;

namespace Cashlog.Core.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = nameof(DatabaseOptions);
    
    public string DataBaseConnectionString { get; init; }
    public DataProviderType DataProviderType { get; init; }
}