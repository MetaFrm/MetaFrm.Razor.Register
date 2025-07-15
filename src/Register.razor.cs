using MetaFrm.Alert;
using MetaFrm.Control;
using MetaFrm.Database;
using MetaFrm.Maui.ApplicationModel;
using MetaFrm.Razor.ViewModels;
using MetaFrm.Service;
using MetaFrm.Web.Bootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Timers;

namespace MetaFrm.Razor
{
    /// <summary>
    /// Register
    /// </summary>
    public partial class Register
    {
        internal RegisterViewModel RegisterViewModel { get; set; } = new();

        private bool _isFocusElement = false;//등록 버튼 클릭하고 AccessCode로 포커스가 한번만 가도록

        [Inject] IBrowser? Browser { get; set; }

        private string? TermsOfServiceUrl { get; set; }

        private string? PrivacyPolicyUrl { get; set; }

        private TimeSpan RemainingTimeOrg { get; set; } = new TimeSpan(0, 5, 0);

        private TimeSpan RemainingTime { get; set; }

        /// <summary>
        /// OnInitialized
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.RegisterViewModel = this.CreateViewModel<RegisterViewModel>();

            try
            {
                this.TermsOfServiceUrl = this.GetAttribute(nameof(this.TermsOfServiceUrl));
                this.PrivacyPolicyUrl = this.GetAttribute(nameof(this.PrivacyPolicyUrl));

                string[] time = this.GetAttribute(nameof(this.RemainingTime)).Split(":");

                this.RemainingTimeOrg = new TimeSpan(time[0].ToInt(), time[1].ToInt(), time[2].ToInt());

                this.RemainingTime = new TimeSpan(this.RemainingTimeOrg.Ticks);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// OnAfterRender
        /// </summary>
        /// <param name="firstRender"></param>
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            {
                if (this.AuthState.IsLogin())
                    this.Navigation?.NavigateTo("/", true);

                ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "email");
            }

