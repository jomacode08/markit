using markit.Application.Models.Authentication.Enums;
using markit.Domain.Entities;

namespace markit.Application.Models.Authentication
{
    public class UserViewModel
    {
        public UserViewModel
        (
            string email,
            string? password,
            string firstName,
            string lastName,
            string? picture,
            AccessType accessType,
            DateOnly? birthDate = null,
            Gender? gender = null
        )
        {
            _Email = email;
            Password = password;
            FirstName = firstName;
            LastName = lastName;
            Picture = picture;
            AccessType = accessType;
            BirthDate = birthDate;
            Gender = gender;
        }

        private string _Email {  get; set; }
        public string? Password { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string? Picture { get; private set; }
        public AccessType AccessType { get; private set; }
        public DateOnly? BirthDate { get; private set; }
        public Gender? Gender { get; private set; }

        public string Email
        {
            get
            {
                return _Email.ToLower();
            }

            private set { }
        }
    }
}
