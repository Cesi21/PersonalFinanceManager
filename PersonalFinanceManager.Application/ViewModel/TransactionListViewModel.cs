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

        public TransactionListViewModel()
            : this(new InMemoryTransactionRepository()) { }

        public TransactionListViewModel(ITransactionRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            Transactions = new ObservableCollection<Transaction>();
            LoadTransactions();
        }

        public void AddNewTransaction(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            _repository.Add(transaction);
            Transactions.Add(transaction);
            OnPropertyChanged(nameof(TotalIncome));
            OnPropertyChanged(nameof(TotalExpense));
            OnPropertyChanged(nameof(Balance));
        }

        public void UpdateExistingTransaction(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            _repository.Update(transaction);
            LoadTransactions();
        }

        public void DeleteTransaction(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            _repository.Delete(transaction.Id);
            Transactions.Remove(transaction);
            OnPropertyChanged(nameof(TotalIncome));
            OnPropertyChanged(nameof(TotalExpense));
            OnPropertyChanged(nameof(Balance));
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
