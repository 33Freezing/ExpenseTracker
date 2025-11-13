using ExpenseTracker.Components.Pages.Dialogs;
using MudBlazor;

namespace ExpenseTracker.Services
{
    public class NotificationService
    {
        private readonly IDialogService _dialogService;

        public NotificationService(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public async Task ShowSuccessAsync(string message)
        {
            var options = new DialogOptions { CloseOnEscapeKey = true };
            var parameters = new DialogParameters<AlertDialog>
            {
                { x => x.Message, message },
                { x => x.Title, "Success" }
            };
            await _dialogService.ShowAsync<AlertDialog>("Success", parameters, options);
        }
        
        public async Task ShowErrorAsync(string message){
            var options = new DialogOptions { CloseOnEscapeKey = true };
            var parameters = new DialogParameters<AlertDialog>
                {
                    { x => x.Message,  message },
                    { x => x.Title, "Failure" }
                };

            var dialog = await _dialogService.ShowAsync<AlertDialog>("Failure", parameters, options);
        }
    }
}
