using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorWebApp.Shared
{
    public class UserLogin
    {
        [Required(ErrorMessage = "Please eneter the user name")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Please eneter password")]
        public string Password { get; set; }
    }
}
