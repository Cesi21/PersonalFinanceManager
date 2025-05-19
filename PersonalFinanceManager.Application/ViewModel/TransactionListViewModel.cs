using System;
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
        public ObservableCollection<Transaction> Transactions { get; }

        public decimal TotalIncome => Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        public decimal TotalExpense => Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        public decimal Balance => TotalIncome - TotalExpense;
        private struct UndoRedoItem
        {
            public Action Undo;
            public Action Redo;
        }

        private readonly Stack<UndoRedoItem> _undoStack = new();
        private readonly Stack<UndoRedoItem> _redoStack = new();

        public bool CanUndo => _undoStack.Any();
        public bool CanRedo => _redoStack.Any();

        public TransactionListViewModel()
            : this(new JsonFileTransactionRepository()) { }

        public TransactionListViewModel(ITransactionRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            Transactions = new ObservableCollection<Transaction>();
            LoadTransactions();
        }

        public void AddNewTransaction(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            // shrani undo/redo zapise
            _undoStack.Push(new UndoRedoItem
            {
                Undo = () => DeleteInternal(transaction),
                Redo = () => AddInternal(transaction)
            });
            _redoStack.Clear();

            AddInternal(transaction);
            NotifyAll();
        }

        public void UpdateExistingTransaction(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            var old = _repository.GetById(transaction.Id);
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
                Redo = () => UpdateInternal(transaction)
            });
            _redoStack.Clear();

            UpdateInternal(transaction);
            NotifyAll();
        }

        public void DeleteTransaction(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            // kopija za undo
            var backup = new Transaction
            {
                Id = transaction.Id,
                Date = transaction.Date,
                Category = transaction.Category,
                Type = transaction.Type,
                Amount = transaction.Amount,
                Description = transaction.Description
            };

            _undoStack.Push(new UndoRedoItem
            {
                Undo = () => AddInternal(backup),
                Redo = () => DeleteInternal(backup)
            });
            _redoStack.Clear();

            DeleteInternal(transaction);
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
            Transactions.Add(tx);
        }

        private void UpdateInternal(Transaction tx)
        {
            _repository.Update(tx);
            LoadTransactions();
        }

        private void DeleteInternal(Transaction tx)
        {
            _repository.Delete(tx.Id);
            var existing = Transactions.FirstOrDefault(t => t.Id == tx.Id);
            if (existing != null) Transactions.Remove(existing);
        }

        private void NotifyAll()
        {
            OnPropertyChanged(nameof(TotalIncome));
            OnPropertyChanged(nameof(TotalExpense));
            OnPropertyChanged(nameof(Balance));
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
        }

        public void LoadTransactions()
        {
            Transactions.Clear();
            foreach (var tx in _repository.GetAll())
                Transactions.Add(tx);
            OnPropertyChanged(nameof(TotalIncome));
            OnPropertyChanged(nameof(TotalExpense));
            OnPropertyChanged(nameof(Balance));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
