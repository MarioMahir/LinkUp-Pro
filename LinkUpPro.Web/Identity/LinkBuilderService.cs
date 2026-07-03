using LinkUpPro.Application.Interfaces;

namespace LinkUpPro.Web.Identity
{
    public class LinkBuilderService : ILinkBuilderService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LinkBuilderService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string BuildAbsoluteUrl(string relativePath)
        {
            var request = _httpContextAccessor.HttpContext!.Request;

            return $"{request.Scheme}://{request.Host}{relativePath}";
        }
    }
}
