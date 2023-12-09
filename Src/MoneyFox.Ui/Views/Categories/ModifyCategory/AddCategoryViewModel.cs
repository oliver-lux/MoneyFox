namespace MoneyFox.Ui.Views.Categories.ModifyCategory;

using Core.Common.Interfaces;
using Core.Features.CategoryCreation;
using MediatR;

public sealed class AddCategoryViewModel : ModifyCategoryViewModel
{
    private readonly IMediator mediator;

    public AddCategoryViewModel(IMediator mediator, IDialogService dialogService) : base(mediator: mediator, service: dialogService)
    {
        this.mediator = mediator;
        SelectedCategory = new()
        {
            Id = 0,
            Name = string.Empty,
            Note = string.Empty,
            RequireNote = false,
            Created = DateTime.MinValue,
            LastModified = null
        };
    }

    protected override Task SaveCategoryAsync()
    {
        var command = new CreateCategory.Command(Name: SelectedCategory.Name, Note: SelectedCategory.Note, RequireNote: SelectedCategory.RequireNote);

        return mediator.Send(command);
    }
}
