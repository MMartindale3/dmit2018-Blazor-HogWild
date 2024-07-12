#nullable disable
using HogWildSystem.BLL;
using HogWildSystem.ViewModels;
using Microsoft.AspNetCore.Components;
namespace HogWildWebApp.Components.Pages.SamplePages
{
    public partial class Working
    {
        #region Fields
        private string feedback;
        private WorkingVersionsView workingVersionsView = new WorkingVersionsView();
        #endregion

        #region Properties
        [Inject]
        protected WorkingVersionsService WorkingVersionsService { get; set; }
        #endregion

        #region Methods
        private void GetWorkingVersions()
        {
            try
            {
                workingVersionsView = WorkingVersionsService.GetWorkingVersion();
            }
            #region catch all exceptions
            catch (AggregateException ex)
            {
                foreach (var error in ex.InnerExceptions)
                {
                    feedback = error.Message;
                }
            }

            catch (ArgumentNullException ex)
            {
                feedback = GetInnerException(ex).Message;
            }

            catch (Exception ex)
            {
                feedback = GetInnerException(ex).Message;
            }
            #endregion
        }
        private Exception GetInnerException(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }
        #endregion
    }
}
