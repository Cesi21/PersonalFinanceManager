using System;
using System.Linq;
using PersonalFinanceManager.Domain.Models;

namespace PersonalFinanceManager.Application.ViewModel;

public class MainViewModel
{
    public TransactionListViewModel Transactions { get; }
    public RecurringTransactionListViewModel Recurring { get; }

    public MainViewModel()
    {
        Transactions = new TransactionListViewModel();
        Recurring = new RecurringTransactionListViewModel();
        ApplyRecurringForCurrentMonth();
    }

    private void ApplyRecurringForCurrentMonth()
    {
        var now = DateTime.Today;
        foreach (var r in Recurring.Items)
        {
            var date = new DateTime(now.Year, now.Month, Math.Min(r.DayOfMonth, DateTime.DaysInMonth(now.Year, now.Month)));
            bool exists = Transactions.FilteredTransactions.Any(t => t.Description == r.Description && t.Date == date && t.Amount == r.Amount);
            if (!exists)
            {
                var tx = new Transaction
                {
                    Date = date,
                    Category = r.Category,
                    Description = r.Description,
                    Amount = r.Amount,
                    Type = r.Type
                };
                Transactions.AddNewTransaction(tx);
            }
        }
    }
}
