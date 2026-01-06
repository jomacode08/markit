using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Common
{
    [ApiController]
    [Route("[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
    }
}