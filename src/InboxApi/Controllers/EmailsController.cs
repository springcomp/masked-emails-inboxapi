using System.Collections.Generic;
using System.Threading.Tasks;
using InboxApi.Interop;
using InboxApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InboxApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public sealed class EmailsController : ControllerBase
    {
        private readonly IInboxService inboxService_;
        private readonly ILogger<EmailsController> logger_;

        public EmailsController(IInboxService inboxService, ILogger<EmailsController> logger)
        {
            inboxService_ = inboxService;
            logger_ = logger;
        }

        [Route("list")]
        [HttpPost]
        public async Task<ActionResult> GetEmails([FromBody] string[] inboxes)
        {
            var results = new List<InboxMessageSpec>();
            foreach (var inbox in inboxes)
            {
                await foreach (var message in inboxService_.GetMessagesAsync(inbox))
                    results.Add(message);
            }

            return Ok(results.ToArray());
        }

        [Route("body")]
        [HttpPost]
        public async Task<ActionResult> GetEmail([FromBody] string path)
        {
            var message = await inboxService_.GetMessageAsync(path);
            return Ok(message);
        }
    }
}