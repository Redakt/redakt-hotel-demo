using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Redakt.ContentManagement.NodeCollections;
using Redakt.ContentManagement.NodeCollections.Aggregates;
using Redakt.ContentManagement.NodeCollections.Commands;
using Redakt.EventSourcing;

namespace RedaktHotel.BackOfficeExtensions.Components
{
    public partial class WelcomeOnboard
    {
        #region [ Dependency Injection ]
        [Inject]
        private INodeCollectionService CollectionService { get; set; }

        [Inject]
        private IHttpContextAccessor HttpContextAccessor { get; set; }

        [Inject]
        private ICommandBus CommandBus { get; set; }
        #endregion

        #region [ Properties ]
        private string Hostname { get; set; }
        #endregion

        #region [ Overrides ]
        protected override async Task OnInitializedAsync()
        {
            this.Hostname = this.HttpContextAccessor.HttpContext.Request.Host.ToUriComponent();

            // By default, the demo site hostname is set to "localhost:5001" (the default application url for ASP.NET Core). If the current request hostname is different, update the hostname for the demo site accordingly.
            if (this.Hostname != "localhost:5001")
            {
                var sites = await this.CollectionService.GetCollectionsOfTypeAsync(NodeCollectionTypes.Site);
                var site = sites.First();

                foreach (var host in site.Hosts)
                {
                    await this.CommandBus.PublishAsync(new SetNodeCollectionHost(NodeCollectionId.With(site.Id), new NodeCollectionHost
                    {
                        Id = host.Id,
                        Scheme = host.Scheme,
                        Hostname = this.Hostname,
                        Cultures = host.Cultures.ToList(),
                        RootPath = host.RootPath
                    }));
                }
            }
        }
        #endregion
    }
}
