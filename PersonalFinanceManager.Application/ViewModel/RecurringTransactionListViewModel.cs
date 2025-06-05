using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using PersonalFinanceManager.Domain.Models;
using PersonalFinanceManager.Domain.Repositories;
using PersonalFinanceManager.Infrastructure.Repositories;

namespace PersonalFinanceManager.Application.ViewModel;

public class RecurringTransactionListViewModel : INotifyPropertyChanged
{
    private readonly IRecurringTransactionRepository _repo;

    public ObservableCollection<RecurringTransaction> Items { get; } = new();

    public RecurringTransactionListViewModel() : this(new JsonFileRecurringTransactionRepository()) {}

    public RecurringTransactionListViewModel(IRecurringTransactionRepository repo)
    {
        _repo = repo;
        Load();
    }

    public void Load()
    {
        Items.Clear();
        foreach (var r in _repo.GetAll())
            Items.Add(r);
    }

    public void Add(RecurringTransaction tx)
    {
        _repo.Add(tx);
        Items.Add(tx);
        OnPropertyChanged(nameof(Items));
    }

    public void Update(RecurringTransaction tx)
    {
        _repo.Update(tx);
        var idx = Items.ToList().FindIndex(t => t.Id == tx.Id);
        if (idx >= 0) Items[idx] = tx;
        OnPropertyChanged(nameof(Items));
    }

    public void Delete(RecurringTransaction tx)
    {
        _repo.Delete(tx.Id);
        Items.Remove(tx);
        OnPropertyChanged(nameof(Items));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
