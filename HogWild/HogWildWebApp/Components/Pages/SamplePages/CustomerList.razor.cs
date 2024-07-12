using HogWildSystem.BLL;
using HogWildSystem.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace HogWildWebApp.Components.Pages.SamplePages
{
    public partial class CustomerList
    {
        #region Fields
        // The last name
        private string lastName;

        // the phone number
        private string phoneNumber;

        // the feedback message
        private string feedbackMessage;

        // the error message
        private string errorMessage;

        // has feedback
        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);

        // has error
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);

        // error details
        private List<string> errorDetails = new();
        #endregion

        #region Properties
        // Injects CustomerService dependency
        [Inject]
        protected CustomerService CustomerService { get; set; }

        // Injects NavigationManager dependency
        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        // gets or sets the CustomerSearchView
        protected List<CustomerSearchView> Customers { get; set; } = new();

        // gets or sets pagination with an initial value of 10 items per page
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

                if (string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(phoneNumber))
                {
                    throw new ArgumentException("Please enter a last name and/or phone number");
                }

                Customers = CustomerService.GetCustomers(lastName, phoneNumber);
                if (Customers.Count > 0)
                {
                    feedbackMessage = "Search for customer(s) was successful";
                }
                else
                {
                    feedbackMessage = "No customers were found with your search criteria";
                }
            }
            catch (AggregateException ex) 
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
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

        }

        private void EditCustomer(int customerID)
        {

        }

        private void NewInvoice(int customerID)
        {

        }
        #endregion
    }
}
