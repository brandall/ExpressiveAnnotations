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
        [Required]
        public string LastName { get; set; }

        [AssertThat("FirstName == LastName || Age > 9", ErrorMessage = "Min age is 10, unless first name is the same as last name.")]
        public int Age { get; set; }

        [RequiredIf("Age > 17", AllowEmptyStrings = false, ErrorMessage = "Email is required if person is 18 or older.")]
        public string Email { get; set; }
    }
}