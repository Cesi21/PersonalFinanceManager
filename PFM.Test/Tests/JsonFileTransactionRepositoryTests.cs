using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalFinanceManager.Domain.Models;
using PersonalFinanceManager.Infrastructure.Repositories;

namespace PFM.Test.Tests
{
    public class JsonFileTransactionRepositoryTests : IDisposable
    {
        private readonly string _path;
        private readonly JsonFileTransactionRepository _repo;

        public JsonFileTransactionRepositoryTests()
        {
            // unikatna temp datoteka za vsak test
            _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
            _repo = new JsonFileTransactionRepository(_path);
        }

        [Fact]
        public void Add_Update_Delete_PersistsCorrectly()
        {
            // initially empty
            Assert.Empty(_repo.GetAll());

            // Add
            var tx = new Transaction
            {
                Date = new DateTime(2025, 5, 19),
                Category = Category.Other,
                Type = TransactionType.Income,
                Amount = 123.45m,
                Description = "Test"
            };
            _repo.Add(tx);
            var all1 = _repo.GetAll().ToList();
            Assert.Single(all1);
            Assert.Equal(123.45m, all1[0].Amount);

            // Update
            tx.Amount = 200m;
            _repo.Update(tx);
            var all2 = _repo.GetAll().ToList();
            Assert.Single(all2);
            Assert.Equal(200m, all2[0].Amount);

            // Reload repo from same file and check
            var repo2 = new JsonFileTransactionRepository(_path);
            var all3 = repo2.GetAll().ToList();
            Assert.Single(all3);
            Assert.Equal(200m, all3[0].Amount);

            // Delete
            _repo.Delete(tx.Id);
            Assert.Empty(_repo.GetAll());
        }

        [Fact]
        public void CorruptedFile_IsBackedUpAndResets()
        {
            // zapisi neveljaven JSON
            File.WriteAllText(_path, "{ not valid json ");
            // nova instanca repo bo ulovila JsonException
            var repo2 = new JsonFileTransactionRepository(_path);
            // .bak datoteka naj obstaja
            Assert.True(File.Exists(_path + ".bak"));
            // new repo ima prazen seznam
            Assert.Empty(repo2.GetAll());
        }

        public void Dispose()
        {
            // pospravi temp file/i
            try { File.Delete(_path); } catch { }
            try { File.Delete(_path + ".bak"); } catch { }
        }
    }
}
