using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PersonalFinanceManager.Domain.Models;
using PersonalFinanceManager.Domain.Repositories;

namespace PersonalFinanceManager.Infrastructure.Repositories
{
    public class JsonFileTransactionRepository : ITransactionRepository
    {
        private readonly string _filePath;
        private readonly List<Transaction> _transactions;
        private readonly object _lock = new();

        public JsonFileTransactionRepository(string filePath = "transactions.json")
        {
            _filePath = filePath;
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _transactions = JsonSerializer.Deserialize<List<Transaction>>(json)
                                ?? new List<Transaction>();
            }
            else
            {
                _transactions = new List<Transaction>();
            }
        }

        public IEnumerable<Transaction> GetAll()
        {
            lock (_lock) { return _transactions.ToList(); }
        }

        public Transaction? GetById(Guid id)
        {
            lock (_lock) { return _transactions.FirstOrDefault(t => t.Id == id); }
        }

        public void Add(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            lock (_lock)
            {
                _transactions.Add(transaction);
                SaveToFile();
            }
        }

        public void Update(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            lock (_lock)
            {
                var idx = _transactions.FindIndex(t => t.Id == transaction.Id);
                if (idx < 0) throw new KeyNotFoundException($"Id {transaction.Id} not found.");
                _transactions[idx] = transaction;
                SaveToFile();
            }
        }

        public void Delete(Guid id)
        {
            lock (_lock)
            {
                var existing = _transactions.FirstOrDefault(t => t.Id == id);
                if (existing != null)
                {
                    _transactions.Remove(existing);
                    SaveToFile();
                }
            }
        }

        private void SaveToFile()
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_transactions, opts);
            File.WriteAllText(_filePath, json);
        }
    }
}
