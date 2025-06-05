using System;
using System.Collections.Generic;
using PersonalFinanceManager.Domain.Models;

namespace PersonalFinanceManager.Domain.Repositories
{
    public interface IRecurringTransactionRepository
    {
        IEnumerable<RecurringTransaction> GetAll();
        RecurringTransaction? GetById(Guid id);
        void Add(RecurringTransaction transaction);
        void Update(RecurringTransaction transaction);
        void Delete(Guid id);
    }
}
