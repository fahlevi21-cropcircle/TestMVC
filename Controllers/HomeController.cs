using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Security.Claims;
using System.Text.Json;
using Azure;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestMVC.Data;
using TestMVC.Models;

namespace TestMVC.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAuthService _authService;
    private readonly IUtilityService _utilityService;

    public HomeController(ILogger<HomeController> logger, IAuthService authService, IUtilityService utilityService)
    {
        _logger = logger;
        _authService = authService;
        _utilityService = utilityService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction(nameof(Index));
        }
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel data)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        SearchResponse auth = _authService.LDAPLogin(data.Username, data.Password);

        if (auth == null)
        {
            ModelState.AddModelError("", "Login failed. invalid credential");
            return View();
        }

        string mail = auth.Entries[0].Attributes["mail"][0].ToString();
        string uid = auth.Entries[0].Attributes["uid"][0].ToString();
        string name = auth.Entries[0].Attributes["cn"][0].ToString();

        var user = new User { Id = uid, Name = name, Email = mail, TFA = true };
        var payload = JsonSerializer.Serialize(user);
        HttpContext.Session.SetString("user", payload);
        HttpContext.Session.SetString("name", name);

        return RedirectToAction(nameof(Otp));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Otp()
    {
        var payload = HttpContext.Session.GetString("user");

        if (string.IsNullOrEmpty(payload))
        {
            return RedirectToAction(nameof(Login));
        }

        var user = JsonSerializer.Deserialize<User>(payload);
        var otp = await _authService.GenerateUserTokenAsync(user.Id, user.Email);

        ViewBag.Token = otp;
        ViewBag.Email = user.Email;

        string message = $"Dear {user.Name}, here is your requested code : {otp}";
        await _utilityService.SendEmail(user.Email, "Two Factor Authentication Code", message);

        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> OtpCheck(string token)
    {
        var payload = HttpContext.Session.GetString("user");

        if (string.IsNullOrEmpty(payload))
        {
            return RedirectToAction(nameof(Login));
        }

        var user = JsonSerializer.Deserialize<User>(payload);

        var match = await _authService.ValidateUserTokenAsync(user.Id, token);

        if (!match)
        {
            ViewBag.Message = "Incorrect code";
            return View("Otp");
        }

        var claims = new List<Claim> {
         new Claim(ClaimTypes.Name, user.Name),
         new Claim(ClaimTypes.Email, user.Email),
         new Claim(ClaimTypes.PrimarySid, user.Id),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync();

        return RedirectToAction(nameof(Login));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
