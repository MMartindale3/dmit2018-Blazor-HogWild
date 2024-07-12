namespace HogWildWebApp.Components.Pages.SamplePages
{
    public partial class Basics
    {
        #region Random Number Generation - Method Utilization

        private string? myName;
        private int oddEvenValue;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            RandomValue(); // initializes our oddEvenValue
        }

        private void RandomValue()
        {
            Random rng = new Random();

            oddEvenValue = rng.Next(0, 26);

            if (oddEvenValue % 2 == 0)
            {
                myName = $"Melissa is even {oddEvenValue}";
            }
            else
            {
                myName = null;
            }

            InvokeAsync(StateHasChanged);
        }

        #endregion

        #region  Text Boxes

        private string email;
        private string password;
        private DateTime? date = DateTime.Today;
        private string feedback;

        private void TextSubmit()
        {
            feedback = $"Email : {email} - Password : {password} - Date : {date}";

            InvokeAsync(StateHasChanged);
        }
        #endregion
    }
}
