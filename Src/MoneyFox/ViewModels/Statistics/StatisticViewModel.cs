using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MediatR;
using MoneyFox.Application.Common.Extensions;
using MoneyFox.Application.Common.Messages;
using MoneyFox.Application.Resources;
using MoneyFox.Common;
using SkiaSharp;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace MoneyFox.ViewModels.Statistics
{
    /// <summary>
    /// Represents the statistic view.
    /// </summary>
    public abstract class StatisticViewModel : ViewModelBase
    {
        private DateTime startDate;
        private DateTime endDate;

        protected readonly IMediator Mediator;

        protected SKColor BackgroundColor { get; }

        /// <summary>
        /// Creates a StatisticViewModel Object and passes the first and last day of the current month     as a start
        /// and end date.
        /// </summary>
        protected StatisticViewModel(IMediator mediator) : this(DateTime.Today.GetFirstDayOfMonth(),
                                                                                                 DateTime.Today.GetLastDayOfMonth(),
                                                                                                 mediator)
        {
        }

        /// <summary>
        /// Creates a Statistic ViewModel with custom start and end date
        /// </summary>
        protected StatisticViewModel(DateTime startDate,
                                     DateTime endDate,
                                     IMediator mediator)
        {
            StartDate = startDate;
            EndDate = endDate;
            Mediator = mediator;

            BackgroundColor = SKColor.Parse(ResourceHelper.GetCurrentBackgroundColor().ToHex());

            MessengerInstance.Register<DateSelectedMessage>(this,
                                                            async message =>
                                                            {
                                                                StartDate = message.StartDate;
                                                                EndDate = message.EndDate;
                                                                await LoadAsync();
                                                            });
        }

        public RelayCommand ShowFilterDialogCommand => new RelayCommand(async () => await new DateSelectionPopup().ShowAsync());

        public RelayCommand LoadedCommand => new RelayCommand(async() => await LoadAsync());

        /// <summary>
        /// Start date for a custom statistic
        /// </summary>
        public DateTime StartDate
        {
            get => startDate;
            set
            {
                startDate = value;
                RaisePropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(Title));
            }
        }

        /// <summary>
        /// End date for a custom statistic
        /// </summary>
        public DateTime EndDate
        {
            get => endDate;
            set
            {
                endDate = value;
                RaisePropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(Title));
            }
        }

        /// <summary>
        /// Returns the title for the CategoryViewModel view
        /// </summary>
        public string Title
                      => $"{Strings.StatisticsTimeRangeTitle} {StartDate.ToString("d", CultureInfo.InvariantCulture)} - {EndDate.ToString("d", CultureInfo.InvariantCulture)}";

        protected abstract Task LoadAsync();
    }
}
