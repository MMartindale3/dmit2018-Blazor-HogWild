using BlazorDialog;
using HogWildSystem.BLL;
using HogWildSystem.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using static MudBlazor.Icons;

#nullable disable
namespace HogWildWebApp.Components.Pages.SamplePages
{
    public partial class InvoiceEdit
    {
        #region Fields
        private string description;
        private int categoryID;
        private List<LookupView> partCategories;
        private List<PartView> parts = new List<PartView>();
        private InvoiceView invoice;


        DateTime date = DateTime.Now;
        #endregion

        #region Feedback and Error Messaging
        private string feedbackMessage;
        private string errorMessage;
        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);

        private List<string> errorDetails = new List<string>();
        #endregion

        #region Properties
        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected InvoiceService InvoiceService { get; set; }

        [Inject]
        protected PartService PartService { get; set; }

        [Inject]
        protected CategoryLookupService CategoryLookupService { get; set; }

        [Inject]
        protected IDialogService DialogService { get; set; }

        [Inject]
        protected IBlazorDialogService BlazorDialogService { get; set; }

        [Parameter]
        public int InvoiceID { get; set; }

        [Parameter]
        public int CustomerID { get; set; }

        [Parameter]
        public int EmployeeID { get; set; }

        #endregion

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            try
            {
                errorDetails.Clear();
                errorMessage = string.Empty;
                feedbackMessage = string.Empty;

                partCategories = CategoryLookupService.GetLookups("Part Categories");

                invoice = InvoiceService.GetInvoice(InvoiceID, CustomerID, EmployeeID);
                
                await InvokeAsync(StateHasChanged);
            }
            catch (AggregateException ex)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage += Environment.NewLine;
                }
                errorMessage += "Unable to search for invoice.";
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

        private async Task SearchParts()
        {
            try
            {
                errorDetails.Clear();
                errorMessage = string.Empty;
                feedbackMessage = string.Empty;

                parts.Clear();

                if (categoryID <= 0 && string.IsNullOrWhiteSpace(description))
                {
                    throw new ArgumentException("Provide a category and/or description!");
                }

                List<int> existingPartIDs =
                    invoice.InvoiceLines.Select(line => line.PartID).ToList();

                parts = PartService.GetParts(categoryID, description, existingPartIDs);

                await InvokeAsync(StateHasChanged);

                if (parts.Count > 0)
                {
                    feedbackMessage =
                        "Search for part(s) was successful!";
                }
                else
                {
                    feedbackMessage =
                        "No parts were found with your search criteria.";
                }
            }
            catch (AggregateException ex)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage += Environment.NewLine;
                }
                errorMessage += "Unable to search for invoice.";
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


        private async Task AddPart(int partID)
        {
            PartView part = PartService.GetPart(partID);

            InvoiceLineView invoiceLine = new InvoiceLineView();
            invoiceLine.PartID = part.PartID;
            invoiceLine.Description = part.Description;
            invoiceLine.Price = part.Price;
            invoiceLine.Taxable = part.Taxable;
            invoiceLine.Quantity = 0;
            invoice.InvoiceLines.Add(invoiceLine);

            parts.Remove(parts.Where(line => line.PartID == partID).FirstOrDefault());

            await InvokeAsync(StateHasChanged);
        }

        private async Task DeleteInvoiceLine(int partID)
        {
            InvoiceLineView invoiceLine = invoice.InvoiceLines
                                                .Where(line => line.PartID == partID)   
                                                .Select(line => line)
                                                .FirstOrDefault();

            string bodyText = $"Are you sure you wish to remove {invoiceLine.Description}?";

            string dialogResult =
                await BlazorDialogService.ShowComponentAsDialog<string>(
                    new ComponentAsDialogOptions(typeof(SimpleComponentDialog))
                    {
                        Size = DialogSize.Normal,
                        Parameters = new()
                        {
                            { nameof(SimpleComponentDialog.Input), "Remove Invoice Line" },
                            { nameof(SimpleComponentDialog.BodyText), bodyText }
                        }
                    });

            if (dialogResult == "Ok")
            {
                invoice.InvoiceLines.Remove(invoiceLine);

                if (categoryID > 0 || !string.IsNullOrWhiteSpace(description))
                {
                    await SearchParts();
                    
                }
                UpdateSubtotalAndTax();
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task Close()
        {
            string bodyText = "Do you wish to close the invoice editor?";

            string dialogResult =
               await BlazorDialogService.ShowComponentAsDialog<string>(
                   new ComponentAsDialogOptions(typeof(SimpleComponentDialog))
                   {
                       Size = DialogSize.Normal,
                       Parameters = new()
                       {
                            { nameof(SimpleComponentDialog.Input), "Reverse Invoice Edits" },
                            { nameof(SimpleComponentDialog.BodyText), bodyText }
                       }
                   });

            if (dialogResult == "Ok")
            {
                NavigationManager.NavigateTo($"/SamplePages/CustomerEdit/{CustomerID}");
            }
        }

        private void UpdateSubtotalAndTax()
        {
            invoice.SubTotal = invoice.InvoiceLines.Where(line => !line.RemoveFromViewFlag)
                                                    .Sum(line => line.Quantity * line.Price);
            invoice.Tax = invoice.InvoiceLines
                                .Where(line => !line.RemoveFromViewFlag)
                                .Sum(line => line.Taxable? line.Quantity * line.Price * 0.05m : 0);
        }

        private void Save()
        {
            try
            {
                errorDetails.Clear();
                errorMessage = string.Empty;
                feedbackMessage = string.Empty;

                InvoiceService.Save(invoice);

                feedbackMessage = "Invoice was successfully saved!";
            }
            catch (AggregateException ex)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage += Environment.NewLine;
                }
                errorMessage += "Unable to search for invoice.";
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
    }
}
