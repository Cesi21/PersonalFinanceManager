using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using PersonalFinanceManager.Domain.Models;
using PersonalFinanceManager.Domain.Repositories;
using PersonalFinanceManager.Infrastructure.Repositories;

namespace PersonalFinanceManager.Application.ViewModels
{
    public class TransactionListViewModel : INotifyPropertyChanged
    {
        private readonly ITransactionRepository _repository;
        private List<Transaction> _allTransactions = new();

        public ObservableCollection<Transaction> FilteredTransactions { get; }

        // Filters

        private DateTime? _filterStartDate;
        public DateTime? FilterStartDate
        {
            get => _filterStartDate;
            set
            {
                if (_filterStartDate != value)
                {
                    _filterStartDate = value;
                    OnPropertyChanged(nameof(FilterStartDate));
                    ApplyFilters();
                }
            }
        }

        private DateTime? _filterEndDate;
        public DateTime? FilterEndDate
        {
            get => _filterEndDate;
            set
            {
                if (_filterEndDate != value)
                {
                    _filterEndDate = value;
                    OnPropertyChanged(nameof(FilterEndDate));
                    ApplyFilters();
                }
            }
        }

        private Category? _filterCategory;
        public Category? FilterCategory
        {
            get => _filterCategory;
            set
            {
                if (_filterCategory != value)
                {
                    _filterCategory = value;
                    OnPropertyChanged(nameof(FilterCategory));
                    ApplyFilters();
                }
            }
        }

        private TransactionType? _filterType;
        public TransactionType? FilterType
        {
            get => _filterType;
            set
            {
                if (_filterType != value)
                {
                    _filterType = value;
                    OnPropertyChanged(nameof(FilterType));
                    ApplyFilters();
                }
            }
        }

        // tatistics on filtered set 

        public decimal TotalIncome => FilteredTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        public decimal TotalExpense => FilteredTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        public decimal Balance => TotalIncome - TotalExpense;

        // Grouped data for charts

        public Dictionary<string, double> ExpenseByCategory =>
            FilteredTransactions
              .Where(t => t.Type == TransactionType.Expense)
              .GroupBy(t => t.Category)
              .ToDictionary(g => g.Key.ToString(), g => (double)g.Sum(t => t.Amount));

        public Dictionary<string, double> IncomeByCategory =>
            FilteredTransactions
              .Where(t => t.Type == TransactionType.Income)
              .GroupBy(t => t.Category)
              .ToDictionary(g => g.Key.ToString(), g => (double)g.Sum(t => t.Amount));

        // Undo / Redo

        private struct UndoRedoItem
        {
            public Action Undo;
            public Action Redo;
        }
        private readonly Stack<UndoRedoItem> _undoStack = new();
        private readonly Stack<UndoRedoItem> _redoStack = new();

        public bool CanUndo => _undoStack.Any();
        public bool CanRedo => _redoStack.Any();

        // Constructor

        public TransactionListViewModel()
            : this(new JsonFileTransactionRepository()) { }

        public TransactionListViewModel(ITransactionRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            FilteredTransactions = new ObservableCollection<Transaction>();
            LoadTransactions();
        }

        // CRUD + Undo/Redo

        public void AddNewTransaction(Transaction tx)
        {
            if (tx == null) throw new ArgumentNullException(nameof(tx));

            _undoStack.Push(new UndoRedoItem
            {
                Undo = () => DeleteInternal(tx),
                Redo = () => AddInternal(tx)
            });
            _redoStack.Clear();

            AddInternal(tx);
            NotifyAll();
        }

        public void UpdateExistingTransaction(Transaction tx)
        {
            if (tx == null) throw new ArgumentNullException(nameof(tx));
            var old = _repository.GetById(tx.Id);
            if (old == null) return;

            var backup = new Transaction
            {
                Id = old.Id,
                Date = old.Date,
                Category = old.Category,
                Type = old.Type,
                Amount = old.Amount,
                Description = old.Description
            };

            _undoStack.Push(new UndoRedoItem
            {
                Undo = () => UpdateInternal(backup),
                Redo = () => UpdateInternal(tx)
            });
            _redoStack.Clear();

            UpdateInternal(tx);
            NotifyAll();
        }

        public void DeleteTransaction(Transaction tx)
        {
            if (tx == null) throw new ArgumentNullException(nameof(tx));

            var backup = new Transaction
            {
                Id = tx.Id,
                Date = tx.Date,
                Category = tx.Category,
                Type = tx.Type,
                Amount = tx.Amount,
                Description = tx.Description
            };

            _undoStack.Push(new UndoRedoItem
            {
                Undo = () => AddInternal(backup),
                Redo = () => DeleteInternal(backup)
            });
            _redoStack.Clear();

            DeleteInternal(tx);
            NotifyAll();
        }

        public void Undo()
        {
            if (!CanUndo) return;
            var item = _undoStack.Pop();
            item.Undo();
            _redoStack.Push(item);
            NotifyAll();
        }

        public void Redo()
        {
            if (!CanRedo) return;
            var item = _redoStack.Pop();
            item.Redo();
            _undoStack.Push(item);
            NotifyAll();
        }

        private void AddInternal(Transaction tx)
        {
            _repository.Add(tx);
            _allTransactions.Add(tx);
            ApplyFilters();
        }

        private void UpdateInternal(Transaction tx)
        {
            _repository.Update(tx);
            var idx = _allTransactions.FindIndex(x => x.Id == tx.Id);
            if (idx >= 0) _allTransactions[idx] = tx;
            ApplyFilters();
        }

        private void DeleteInternal(Transaction tx)
        {
            _repository.Delete(tx.Id);
            _allTransactions.RemoveAll(x => x.Id == tx.Id);
            ApplyFilters();
        }

        private void NotifyAll()
        {
            OnPropertyChanged(nameof(TotalIncome));
            OnPropertyChanged(nameof(TotalExpense));
            OnPropertyChanged(nameof(Balance));
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(ExpenseByCategory));
            OnPropertyChanged(nameof(IncomeByCategory));
        }

        // Load & Filter

        public void LoadTransactions()
        {
            _allTransactions = _repository.GetAll().ToList();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allTransactions
                .Where(t => !FilterStartDate.HasValue || t.Date >= FilterStartDate.Value)
                .Where(t => !FilterEndDate.HasValue || t.Date <= FilterEndDate.Value)
                .Where(t => !FilterCategory.HasValue || t.Category == FilterCategory.Value)
                .Where(t => !FilterType.HasValue || t.Type == FilterType.Value)
                .ToList();

            FilteredTransactions.Clear();
            foreach (var t in filtered)
                FilteredTransactions.Add(t);

            NotifyAll();
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
