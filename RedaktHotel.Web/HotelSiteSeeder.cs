using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Redakt.ContentManagement.Configuration;
using Redakt.ContentManagement.Content;
using Redakt.ContentManagement.Nodes;
using Redakt.Languages;
using Redakt.Extensions;
using Redakt.Files;
using Redakt.ContentManagement.NestedContent;
using RedaktHotel.Web.Models.Assets;
using Microsoft.AspNetCore.Http;
using RedaktHotel.Web.Models.Pages;
using RedaktHotel.Web.Models.Embedded;
using LoremNET;
using Redakt.Data;
using Redakt.Domain.Commands;
using Redakt.ContentManagement.NodeCollections;
using Redakt.ContentManagement.NodeCollections.Aggregates;
using Redakt.ContentManagement.NodeCollections.Commands;
using Redakt.Dictionary.Commands;
using Redakt.ContentManagement.Models;
using Redakt.Files.Commands;
using Redakt.ContentManagement.Files;

namespace RedaktHotel.Web
{
    /// <summary>
    /// The site seeder is called on startup to generate random content for the demo site. In most scenarios you do not need to create content programmatically, in which case you can ignore this whole class, however this class provides an example in case you do.
    /// </summary>
    public class HotelSiteSeeder : IStartupFilter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CultureInfo _englishCulture = CultureInfo.GetCultureInfo("en");
        private readonly CultureInfo _dutchCulture = CultureInfo.GetCultureInfo("nl");
        private readonly Random _rnd = new Random();

        public HotelSiteSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            this.ProvisionSiteAsync().GetAwaiter().GetResult();

