using System;
using EPiServer.Cms.Shell.UI.Rest.Internal;
using EPiServer.Cms.Shell.UI.Rest.Models.Internal;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Rest;

namespace SingleFileTrasher.Initialization
{
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class CustomUriContextResolverInitializer : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {

            context.Services
                .Intercept<IUriContextResolver>(
                    (locator, uriContextResolver) =>
                        uriContextResolver is CmsContentContextResolver
                            ? new CustomUriContextResolver((CmsContentContextResolver)uriContextResolver)
                            : uriContextResolver)
                .Intercept<IUrlContextResolver>(
                    (locator, urlContextResolver) =>
                        urlContextResolver is CmsContentContextResolver
                            ? new CustomUriContextResolver((CmsContentContextResolver)urlContextResolver)
                            : urlContextResolver);
        }

        public void Initialize(InitializationEngine context) { }
        public void Uninitialize(InitializationEngine context) { }
    }
    public class CustomUriContextResolver : IUriContextResolver, IUrlContextResolver
    {
        private readonly CmsContentContextResolver _cmsContentContextResolver;
        public string Name => this._cmsContentContextResolver.Name;

        public int SortOrder => this._cmsContentContextResolver.SortOrder;

        public CustomUriContextResolver(CmsContentContextResolver cmsContentContextResolver)
        {
            _cmsContentContextResolver = cmsContentContextResolver;
        }

        public bool TryResolveUri(Uri uri, out ClientContextBase instance)
        {
            var result = this._cmsContentContextResolver.TryResolveUri(uri, out instance);
            AssignCustomTrashComponent(instance);
            return result;
        }

        public bool TryResolveUrl(Uri url, out ClientContextBase instance)
        {
            var result = this._cmsContentContextResolver.TryResolveUrl(url, out instance);
            AssignCustomTrashComponent(instance);
            return result;
        }

        private static void AssignCustomTrashComponent(ClientContextBase instance)
        {
            if (instance != null)
            {
                var contentDataContext = (ContentDataContext)instance;
                if (contentDataContext.CustomViewType == "epi-cms/component/Trash")
                {
                    contentDataContext.CustomViewType = "customTrash/Trash";
                }
            }
        }
    }
}
