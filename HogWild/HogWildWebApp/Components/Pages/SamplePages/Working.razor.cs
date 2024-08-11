using HogWildSystem.BLL;
using HogWildSystem.ViewModels;
using Microsoft.AspNetCore.Components;

namespace HogWildWebApp.Components.Pages.SamplePages
{
    public partial class Working
    {
        #region Fields

        private WorkingVersionsView workingVersionsView = new WorkingVersionsView();

        private string feedback;

        #endregion

        [Inject]
        protected WorkingVersionsService WorkingVersionsService { get; set; }

        private void GetWorkingVersions()
        {
            try
            {
                workingVersionsView = WorkingVersionsService.GetWorkingVersion();
            }
            catch (AggregateException ex)
            {
                foreach (var error in ex.InnerExceptions)
                {
                    feedback = error.Message;
                }
            }
            catch (ArgumentException ex)
            {
                feedback = GetInnerException(ex).Message;
            }
            catch (Exception ex)
            {
                feedback = GetInnerException(ex).Message;
            }
        }

        private Exception GetInnerException(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;

            }

            return ex;
        }


    }
}
