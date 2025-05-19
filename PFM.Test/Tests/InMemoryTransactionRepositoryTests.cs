using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using PersonalFinanceManager.Infrastructure.Repositories;
using PersonalFinanceManager.Domain.Models;

namespace PFM.Test.Tests
{
    public class InMemoryTransactionRepositoryTests
    {
        private InMemoryTransactionRepository _repo;

        public InMemoryTransactionRepositoryTests()
        {
            _repo = new InMemoryTransactionRepository();
        }

        [Fact]
        public void Add_ShouldIncreaseCount()
        {
            var tx = new Transaction
            {
                Date = DateTime.Today,
                Category = Category.Groceries,
                Description = "Test dodajanja",
                Amount = 50m,
                Type = TransactionType.Expense
            };

            _repo.Add(tx);
            var all = _repo.GetAll();
            Assert.Single(all);
            Assert.Equal(tx.Id, all.First().Id);
        }

        [Fact]
        public void GetById_ShouldReturnCorrectTransaction()
        {
            var tx = new Transaction { Id = Guid.NewGuid(), Date = DateTime.Today, Amount = 100m, Category = Category.Salary, Type = TransactionType.Income };
            _repo.Add(tx);

            var found = _repo.GetById(tx.Id);

            Assert.NotNull(found);
            Assert.Equal(100m, found.Amount);
            Assert.Equal(TransactionType.Income, found.Type);
        }

        [Fact]
        public void Update_ShouldModifyExistingTransaction()
        {
            var tx = new Transaction { Date = DateTime.Today, Amount = 25m, Category = Category.Utilities, Type = TransactionType.Expense };
            _repo.Add(tx);

            tx.Amount = 75m;
            tx.Description = "Posodobljen opis";
            _repo.Update(tx);

            var updated = _repo.GetById(tx.Id);
            Assert.NotNull(updated);                             
            Assert.Equal(75m, updated!.Amount);                 
            Assert.Equal("Posodobljen opis", updated.Description);
        }

        [Fact]
        public void Delete_ShouldRemoveTransaction()
        {
            var tx = new Transaction { Date = DateTime.Today, Amount = 200m, Category = Category.Other, Type = TransactionType.Expense };
            _repo.Add(tx);

            _repo.Delete(tx.Id);

            var all = _repo.GetAll();
            Assert.Empty(all);
            Assert.Null(_repo.GetById(tx.Id));
        }

        [Fact]
        public void GetAll_ShouldReturnCopy_NotReference()
        {
            var tx = new Transaction { Date = DateTime.Today, Amount = 10m, Category = Category.Groceries, Type = TransactionType.Expense };
            _repo.Add(tx);

            var list1 = _repo.GetAll().ToList();
            var list2 = _repo.GetAll().ToList();
            list1.Clear();

            Assert.Single(list2);
        }
    }
}
