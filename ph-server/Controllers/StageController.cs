using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Protocols;
using Serilog;
using Services;
using Utils;

namespace Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/stage")]
    public class StageController
    {
        private readonly StageService _stageService;

        public StageController(StageService stageService)
        {
            _stageService = stageService;
        }

        [HttpPost]
        [Route("enter")]
        public async Task<StageEnterRes?> StageEnter([FromServices] IHttpContextAccessor httpContextAccessor, [FromBody] StageEnterReq req)
        {
            string accountId = httpContextAccessor.GetUserId();
            if (accountId == "")
            {
                Log.Error("Failed to get accountId from HttpContextAccessor.");
                return null;
            }
            StageEnterRes? res = await _stageService.StageEnter(accountId, req);
            return res;
        }

        [HttpPost]
        [Route("clear")]
        public async Task<StageClearRes?> StageClear([FromServices] IHttpContextAccessor httpContextAccessor, [FromBody] StageClearReq req)
        {
            string accountId = httpContextAccessor.GetUserId();
            if (accountId == "")
            {
                Log.Error("Failed to get accountId from HttpContextAccessor.");
                return null;
            }

            StageClearRes? res = await _stageService.StageClear(accountId, req);
            return res;
        }
    }
}
