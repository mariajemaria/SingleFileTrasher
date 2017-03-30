using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Cms.Shell.UI.Rest.Models;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Security;
using EPiServer.Shell.Services.Rest;

namespace SingleContentItemTrasher.Controllers
{
    [RestStore("extendedWastebasket")]
    public class ExtendedWasteBasketStore : RestControllerBase
    {
        private readonly IContentLoader _contentLoader;
        private readonly IContentRepository _contentRepository;
        private readonly IContentProviderManager _contentProviderManager;

        public ExtendedWasteBasketStore(
            IContentProviderManager contentProviderManager, 
            IContentLoader contentLoader,
            IContentRepository contentRepository)
        {
            _contentProviderManager = contentProviderManager;
            _contentLoader = contentLoader;
            _contentRepository = contentRepository;
        }

        public RestResult PermanentDelete(ContentReference id)
        {
            Validator.ThrowIfNull("id", id);

            var content = _contentLoader.Get<IContent>(id);

            if (!content.QueryDistinctAccess(AccessLevel.Delete))
            {
                throw new HttpException(403, "You do not have access rights to empty this waste basket.");
            }

            var secondReverseParent = _contentLoader.GetAncestors(content.ContentLink).Reverse().ElementAt(1);
            if (!_contentProviderManager.IsWastebasket(secondReverseParent.ContentLink))
            {
                throw new HttpException(400, "Not a valid wastebasket.");
            }

            var actionResponse = new ActionResponse<ContentReference> { ExtraInformation = secondReverseParent.ContentLink };
            _contentRepository.Delete(id, true, AccessLevel.NoAccess);
            return Rest(actionResponse);
        }
    }
}