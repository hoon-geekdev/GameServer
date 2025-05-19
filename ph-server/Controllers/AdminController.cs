using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Protocols;
using Services;
using Utils;
using Serilog;

namespace Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController
    {
        private readonly AdminService _adminService;
        
        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPut]
        [Route("table/upload")]
        [RequestSizeLimit(100_000_000)]
        public async Task TableUpload(List<IFormFile> files)
        {
            await _adminService.TableUpload(files);
        }
    }
}
