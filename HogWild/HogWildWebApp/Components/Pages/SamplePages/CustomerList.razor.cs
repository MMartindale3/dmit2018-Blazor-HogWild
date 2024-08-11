using HogWildSystem.BLL;
using HogWildSystem.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace HogWildWebApp.Components.Pages.SamplePages
{
    public partial class CustomerList
    {
        #region Fields
        private string lastName;
        private string phoneNumber;
        private string feedbackMessage;
        private string errorMessage;

        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);
        private List<string> errorDetails = new List<string>();
        #endregion

        #region Properties
        [Inject]
        protected CustomerService CustomerService { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        protected List<CustomerSearchView> Customers { get; set; } = new List<CustomerSearchView>();

        protected PaginationState Pagination = new PaginationState() { ItemsPerPage = 10 };
        #endregion

        #region Methods
        private void Search()
        {
            try
            {
                errorDetails.Clear();
                errorMessage = string.Empty;
                feedbackMessage = string.Empty;
                Customers.Clear();

                if (string.IsNullOrWhiteSpace(lastName)
                    && string.IsNullOrWhiteSpace(phoneNumber))
                {
                    throw new ArgumentException
                        ("Enter a last name and/or a phone number!");
                }

                Customers = CustomerService.GetCustomers(lastName, phoneNumber);

                if (Customers.Count > 0)
                {
                    feedbackMessage =
                        "Search for customer(s) was successful!";
                }
                else
                {
                    feedbackMessage =
                        "No customers were found with your search criteria.";
                }
            }
            catch (AggregateException ex)
            {
                if(!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage += Environment.NewLine;
                }
                errorMessage += "Unable to search for customer.";
                foreach(Exception error in ex.InnerExceptions)
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

        private void New()
        {
            NavigationManager.NavigateTo("/SamplePages/CustomerEdit/0");
        }

        private void EditCustomer(int customerID)
        {
            NavigationManager.NavigateTo($"/SamplePages/CustomerEdit/{customerID}");
        }

        private void NewInvoice(int customerID)
        {
        }
        #endregion
    }
}
