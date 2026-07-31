namespace BodegaDESAM.Services;

/// <summary>
/// Servicio para gestionar el estado del tema (claro/oscuro) en la aplicación.
/// Este servicio mantiene el estado sincronizado con localStorage en el cliente.
/// </summary>
public class ThemeService
{
    private bool _isDarkMode = false;
    
    public event Action? OnThemeChanged;

    public bool IsDarkMode => _isDarkMode;

    public void SetDarkMode(bool isDark)
    {
        if (_isDarkMode != isDark)
        {
            _isDarkMode = isDark;
            OnThemeChanged?.Invoke();
        }
    }

    public void ToggleTheme()
    {
        SetDarkMode(!_isDarkMode);
    }
}
