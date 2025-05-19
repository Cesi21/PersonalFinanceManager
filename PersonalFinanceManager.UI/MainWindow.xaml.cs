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
using PersonalFinanceManager.Application.ViewModels;
using PersonalFinanceManager.Domain.Models;

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