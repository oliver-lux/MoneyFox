namespace MoneyFox.Ui.Views.Statistics.CategorySummary;

using System.Collections.ObjectModel;
using AutoMapper;
using Common.Navigation;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.Queries;
using MediatR;
using Payments.PaymentModification;
using Resources.Strings;

internal sealed class PaymentForCategoryListViewModel(IMediator mediator, IMapper mapper, INavigationService navigationService) : NavigableViewModel,
    IRecipient<PaymentsForCategoryMessage>
{
    private ReadOnlyObservableCollection<PaymentDayGroup> paymentDayGroups = new(new());

    private string title = string.Empty;

    public string Title
    {
        get => title;
        private set => SetProperty(field: ref title, newValue: value);
    }

    public ReadOnlyObservableCollection<PaymentDayGroup> PaymentDayGroups
    {
        get => paymentDayGroups;

        private set
        {
            SetProperty(field: ref paymentDayGroups, newValue: value);
            OnPropertyChanged(nameof(TotalRevenue));
            OnPropertyChanged(nameof(TotalExpenses));
        }
    }

    public decimal TotalRevenue => PaymentDayGroups.Sum(pdg => pdg.TotalRevenue);

    public decimal TotalExpenses => PaymentDayGroups.Sum(pdg => pdg.TotalExpense);

    public AsyncRelayCommand<PaymentListItemViewModel> GoToEditPaymentCommand => new(pvm => navigationService.GoTo<EditPaymentViewModel>(pvm!.Id));

    public void Receive(PaymentsForCategoryMessage message) { }

    public override async Task OnNavigatedAsync(object? parameter)
    {
        var paymentsForCategoryParameter = (PaymentsForCategoryParameter)parameter!;
        if (paymentsForCategoryParameter.CategoryId.HasValue)
        {
            var category = await mediator.Send(new GetCategoryByIdQuery(paymentsForCategoryParameter.CategoryId.Value));
            Title = string.Format(format: Translations.PaymentsForCategoryTitle, arg0: category.Name);
        }
        else
        {
            Title = Translations.NoCategoryTitle;
        }

        var paymentVms = mapper.Map<List<PaymentListItemViewModel>>(
            mediator.Send(
                    new GetPaymentsForCategorySummary.Query(
                        CategoryId: paymentsForCategoryParameter.CategoryId,
                        DateRangeFrom: paymentsForCategoryParameter.StartDate,
                        DateRangeTo: paymentsForCategoryParameter.EndDate))
                .GetAwaiter()
                .GetResult());

        var dailyGroupedPayments = paymentVms.GroupBy(p => p.Date.Date)
            .OrderByDescending(p => p.Key)
            .Select(g => new PaymentDayGroup(date: DateOnly.FromDateTime(g.Key), payments: g.ToList()))
            .ToList();

        PaymentDayGroups = new(new(dailyGroupedPayments));
    }
}

public record PaymentsForCategoryParameter(int? CategoryId, DateTime StartDate, DateTime EndDate);
