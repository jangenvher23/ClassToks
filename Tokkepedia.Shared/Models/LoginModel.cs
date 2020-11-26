using System;
using System.Collections.Generic;
using System.Text;
using Tokkepedia.Shared.Models.Base;


namespace Tokkepedia.Shared.Models
{
    public class LoginModel : BaseModel
    {
        private string _username;
        private string _password;

        public string Username { get => _username; set { _username = value; RaisePropertyChanged(propertyName: nameof(Username)); } }
        /// <summary>
        ///     RaisePropertyChanged is for when Password been set this will notify that there are changes in the properties
        ///     or a change has been raised
        /// </summary>
        public string Password { get => _password; set { _password = value; RaisePropertyChanged(propertyName: nameof(Password)); } }
        public bool RememberMe { get; set; }
    }
}
