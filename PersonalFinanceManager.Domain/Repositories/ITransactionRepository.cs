using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalFinanceManager.Domain.Models;

namespace PersonalFinanceManager.Domain.Repositories
{
    public interface ITransactionRepository
    {
        IEnumerable<Transaction> GetAll();
        Transaction? GetById(Guid id);
        void Add(Transaction transaction);
        void Update(Transaction transaction);
        void Delete(Guid id);
    }
}
