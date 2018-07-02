using System.ComponentModel;

namespace ExpressiveAnnotations.DotNetCore.WebSample.Models
{
    public class ContactPreferencesViewModel
    {
        [DisplayName("Contact By Email")]
        public bool ContactByEmail { get; set; }

        [DisplayName("Contact By Phone")]
        public bool ContactByPhone { get; set; }

    }
}