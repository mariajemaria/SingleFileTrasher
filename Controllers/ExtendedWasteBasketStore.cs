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

            var content = this._contentLoader.Get<IContent>(id);

            if (!this._contentProviderManager.IsWastebasket(content.ParentLink))
            {
                throw new HttpException(400, "Not a valid wastebasket.");
            }
            if (!content.QueryDistinctAccess(AccessLevel.Delete))
            {
                throw new HttpException(403, "You do not have access rights to empty this waste basket.");
            }

            var actionResponse = new ActionResponse<ContentReference> { ExtraInformation = content.ParentLink };
            _contentRepository.Delete(id, true, AccessLevel.NoAccess);
            return Rest(actionResponse);
        }
    }
}
