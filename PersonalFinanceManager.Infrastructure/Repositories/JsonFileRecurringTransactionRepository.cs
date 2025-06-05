using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using PersonalFinanceManager.Domain.Models;
using PersonalFinanceManager.Domain.Repositories;

namespace PersonalFinanceManager.Infrastructure.Repositories
{
    public class JsonFileRecurringTransactionRepository : IRecurringTransactionRepository
    {
        private readonly string _filePath;
        private readonly object _lock = new();
        private List<RecurringTransaction> _items;

        public JsonFileRecurringTransactionRepository(string? path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                var folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PersonalFinanceManager");
                Directory.CreateDirectory(folder);
                _filePath = Path.Combine(folder, "recurring.json");
            }
            else
            {
                _filePath = path;
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            }

            Debug.WriteLine($"[JsonRecurringRepo] Using {_filePath}");

            if (File.Exists(_filePath))
            {
                try
                {
                    var json = File.ReadAllText(_filePath);
                    _items = JsonSerializer.Deserialize<List<RecurringTransaction>>(json) ?? new();
                }
                catch
                {
                    _items = new();
                }
            }
            else
            {
                _items = new();
            }
        }

        public IEnumerable<RecurringTransaction> GetAll()
        {
            lock (_lock) { return _items.ToList(); }
        }

        public RecurringTransaction? GetById(Guid id)
        {
            lock (_lock) { return _items.FirstOrDefault(i => i.Id == id); }
        }

        public void Add(RecurringTransaction tx)
        {
            if (tx == null) throw new ArgumentNullException(nameof(tx));
            lock (_lock)
            {
                _items.Add(tx);
                Save();
            }
        }

        public void Update(RecurringTransaction tx)
        {
            if (tx == null) throw new ArgumentNullException(nameof(tx));
            lock (_lock)
            {
                var idx = _items.FindIndex(t => t.Id == tx.Id);
                if (idx < 0) throw new KeyNotFoundException();
                _items[idx] = tx;
                Save();
            }
        }

        public void Delete(Guid id)
        {
            lock (_lock)
            {
                var existing = _items.FirstOrDefault(t => t.Id == id);
                if (existing != null)
                {
                    _items.Remove(existing);
                    Save();
                }
            }
        }

        private void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_items, options);
            File.WriteAllText(_filePath, json);
        }
    }
}
