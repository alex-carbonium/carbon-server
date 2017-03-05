using System;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Carbon.Business.Domain;
using Carbon.Owin.Common.WebApi;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("fonts")]
    public class FontsController : BaseApiController
    {
        private const int PageSize = 50;
        private readonly FontManager _fontManager;

        public FontsController(FontManager fontManager)
        {
            _fontManager = fontManager;
        }

        [Route("system")]
        public dynamic GetSystemFonts(int pageNumber = 1)
        {
            var result = new JObject();
            if (pageNumber == 1)
            {
                result["fonts"] = _fontManager.Popular;
            }
            else
            {
                result["fonts"] = new JArray(_fontManager.Collection
                    .Skip((pageNumber - 2) * PageSize).Take(PageSize));
            }
            result["pageCount"] = GetTotalPages();
            return result;
        }

        [Route("search"), HttpGet]
        public dynamic Search(string query, int pageNumber = 1)
        {
            var result = new JObject();
            var fonts = _fontManager.Collection
                .Where(x => x.Value<string>("name").IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1)
                .ToList();
            result["fonts"] = new JArray(fonts.Skip((pageNumber - 1) * PageSize).Take(PageSize));
            result["pageCount"] = (int)Math.Ceiling((decimal)fonts.Count/PageSize);
            return result;
        }

        private int GetTotalPages()
        {
            return (int)Math.Ceiling((decimal)_fontManager.Collection.Count / PageSize) + 1;
        }
    }
}