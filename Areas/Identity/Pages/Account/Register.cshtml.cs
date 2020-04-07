using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using ServiceManager.Models;
using ServiceManager.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Twilio.Exceptions;
using Twilio.Rest.Lookups.V1;
using Twilio;
using Microsoft.Extensions.Options;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;

namespace ServiceManager.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly TwilioSMS _twilioSMS;
        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context,
            CountryService countryService,
            IOptionsSnapshot<TwilioSMS> twilioSMS)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _twilioSMS = twilioSMS.Value;
            AvailableCountries = countryService.GetCountries();
        }

        

    [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public List<SelectListItem> AvailableCountries { get; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public IList<SelectListItem> Options { get; set; }
        public class InputModel
        {

           
            [Required, DataType(DataType.Text), Display(Name = "First Name")]
            public string FirstName { get; set; }
           
            [Required, DataType(DataType.Text), Display(Name = "Last Name")]
            public string LastName { get; set; }
            [Required(ErrorMessage = "Phone Number Required!")]
            //[RegularExpression(@"^\(?([0-9]{4})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$",
             //      ErrorMessage = "Entered phone format is not valid.")]
            [DataType(DataType.Text), Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required, DataType(DataType.Text), Display(Name = "Professional Skill")]
            public string Professional_Skill { get; set; }

            [Display(Name = "Phone number country")]
            public string PhoneNumberCountryCode { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }


            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            
        }
        

            public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            Options =  _context.Profession.OrderBy(a => a.Skill).Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.Skill,
                                      Text = a.Skill
                                  }).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email, FirstName = Input.FirstName, LastName = Input.LastName, Professional_Skill = Input.Professional_Skill, PhoneNumber = Input.PhoneNumber };
                try
                {
                    TwilioClient.Init(_twilioSMS.accountSid, _twilioSMS.authToken);
                    var numberDetails = await PhoneNumberResource.FetchAsync(
                        pathPhoneNumber: new Twilio.Types.PhoneNumber(Input.PhoneNumber),
                        countryCode: Input.PhoneNumberCountryCode,
                        type: new List<string> { "carrier" });

                    // only allow user to set phone number if capable of receiving SMS
                    if (numberDetails?.Carrier != null
                        && numberDetails.Carrier.TryGetValue("type", out var phoneNumberType)
                        && phoneNumberType == "landline")
                    {
                        ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.PhoneNumber)}",
                            $"The number you entered does not appear to be capable of receiving SMS ({phoneNumberType}). Please enter a different value and try again");
                        return Page();
                    }

                   
                }
                catch (ApiException ex)
                {
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.PhoneNumber)}",
                        $"The number you entered was not valid (Twilio code {ex.Code}), please check it and try again");
                    return Page();
                }






                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                      var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {

                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        if (_twilioSMS.Active == "true")
                        {
                            try
                            {
                                var to = new PhoneNumber("+38762912141");
                                var SMSmessage = MessageResource.Create(
                                    to,
                                    from: new PhoneNumber(_twilioSMS.TwilioNumber),
                                    body: $"New User:  {user} has been registered!");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($" Registration Failure to send sms: {ex.Message} ");
                            }
                        }

                        return LocalRedirect(returnUrl);
                    }

                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }


   

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