            if (this.RegisterViewModel.AccessCodeVisible && !this._isFocusElement)
            {
                ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "inputaccesscode");
                this._isFocusElement = true;
            }
        }

        private async Task<bool> OnRegisterClick()
        {
            try
            {
                this.RegisterViewModel.IsBusy = true;

                if (!this.AuthState.IsLogin())
                {
                    Response response;

                    if (!this.RegisterViewModel.Email.IsNullOrEmpty()
                        && this.RegisterViewModel.Password != null && !this.RegisterViewModel.Password.IsNullOrEmpty() && !this.RegisterViewModel.RepeatPassword.IsNullOrEmpty()
                        && !this.RegisterViewModel.InputAccessCode.IsNullOrEmpty() && this.RegisterViewModel.AccessCode == this.RegisterViewModel.InputAccessCode)
                    {
                        ServiceData serviceData = new()
                        {
                            TransactionScope = true,
                            Token = Factory.AccessKey
                        };
                        serviceData["1"].CommandText = this.GetAttribute("Create");
                        serviceData["1"].AddParameter("EMAIL", DbType.NVarChar, 100, this.RegisterViewModel.Email);
                        serviceData["1"].AddParameter("ACCESS_NUMBER", DbType.NVarChar, 4000, this.RegisterViewModel.Password.ComputeHash());
                        serviceData["1"].AddParameter("NICKNAME", DbType.NVarChar, 50, this.RegisterViewModel.Nickname);
                        serviceData["1"].AddParameter("FULLNAME", DbType.NVarChar, 200, this.RegisterViewModel.Fullname);
                        serviceData["1"].AddParameter("PHONENUMBER", DbType.NVarChar, 50, this.RegisterViewModel.PhoneNumber);
                        serviceData["1"].AddParameter("ACCESS_CODE", DbType.NVarChar, 10, this.RegisterViewModel.InputAccessCode);

                        response = await this.ServiceRequestAsync(serviceData);

                        if (response.Status == Status.OK)
                        {
                            this.RegisterViewModel.Password = string.Empty;
                            this.RegisterViewModel.RepeatPassword = string.Empty;

                            this.ToastShow("Register", this.Localization["회원가입이 완료되었습니다."], ToastDuration.Long);

                            this.OnAction(this, new MetaFrmEventArgs { Action = "Login" });
                            return true;
                        }
                        else
                        {
                            if (response.Message != null)
                            {
                                this.ModalShow("Register", response.Message, new() { { "Ok", Btn.Warning } }, EventCallback.Factory.Create<string>(this, OnClickFunctionAsync));
                            }
                        }
                    }
                }
            }
            finally
            {
                this.RegisterViewModel.IsBusy = false;
            }

            return false;
        }
        private async Task OnClickFunctionAsync(string action)
        {
            await Task.Delay(100);

            ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "inputaccesscode");
        }


        readonly System.Timers.Timer timer = new(1000);
        private async void HandleValidSubmitAsync(EditContext context)
        {
            try
            {

                if (this.RegisterViewModel.Email != null && !this.RegisterViewModel.AccessCodeVisible)
                {
                    this.RegisterViewModel.AccessCode = await this.JoinAccessCodeServiceRequestAsync(this.RegisterViewModel.Email);
                    this._isFocusElement = false;
                    this.RegisterViewModel.AccessCodeVisible = true;

                    try
                    {
                        this.timer.Elapsed -= Timer_Elapsed;
                    }
                    catch (Exception)
                    {
                    }
                    this.timer.Elapsed += Timer_Elapsed;
                    this.timer.Start();
                }
            }
            catch (Exception)
            {
            }
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                this.RemainingTime = this.RemainingTime.Add(new TimeSpan(0, 0, -1));

                if (this.RemainingTime.Ticks <= 0)
                {
                    this.RegisterViewModel.AccessCodeVisible = false;
                    this._isFocusElement = true;
                    this.RegisterViewModel.AccessCode = null;
                    this.RegisterViewModel.InputAccessCode = null;
                    this.RegisterViewModel.AccessCodeConfirmVisible = false;
                    this.RemainingTime = new TimeSpan(this.RemainingTimeOrg.Ticks);
                    this.timer.Stop();
                }

                this.InvokeAsync(this.StateHasChanged);
            }
            catch (Exception)
            {
            }
        }
        private void HandleInvalidSubmit(EditContext context)
        {
            this.RegisterViewModel.AccessCodeVisible = false;
        }

        private void EmailKeydown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" && !this.RegisterViewModel.Email.IsNullOrEmpty())
            {
                ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "nickname");
            }
        }
        private void NicknameKeydown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" && !this.RegisterViewModel.Nickname.IsNullOrEmpty())
            {
                ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "fullname");
            }
        }
        private void FullnameKeydown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" && !this.RegisterViewModel.Fullname.IsNullOrEmpty())
            {
                ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "phonenumber");
            }
        }
        private void PhonenumberKeydown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" && !this.RegisterViewModel.PhoneNumber.IsNullOrEmpty())
            {
                ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "password");
            }
        }

        private void PasswordKeydown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" && !this.RegisterViewModel.Password.IsNullOrEmpty())
            {
                ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "repeatpassword");
            }
        }
        private void RepeatPasswordKeydown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" && !this.RegisterViewModel.RepeatPassword.IsNullOrEmpty())
            {
                ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "agreelabel");
            }
        }


        private async void OnLinkClick(string? url)
        {
            try
            {
                this.RegisterViewModel.IsBusy = true;

                if (url == null)
                    return;

                Uri uri = new(url);

                if (this.Browser != null)
                    await this.Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception)
            {
            }
            finally
            {
                this.RegisterViewModel.IsBusy = false;
            }
        }

        private async void InputAccessCodeKeydown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                await this.OnRegisterClick();
                this.StateHasChanged();
            }
        }
    }
}