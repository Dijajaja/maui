using System.ComponentModel;
using System.Runtime.CompilerServices;
using AppTodoPro.Services;
using Microsoft.Maui.Graphics;

namespace AppTodoPro.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly AuthService authService;
    private string email = string.Empty;
    private string password = string.Empty;
    private string confirmPassword = string.Empty;
    private string name = string.Empty;
    private string? errorMessage;
    private string? emailError;
    private string? passwordError;
    private string? confirmPasswordError;
    private string? nameError;
    private string? statusMessage;
    private bool isRegisterMode;
    private bool isBusy;
    private bool showPassword;

    public LoginViewModel(AuthService authService)
    {
        this.authService = authService;
        SubmitCommand = new Command(async () => await SubmitAsync(), () => !IsBusy);
        ToggleModeCommand = new Command(ToggleMode);
        SetLoginModeCommand = new Command(SetLoginMode);
        SetRegisterModeCommand = new Command(SetRegisterMode);
        TogglePasswordCommand = new Command(TogglePassword);
    }

    public string Email
    {
        get => email;
        set
        {
            if (email == value)
            {
                return;
            }

            email = value;
            OnPropertyChanged();
        }
    }

    public string Password
    {
        get => password;
        set
        {
            if (password == value)
            {
                return;
            }

            password = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PasswordStrengthLabel));
            OnPropertyChanged(nameof(PasswordStrengthColor));
            OnPropertyChanged(nameof(PasswordStrengthRatio));
            OnPropertyChanged(nameof(HasPasswordStrength));
        }
    }

    public string ConfirmPassword
    {
        get => confirmPassword;
        set
        {
            if (confirmPassword == value)
            {
                return;
            }

            confirmPassword = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => name;
        set
        {
            if (name == value)
            {
                return;
            }

            name = value;
            OnPropertyChanged();
        }
    }

    public string? EmailError
    {
        get => emailError;
        set
        {
            if (emailError == value)
            {
                return;
            }

            emailError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasEmailError));
        }
    }

    public bool HasEmailError => !string.IsNullOrWhiteSpace(EmailError);

    public string? PasswordError
    {
        get => passwordError;
        set
        {
            if (passwordError == value)
            {
                return;
            }

            passwordError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasPasswordError));
        }
    }

    public bool HasPasswordError => !string.IsNullOrWhiteSpace(PasswordError);

    public bool HasPasswordStrength => IsRegisterMode && !string.IsNullOrWhiteSpace(Password);

    public string PasswordStrengthLabel => GetPasswordStrengthLabel(Password);

    public Color PasswordStrengthColor => GetPasswordStrengthColor(Password);

    public double PasswordStrengthRatio => GetPasswordStrengthRatio(Password);

    public string? ConfirmPasswordError
    {
        get => confirmPasswordError;
        set
        {
            if (confirmPasswordError == value)
            {
                return;
            }

            confirmPasswordError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasConfirmPasswordError));
        }
    }

    public bool HasConfirmPasswordError => !string.IsNullOrWhiteSpace(ConfirmPasswordError);

    public string? NameError
    {
        get => nameError;
        set
        {
            if (nameError == value)
            {
                return;
            }

            nameError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasNameError));
        }
    }

    public bool HasNameError => !string.IsNullOrWhiteSpace(NameError);

    public string? StatusMessage
    {
        get => statusMessage;
        set
        {
            if (statusMessage == value)
            {
                return;
            }

            statusMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasStatusMessage));
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public string? ErrorMessage
    {
        get => errorMessage;
        set
        {
            if (errorMessage == value)
            {
                return;
            }

            errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool IsRegisterMode
    {
        get => isRegisterMode;
        set
        {
            if (isRegisterMode == value)
            {
                return;
            }

            isRegisterMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PrimaryButtonText));
            OnPropertyChanged(nameof(ToggleModeText));
            OnPropertyChanged(nameof(HasPasswordStrength));
            OnPropertyChanged(nameof(PasswordStrengthLabel));
            OnPropertyChanged(nameof(PasswordStrengthColor));
            OnPropertyChanged(nameof(PasswordStrengthRatio));
        }
    }

    public string PrimaryButtonText => IsBusy
        ? "Connexion..."
        : IsRegisterMode ? "Créer le compte" : "Se connecter";

    public string ToggleModeText => IsRegisterMode ? "J'ai déjà un compte" : "Créer un compte";

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (isBusy == value)
            {
                return;
            }

            isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PrimaryButtonText));
            SubmitCommand.ChangeCanExecute();
        }
    }

    public bool ShowPassword
    {
        get => showPassword;
        set
        {
            if (showPassword == value)
            {
                return;
            }

            showPassword = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsPasswordHidden));
        }
    }

    public bool IsPasswordHidden => !ShowPassword;

    public Command SubmitCommand { get; }

    public Command ToggleModeCommand { get; }

    public Command SetLoginModeCommand { get; }

    public Command SetRegisterModeCommand { get; }

    public Command TogglePasswordCommand { get; }

    public event EventHandler? LoginSucceeded;

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task SubmitAsync()
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = null;
        EmailError = null;
        PasswordError = null;
        NameError = null;
        ConfirmPasswordError = null;
        StatusMessage = null;
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email requis.";
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = "Mot de passe requis.";
            }

            return;
        }

        if (IsRegisterMode && string.IsNullOrWhiteSpace(Name))
        {
            NameError = "Nom requis.";
            return;
        }

        if (IsRegisterMode && string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ConfirmPasswordError = "Confirmation requise.";
            return;
        }

        if (IsRegisterMode && !string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
        {
            ConfirmPasswordError = "Les mots de passe ne correspondent pas.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = IsRegisterMode ? "Création du compte..." : "Connexion en cours...";
            if (IsRegisterMode)
            {
                var registered = await authService.RegisterAsync(Email, Password, Name);
                if (registered is null)
                {
                    ErrorMessage = "Compte déjà existant ou informations invalides.";
                    return;
                }

                authService.SetCurrentUser(registered.Id);
            }
            else
            {
                var account = await authService.LoginAsync(Email, Password);
                if (account is null)
                {
                    ErrorMessage = "Email ou mot de passe incorrect.";
                    EmailError = "Email ou mot de passe incorrect.";
                    PasswordError = "Email ou mot de passe incorrect.";
                    return;
                }

                authService.SetCurrentUser(account.Id);
            }

            Password = string.Empty;
            ConfirmPassword = string.Empty;
            Name = string.Empty;
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            StatusMessage = null;
            IsBusy = false;
        }
    }

    private void ToggleMode()
    {
        IsRegisterMode = !IsRegisterMode;
        ErrorMessage = null;
        EmailError = null;
        PasswordError = null;
        NameError = null;
        ConfirmPasswordError = null;
    }

    private void SetLoginMode()
    {
        IsRegisterMode = false;
        ErrorMessage = null;
        EmailError = null;
        PasswordError = null;
        NameError = null;
        ConfirmPasswordError = null;
    }

    private void SetRegisterMode()
    {
        IsRegisterMode = true;
        ErrorMessage = null;
        EmailError = null;
        PasswordError = null;
        NameError = null;
        ConfirmPasswordError = null;
    }

    private void TogglePassword()
    {
        ShowPassword = !ShowPassword;
    }

    private static string GetPasswordStrengthLabel(string raw)
    {
        var score = GetPasswordScore(raw);
        return score switch
        {
            <= 1 => "Faible",
            2 => "Moyen",
            3 => "Fort",
            _ => "Très fort"
        };
    }

    private static Color GetPasswordStrengthColor(string raw)
    {
        var score = GetPasswordScore(raw);
        return score switch
        {
            <= 1 => Color.FromArgb("#D32F2F"),
            2 => Color.FromArgb("#FB8C00"),
            3 => Color.FromArgb("#43A047"),
            _ => Color.FromArgb("#1B5E20")
        };
    }

    private static int GetPasswordScore(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return 0;
        }

        var score = 0;
        if (raw.Length >= 8)
        {
            score++;
        }

        if (raw.Any(char.IsLower))
        {
            score++;
        }

        if (raw.Any(char.IsUpper))
        {
            score++;
        }

        if (raw.Any(char.IsDigit))
        {
            score++;
        }

        if (raw.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            score++;
        }

        if (raw.Length <= 5)
        {
            return 1;
        }

        return Math.Min(4, score);
    }

    private static double GetPasswordStrengthRatio(string raw)
    {
        var score = GetPasswordScore(raw);
        if (score <= 0)
        {
            return 0;
        }

        return Math.Min(1.0, score / 4.0);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
