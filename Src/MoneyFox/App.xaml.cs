﻿namespace MoneyFox
{

    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Common.Exceptions;
    using Core._Pending_.Common.Facades;
    using Core.ApplicationCore.UseCases.BackupUpload;
    using Core.ApplicationCore.UseCases.DbBackup;
    using Core.Commands.Payments.ClearPayments;
    using Core.Commands.Payments.CreateRecurringPayments;
    using Core.Common;
    using Core.Common.Interfaces;
    using Core.Resources;
    using InversionOfControl;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Mobile.Infrastructure.Adapters;
    using Serilog;
    using ViewModels;
    using Xamarin.Forms;

    public partial class App
    {
        private bool isRunning;

        public App(Action<IServiceCollection> addPlatformServices = null)
        {
            Device.SetFlags(new[] { "AppTheme_Experimental", "SwipeView_Experimental" });
            var settingsFacade = new SettingsFacade(new SettingsAdapter());
            CultureHelper.CurrentCulture = new CultureInfo(settingsFacade.DefaultCulture);
            InitializeComponent();
            SetupServices(addPlatformServices);
            MainPage = new AppShell();
            if (!settingsFacade.IsSetupCompleted)
            {
                Shell.Current.GoToAsync(ViewModelLocator.WelcomeViewRoute).Wait();
            }
        }

        private static IServiceProvider? ServiceProvider { get; set; }

        internal static BaseViewModel GetViewModel<TViewModel>() where TViewModel : BaseViewModel
        {
            return ServiceProvider?.GetService<TViewModel>() ?? throw new ResolveViewModeException<TViewModel>();
        }

        protected override void OnStart()
        {
            StartupTasksAsync().ConfigureAwait(false);
        }

        protected override void OnResume()
        {
            StartupTasksAsync().ConfigureAwait(false);
        }

        protected override async void OnSleep()
        {
            await StartupTasksAsync();
        }

        private static void SetupServices(Action<IServiceCollection>? addPlatformServices)
        {
            var services = new ServiceCollection();
            addPlatformServices?.Invoke(services);
            new MoneyFoxConfig().Register(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private async Task StartupTasksAsync()
        {
            // Don't execute this again when already running
            if (isRunning)
            {
                return;
            }

            if (ServiceProvider == null)
            {
                return;
            }

            isRunning = true;
            var toastService = ServiceProvider.GetService<IToastService>();
            var settingsFacade = ServiceProvider.GetService<ISettingsFacade>();
            var mediator = ServiceProvider.GetService<IMediator>();
            try
            {
                if (settingsFacade.IsBackupAutoUploadEnabled && settingsFacade.IsLoggedInToBackupService)
                {
                    var backupService = ServiceProvider.GetService<IBackupService>();
                    await backupService.RestoreBackupAsync();
                }

                await mediator.Send(new ClearPaymentsCommand());
                await mediator.Send(new CreateRecurringPaymentsCommand());
                var uploadResult = await mediator.Send(new UploadBackup.Command());
                if (uploadResult == UploadBackup.UploadResult.Successful)
                {
                    await toastService.ShowToastAsync(Strings.BackupCreatedMessage);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(exception: ex, messageTemplate: "Error during startup");
            }
            finally
            {
                settingsFacade.LastExecutionTimeStampSyncBackup = DateTime.Now;
                isRunning = false;
            }
        }
    }

}
