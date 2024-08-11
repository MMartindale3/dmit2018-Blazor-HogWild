using HogWildSystem.DAL;
using HogWildSystem.Entities;
using HogWildSystem.ViewModels;

#nullable disable
namespace HogWildSystem.BLL
{
    public class InvoiceService
    {
        private readonly HogWildContext _hogWildContext;

        internal InvoiceService(HogWildContext hogWildContext)
        {
            _hogWildContext = hogWildContext;
        }

        public InvoiceView GetInvoice(int invoiceID, int customerID, int employeeID)
        {
            InvoiceView invoice = null;

            if (customerID > 0 && invoiceID == 0)
            {
                invoice = new InvoiceView();
                invoice.CustomerID = customerID;
                invoice.EmployeeID = employeeID;
                invoice.InvoiceDate = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }
            else
            {
                invoice = _hogWildContext.Invoices
                            .Where(invoice => invoice.InvoiceID == invoiceID && !invoice.RemoveFromViewFlag)
                            .Select(invoice => new InvoiceView
                            {
                                InvoiceID = invoice.InvoiceID,
                                InvoiceDate = invoice.InvoiceDate,
                                CustomerID = invoice.CustomerID,
                                EmployeeID = invoice.EmployeeID,
                                SubTotal = invoice.SubTotal,
                                Tax = invoice.Tax,
                                InvoiceLines = _hogWildContext.InvoiceLines
                                                .Where(line => line.InvoiceID == invoiceID && !invoice.RemoveFromViewFlag)
                                                .Select(line => new InvoiceLineView
                                                {
                                                    InvoiceLineID = line.InvoiceLineID,
                                                    InvoiceID = line.InvoiceID,
                                                    PartID = line.PartID,
                                                    Quantity = line.Quantity,
                                                    Description = line.Part.Description,
                                                    Price = line.Price,
                                                    Taxable = line.Part.Taxable,
                                                    RemoveFromViewFlag = line.RemoveFromViewFlag
                                                })
                                                .ToList()
                            })
                            .FirstOrDefault();

                customerID = invoice.CustomerID;
            }

            invoice.CustomerName = GetCustomerFullName(customerID);
            invoice.EmployeeName = GetEmployeeFullName(employeeID);

            return invoice;
        }

        public string GetCustomerFullName(int customerID)
        {
            return _hogWildContext.Customers
                    .Where(customer => customer.CustomerID == customerID)
                    .Select(customer => $"{customer.FirstName} {customer.LastName}")
                    .FirstOrDefault();
        }

        public string GetEmployeeFullName(int employeeID)
        {
            return _hogWildContext.Employees
                    .Where(employee => employee.EmployeeID == employeeID)
                    .Select(employee => $"{employee.FirstName} {employee.LastName}")
                    .FirstOrDefault();
        }

        public List<InvoiceView> GetCustomerInvoices(int customerID)
        {
            return _hogWildContext.Invoices
                    .Where(invoice => invoice.CustomerID == customerID
                        && !invoice.RemoveFromViewFlag)
                    .Select(invoice => new InvoiceView
                    {
                        InvoiceID = invoice.InvoiceID,
                        InvoiceDate = invoice.InvoiceDate,
                        CustomerID = invoice.CustomerID,
                        SubTotal = invoice.SubTotal,
                        Tax = invoice.Tax
                    })
                    .ToList();
        }

