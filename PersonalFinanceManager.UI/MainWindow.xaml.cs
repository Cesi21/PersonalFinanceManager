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

namespace PersonalFinanceManager.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TransactionListViewModel VM => (TransactionListViewModel)DataContext;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new TransactionListViewModel();

            Loaded += (_, __) => RefreshCharts();

            VM.PropertyChanged += ViewModel_PropertyChanged;

        }


        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Osveži grafe, če se spremeni karkoli, kar vpliva na podatke
            if (e.PropertyName.StartsWith("ExpenseByCategory") ||
                e.PropertyName.StartsWith("IncomeByCategory") ||
                e.PropertyName.StartsWith("Filter") ||
                e.PropertyName == nameof(VM.ShowExpensesChart) ||
                e.PropertyName == nameof(VM.ShowIncomeChart))
            {
                RefreshCharts();
            }
        }

        private void RefreshCharts()
        {
            // ––– PieChart (stroški) ––––––––––––––––––––––––––––––––––––––––––––––
            var pieData = VM.ExpenseByCategory; // Dictionary<string,double>
            ExpensePieChart.Series = new SeriesCollection(
                pieData.Select(kv =>
                    new PieSeries
                    {
                        Title = kv.Key,
                        Values = new ChartValues<double> { kv.Value },
                        DataLabels = true
                    })
            );

            // ––– BarChart (samo prihodki) –––––––––––––––––––––––––––––––––––––––––
            var labels = VM.IncomeByCategory.Keys.ToArray();
            var values = new ChartValues<double>(VM.IncomeByCategory.Values);

            IncomeBarChart.Series = new SeriesCollection {
                new ColumnSeries {
                    Title      = "Prihodki",
                    Values     = values,
                    DataLabels = true
                }
            };

            // Osveži osi – et voilà, stolpčni graf samo za prihodke
            IncomeBarChart.AxisX.Clear();
            IncomeBarChart.AxisX.Add(new Axis
            {
                Title = "Kategorija",
                Labels = labels
            });
            IncomeBarChart.AxisY.Clear();
            IncomeBarChart.AxisY.Add(new Axis
            {
                Title = "Znesek"
            });
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
                VM.AddNewTransaction(newTx);
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
                VM.UpdateExistingTransaction(clone);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsDataGrid.SelectedItem is Transaction selected)
            {
                VM.DeleteTransaction(selected);
            }
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            VM.Undo();
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            VM.Redo();
        }
    }
}