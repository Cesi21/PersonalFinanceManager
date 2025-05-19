using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PersonalFinanceManager.Domain.Models;

namespace PersonalFinanceManager.UI
{
    public partial class TransactionDialog : Window
    {
        public Transaction Transaction { get; }

        public TransactionDialog(Transaction transaction)
        {
            InitializeComponent();
            Transaction = transaction;
            DataContext = Transaction;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (DatePickerControl.SelectedDate == null)
            {
                MessageBox.Show("Izberi datum transakcije.",
                                "Neveljaven vnos",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (CategoryComboBox.SelectedValue == null)
            {
                MessageBox.Show("Izberi kategorijo.",
                                "Neveljaven vnos",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }


            if (!decimal.TryParse(AmountTextBox.Text, out var amount) || amount <= 0)
            {
                MessageBox.Show("Vnesi pozitivno decimalno število za znesek.",
                                "Neveljaven vnos",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }


            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
               MessageBox.Show("Neveljaven vnos");
                return;
            }


            DialogResult = true;
        }
    }
}
