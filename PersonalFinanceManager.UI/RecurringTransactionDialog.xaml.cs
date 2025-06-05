using System;
using System.Windows;
using PersonalFinanceManager.Domain.Models;

namespace PersonalFinanceManager.UI
{
    public partial class RecurringTransactionDialog : Window
    {
        public RecurringTransaction Transaction { get; }

        public RecurringTransactionDialog(RecurringTransaction tx)
        {
            InitializeComponent();
            Transaction = tx;
            DataContext = Transaction;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (Transaction.DayOfMonth < 1 || Transaction.DayOfMonth > 31)
            {
                MessageBox.Show("Enter valid day of month (1-31)", "Invalid", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Transaction.Amount <= 0)
            {
                MessageBox.Show("Amount must be positive", "Invalid", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(Transaction.Description))
            {
                MessageBox.Show("Description required", "Invalid", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }
    }
}
