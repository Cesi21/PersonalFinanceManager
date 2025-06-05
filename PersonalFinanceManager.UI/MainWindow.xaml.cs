using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts.Wpf;
using LiveCharts;
using PersonalFinanceManager.Application.ViewModel;
using PersonalFinanceManager.Domain.Models;
using System.ComponentModel;
using Microsoft.Win32;
using Serilog;
using System.IO;

namespace PersonalFinanceManager.UI
{

    public partial class MainWindow : Window
    {
        private MainViewModel VM => (MainViewModel)DataContext;
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();

            Loaded += (_, __) => RefreshCharts();

            VM.Transactions.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName.StartsWith("ExpenseByCategory")
                 || e.PropertyName.StartsWith("IncomeByCategory")
                 || e.PropertyName.StartsWith("Filter"))
                {
                    RefreshCharts();
                }
            };
        }


        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.StartsWith("ExpenseByCategory") ||
                e.PropertyName.StartsWith("IncomeByCategory") ||
                e.PropertyName.StartsWith("Filter") ||
                e.PropertyName == nameof(TransactionListViewModel.ShowExpensesChart) ||
                e.PropertyName == nameof(TransactionListViewModel.ShowIncomeChart))
            {
                RefreshCharts();
            }
        }

        private void RefreshCharts()
        {
            // PIE CHART Stroški
            var pieData = VM.Transactions.ExpenseByCategory; // Dictionary<string,double>
            ExpensePieChart.Series.Clear();
            foreach (var kv in pieData)
            {
                ExpensePieChart.Series.Add(new PieSeries
                {
                    Title = kv.Key,
                    Values = new ChartValues<double> { kv.Value },
                    DataLabels = true,
                    LabelPoint = cp => $"{cp.Y} ({cp.Participation:P0})"
                });
            }

            // BAR CHART Prihodki
            var labels = VM.Transactions.IncomeByCategory.Keys.ToArray();
            var values = new ChartValues<double>(VM.Transactions.IncomeByCategory.Values);

            IncomeBarChart.Series.Clear();
            IncomeBarChart.Series.Add(new ColumnSeries
            {
                Title = "Income",
                Values = values,
                DataLabels = true
            });
            IncomeBarChart.AxisX[0].Labels = labels;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newTx = new Transaction
            {
                Date = DateTime.Today,
                Category = Category.Groceries,
                Type = TransactionType.Expense,
                Amount = 0m,
                Description = string.Empty
            };

            var dlg = new TransactionDialog(newTx) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                VM.Transactions.AddNewTransaction(newTx);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsDataGrid.SelectedItem is not Transaction selected)
                return;

            // naredimo kopijo, da original uporabimo šele ob Confirm
            var clone = new Transaction
            {
                Id = selected.Id,
                Date = selected.Date,
                Category = selected.Category,
                Type = selected.Type,
                Amount = selected.Amount,
                Description = selected.Description
            };

            var dlg = new TransactionDialog(clone) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                VM.Transactions.UpdateExistingTransaction(clone);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsDataGrid.SelectedItem is Transaction selected)
            {
                var msg = $"Ali ste prepričani, da želite izbrisati transakcijo:\n\n" +
                          $"{selected.Date:d} | {selected.Category} | {selected.Description} | {selected.Amount:C}";
                var result = MessageBox.Show(
                    msg,
                    "Potrditev brisanja",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    VM.Transactions.DeleteTransaction(selected);
                }
            }
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = "transactions.csv"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Date,Category,Description,Amount,Type");
                foreach (var tx in VM.Transactions.FilteredTransactions)
                {
                    var desc = tx.Description?.Replace(",", ";") ?? "";
                    sb.AppendLine($"{tx.Date:yyyy-MM-dd},{tx.Category},{desc},{tx.Amount},{tx.Type}");
                }
                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Export completed.", "Export CSV", MessageBoxButton.OK, MessageBoxImage.Information);
                Log.Information("Exported {Count} transactions to {Path}", VM.Transactions.FilteredTransactions.Count, dlg.FileName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to export CSV to {Path}", dlg.FileName);
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            VM.Transactions.FilterStartDate = null;
            VM.Transactions.FilterEndDate = null;
            VM.Transactions.FilterCategory = null;
            VM.Transactions.FilterType = null;
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            VM.Transactions.Undo();
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            VM.Transactions.Redo();
        }

        private void AddRecurring_Click(object sender, RoutedEventArgs e)
        {
            var tx = new RecurringTransaction
            {
                DayOfMonth = 1,
                Category = Category.Other,
                Type = TransactionType.Expense,
                Amount = 0m,
                Description = string.Empty
            };
            var dlg = new RecurringTransactionDialog(tx) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                VM.Recurring.Add(tx);
            }
        }

        private void EditRecurring_Click(object sender, RoutedEventArgs e)
        {
            if (RecurringDataGrid.SelectedItem is not RecurringTransaction selected)
                return;
            var clone = new RecurringTransaction
            {
                Id = selected.Id,
                DayOfMonth = selected.DayOfMonth,
                Category = selected.Category,
                Type = selected.Type,
                Amount = selected.Amount,
                Description = selected.Description
            };
            var dlg = new RecurringTransactionDialog(clone) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                VM.Recurring.Update(clone);
            }
        }

        private void DeleteRecurring_Click(object sender, RoutedEventArgs e)
        {
            if (RecurringDataGrid.SelectedItem is RecurringTransaction selected)
            {
                VM.Recurring.Delete(selected);
            }
        }
    }
}