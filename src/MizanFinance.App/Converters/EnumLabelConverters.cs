using System.Globalization;
using System.Windows.Data;
using MizanFinance.Core.Enums;

namespace MizanFinance.App.Converters;

public class TransactionTypeLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            null => "Tous les types",
            TransactionType.Revenue => "Revenu",
            TransactionType.Expense => "Dépense",
            TransactionType.Transfer => "Virement interne",
            TransactionType.Deposit => "Dépôt",
            TransactionType.Withdrawal => "Retrait",
            TransactionType.Refund => "Remboursement",
            TransactionType.PaymentReceived => "Paiement reçu",
            TransactionType.PaymentIssued => "Paiement émis",
            TransactionType.Other => "Autre",
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public class PaymentMethodLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            PaymentMethod.Cash => "Espèces",
            PaymentMethod.Cheque => "Chèque",
            PaymentMethod.BankTransfer => "Virement",
            PaymentMethod.Card => "Carte",
            PaymentMethod.Other => "Autre",
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public class AccountTypeLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            AccountType.Cash => "Caisse",
            AccountType.Bank => "Banque",
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
