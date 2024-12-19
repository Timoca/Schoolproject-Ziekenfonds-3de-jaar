namespace Groepsreizen_team_tet.Validatie
{
    public class MaxDateAttribute : ValidationAttribute
    {
        private readonly DateTime _maxDate;

        public MaxDateAttribute(string maxDateString)
        {
            if (!DateTime.TryParse(maxDateString, out _maxDate))
            {
                _maxDate = DateTime.Today;
            }
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateValue)
            {
                if (dateValue > _maxDate)
                {
                    return new ValidationResult(ErrorMessage ?? $"De {validationContext.DisplayName} mag niet in de toekomst liggen.");
                }
            }

            return ValidationResult.Success;
        }
    }
}