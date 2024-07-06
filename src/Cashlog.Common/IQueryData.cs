namespace Cashlog.Common;

public interface IQueryData
{
    int Version { get; set; }
    MenuType MenuType { get; set; }
    string ChatToken { get; set; }
}