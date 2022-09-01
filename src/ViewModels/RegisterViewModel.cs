using MetaFrm.MVVM;
using System.ComponentModel.DataAnnotations;

namespace MetaFrm.Razor.ViewModels
{
    /// <summary>
    /// RegisterViewModel
    /// </summary>
    public partial class RegisterViewModel : BaseViewModel
    {
        /// <summary>
        /// Email
        /// </summary>
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        [Required]
        [MinLength(6)]
        public string? Password { get; set; }

        /// <summary>
        /// RepeatPassword
        /// </summary>
        [Required]
        [CompareAttribute("Password")]
        [Display(Name = "Repeat Password")]
        [MinLength(6)]
        public string? RepeatPassword { get; set; }

        /// <summary>
        /// Nickname
        /// </summary>
        [Required]
        [MinLength(3)]
        public string? Nickname { get; set; }

        /// <summary>
        /// Fullname
        /// </summary>
        [Required]
        [Display(Name = "Full name")]
        [MinLength(3)]
        public string? Fullname { get; set; }

        /// <summary>
        /// PhoneNumber
        /// </summary>
        [Required]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Agree
        /// </summary>
        [Required]
        public bool Agree { get; set; }

        /// <summary>
        /// AccessCodeVisible
        /// </summary>
        public bool AccessCodeVisible { get; set; }

        /// <summary>
        /// AccessCode
        /// </summary>
        public string? AccessCode { get; set; }

        private string? _inputAccessCode;
        /// <summary>
        /// InputAccessCode
        /// </summary>
        public string? InputAccessCode
        {
            get {
                return this._inputAccessCode;
            }
            set
            {
                this._inputAccessCode = value;

                this.AccessCodeConfirmVisible = this._inputAccessCode == this.AccessCode;

            }
        }

        /// <summary>
        /// AccessCodeConfirmVisible
        /// </summary>
        public bool AccessCodeConfirmVisible { get; set; }
    }
}