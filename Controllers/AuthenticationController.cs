using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Service.AuthService.Options;
using Service.AuthService;
using Data.POCO;

namespace University.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController :ControllerBase
    {
        private readonly AuthService _authService;
        private readonly AuthServiceOptions _options;
        private readonly CookieOptions _defaultCookieOptions;

        public AuthenticationController(AuthService authService, IOptions<AuthServiceOptions> options)
        {
            _authService = authService;
            _options = options.Value;
            _defaultCookieOptions = new CookieOptions()
            {
                Expires = DateTimeOffset.UtcNow.AddSeconds(_options.RefreshRokenExpirationtime),
                HttpOnly = _options.HttpOnly,
                Secure = _options.Secure,
                SameSite = _options.SameSite,
            };

        }

        [HttpPost]
        [Route("SignUp")]
        public async Task<ActionResult> SignUp([FromBody]SignUpRequest request)
        {
            var response = await _authService.SignUp(request);
            return Ok(response);
        }


    }
}
