using System;

namespace PersonalFinanceManager.Domain.Models
{
    public class RecurringTransaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int DayOfMonth { get; set; }
        public Category Category { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
    }
}
