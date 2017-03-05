using System.Web.Http;
using Carbon.Business.Services;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("data")]
    public class DataController : ApiController
    {
        private readonly DataService _dataService;

        public DataController(DataService dataService)
        {
            _dataService = dataService;
        }

        [Route("generate"), HttpPost]
        public IHttpActionResult Generate(string fields, int rows)
        {
            return Ok(_dataService.Generate(fields, rows));
        }

        [Route("discover"), HttpGet]
        public IHttpActionResult Discover()
        {
            return Ok(_dataService.Discover());
        }
    }
}