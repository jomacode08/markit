using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Common
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
    }
}