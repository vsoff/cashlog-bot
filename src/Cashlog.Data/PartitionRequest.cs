namespace Cashlog.Data;

public class PartitionRequest
{
    public PartitionRequest(int take, int page)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        Take = take;
        Page = page;
    }

    public int Take { get; }
    public int Page { get; }
    public int Skip => (Page - 1) * Take;
}