            return next;
        }

        private class SeederContext
        {
            public string SiteId { get; set; }

            public string AssetLibraryId { get; set; }

            public string StaffMembersFolderId { get; set; }

            public string ImagesFolderId { get; set; }

            public List<string> AllImages { get; } = new List<string>();

            public string BlogParentId { get; set; }

            public List<string> Rooms { get; } = new List<string>();

            public List<string> Facilities { get; } = new List<string>();

            public string LabelsCategoryId { get; set; }

            public List<ICommand> Commands { get; } = new List<ICommand>();
        }

        private async Task ProvisionSiteAsync()
        {
            using var scope = _serviceProvider.CreateScope();

            var languageService = scope.ServiceProvider.GetRequiredService<ILanguageService>();

            // If languages exist we assume that the site was already created before
            if ((await languageService.GetAllLanguagesAsync()).Any()) return;

            // Create a context helper class to store variables in.
            var context = new SeederContext();

            // Add the languages we want to use in the system
            await languageService.SaveLanguageAsync(new ApplicationLanguage(_englishCulture));
            //await _languageService.SaveLanguageAsync(new ApplicationLanguage(CultureInfo.GetCultureInfo("en-US")));
            await languageService.SaveLanguageAsync(new ApplicationLanguage(_dutchCulture));

            // Create default dictionary categories
            await this.CreateDictionaryAsync(scope.ServiceProvider, context);

            // Create asset libraries and folders
            await this.CreateLibrariesAsync(scope.ServiceProvider, context);

            // Create fictional staff members
            await this.CreateStaffMembersAsync(scope.ServiceProvider, context);

            // Get the http context accessor to get the current hostname.
            var hca = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var hostname = hca?.HttpContext?.Request?.Host.ToUriComponent() ?? "localhost:5001";

            // Create the site
            await this.CreateSiteAsync(scope.ServiceProvider, context, hostname);

            // Create the site's pages
            await this.CreatePagesAsync(scope.ServiceProvider, context);
        }

        private async Task CreateDictionaryAsync(IServiceProvider serviceProvider, SeederContext context)
        {
            var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();

            // Create labels dictionary category
            context.LabelsCategoryId = IdGenerator.GenerateId();
            await dispatcher.ExecuteAsync(new CreateDictionaryCategory(context.LabelsCategoryId, "Labels"));
            
            await this.CreateDictionaryEntryAsync(dispatcher, context.LabelsCategoryId, "TheRedaktHotelAndResort", "The Redakt Hotel and Resort", "Het Redakt Hotel en Resort");
            await this.CreateDictionaryEntryAsync(dispatcher, context.LabelsCategoryId, "LearnMore", "Learn More", "Meer weten");
            await this.CreateDictionaryEntryAsync(dispatcher, context.LabelsCategoryId, "ReadMore", "Read More", "Lees verder");

            await this.CreateDictionaryEntryAsync(dispatcher, context.LabelsCategoryId, "news", "News", "Nieuws");
            await this.CreateDictionaryEntryAsync(dispatcher, context.LabelsCategoryId, "hotel", "Hotel", "Hotel");
            await this.CreateDictionaryEntryAsync(dispatcher, context.LabelsCategoryId, "dining", "Dining", "Dineren");
            await this.CreateDictionaryEntryAsync(dispatcher, context.LabelsCategoryId, "facilities", "Facilities", "Faciliteiten");
            await this.CreateDictionaryEntryAsync(dispatcher, context.LabelsCategoryId, "offers", "Offers", "Aanbiedingen");
            await this.CreateDictionaryEntryAsync(dispatcher, context.LabelsCategoryId, "events", "Events", "Evenementen");
            await this.CreateDictionaryEntryAsync(dispatcher, context.LabelsCategoryId, "excursions", "Excursions", "Excursies");
            await this.CreateDictionaryEntryAsync(dispatcher, context.LabelsCategoryId, "spa", "Spa & wellness", "Spa & wellness");

            // Create booking dictionary category
            var bookingCategoryId = IdGenerator.GenerateId();
            await dispatcher.ExecuteAsync(new CreateDictionaryCategory(bookingCategoryId, "Booking"));
            
            await this.CreateDictionaryEntryAsync(dispatcher, bookingCategoryId, "Booking", "Booking", "Reservering");
            await this.CreateDictionaryEntryAsync(dispatcher, bookingCategoryId, "BookThisRoom", "Book this room", "Reserveer kamer");
            await this.CreateDictionaryEntryAsync(dispatcher, bookingCategoryId, "CheckInDate", "Check In", "Incheckdatum");
            await this.CreateDictionaryEntryAsync(dispatcher, bookingCategoryId, "CheckOutDate", "Check Out", "Uitcheckdatum");
            await this.CreateDictionaryEntryAsync(dispatcher, bookingCategoryId, "Adults", "Adults", "Volwassenen");
            await this.CreateDictionaryEntryAsync(dispatcher, bookingCategoryId, "Adult", "Adult", "Volwassene");
            await this.CreateDictionaryEntryAsync(dispatcher, bookingCategoryId, "Children", "Children", "Kinderen");
            await this.CreateDictionaryEntryAsync(dispatcher, bookingCategoryId, "Child", "Child", "Kind");
            await this.CreateDictionaryEntryAsync(dispatcher, bookingCategoryId, "BookYourStay", "Book your stay with us", "Reserveer uw verblijf bij ons");
            await this.CreateDictionaryEntryAsync(dispatcher, bookingCategoryId, "BookNow", "Book now", "Reserveer nu");
        }

        private async Task CreateDictionaryEntryAsync(ICommandDispatcher dispatcher, string categoryId, string key, string englishLabel, string dutchLabel)
        {
            var itemId = IdGenerator.GenerateId();
            await dispatcher.ExecuteAsync(new CreateDictionaryItem(itemId, categoryId, key));

            var values = new Dictionary<CultureInfo, string>
            {
                [_englishCulture] = englishLabel, 
                [_dutchCulture] = dutchLabel
            };

            await dispatcher.ExecuteAsync(new SetDictionaryItemValues(itemId, values));
        }

        private async Task CreateLibrariesAsync(IServiceProvider serviceProvider, SeederContext context)
        {
            var folderDefinition = ContentTypeDefinition.Lookup<MediaFolder>();
            var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();

            // Create media library
            context.AssetLibraryId = IdGenerator.GenerateId();
            await dispatcher.ExecuteAsync(new CreateNodeCollection(context.AssetLibraryId, NodeCollectionTypes.AssetLibrary, "Media", 0));
            await dispatcher.ExecuteAsync(new SetNodeCollectionMapping(context.AssetLibraryId, new NodeCollectionMapping { Id = IdGenerator.GenerateId(), Culture = _englishCulture }));
            await dispatcher.ExecuteAsync(new SetNodeCollectionMapping(context.AssetLibraryId, new NodeCollectionMapping { Id = IdGenerator.GenerateId(), Culture = _dutchCulture }));
            //await dispatcher.ExecuteAsync(new AddNodeCollectionMapping(context.AssetLibraryId, IdGenerator.GenerateId(), CultureInfo.GetCultureInfo("en-US"), _englishCulture));

            // Create staff library
            var staffLibraryId = IdGenerator.GenerateId();
            await dispatcher.ExecuteAsync(new CreateNodeCollection(staffLibraryId, NodeCollectionTypes.AssetLibrary, "Staff", 0));
            await dispatcher.ExecuteAsync(new SetNodeCollectionMapping(staffLibraryId, new NodeCollectionMapping { Id = IdGenerator.GenerateId(), Culture = _englishCulture }));
            await dispatcher.ExecuteAsync(new SetNodeCollectionMapping(staffLibraryId, new NodeCollectionMapping { Id = IdGenerator.GenerateId(), Culture = _dutchCulture }));

            context.ImagesFolderId = IdGenerator.GenerateId();
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.CreateRootNode(context.ImagesFolderId, folderDefinition, "Images", context.AssetLibraryId, 0));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.CreateRootNode(IdGenerator.GenerateId(), folderDefinition, "Videos", context.AssetLibraryId, 1));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.CreateRootNode(IdGenerator.GenerateId(), folderDefinition, "Documents", context.AssetLibraryId, 2));

            context.StaffMembersFolderId = IdGenerator.GenerateId();
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.CreateRootNode(context.StaffMembersFolderId, ContentTypeDefinition.Lookup<StaffFolder>(), "Staff Members", staffLibraryId, 0));
        }

        private async Task CreateSiteAsync(IServiceProvider serviceProvider, SeederContext context, string hostname)
        {
            var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();

            context.SiteId = IdGenerator.GenerateId();
            await dispatcher.ExecuteAsync(new CreateNodeCollection(context.SiteId, NodeCollectionTypes.Site, "The Redakt Hotel", 0));
            await dispatcher.ExecuteAsync(new SetNodeCollectionMapping(context.SiteId, new NodeCollectionMapping { Id = IdGenerator.GenerateId(), Culture = _englishCulture }));
            await dispatcher.ExecuteAsync(new SetNodeCollectionMapping(context.SiteId, new NodeCollectionMapping { Id = IdGenerator.GenerateId(), Culture = _dutchCulture }));

            await dispatcher.ExecuteAsync(new SetNodeCollectionHost(context.SiteId, new NodeCollectionHost { Id = IdGenerator.GenerateId(), Hostname = hostname, RootPath = "/en", Cultures = new List<CultureInfo> { _englishCulture } }));
            await dispatcher.ExecuteAsync(new SetNodeCollectionHost(context.SiteId, new NodeCollectionHost { Id = IdGenerator.GenerateId(), Hostname = hostname, RootPath = "/nl", Cultures = new List<CultureInfo> { _dutchCulture } }));
        }

        private async Task CreatePagesAsync(IServiceProvider serviceProvider, SeederContext context)
        {
            var homepageId = await this.CreateHomepageAsync(serviceProvider, context);

            // Rooms & Suites 
            await this.CreateRoomsPagesAsync(serviceProvider, context, homepageId);

            // The Hotel
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<ContentPage>());
            this.SetProperty(content, nameof(ContentPage.NavigationTitle), "The Hotel", "Het Hotel");
            await this.CreatePageAsync(serviceProvider, context, homepageId, content, "The Hotel", "Het Hotel");

            // Facilities
            await this.CreateFacilitiesPagesAsync(serviceProvider, context, homepageId);

            // Booking
            content = new LocalizedContent(ContentTypeDefinition.Lookup<BookingPage>());
            this.SetProperty(content, nameof(ContentPage.NavigationTitle), "Bookings", "Reserveringen");
            this.SetProperty(content, nameof(ContentPage.HideInNavigation), true);
            await this.CreatePageAsync(serviceProvider, context, homepageId, content, "Bookings", "Reserveringen");

            // Blog
            await this.CreateBlogPagesAsync(serviceProvider, context, homepageId);

            // Contact
            content = new LocalizedContent(ContentTypeDefinition.Lookup<ContactPage>());
            await this.CreatePageAsync(serviceProvider, context, homepageId, content, "Contact", "Contact");

            // Homepage content
            await this.AddHomepageContentAsync(serviceProvider, context, homepageId);
        }

        private async Task<string> CreateHomepageAsync(IServiceProvider serviceProvider, SeederContext context)
        {
            var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();

            // Events
            var nodeId = IdGenerator.GenerateId();
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.CreateRootNode(nodeId, ContentTypeDefinition.Lookup<Homepage>(), "Homepage", context.SiteId, 0));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.SetNodeView(nodeId, "Homepage"));

            return nodeId;
        }

        private async Task AddHomepageContentAsync(IServiceProvider serviceProvider, SeederContext context, string homepageId)
        {
            var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();

            var contentType = ContentTypeDefinition.Lookup<Homepage>();
            var content = new LocalizedContent(contentType);
            this.SetProperty(content, nameof(Homepage.PageTitle), "Welcome", "Welkom");
            this.SetProperty(content, nameof(Homepage.NavigationTitle), "Home", "Home");

            // Header slider items
            var sliderContentType = ContentTypeDefinition.Lookup<SliderItem>();

            var sliderContent1 = new LocalizedContent(sliderContentType);
            var sliderImageId = await CreateImageAsync(serviceProvider, context, Path.Combine(Directory.GetCurrentDirectory(), @"..\seed-assets\hotel\slider-item-1.jpg"), "Slider Image 1");
            this.SetProperty(sliderContent1, nameof(SliderItem.BackgroundImage), new NestedContentItem(sliderImageId));
            this.SetProperty(sliderContent1, nameof(SliderItem.Caption), "Welcome to The Redakt Hotel and Resort", "Welkom bij het Redakt Hotel en Resort");
            this.SetProperty(sliderContent1, nameof(SliderItem.Title), "Experience the Luxury", "Ervaar de Luxe");
            this.SetProperty(sliderContent1, nameof(SliderItem.Subtitle), "in our Hotel", "in ons Hotel");
            content.AddValue(nameof(Homepage.SliderItems), CultureInfo.InvariantCulture, new NestedContentItem(sliderContent1));

            var sliderContent2 = new LocalizedContent(sliderContentType);
            this.SetProperty(sliderContent2, nameof(SliderItem.BackgroundImage), new NestedContentItem(sliderImageId));
            this.SetProperty(sliderContent2, nameof(SliderItem.Caption), "Welcome to The Redakt Hotel and Resort", "Welkom bij het Redakt Hotel en Resort");
            this.SetProperty(sliderContent2, nameof(SliderItem.Title), "Experience Exceptional Dining", "Ervaar Uitzonderlijk Dineren");
            this.SetProperty(sliderContent2, nameof(SliderItem.Subtitle), "in our Restaurants", "in onze Restaurants");
            content.AddValue(nameof(Homepage.SliderItems), CultureInfo.InvariantCulture, new NestedContentItem(sliderContent2));

            var sliderContent3 = new LocalizedContent(sliderContentType);
            this.SetProperty(sliderContent3, nameof(SliderItem.BackgroundImage), new NestedContentItem(sliderImageId));
            this.SetProperty(sliderContent3, nameof(SliderItem.Caption), "Welcome to The Redakt Hotel and Resort", "Welkom bij het Redakt Hotel en Resort");
            this.SetProperty(sliderContent3, nameof(SliderItem.Title), "Experience Ultimate Relaxation", "Ervaar Ultieme Ontspanning");
            this.SetProperty(sliderContent3, nameof(SliderItem.Subtitle), "in our Spa & Wellness", "in onze Spa & Wellness");
            content.AddValue(nameof(Homepage.SliderItems), CultureInfo.InvariantCulture, new NestedContentItem(sliderContent3));

            // Room carousel
            content.AddValue(nameof(Homepage.Modules), CultureInfo.InvariantCulture, new NestedContentItem(this.CreateRoomCarouselModule(serviceProvider, context)));

            // Text content
            var textContent = new LocalizedContent(ContentTypeDefinition.Lookup<TextWithImage>());
            var imageId = await CreateImageAsync(serviceProvider, context, Path.Combine(Directory.GetCurrentDirectory(), @"..\seed-assets\home\home-text-image.png"), "Homepage Image");
            textContent.SetValue(nameof(TextWithImage.Image), CultureInfo.InvariantCulture, new NestedContentItem(imageId));
            textContent.SetValue(nameof(TextWithImage.HeadingCaption), _englishCulture, "Welcome to The Redakt");
            textContent.SetValue(nameof(TextWithImage.Heading), _englishCulture, "The Perfect Escape");
            textContent.SetValue(nameof(TextWithImage.BodyText), _englishCulture, Lorem.Paragraph(8, 16, 5, 10));
            textContent.SetValue(nameof(TextWithImage.HeadingCaption), _dutchCulture, "Welkom bij The Redakt");
            textContent.SetValue(nameof(TextWithImage.Heading), _dutchCulture, "De Perfecte Ontsnapping");
            textContent.SetValue(nameof(TextWithImage.BodyText), _dutchCulture, Lorem.Paragraph(8, 16, 5, 10));
            content.AddValue(nameof(Homepage.Modules), CultureInfo.InvariantCulture, new NestedContentItem(textContent));

            // Facilities grid
            var facilitiesContent = this.CreateFacilitiesModule(serviceProvider, context);
            facilitiesContent.SetValue(nameof(FacilitiesGrid.HeadingCaption), _englishCulture, "Our Facilities");
            facilitiesContent.SetValue(nameof(FacilitiesGrid.Heading), _englishCulture, "Explore The Redakt");
            facilitiesContent.SetValue(nameof(FacilitiesGrid.IntroText), _englishCulture, Lorem.Paragraph(5, 10, 4, 8));
            facilitiesContent.SetValue(nameof(FacilitiesGrid.HeadingCaption), _dutchCulture, "Onze Faciliteiten");
            facilitiesContent.SetValue(nameof(FacilitiesGrid.Heading), _dutchCulture, "Ontdek The Redakt");
            facilitiesContent.SetValue(nameof(FacilitiesGrid.IntroText), _dutchCulture, Lorem.Paragraph(5, 10, 4, 8));
            content.AddValue(nameof(Homepage.Modules), CultureInfo.InvariantCulture, new NestedContentItem(facilitiesContent));

            // Offers mosaic
            var offersContent = await this.CreateOffersModuleAsync(serviceProvider, context);
            offersContent.SetValue(nameof(OffersMosaic.HeadingCaption), _englishCulture, "Our Offers");
            offersContent.SetValue(nameof(OffersMosaic.Heading), _englishCulture, "The Redakt Special Offers");
            offersContent.SetValue(nameof(OffersMosaic.IntroText), _englishCulture, Lorem.Paragraph(5, 10, 4, 8));
            offersContent.SetValue(nameof(OffersMosaic.HeadingCaption), _dutchCulture, "Onze Aanbiedingen");
            offersContent.SetValue(nameof(OffersMosaic.Heading), _dutchCulture, "The Redakt Speciale Aanbiedingen");
            offersContent.SetValue(nameof(OffersMosaic.IntroText), _dutchCulture, Lorem.Paragraph(5, 10, 4, 8));
            content.AddValue(nameof(Homepage.Modules), CultureInfo.InvariantCulture, new NestedContentItem(offersContent));

            // Latest Blog Articles
            var blogContent = new LocalizedContent(ContentTypeDefinition.Lookup<LatestBlogArticles>());
            blogContent.SetValue(nameof(LatestBlogArticles.Parent), CultureInfo.InvariantCulture, new Link { PageId = context.BlogParentId });
            blogContent.SetValue(nameof(LatestBlogArticles.HeadingCaption), _englishCulture, "The Redakt Blog");
            blogContent.SetValue(nameof(LatestBlogArticles.Heading), _englishCulture, "Latest News");
            blogContent.SetValue(nameof(LatestBlogArticles.IntroText), _englishCulture, Lorem.Paragraph(5, 10, 4, 8));
            blogContent.SetValue(nameof(LatestBlogArticles.HeadingCaption), _dutchCulture, "Het Blog");
            blogContent.SetValue(nameof(LatestBlogArticles.Heading), _dutchCulture, "Laatste Nieuws");
            blogContent.SetValue(nameof(LatestBlogArticles.IntroText), _dutchCulture, Lorem.Paragraph(5, 10, 4, 8));
            content.AddValue(nameof(Homepage.Modules), CultureInfo.InvariantCulture, new NestedContentItem(blogContent));

            var contentId = IdGenerator.GenerateId();
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Content.Commands.CreateContent(contentId, contentType));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Content.Commands.AddContentRevision(contentId, IdGenerator.GenerateId(), content.Properties));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.AddNodeVersion(homepageId, IdGenerator.GenerateId(), "Version 1", contentId, NodeVersionStateKey.New));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(homepageId, _englishCulture, contentId, null));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(homepageId, _dutchCulture, contentId, null));
        }

        private async Task CreateFacilitiesPagesAsync(IServiceProvider serviceProvider, SeederContext context, string parentId)
        {
            // Create Facilities list
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<SimplePage>());
            this.SetProperty(content, nameof(ContentPage.NavigationTitle), "Facilities", "Faciliteiten");

            var facilitiesPage = await this.CreatePageAsync(serviceProvider, context, parentId, content, "Facilities", "Faciliteiten", "FacilitiesList");

            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "buffet-restaurants", "Buffet Restaurants", "Buffetten");
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "private-dining", "A-la-carte & Private Dining", "A-la-carte & Prive Dineren");
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "skybar-lounge", "Bars & Lounges", "Bars & Lounges");
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "spa-wellness", "Spa & Beauty Center", "Spa & Wellness");
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "pools-waterpark", "Pools & Waterpark", "Zwembaden & Waterpark");
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "games-recreation", "Games & Recreation", "Spel & Ontspanning");
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "sport-fitness", "Sport & Fitness", "Sport & Fitness");
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "conference-events", "Conference & Events", "Conferentie & Evenementen");
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "wedding", "Wedding Venue", "Trouwlocatie");
        }

        private async Task CreateFacilityPageAsync(IServiceProvider serviceProvider, SeederContext context, string facilitiesParentId, string imageFolder, string englishName, string dutchName)
        {
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<FacilityPage>());
            var images = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), @$"..\seed-assets\facilities\{imageFolder}"));

            var imageAssets = new List<string>();
            for (var i = 0; i < images.Length; i++)
            {
                var imageId = await CreateImageAsync(serviceProvider, context, images[i], $"{englishName} Image {i + 1}");
                content.AddValue(nameof(FacilityPage.Images), CultureInfo.InvariantCulture, new NestedContentItem(imageId));
                imageAssets.Add(imageId);
            }

            this.SetProperty(content, nameof(FacilityPage.ListDescription), Lorem.Sentence(8, 14), Lorem.Sentence(8, 14));

            var facilityPageId = await this.CreatePageAsync(serviceProvider, context, facilitiesParentId, content, englishName, dutchName);

            context.Facilities.Add(facilityPageId);
        }

        private async Task CreateBlogPagesAsync(IServiceProvider serviceProvider, SeederContext context, string parentId)
        {
            // Create Blog overview
            var blogContent = new LocalizedContent(ContentTypeDefinition.Lookup<SimplePage>());
            this.SetProperty(blogContent, nameof(ContentPage.NavigationTitle), "Blog", "Blog");

            context.BlogParentId = await this.CreatePageAsync(serviceProvider, context, parentId, blogContent, "Blog", "Blog", "BlogOverview");

            var articleDate = DateTime.Today.AddDays(-_rnd.Next(0, 7));
            var imageFiles = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), @$"..\seed-assets\blog"));
            foreach (var imageFile in imageFiles)
            {
                var imageId = await CreateImageAsync(serviceProvider, context, imageFile, $"Blog Article Image {articleDate.ToShortDateString()}");
                var categories = Path.GetFileNameWithoutExtension(imageFile).Split('-').Skip(1);

                var content = new LocalizedContent(ContentTypeDefinition.Lookup<BlogArticle>());
                this.SetProperty(content, nameof(BlogArticle.Image), new NestedContentItem(imageId));
                this.SetProperty(content, nameof(BlogArticle.Categories), categories);
                this.SetProperty(content, nameof(BlogArticle.PublicationDate), articleDate);
                this.SetProperty(content, nameof(BlogArticle.Author), Lorem.Words(2, 3));
                this.SetProperty(content, nameof(BlogArticle.ListDescription), Lorem.Sentence(8, 14), Lorem.Sentence(8, 14));
                this.SetProperty(content, nameof(BlogArticle.Body), "<p>" + string.Join("</p><p>", Lorem.Paragraphs(6, 16, 5, 10, 3)) + "</p>", "<p>" + string.Join("</p><p>", Lorem.Paragraphs(6, 16, 5, 10, 3)) + "</p>");

                await this.CreatePageAsync(serviceProvider, context, context.BlogParentId, content, Lorem.Words(4, 8), Lorem.Words(4, 8));
                
                articleDate = articleDate.AddDays(-_rnd.Next(0, 7));
            }
        }

        private async Task CreateRoomsPagesAsync(IServiceProvider serviceProvider, SeederContext context, string parentId)
        {
            // Create Rooms list
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<SimplePage>());
            this.SetProperty(content, nameof(ContentPage.NavigationTitle), "Rooms", "Kamers");

            var roomsPageId = await this.CreatePageAsync(serviceProvider, context, parentId, content, "Rooms", "Kamers", "RoomList");

            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "twin-sea-view", "Twin Sea View", "Tweepersoonskamer Zeezicht");
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "twin-private-pool", "Twin Private Pool", "Tweepersoonskamer met Privezwembad");
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "bungalow-garden-view", "Bungalow Garden View", "Bungalow Tuinzicht");
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "bungalow-sea-view", "Bungalow Sea View", "Bungalow Zeezicht");
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "bungalow-sharing-pool", "Bungalow Sharing Pool", "Bungalow met gedeeld zwembad");
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "bungalow-family-garden-view", "Bungalow Family Garden View", "Familiebungalow Tuinzicht");
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "bungalow-family-side-sea-view", "Bungalow Family Side Sea View", "Familiebungalow Zeezicht");
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "maisonette-garden-view", "Maisonette Garden View", "Maisonette Tuinzicht");
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "suite-sea-view", "Suite Sea View", "Suite Zeezicht");
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "villa-sea-view-private-pool", "Villa Sea View Private Pool", "Villa Zeezicht met Privezwembad");
        }

        private async Task CreateRoomPageAsync(IServiceProvider serviceProvider, SeederContext context, string roomsPageId, string imageFolder, string englishName, string dutchName)
        {
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<RoomDetail>());
            var images = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), @$"..\seed-assets\rooms\{imageFolder}"));

            var mainImageId = await CreateImageAsync(serviceProvider, context, images.First(), $"{englishName} Image 1");
            this.SetProperty(content, nameof(RoomDetail.MainImage), new NestedContentItem(mainImageId));

            for (var i = 1; i < images.Length; i++)
            {
                var additionalImageId = await CreateImageAsync(serviceProvider, context, images[i], $"{englishName} Image {i + 1}");
                content.AddValue(nameof(RoomDetail.AdditionalImages), CultureInfo.InvariantCulture, new NestedContentItem(additionalImageId));
            }

            this.SetProperty(content, nameof(RoomDetail.NightlyRate), 300);
            this.SetProperty(content, nameof(RoomDetail.DiscountedFrom), 360);
            this.SetProperty(content, nameof(RoomDetail.ListDescription), Lorem.Sentence(8, 14), Lorem.Sentence(8, 14));
            this.SetProperty(content, nameof(RoomDetail.LongDescription), "<p>" + string.Join("</p><p>", Lorem.Paragraphs(6, 16, 5, 10, 3)) + "</p>", "<p>" + string.Join("</p><p>", Lorem.Paragraphs(6, 16, 5, 10, 3)) + "</p>");

            var roomPage = await this.CreatePageAsync(serviceProvider, context, roomsPageId, content, englishName, dutchName);

            context.Rooms.Add(roomPage);
        }

        private async Task<string> CreatePageAsync(IServiceProvider serviceProvider, SeederContext context, string parentId, ILocalizedContent content, string englishName, string dutchName, string viewName = null)
        {
            var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
            var contentType = content.ContentType();

            this.SetProperty(content, nameof(PageBase.PageTitle), englishName, dutchName);
            if (typeof(SimplePage).IsAssignableFrom(contentType.ViewModelType))
            {
                this.SetProperty(content, nameof(ContentPage.HeadingIntro), Lorem.Paragraph(8, 16, 3, 5), Lorem.Paragraph(8, 16, 3, 5));
                this.SetProperty(content, nameof(ContentPage.HeadingCaption), Lorem.Words(3, 6), Lorem.Words(3, 6));
            }

            // Events
            var nodeId = IdGenerator.GenerateId();
            var contentId = IdGenerator.GenerateId();
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Content.Commands.CreateContent(contentId, contentType));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Content.Commands.AddContentRevision(contentId, IdGenerator.GenerateId(), content.Properties));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.CreateNode(nodeId, contentType, englishName, parentId, 0));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.SetNodeView(nodeId, viewName ?? contentType.AllowedViewNames.First()));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.AddNodeVersion(nodeId, IdGenerator.GenerateId(), "Version 1", contentId, NodeVersionStateKey.New));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _englishCulture, contentId, englishName.UrlFriendly()));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _dutchCulture, contentId, dutchName.UrlFriendly()));

            return nodeId;
        }

        // Modules
        private LocalizedContent CreateRoomCarouselModule(IServiceProvider serviceProvider, SeederContext context)
        {
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<RoomCarousel>());

            foreach (var roomPageId in context.Rooms.Take(6))
            {
                content.AddValue(nameof(RoomCarousel.Rooms), CultureInfo.InvariantCulture, new Link { PageId = roomPageId });
            }

            return content;
        }

        // Modules
        private LocalizedContent CreateFacilitiesModule(IServiceProvider serviceProvider, SeederContext context)
        {
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<FacilitiesGrid>());

            foreach (var facilitiesPageId in context.Facilities.Take(9))
            {
                content.AddValue(nameof(FacilitiesGrid.Facilities), CultureInfo.InvariantCulture, new Link { PageId = facilitiesPageId });
            }

            return content;
        }

        private async Task<LocalizedContent> CreateOffersModuleAsync(IServiceProvider serviceProvider, SeederContext context)
        {
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<OffersMosaic>());

            foreach (var imageFile in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), @$"..\seed-assets\offers")))
            {
                var imageId = await CreateImageAsync(serviceProvider, context, imageFile, "Offer");
                var offer = new LocalizedContent(ContentTypeDefinition.Lookup<OfferItem>());

                this.SetProperty(offer, nameof(OfferItem.Image), new NestedContentItem(imageId));
                this.SetProperty(offer, nameof(OfferItem.Title), Lorem.Words(3, 6), Lorem.Words(3, 6));
                this.SetProperty(offer, nameof(OfferItem.Text), Lorem.Paragraph(5, 8, 2, 4), Lorem.Paragraph(5, 8, 2, 4));

                content.AddValue(nameof(OffersMosaic.Offers), CultureInfo.InvariantCulture, new NestedContentItem(offer));
            }

            return content;
        }

        private LocalizedContent CreateTextWithImageModule(CultureInfo culture, string imageId = null, string heading = null, string headingCaption = null)
        {
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<TextWithImage>());

            if (headingCaption != null) content.SetValue(nameof(TextWithImage.HeadingCaption), culture, headingCaption);
            if (heading != null) content.SetValue(nameof(TextWithImage.Heading), culture, heading);
            content.SetValue(nameof(TextWithImage.BodyText), culture, Lorem.Paragraph(8, 16, 5, 10));
            if (imageId != null) content.SetValue(nameof(TextWithImage.Image), culture, new NestedContentItem(imageId));

            return content;
        }

        private LocalizedContent CreateImageGalleryModule(IEnumerable<string> imageIds, CultureInfo culture, string heading = null, string headingCaption = null)
        {
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<ImageGallery>());

            if (headingCaption != null) content.SetValue(nameof(ImageGallery.HeadingCaption), culture, headingCaption);
            if (!string.IsNullOrWhiteSpace(heading))
            {
                content.SetValue(nameof(ImageGallery.Heading), culture, heading);
                content.SetValue(nameof(ImageGallery.IntroText), culture, Lorem.Paragraph(8, 16, 3, 5));
            }
            content.SetValues(nameof(ImageGallery.Images), CultureInfo.InvariantCulture, imageIds.Select(x => new NestedContentItem(x)));

            return content;
        }

        private void SetProperty(ILocalizedContent content, string propertyKey, object englishValue, object dutchValue)
        {
            content.SetValue(propertyKey, _englishCulture, englishValue);
            content.SetValue(propertyKey, _dutchCulture, dutchValue);
        }

        private void SetProperty(ILocalizedContent content, string propertyKey, object invariantValue)
        {
            content.SetValue(propertyKey, CultureInfo.InvariantCulture, invariantValue);
        }

        private async Task CreateStaffMembersAsync(IServiceProvider serviceProvider, SeederContext context)
        {
            await this.CreateStaffMemberAsync(serviceProvider, context, 8, "Edward Sanderson", "General Manager", "Algemeen Directeur");
            await this.CreateStaffMemberAsync(serviceProvider, context, 2, "Sam McLean", "Hotel Manager", "Directeur Hotel");
            await this.CreateStaffMemberAsync(serviceProvider, context, 10, "Danielle Mills", "Spa Manager", "Manager Spa");
            await this.CreateStaffMemberAsync(serviceProvider, context, 7, "Bao Tan", "Restaurant Manager", "Manager Restaurant");
            await this.CreateStaffMemberAsync(serviceProvider, context, 1, "Sarah Kerr", "Director of Housekeeping", "Housekeeping");
            await this.CreateStaffMemberAsync(serviceProvider, context, 4, "Annie Gould", "Front Desk Supervisor", "Front Desk Supervisor");
            await this.CreateStaffMemberAsync(serviceProvider, context, 3, "John Robbins", "Head Concierge", "Hoofd Concierge");
            await this.CreateStaffMemberAsync(serviceProvider, context, 11, "Declan Harris", "Executive Chef", "Chef-kok");
            await this.CreateStaffMemberAsync(serviceProvider, context, 12, "Louise Robertson", "Event Manager", "Event Manager");
        }

        private async Task CreateStaffMemberAsync(IServiceProvider serviceProvider, SeederContext context, int imageNumber, string name, string roleEnglish, string roleDutch)
        {
            var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();

            var fileId = IdGenerator.GenerateId();
            await using (var fileStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), @$"..\seed-assets\staff\staff-{imageNumber}.jpg")))
            {
                await dispatcher.ExecuteAsync(new CreateFile(fileId, fileStream, $"staff-{imageNumber}.jpg", "image/jpeg"));
            }

            var contentType = ContentTypeDefinition.Lookup<StaffMember>();
            var content = new LocalizedContent(contentType);

            this.SetProperty(content, nameof(StaffMember.Picture), new MediaFile(fileId));
            this.SetProperty(content, nameof(StaffMember.Name), name);
            this.SetProperty(content, nameof(StaffMember.Role), roleEnglish, roleDutch);

            // Events
            var nodeId = IdGenerator.GenerateId();
            var versionId = IdGenerator.GenerateId();
            var contentId = IdGenerator.GenerateId();
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Content.Commands.CreateContent(contentId, contentType));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Content.Commands.AddContentRevision(contentId, IdGenerator.GenerateId(), content.Properties));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.CreateNode(nodeId, contentType, name, context.StaffMembersFolderId, 0));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.AddNodeVersion(nodeId, versionId, "Version 1", contentId, NodeVersionStateKey.New));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _englishCulture, contentId, null));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _dutchCulture, contentId, null));
        }

        private async Task<string> CreateImageAsync(IServiceProvider serviceProvider, SeederContext context, string imageFile, string nodeName)
        {
            var fileService = serviceProvider.GetRequiredService<IFileService>();
            var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();

            var fileId = IdGenerator.GenerateId();
            await using (var fileStream = File.OpenRead(imageFile))
            {
                await dispatcher.ExecuteAsync(new CreateFile(fileId, fileStream, Path.GetFileName(imageFile), "image/jpeg"));
            }

            var contentType = ContentTypeDefinition.Lookup<Image>();
            var content = new LocalizedContent(contentType);

            this.SetProperty(content, nameof(Image.File), new MediaFile(fileId));

            // Events
            var nodeId = IdGenerator.GenerateId();
            var contentId = IdGenerator.GenerateId();
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Content.Commands.CreateContent(contentId, contentType));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Content.Commands.AddContentRevision(contentId, IdGenerator.GenerateId(), content.Properties));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.CreateNode(nodeId, contentType, nodeName, context.ImagesFolderId, 0));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.AddNodeVersion(nodeId, IdGenerator.GenerateId(), "Version 1", contentId, NodeVersionStateKey.New));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _englishCulture, contentId, null));
            await dispatcher.ExecuteAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _dutchCulture, contentId, null));

            context.AllImages.Add(nodeId);

            return nodeId;
        }
    }
}
