using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FeedbackApp.Pages.Admin;

public class LoginModel : PageModel
{
    // Simple hardcoded credentials for demo purposes
    // In production, use ASP.NET Core Identity or similar
    private const string AdminUsername = "admin";
    private const string AdminPassword = "admin123";

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        // If already logged in, redirect to dashboard
        if (HttpContext.Session.GetString("IsAdmin") == "true")
        {
            return RedirectToPage("/Admin/Index");
        }
        return Page();
    }

    public IActionResult OnPost()
    {
        if (Username == AdminUsername && Password == AdminPassword)
        {
            HttpContext.Session.SetString("IsAdmin", "true");
            return RedirectToPage("/Admin/Index");
        }

        ErrorMessage = "Invalid username or password";
        return Page();
    }
}
