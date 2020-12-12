using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Redakt;
using Redakt.ContentManagement;
using Redakt.ContentManagement.NodeCollections;
using Redakt.ContentManagement.NodeCollections.Aggregates;
using Redakt.ContentManagement.NodeCollections.Commands;
using Redakt.EventSourcing;
using Redakt.Extensions;

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

        [Inject]
        private IRedaktSystem System { get; set; }
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
                var site = await this.CollectionService.GetCollectionsOfTypeAsync(NodeCollectionTypes.Site).FirstAsync();

                foreach (var host in site.Hosts)
                {
                    await this.CommandBus.PublishAsync(new SetNodeCollectionHost(NodeCollectionId.With(site.Id), new NodeCollectionHostAggregate
                    {
                        Id = NodeCollectionHostId.With(host.Id),
                        Scheme = host.Scheme,
                        Hostname = this.Hostname,
                        Cultures = host.Cultures.ToList(),
                        RootPath = host.RootPath
                    }));
                }
            }

            this.System.Module<ContentManagementModule>().IsProvisioned = true;
        }
        #endregion
    }
}
