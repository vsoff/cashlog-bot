﻿using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Modules.Calculator;

public class ClosingPeriodResult
{
    public BillingPeriod PreviousPeriod { get; set; }
    public BillingPeriod NewPeriod { get; set; }
    public MoneyOperation[] Debts { get; set; }
}