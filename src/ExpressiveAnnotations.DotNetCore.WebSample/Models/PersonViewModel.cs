using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.DotNetCore.Attributes;

namespace ExpressiveAnnotations.DotNetCore.WebSample.Models
{
    public class PersonViewModel
    {
        public int Id { get; set; }

        [DisplayName("First Name")]
        [Required]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        [Required(AllowEmptyStrings = false)]
        public string LastName { get; set; }

        [AssertThat("FirstName == LastName || Age > 9", ErrorMessage = "Min age is 10, unless first name is the same as last name.")]
        public int Age { get; set; }

        [RequiredIf("Age < 18", AllowEmptyStrings = false, ErrorMessage = "Email is required if person is under 18.")]
        public string Email { get; set; }

        [RequiredIf("ContactPreferences.ContactByPhone && (Email == null || Email == '')")]
        [AssertThat(@"IsRegexMatch(Phone, '^\\d+$')", Priority = 1)]
        [AssertThat("Length(Phone) > 8 && Length(Phone) < 16", Priority = 2)]
        public string Phone { get; set; }

        public ContactPreferencesViewModel ContactPreferences { get; set; }
    }
}