using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Protocols;
using Services;
using Utils;

namespace Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }
        
        [HttpPost]
        [Route("login")]
        public async Task<LoginRes> LoginAccount([FromBody] LoginReq req)
        {
            LoginRes res = await _accountService.LoginAccount(req);
            return res;
        }
        
        [HttpPost]
        [Authorize]
        // 토큰 갱신
        [Route("refreshToken")]
        public async Task<TokenRefreshRes?> RefreshToken([FromServices] IHttpContextAccessor httpContextAccessor)
        {
            string accountId = httpContextAccessor.GetUserId();
            if (accountId == "")
                throw new Exception("Failed to get accountId from HttpContextAccessor.");

            string token = await _accountService.RefreshJwt(accountId);
            if (token == null)
                throw new Exception($"Failed to refresh JWT. accountId: {accountId}");

            TokenRefreshRes res = new TokenRefreshRes
            {
                Token = token
            };
            return res;
        }

        [HttpPost]
        [Authorize]
        // 계정정보
        [Route("accountInfo")]
        public async Task<AccountInfoRes?> GetAccountInfo([FromServices] IHttpContextAccessor httpContextAccessor)
        {
            string accountId = httpContextAccessor.GetUserId();
            if (accountId == "")
                throw new Exception("Failed to get accountId from HttpContextAccessor.");
            
            AccountInfoRes? res = await _accountService.GetAccountInfo(accountId);
            
            return res;
        }

        [HttpPost]
        [Authorize]
        // 재화정보
        [Route("currencyInfo")]
        public async Task<AccountCurrencyInfoRes> GetCurrencyInfo(
            [FromServices] IHttpContextAccessor httpContextAccessor)
        {
            string userId = httpContextAccessor.GetUserId();
            if (userId == "")
                throw new Exception("Failed to get userId from HttpContextAccessor.");
            
            AccountCurrencyInfoRes? res = await _accountService.GetAccountCurrencyInfo(userId);

            return res;
        }
    }
}