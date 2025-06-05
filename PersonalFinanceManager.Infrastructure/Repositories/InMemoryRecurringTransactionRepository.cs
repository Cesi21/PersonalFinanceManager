using System;
using System.Collections.Generic;
using System.Linq;
using PersonalFinanceManager.Domain.Models;
using PersonalFinanceManager.Domain.Repositories;

namespace PersonalFinanceManager.Infrastructure.Repositories
{
    public class InMemoryRecurringTransactionRepository : IRecurringTransactionRepository
    {
        private readonly List<RecurringTransaction> _list = new();

        public IEnumerable<RecurringTransaction> GetAll() => _list.ToList();

        public RecurringTransaction? GetById(Guid id) => _list.FirstOrDefault(t => t.Id == id);

        public void Add(RecurringTransaction tx)
        {
            if (tx == null) throw new ArgumentNullException(nameof(tx));
            _list.Add(tx);
        }

        public void Update(RecurringTransaction tx)
        {
            if (tx == null) throw new ArgumentNullException(nameof(tx));
            var idx = _list.FindIndex(t => t.Id == tx.Id);
            if (idx >= 0) _list[idx] = tx;
            else throw new KeyNotFoundException();
        }

        public void Delete(Guid id)
        {
            var tx = GetById(id);
            if (tx != null) _list.Remove(tx);
        }
    }
}
