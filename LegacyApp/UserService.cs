using System;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!ValidateUserInput(firstName, lastName, email, dateOfBirth))
                return false;

            var client = GetClientById(clientId);

            var user = CreateUser(firstName, lastName, email, dateOfBirth, client);

            if (user.HasCreditLimit && user.CreditLimit < 500)
                return false;

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool ValidateUserInput(string firstName, string lastName, string email, DateTime dateOfBirth)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                return false;

            if (!email.Contains("@") || !email.Contains("."))
                return false;

            int age = CalculateAge(dateOfBirth);
            return age >= 21;
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
                age--;
            return age;
        }

        private Client GetClientById(int clientId)
        {
            var clientRepository = new ClientRepository();
            return clientRepository.GetById(clientId);
        }

        private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        {
            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName,
                HasCreditLimit = true
            };

            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else if (client.Type == "ImportantClient")
            {
                user.CreditLimit = GetCreditLimit(lastName, dateOfBirth) * 2;
            }
            else
            {
                user.CreditLimit = GetCreditLimit(lastName, dateOfBirth);
            }

            return user;
        }

        private int GetCreditLimit(string lastName, DateTime dateOfBirth)
        {
            using var userCreditService = new UserCreditService();
            return userCreditService.GetCreditLimit(lastName, dateOfBirth);
        }
    }
}
