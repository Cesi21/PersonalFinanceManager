using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalFinanceManager.Application.ViewModel;
using PersonalFinanceManager.Domain.Models;
using PersonalFinanceManager.Domain.Repositories;

namespace PFM.Test.Tests
{
    class InMemoryRepo : ITransactionRepository
    {
        private readonly List<Transaction> _store = new List<Transaction>();

        public IEnumerable<Transaction> GetAll() => _store.ToList();
        public Transaction? GetById(Guid id) => _store.FirstOrDefault(t => t.Id == id);
        public void Add(Transaction tx)
        {
            if (tx.Id == Guid.Empty) tx.Id = Guid.NewGuid();
            _store.Add(tx);
        }
        public void Update(Transaction tx)
        {
            var idx = _store.FindIndex(t => t.Id == tx.Id);
            if (idx >= 0) _store[idx] = tx;
            else throw new KeyNotFoundException();
        }
        public void Delete(Guid id)
        {
            var tx = _store.FirstOrDefault(t => t.Id == id);
            if (tx != null) _store.Remove(tx);
        }
    }

    public class TransactionListViewModelTests
    {
        private Transaction New(DateTime date, decimal amt, TransactionType type, Category cat = Category.Other)
            => new Transaction { Date = date, Amount = amt, Type = type, Category = cat, Description = "" };

        [Fact]
        public void Add_Update_Delete_Undo_Redo_Works()
        {
            var repo = new InMemoryRepo();
            var vm = new TransactionListViewModel(repo);

            // 1) Add
            var tx1 = New(DateTime.Today, 100m, TransactionType.Income);
            vm.AddNewTransaction(tx1);
            Assert.Single(vm.FilteredTransactions);
            Assert.Equal(100m, vm.TotalIncome);
            Assert.Equal(0m, vm.TotalExpense);

            // 2) Update
            tx1.Amount = 50m;
            vm.UpdateExistingTransaction(tx1);
            Assert.Equal(50m, vm.TotalIncome);

            // 3) Delete
            vm.DeleteTransaction(tx1);
            Assert.Empty(vm.FilteredTransactions);

            // 4) Undo (should restore delete)
            vm.Undo();
            Assert.Single(vm.FilteredTransactions);

            // 5) Undo (undo update => back to original 100)
            vm.Undo();
            Assert.Equal(100m, vm.TotalIncome);

            // 6) Undo (undo add => empty)
            vm.Undo();
            Assert.Empty(vm.FilteredTransactions);

            // 7) Redo x3
            vm.Redo(); 
            vm.Redo(); 
            vm.Redo(); 
            Assert.Empty(vm.FilteredTransactions);

            // Clean slate: undo delete
            vm.Undo();
            Assert.Single(vm.FilteredTransactions);
        }

        [Fact]
        public void Filters_ByDateCategoryAndType_Work()
        {
            var repo = new InMemoryRepo();
            var vm = new TransactionListViewModel(repo);

            var t1 = New(new DateTime(2025, 1, 1), 10m, TransactionType.Expense, Category.Groceries);
            var t2 = New(new DateTime(2025, 2, 1), 20m, TransactionType.Expense, Category.Rent);
            var t3 = New(new DateTime(2025, 3, 1), 30m, TransactionType.Income, Category.Salary);
            vm.AddNewTransaction(t1);
            vm.AddNewTransaction(t2);
            vm.AddNewTransaction(t3);

            // date filter
            vm.FilterStartDate = new DateTime(2025, 2, 1);
            Assert.DoesNotContain(vm.FilteredTransactions, tx => tx.Date < vm.FilterStartDate);

            // category filter
            vm.FilterStartDate = null;
            vm.FilterCategory = Category.Rent;
            Assert.Single(vm.FilteredTransactions);
            Assert.Equal(Category.Rent, vm.FilteredTransactions[0].Category);

            // type filter
            vm.FilterCategory = null;
            vm.FilterType = TransactionType.Income;
            Assert.Single(vm.FilteredTransactions);
            Assert.Equal(TransactionType.Income, vm.FilteredTransactions[0].Type);

            // clear filters
            vm.FilterType = null;
            Assert.Equal(3, vm.FilteredTransactions.Count);
        }
    }
}
