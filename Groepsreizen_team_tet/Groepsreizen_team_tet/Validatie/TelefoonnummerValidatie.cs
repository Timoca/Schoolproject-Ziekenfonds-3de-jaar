namespace Groepsreizen_team_tet.Validatie
{
    public class TelefoonnummerValidatie: ValidationAttribute
    {
        private readonly int minCijfers;
        private readonly int maxCijfers;

        public TelefoonnummerValidatie(int minCijfers, int maxCijfers)
        {
            this.minCijfers = minCijfers;
            this.maxCijfers = maxCijfers;
            ErrorMessage = $"Telefoonnummer moet {minCijfers} cijfers lang zijn en mag alleen cijfers en de symbolen -, (, ), / en . bevatten.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult(ErrorMessage);
            }

            var phoneNumber = value.ToString();
            Console.WriteLine($"TelefoonnummerValidatie: Ingevoerde telefoonnummer - {phoneNumber}");

            // Controleer of alleen toegestane tekens aanwezig zijn
            var allowedChars = phoneNumber!.All(c => char.IsDigit(c) || "-()/.".Contains(c) || char.IsWhiteSpace(c));
            if (!allowedChars)
            {
                Console.WriteLine("TelefoonnummerValidatie: Ongewenste tekens gevonden");
                return new ValidationResult(ErrorMessage);
            }

            // Tel het aantal cijfers
            var digitCount = phoneNumber!.Count(char.IsDigit);
            Console.WriteLine($"TelefoonnummerValidatie: Aantal cijfers - {digitCount}");
            if (digitCount < minCijfers || digitCount > maxCijfers)
            {
                Console.WriteLine("TelefoonnummerValidatie: Ongeldig aantal cijfers");
                return new ValidationResult(ErrorMessage);
            }
            Console.WriteLine("TelefoonnummerValidatie: Validatie geslaagd");
            return ValidationResult.Success;
        }
    }
}
