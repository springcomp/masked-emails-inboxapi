using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InboxApi.Controllers
{
    [ApiController()]
    [Route("")]
    public sealed class HomeController : ControllerBase
    {
        [HttpGet()]
        [Route("")]
        public ContentResult Index()
        {
            return Content(@"
<html>
<head>
<link href=""https://fonts.googleapis.com/css?family=Roboto:300,400,500&display=swap"" rel=""stylesheet"">
<link href=""https://fonts.googleapis.com/icon?family=Material+Icons"" rel=""stylesheet"">
</head>
<body>
  <span class=""title"">Masked Emails Inbox API</span>
</body></html>
", "text/html");
        }
    }
}