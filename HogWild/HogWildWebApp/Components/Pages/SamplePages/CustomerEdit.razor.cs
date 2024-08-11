using HogWildSystem.BLL;
using HogWildSystem.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace HogWildWebApp.Components.Pages.SamplePages
{
    public partial class CustomerEdit : ComponentBase
    {
        #region Fields
        private CustomerEditView customer = new CustomerEditView();
        private List<LookupView> provinces = new List<LookupView>();
        private List<LookupView> countries = new List<LookupView>();
        private List<LookupView> statusLookups = new List<LookupView>();
        private List<InvoiceView> invoices = new List<InvoiceView>();
        private bool disableViewButton => !disableSaveButton;
        #endregion

        #region Properties
        [Inject]
        protected CustomerService CustomerService { get; set; }

        [Inject]
        protected CategoryLookupService CategoryLookupService { get; set; }

        [Inject]
        protected InvoiceService InvoiceService { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Parameter]
        public int CustomerID { get; set; } = 0;
        #endregion

        #region Feedback and Error Messaging
        private string feedbackMessage;
        private string errorMessage;
        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);

        private List<string> errorDetails = new List<string>();
        #endregion

        #region Validation
        private EditContext editContext;
        private string closeButtonText = "Close";
        private Color closeButtonColor = Color.Success;

        private bool disableSaveButton => !editContext.IsModified() || !editContext.Validate();
        private ValidationMessageStore messageStore;
        #endregion

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            try
            {
                editContext = new EditContext(customer);

                editContext.OnValidationRequested += EditContext_OnValidationRequested;

                messageStore = new ValidationMessageStore(editContext);

                editContext.OnFieldChanged += EditContext_OnFieldChanged;

                errorDetails.Clear();
                errorMessage = string.Empty;
                feedbackMessage = string.Empty;

                if (CustomerID > 0)
                {
                    customer = CustomerService.GetCustomer(CustomerID);
                    invoices = InvoiceService.GetCustomerInvoices(CustomerID);
                }

                provinces = CategoryLookupService.GetLookups("Province");
                countries = CategoryLookupService.GetLookups("Country");
                statusLookups = CategoryLookupService.GetLookups("Customer Status");

                await InvokeAsync(StateHasChanged);
            }
            catch (AggregateException ex)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage += Environment.NewLine;
                }
                errorMessage += "Unable to search for customer.";
                foreach (Exception error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
            catch (Exception ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
        }

        private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e)
        {
            editContext.Validate();
            closeButtonText = editContext.IsModified() ? "Cancel" : "Close";
            closeButtonColor = editContext.IsModified() ? Color.Warning : Color.Default;
        }

        private void EditContext_OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
        {
            messageStore?.Clear();

            if (string.IsNullOrWhiteSpace(customer.FirstName))
            {
                messageStore?.Add(() => customer.FirstName ,"First name is required!");
            }

            if (string.IsNullOrWhiteSpace(customer.LastName))
            {
                messageStore?.Add(() => customer.LastName, "Last name is required!");
            }

            if (string.IsNullOrWhiteSpace(customer.Phone))
            {
                messageStore?.Add(() => customer.Phone, "Phone is required!");
            }

            if (string.IsNullOrWhiteSpace(customer.Email))
            {
                messageStore?.Add(() => customer.Email, "Email is required!");
            }
        }

        private void Save()
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;

            try
            {
                customer = CustomerService.Save(customer);

                feedbackMessage = "Data was successfully saved!";
            }
            catch (AggregateException ex)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage += Environment.NewLine;
                }
                errorMessage += "Unable to add or edit customer!";
                foreach (Exception error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
            catch (Exception ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
        }

        private async void Cancel()
        {
            NavigationManager.NavigateTo("/SamplePages/CustomerList");
        }

        private void NewInvoice()
        {
            NavigationManager.NavigateTo($"/SamplePages/InvoiceEdit/0/{CustomerID}/1");
        }

        private void EditInvoice(int invoiceID)
        {
            NavigationManager.NavigateTo($"/SamplePages/InvoiceEdit/{invoiceID}/{CustomerID}/1");
        }
    }
}