        public InvoiceView Save(InvoiceView invoiceView)
        {
            #region Business Logic and Parameter Exceptions
            List<Exception> errorList = new List<Exception>();

            if (invoiceView == null)
            {
                throw new ArgumentNullException("No invoice was supplied!");
            }

            if (invoiceView.InvoiceLines.Count == 0)
            {
                errorList.Add(new Exception("Invoice must have invoice lines!"));
            }

            if (invoiceView.CustomerID <= 0)
            {
                errorList.Add(new Exception("No customer was supplied!"));
            }

            foreach (var invoiceLine in invoiceView.InvoiceLines)
            {
                if (invoiceLine.Quantity <= 0)
                {
                    errorList.Add(new Exception($"Invoice line {invoiceLine.Description} has a quantity less than 1"));
                }
            }

            if (errorList.Count > 0)
            {
                throw new AggregateException("Unable to Save invoice.  Check concerns:", errorList);
            }
            #endregion

            #region Fetching Data and Setting Up References
            List<InvoiceLineView> referenceInvoiceLineViews =
                            _hogWildContext.InvoiceLines.Where(line => line.InvoiceID == invoiceView.InvoiceID)
                                                        .Select(line => new InvoiceLineView
                                                        {
                                                            InvoiceLineID = line.InvoiceLineID,
                                                            InvoiceID = line.InvoiceID,
                                                            PartID = line.PartID,
                                                            Quantity = line.Quantity,
                                                            Description = line.Part.Description,
                                                            Price = line.Price,
                                                            Taxable = line.Part.Taxable,
                                                            RemoveFromViewFlag = line.RemoveFromViewFlag
                                                        })
                                                        .ToList();

            List<Part> parts = _hogWildContext.Parts.Select(part => part).ToList();

            Invoice invoice = _hogWildContext.Invoices.Where(invoice => invoice.InvoiceID == invoiceView.InvoiceID)
                                                    .Select(invoice => invoice)
                                                    .FirstOrDefault();

            List<InvoiceLine> invoiceLines = _hogWildContext.InvoiceLines.Where(line => line.InvoiceID == invoiceView.InvoiceID)
                                                                        .Select(line => line)
                                                                        .ToList();
            #endregion

            #region Invoice Existence Check and Initialization
            if (invoice == null)
            {
                invoice = new Invoice();
            }

            invoice.InvoiceDate = invoiceView.InvoiceDate;
            invoice.CustomerID = invoiceView.CustomerID;
            invoice.EmployeeID = invoiceView.EmployeeID;
            #endregion

            #region Processing Invoice Lines
            foreach (var invoiceLineView in invoiceView.InvoiceLines)
            {
                if (invoiceLineView.InvoiceLineID > 0)
                {
                    InvoiceLine? invoiceLine = invoice.InvoiceLines
                                        .Where(line => line.InvoiceLineID == invoiceLineView.InvoiceLineID)
                                        .Select(line => line)
                                        .FirstOrDefault();

                    if (invoiceLine == null)
                    {
                        errorList.Add(
                            new Exception(
                                $"Invoice line for {invoiceLineView.Description} cannot be found in the existing invoice lines"
                                ));
                    }

                    invoiceLine.Quantity = invoiceLineView.Quantity;
                    invoiceLine.RemoveFromViewFlag = invoiceLineView.RemoveFromViewFlag;
                }
                else
                {
                    InvoiceLine invoiceLine = new InvoiceLine();
                    invoiceLine.PartID = invoiceLineView.PartID;
                    invoiceLine.Quantity = invoiceLineView.Quantity;
                    invoiceLine.Price = invoiceLineView.Price;
                    invoiceLine.RemoveFromViewFlag = invoiceLineView.RemoveFromViewFlag;

                    Part? part = parts.Where(part => part.PartID == invoiceLineView.PartID)
                                        .Select(part => part)
                                        .FirstOrDefault();

                    part.QOH -= invoiceLineView.Quantity;

                    _hogWildContext.Parts.Update(part);

                    invoice.InvoiceLines.Add(invoiceLine);
                }

                invoice.SubTotal += invoiceLineView.Quantity * invoiceLineView.Price;

                bool isTaxable = _hogWildContext.Parts.Where(part => part.PartID == invoiceLineView.PartID)
                                                    .Select(part => part.Taxable)
                                                    .FirstOrDefault();

                invoice.Tax += isTaxable ? invoiceLineView.Quantity * invoiceLineView.Price * 0.05m : 0;
                //invoice.Tax = isTaxable ? invoice.SubTotal * 0.05m : 0;
            }
            #endregion

            #region Update Part Quantity On Hand (QOH)
            foreach (var invoiceLineView in invoiceView.InvoiceLines)
            {
                Part part = parts.Where(part => part.PartID == invoiceLineView.PartID)
                                    .Select(part => part)
                                    .FirstOrDefault();

                InvoiceLineView referenceInvoiceLineView = referenceInvoiceLineViews
                                    .Where(line => line.InvoiceLineID == invoiceLineView.InvoiceLineID)
                                    .Select(part => part)
                                    .FirstOrDefault();

                if (referenceInvoiceLineView != null)
                {
                    if (referenceInvoiceLineView.Quantity != invoiceLineView.Quantity)
                    {
                        part.QOH -= referenceInvoiceLineView.Quantity - invoiceLineView.Quantity;

                        _hogWildContext.Parts.Update(part);
                    }
                }
            }
            #endregion

            #region Remove Any Invoice Lines That Have Been Deleted By the User
            foreach (var referenceInvoiceLine in referenceInvoiceLineViews)
            {
                if (!invoiceView.InvoiceLines.Any(line => line.InvoiceLineID == referenceInvoiceLine.InvoiceLineID))
                {
                    Part part = parts.Where(part => part.PartID == referenceInvoiceLine.PartID)
                                        .Select(part => part)
                                        .FirstOrDefault();

                    part.QOH += referenceInvoiceLine.Quantity;

                    _hogWildContext.Parts.Update(part);

                    InvoiceLine deletedInvoiceLine =
                            _hogWildContext.InvoiceLines.Where(line => line.InvoiceLineID == referenceInvoiceLine.InvoiceLineID)
                                                        .Select(line => line)
                                                        .FirstOrDefault();

                    // Not in the slides.  Need to remove the lines from the local version of the invoice, else the lines that were to be deleted
                    // in the next line will later be readded when we Save the invoice to the database.
                    invoice.InvoiceLines.Remove(deletedInvoiceLine);

                    _hogWildContext.InvoiceLines.Remove(deletedInvoiceLine);
                }
            }
            #endregion

            #region Update SuTotal and Tax
            invoice.SubTotal = 0;
            invoice.Tax = 0;
            foreach (var invoiceLineView in invoice.InvoiceLines)
            {
                invoice.SubTotal += invoiceLineView.Quantity * invoiceLineView.Price;

                bool isTaxable = _hogWildContext.Parts.Where(part => part.PartID == invoiceLineView.PartID)
                                                    .Select(part => part.Taxable)
                                                    .FirstOrDefault();

                invoice.Tax += isTaxable ? invoiceLineView.Quantity * invoiceLineView.Price * 0.05m : 0;
            }
            #endregion

            #region Final Error Check and Save Operation
            if (errorList.Count > 0)
            {
                _hogWildContext.ChangeTracker.Clear();

                throw new AggregateException("Unable to add or edit invoice.  Check concerns:", errorList);
            }
            else
            {
                if (invoice.InvoiceID <= 0)
                {
                    _hogWildContext.Invoices.Add(invoice);
                }
                else
                {
                    _hogWildContext.Invoices.Update(invoice);
                }

                _hogWildContext.SaveChanges();
            }
            #endregion

            return GetInvoice(invoice.InvoiceID, invoice.CustomerID, invoice.EmployeeID);
        }
    }
}
