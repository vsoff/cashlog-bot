using System;

namespace Cashlog.Data
{
    public class PartitionRequest
    {
        public PartitionRequest(int take, int page)
        {
            if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));
            if (page <= 0) throw new ArgumentOutOfRangeException(nameof(take));
            Take = take;
            Page = page;
        }

        public int Take { get; }
        public int Page { get; }
        public int Skip => (Page - 1) * Take;
    }
}