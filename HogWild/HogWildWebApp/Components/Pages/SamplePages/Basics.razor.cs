using HogWildWebApp.Data.ViewModels;
using Microsoft.AspNetCore.Components;

namespace HogWildWebApp.Components.Pages.SamplePages
{
   
    public partial class Basics
    {
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            RandomValue();

            PopulateList();
        }

        #region Random Number Generation - Method Utilization

        private string? myName;
        private int oddEvenValue;

        private void RandomValue()
        {
            Random rng = new Random();

            oddEvenValue = rng.Next(0, 26);

            if (oddEvenValue % 2 == 0)
            {
                myName = $"Shane is even {oddEvenValue}";
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
        private string leftFeedback;

        private void TextSubmit()
        {
            leftFeedback = $"Email : {email} - Password : {password} - Date : {date}";

            InvokeAsync(StateHasChanged);
        }
        #endregion

        #region Radio Buttons, CheckBoxes, and Text Area

        private string meal = "breakfast";
        private string[] Meals { get; set; } = new string[] { "breakfast", "lunch", "dinner", "snacks" };

        private bool acceptanceBox;

        private string messageBody;

        private string middleFeedback;

        private void HandleMealSelection(ChangeEventArgs e)
        {
            meal = e.Value.ToString();
        }

        private void RadioCheckAreaSubmit()
        {
            middleFeedback = $"Meal : {meal} - Acceptance : {acceptanceBox} - Message : {messageBody}";

            InvokeAsync(StateHasChanged);
        }

        #endregion

        #region List and Sliders

        private List<SelectionView> rides;
        private int myRide;
        private string vacationSpot;
        private List<string> vacationSpots;
        private int reviewRating = 5;

        private string rightFeedback;

        private void PopulateList()
        {
            int i = 0;

            rides = new List<SelectionView>();
            rides.Add(new SelectionView() { ValueID = ++i, DisplayText = "Car" });
            rides.Add(new SelectionView() { ValueID = ++i, DisplayText = "Bus" });
            rides.Add(new SelectionView() { ValueID = ++i, DisplayText = "Bike" });
            rides.Add(new SelectionView() { ValueID = ++i, DisplayText = "Motorcycle" });
            rides.Add(new SelectionView() { ValueID = ++i, DisplayText = "Boat" });
            rides.Add(new SelectionView() { ValueID = ++i, DisplayText = "Plane" });

            rides.Sort((left, right) => left.DisplayText.CompareTo(right.DisplayText));

            vacationSpots = new List<string>();
            vacationSpots.Add("California");
            vacationSpots.Add("Carribean");
            vacationSpots.Add("Cruising");
            vacationSpots.Add("Europe");
            vacationSpots.Add("Florida");
            vacationSpots.Add("Mexico");
        }

        private void ListSliderSubmit()
        {
            rightFeedback = $"Ride : {myRide} - Vacation : {vacationSpot} - Review Rating : {reviewRating}";

            InvokeAsync(StateHasChanged);
        }

        #endregion
    }
}
