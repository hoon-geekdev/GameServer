using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Protocols;
using Services;
using Utils;
using Serilog;

namespace Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/item")]
    public class ItemController
    {
        private readonly ItemService _itemService;

        public ItemController(ItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpPost]
        [Route("acquire")]
        public async Task<ItemAcqRes?> AcquireItem([FromServices] IHttpContextAccessor httpContextAccessor, [FromBody] ItemAcqReq req)
        {
            string accountId = httpContextAccessor.GetUserId();
            if (accountId == "")
            {
                Log.Error("Failed to get accountId from HttpContextAccessor.");
                return null;
            }

            ItemAcqRes? res = await _itemService.ItemAcq(accountId, req);
            return res;
        }

        [HttpPost]
        [Route("list")]
        public async Task<ItemListRes?> GetItemList([FromServices] IHttpContextAccessor httpContextAccessor)
        {
            string accountId = httpContextAccessor.GetUserId();
            if (accountId == "")
            {
                Log.Error("Failed to get accountId from HttpContextAccessor.");
                return null;
            }

            ItemListRes? res = await _itemService.ItemList(accountId);
            return res;
        }

        [HttpPost]
        [Route("unequip")]
        public async Task<ItemUnequipRes?> UnequipItem([FromServices] IHttpContextAccessor httpContextAccessor, [FromBody] ItemUnequipReq req)
        {
            string accountId = httpContextAccessor.GetUserId();
            if (accountId == "")
            {
                Log.Error("Failed to get accountId from HttpContextAccessor.");
                return null;
            }

            ItemUnequipRes? res = await _itemService.ItemUnequip(accountId, req);
            return res;
        }
    }
}
