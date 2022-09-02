using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace jwtTests.Controllers;
[Authorize]
[ApiController]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly IConfiguration _config;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private string generatedToken = null;


    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration config, ITokenService tokenService, IUserRepository userRepository)
    {
        _logger = logger;
        _tokenService = tokenService;
        _userRepository = userRepository;
        _config = config;
    }
    [HttpGet]
    [Route("api/getForecasts")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
    [AllowAnonymous]
    [Route("api/login")]
    [HttpPost]
    public string Login(UserModel userModel)
    {
        if (string.IsNullOrEmpty(userModel.UserName) || string.IsNullOrEmpty(userModel.Password))
        {
            var tokenResponse = new Token { token = "" };
            return Newtonsoft.Json.JsonConvert.SerializeObject(tokenResponse);
        }

        IActionResult response = Unauthorized();
        var validUser = _userRepository.GetUser(userModel);

        if (validUser != null)
        {
            generatedToken = _tokenService.BuildToken(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(),
            validUser);

            if (generatedToken != null)
            {
                HttpContext.Session.SetString("Token", generatedToken);
                var tokenResponse = new Token { token = generatedToken };
                return Newtonsoft.Json.JsonConvert.SerializeObject(tokenResponse);
            }
            else
            {
                var tokenResponse = new Token { token = "" };
                return Newtonsoft.Json.JsonConvert.SerializeObject(tokenResponse);
            }
        }
        else
        {
            var tokenResponse = new Token { token = "" };
            return Newtonsoft.Json.JsonConvert.SerializeObject(tokenResponse);
        }
    }

}

