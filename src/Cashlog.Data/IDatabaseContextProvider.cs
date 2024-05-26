namespace Cashlog.Data;

public interface IDatabaseContextProvider
{
    ApplicationContext Create();
}