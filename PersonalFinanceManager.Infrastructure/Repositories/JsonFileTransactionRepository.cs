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
    public class JsonFileTransactionRepository : ITransactionRepository
    {
        private readonly string _filePath;
        private List<Transaction> _transactions;
        private readonly object _lock = new();

        public JsonFileTransactionRepository(string? customPath = null)
        {
            // 1) Privzeta pot %LOCALAPPDATA%\PersonalFinanceManager\transactions.json
            if (string.IsNullOrWhiteSpace(customPath))
            {
                var folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PersonalFinanceManager"
                );
                Directory.CreateDirectory(folder);
                _filePath = Path.Combine(folder, "transactions.json");
            }
            else
            {
                _filePath = customPath;
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            }

            Debug.WriteLine($"[JsonRepo] Using file: {_filePath}");

            // 2) Naloži ali inicializiraj
            if (File.Exists(_filePath))
            {
                try
                {
                    var json = File.ReadAllText(_filePath);
                    _transactions = JsonSerializer.Deserialize<List<Transaction>>(json)
                                 ?? new List<Transaction>();
                    Debug.WriteLine($"[JsonRepo] Loaded {_transactions.Count} transactions.");
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"[JsonRepo] JSON load failed: {ex.Message}");
                    // preimenuj poškodovano datoteko v .bak
                    var backup = _filePath + ".bak";
                    File.Move(_filePath, backup, overwrite: true);
                    Debug.WriteLine($"[JsonRepo] Renamed corrupted file to {backup}");
                    _transactions = new List<Transaction>();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[JsonRepo] Unexpected error loading JSON: {ex}");
                    _transactions = new List<Transaction>();
                }
            }
            else
            {
                Debug.WriteLine("[JsonRepo] File not found, starting with empty list.");
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
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_transactions, options);
                File.WriteAllText(_filePath, json);
                Debug.WriteLine($"[JsonRepo] Saved {_transactions.Count} transactions.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JsonRepo] Error saving JSON: {ex}");
                throw;
            }
        }
    }
}
