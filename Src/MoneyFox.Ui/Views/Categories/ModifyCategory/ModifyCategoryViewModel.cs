namespace MoneyFox.Ui.Views.Categories.ModifyCategory;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.Common.Interfaces;
using Core.Queries;
using MediatR;
using Messages;
using Resources.Strings;

public abstract class ModifyCategoryViewModel : BasePageViewModel
{
    private readonly IDialogService dialogService;
    private readonly IMediator mediator;

    private CategoryViewModel selectedCategory = null!;

    protected ModifyCategoryViewModel(IMediator mediator, IDialogService dialogService)
    {
        this.mediator = mediator;
        this.dialogService = dialogService;
    }

    public AsyncRelayCommand SaveCommand => new(async () => await SaveCategoryBaseAsync());

    public CategoryViewModel SelectedCategory
    {
        get => selectedCategory;

        set
        {
            selectedCategory = value;
            OnPropertyChanged();
        }
    }

    protected abstract Task SaveCategoryAsync();

    protected virtual async Task SaveCategoryBaseAsync()
    {
        if (string.IsNullOrEmpty(SelectedCategory.Name))
        {
            await dialogService.ShowMessageAsync(title: Translations.MandatoryFieldEmptyTitle, message: Translations.NameRequiredMessage);

            return;
        }

        if (await mediator.Send(new GetIfCategoryWithNameExistsQuery(SelectedCategory.Name, SelectedCategory.Id)))
        {
            await dialogService.ShowMessageAsync(title: Translations.DuplicatedNameTitle, message: Translations.DuplicateCategoryMessage);

            return;
        }

        await dialogService.ShowLoadingDialogAsync(Translations.SavingCategoryMessage);
        await SaveCategoryAsync();
        Messenger.Send(new CategoriesChangedMessage());
        await dialogService.HideLoadingDialogAsync();
        await Shell.Current.Navigation.PopModalAsync();
    }
}
