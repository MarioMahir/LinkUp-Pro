namespace LinkUpPro.Application.ViewModels
{
    public class ConfirmActionViewModel
    {
        public string Title { get; set; } = "Confirmar acción";

        public string Message { get; set; } = string.Empty;

        public string FormController { get; set; } = string.Empty;

        public string FormAction { get; set; } = string.Empty;

        public Dictionary<string, string> HiddenFields { get; set; } = new();

        public string CancelUrl { get; set; } = string.Empty;

        public string ConfirmButtonText { get; set; } = "Aceptar";

        public string ConfirmButtonClass { get; set; } = "btn-danger";
    }
}
