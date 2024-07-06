namespace Cashlog.Core.Calculator;

public class ReceiptCalculatorInfo
{
    public double Amount { get; set; }
    public long CustomerId { get; set; }
    public long[] ConsumerIds { get; set; }
}