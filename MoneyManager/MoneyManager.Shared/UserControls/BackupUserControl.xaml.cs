﻿#region

using Windows.UI.Xaml;
using Microsoft.Practices.ServiceLocation;
using MoneyManager.Business.ViewModels;

#endregion

namespace MoneyManager.UserControls {
	public sealed partial class BackupUserControl {
		public BackupUserControl() {
			InitializeComponent();
		}

		private BackupViewModel backupView {
			get { return ServiceLocator.Current.GetInstance<BackupViewModel>(); }
		}

		private async void LoginToOneDrive(object sender, RoutedEventArgs e) {
			await backupView.LogInToOneDrive();
		}

		private async void CreateBackup(object sender, RoutedEventArgs e) {
			await backupView.CreateBackup();
		}

		private async void RestoreBackup(object sender, RoutedEventArgs e) {
			await backupView.RestoreBackup();
		}
	}
}