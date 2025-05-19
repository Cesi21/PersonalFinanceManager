using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PersonalFinanceManager.Domain.Models;
using PersonalFinanceManager.Domain.Repositories;


namespace PersonalFinanceManager.Infrastructure.Repositories
{
    public class InMemoryTransactionRepository : ITransactionRepository
    {
        private readonly List<Transaction> _transactions = new();

        public IEnumerable<Transaction> GetAll()
        {
            // Vrnemo kopijo, da zaščitimo interno listo
            return _transactions.ToList();
        }

        public Transaction? GetById(Guid id)
        {
            return _transactions.FirstOrDefault(t => t.Id == id);
        }

        public void Add(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));
            _transactions.Add(transaction);
        }

        public void Update(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var existing = GetById(transaction.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Transakcija z Id {transaction.Id} ni bila najdena.");

            existing.Date = transaction.Date;
            existing.Category = transaction.Category;
            existing.Description = transaction.Description;
            existing.Amount = transaction.Amount;
            existing.Type = transaction.Type;
        }

        public void Delete(Guid id)
        {
            var existing = GetById(id);
            if (existing != null)
                _transactions.Remove(existing);
        }
    }
}
