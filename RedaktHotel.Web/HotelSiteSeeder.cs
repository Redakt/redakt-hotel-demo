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
using Redakt.ContentManagement.NestedContent;
using RedaktHotel.Web.Models.Assets;
using Microsoft.AspNetCore.Http;
using RedaktHotel.Web.Models.Pages;
using RedaktHotel.Web.Models.Embedded;
using LoremNET;
using Redakt.ContentManagement.Files;
using Redakt.ContentManagement.NodeCollections;
using Redakt.ContentManagement.NodeCollections.Aggregates;
using Redakt.ContentManagement.NodeCollections.Commands;
using Redakt.ContentManagement.Nodes.Aggregates;
using Redakt.Dictionary.Aggregates;
using Redakt.Dictionary.Commands;
using Redakt.EventSourcing;
using Redakt.Files.Aggregates;
using Redakt.Files.Commands;
using Redakt.Extensions;
using Redakt.Files;
using Microsoft.Extensions.Logging;

namespace RedaktHotel.Web
{
    /// <summary>
    /// The site seeder is called on startup to generate random content for the demo site. In most scenarios you do not need to create content programmatically, in which case you can ignore this whole class, however this class provides an example in case you do.
    /// </summary>
    public class HotelSiteSeeder : IStartupFilter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly CultureInfo _englishCulture = CultureInfo.GetCultureInfo("en");
        private readonly CultureInfo _dutchCulture = CultureInfo.GetCultureInfo("nl");
        private readonly Random _rnd = new Random();

        public HotelSiteSeeder(IServiceProvider serviceProvider, ILogger<HotelSiteSeeder> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            this.ProvisionSiteAsync().GetAwaiter().GetResult();

            return next;
        }

        private class SeederContext
        {
            public NodeCollectionId SiteId { get; set; }

            public NodeCollectionId AssetLibraryId { get; set; }

            public NodeId StaffMembersFolderId { get; set; }

            public NodeId ImagesFolderId { get; set; }

            public List<string> AllImages { get; } = new List<string>();

            public NodeId BlogParentId { get; set; }

            public List<string> Rooms { get; } = new List<string>();

            public List<string> Facilities { get; } = new List<string>();

            public DictionaryCategoryId LabelsCategoryId { get; set; }
        }

        private async Task ProvisionSiteAsync()
        {
            _logger.LogInformation("First-time startup initialization is seeding the demo site. Please wait a few moments...");

            using var scope = _serviceProvider.CreateScope();

            var languageService = scope.ServiceProvider.GetRequiredService<ILanguageService>();

            // If languages exist we assume that the site was already created before
            if (await languageService.GetAllLanguagesAsync().AnyAsync()) return;

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
            var commandBus = serviceProvider.GetRequiredService<ICommandBus>();

            // Create labels dictionary category
            context.LabelsCategoryId = DictionaryCategoryId.New;
            await commandBus.PublishAsync(new CreateDictionaryCategory(context.LabelsCategoryId, "Labels"));

            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "TheRedaktHotelAndResort", "The Redakt Hotel and Resort", "Het Redakt Hotel en Resort");
            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "LearnMore", "Learn More", "Meer weten");
            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "ReadMore", "Read More", "Lees verder");

            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "news", "News", "Nieuws");
            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "hotel", "Hotel", "Hotel");
            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "dining", "Dining", "Dineren");
            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "facilities", "Facilities", "Faciliteiten");
            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "offers", "Offers", "Aanbiedingen");
            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "events", "Events", "Evenementen");
            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "excursions", "Excursions", "Excursies");
            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "spa", "Spa & wellness", "Spa & wellness");

            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "search", "SEARCH", "ZOEKEN");
            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "tags", "TAGS", "TAGS");
            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "categories", "CATEGORIES", "CATEGORIEN");
            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "upcomingEvents", "UPCOMING EVENTS", "KOMENDE EVENEMENTEN");
            await this.CreateDictionaryEntryAsync(commandBus, context.LabelsCategoryId, "recentPosts", "RECENT POSTS", "RECENTE ARTIKELEN");

            // Create booking dictionary category
            var bookingCategoryId = DictionaryCategoryId.New;
            await commandBus.PublishAsync(new CreateDictionaryCategory(bookingCategoryId, "Booking"));

            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "Booking", "Booking", "Reservering");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "BookThisRoom", "Book this room", "Reserveer kamer");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "CheckInDate", "CHECK IN", "INCHECKDATUM");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "CheckOutDate", "CHECK OUT", "UITCHECKDATUM");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "Adults", "ADULTS", "VOLWASSENEN");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "Adult", "ADULT", "VOLWASSENE");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "Children", "CHILDREN", "KINDEREN");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "Child", "CHILD", "KIND");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "BookYourStay", "Book your stay with us", "Reserveer uw verblijf bij ons");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "CheckAvailability", "Check availability", "Bekijk beschikbaarheid");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "BookNow", "Book now", "Reserveer nu");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "FirstName", "GIVEN NAME", "VOORNAAM");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "LastName", "FAMILY NAME", "ACHTERNAAM");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "EmailAddress", "EMAIL ADDRESS", "E-MAILADRES");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "PhoneNumber", "PHONE NUMBER", "TELEFOONNUMMER");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "AddressLine1", "ADDRESS LINE 1", "ADRESREGEL 1");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "AddressLine2", "ADDRESS LINE 2", "ADRESREGEL 2");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "City", "CITY", "PLAATS");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "Country", "COUNTRY", "LAND");
            await this.CreateDictionaryEntryAsync(commandBus, bookingCategoryId, "SpecialRequirements", "SPECIAL REQUIREMENTS", "BIJZONDERE VEREISTEN");
        }

        private async Task CreateDictionaryEntryAsync(ICommandBus commandBus, string categoryId, string key, string englishLabel, string dutchLabel)
        {
            var itemId = DictionaryItemId.New;
            await commandBus.PublishAsync(new CreateDictionaryItem(itemId, categoryId, key));

            var values = new Dictionary<CultureInfo, string>
            {
                [_englishCulture] = englishLabel,
                [_dutchCulture] = dutchLabel
            };

            await commandBus.PublishAsync(new SetDictionaryItemValues(itemId, values));
        }

        private async Task CreateLibrariesAsync(IServiceProvider serviceProvider, SeederContext context)
        {
            var folderDefinition = ContentTypeDefinition.Lookup<MediaFolder>();
            var commandBus = serviceProvider.GetRequiredService<ICommandBus>();

            // Create media library
            context.AssetLibraryId = NodeCollectionId.New;
            await commandBus.PublishAsync(new CreateNodeCollection(context.AssetLibraryId, NodeCollectionTypes.AssetLibrary, "Media", 0));
            await commandBus.PublishAsync(new SetNodeCollectionMapping(context.AssetLibraryId, new NodeCollectionMappingAggregate { Id = NodeCollectionMappingId.New, Culture = _englishCulture }));
            await commandBus.PublishAsync(new SetNodeCollectionMapping(context.AssetLibraryId, new NodeCollectionMappingAggregate { Id = NodeCollectionMappingId.New, Culture = _dutchCulture }));
            //await commandBus.PublishAsync(new AddNodeCollectionMapping(context.AssetLibraryId, IdGenerator.GenerateId(), CultureInfo.GetCultureInfo("en-US"), _englishCulture));

            // Create staff library
            var staffLibraryId = NodeCollectionId.New;
            await commandBus.PublishAsync(new CreateNodeCollection(staffLibraryId, NodeCollectionTypes.AssetLibrary, "Staff", 0));
            await commandBus.PublishAsync(new SetNodeCollectionMapping(staffLibraryId, new NodeCollectionMappingAggregate { Id = NodeCollectionMappingId.New, Culture = _englishCulture }));
            await commandBus.PublishAsync(new SetNodeCollectionMapping(staffLibraryId, new NodeCollectionMappingAggregate { Id = NodeCollectionMappingId.New, Culture = _dutchCulture }));

            context.ImagesFolderId = NodeId.New;
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.CreateRootNode(context.ImagesFolderId, folderDefinition, "Images", context.AssetLibraryId, 0));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.CreateRootNode(NodeId.New, folderDefinition, "Videos", context.AssetLibraryId, 1));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.CreateRootNode(NodeId.New, folderDefinition, "Documents", context.AssetLibraryId, 2));

            context.StaffMembersFolderId = NodeId.New;
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.CreateRootNode(context.StaffMembersFolderId, ContentTypeDefinition.Lookup<StaffFolder>(), "Staff Members", staffLibraryId, 0));
        }

        private async Task CreateSiteAsync(IServiceProvider serviceProvider, SeederContext context, string hostname)
        {
            var commandBus = serviceProvider.GetRequiredService<ICommandBus>();

            context.SiteId = NodeCollectionId.New;
            await commandBus.PublishAsync(new CreateNodeCollection(context.SiteId, NodeCollectionTypes.Site, "The Redakt Hotel", 0));
            await commandBus.PublishAsync(new SetNodeCollectionMapping(context.SiteId, new NodeCollectionMappingAggregate { Id = NodeCollectionMappingId.New, Culture = _englishCulture }));
            await commandBus.PublishAsync(new SetNodeCollectionMapping(context.SiteId, new NodeCollectionMappingAggregate { Id = NodeCollectionMappingId.New, Culture = _dutchCulture }));

            await commandBus.PublishAsync(new SetNodeCollectionHost(context.SiteId, new NodeCollectionHostAggregate { Id = NodeCollectionHostId.New, Hostname = hostname, RootPath = "/en", Cultures = new List<CultureInfo> { _englishCulture } }));
            await commandBus.PublishAsync(new SetNodeCollectionHost(context.SiteId, new NodeCollectionHostAggregate { Id = NodeCollectionHostId.New, Hostname = hostname, RootPath = "/nl", Cultures = new List<CultureInfo> { _dutchCulture } }));
        }

        private async Task CreatePagesAsync(IServiceProvider serviceProvider, SeederContext context)
        {
            var homepageId = await this.CreateHomepageAsync(serviceProvider, context);

            // Rooms & Suites 
            await this.CreateRoomsPagesAsync(serviceProvider, context, homepageId);

            // The Hotel
            var content = EmbeddedContent.New(ContentTypeDefinition.Lookup<ContentPage>());
            this.AddContentValue(content, nameof(ContentPage.NavigationTitle), "The Hotel", "Het Hotel");
            await this.CreatePageAsync(serviceProvider, context, homepageId, content, "The Hotel", "Het Hotel", 1);

            // Facilities
            await this.CreateFacilitiesPagesAsync(serviceProvider, context, homepageId);

            // Booking
            content = EmbeddedContent.New(ContentTypeDefinition.Lookup<BookingPage>());
            this.AddContentValue(content, nameof(ContentPage.NavigationTitle), "Bookings", "Reserveringen");
            this.AddContentValue(content, nameof(ContentPage.HideInNavigation), true);
            await this.CreatePageAsync(serviceProvider, context, homepageId, content, "Bookings", "Reserveringen", 3);

            // Blog
            await this.CreateBlogPagesAsync(serviceProvider, context, homepageId);

            // Contact
            content = EmbeddedContent.New(ContentTypeDefinition.Lookup<ContactPage>());
            await this.CreatePageAsync(serviceProvider, context, homepageId, content, "Contact", "Contact", 5);

            // Homepage content
            await this.AddHomepageContentAsync(serviceProvider, context, homepageId);
        }

        private async Task<NodeId> CreateHomepageAsync(IServiceProvider serviceProvider, SeederContext context)
        {
            var commandBus = serviceProvider.GetRequiredService<ICommandBus>();

            // Events
            var nodeId = NodeId.New;
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.CreateRootNode(nodeId, ContentTypeDefinition.Lookup<Homepage>(), "Homepage", context.SiteId, 0));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.SetNodeView(nodeId, "Homepage"));

            return nodeId;
        }

        private async Task AddHomepageContentAsync(IServiceProvider serviceProvider, SeederContext context, NodeId homepageId)
        {
            var commandBus = serviceProvider.GetRequiredService<ICommandBus>();
            var contentService = serviceProvider.GetRequiredService<IContentService>();

            var contentType = ContentTypeDefinition.Lookup<Homepage>();
            var content = EmbeddedContent.New(contentType);
            this.AddContentValue(content, nameof(Homepage.PageTitle), "Welcome", "Welkom");
            this.AddContentValue(content, nameof(Homepage.NavigationTitle), "Home", "Home");

            // Header slider items
            var sliderContentType = ContentTypeDefinition.Lookup<SliderItem>();

            var sliderContent1 = EmbeddedContent.New(sliderContentType);
            var sliderImageId = await CreateImageAsync(serviceProvider, context, Path.Combine(Directory.GetCurrentDirectory(), @"../seed-assets/hotel/slider-item-1.jpg".ToSafeFilePath()), "Slider Image 1");
            this.AddContentValue(sliderContent1, nameof(SliderItem.BackgroundImage), NodeReference.New(sliderImageId));
            this.AddContentValue(sliderContent1, nameof(SliderItem.Caption), "Welcome to The Redakt Hotel and Resort", "Welkom bij het Redakt Hotel en Resort");
            this.AddContentValue(sliderContent1, nameof(SliderItem.Title), "Experience the Luxury", "Ervaar de Luxe");
            this.AddContentValue(sliderContent1, nameof(SliderItem.Subtitle), "in our Hotel", "in ons Hotel");
            this.AddContentValue(content, nameof(Homepage.SliderItems), sliderContent1);

            var sliderContent2 = EmbeddedContent.New(sliderContentType);
            this.AddContentValue(sliderContent2, nameof(SliderItem.BackgroundImage), NodeReference.New(sliderImageId));
            this.AddContentValue(sliderContent2, nameof(SliderItem.Caption), "Welcome to The Redakt Hotel and Resort", "Welkom bij het Redakt Hotel en Resort");
            this.AddContentValue(sliderContent2, nameof(SliderItem.Title), "Experience Exceptional Dining", "Ervaar Uitzonderlijk Dineren");
            this.AddContentValue(sliderContent2, nameof(SliderItem.Subtitle), "in our Restaurants", "in onze Restaurants");
            this.AddContentValue(content, nameof(Homepage.SliderItems), sliderContent2);

            var sliderContent3 = EmbeddedContent.New(sliderContentType);
            this.AddContentValue(sliderContent3, nameof(SliderItem.BackgroundImage), NodeReference.New(sliderImageId));
            this.AddContentValue(sliderContent3, nameof(SliderItem.Caption), "Welcome to The Redakt Hotel and Resort", "Welkom bij het Redakt Hotel en Resort");
            this.AddContentValue(sliderContent3, nameof(SliderItem.Title), "Experience Ultimate Relaxation", "Ervaar Ultieme Ontspanning");
            this.AddContentValue(sliderContent3, nameof(SliderItem.Subtitle), "in our Spa & Wellness", "in onze Spa & Wellness");
            this.AddContentValue(content, nameof(Homepage.SliderItems), sliderContent3);

            // Room carousel
            this.AddContentValue(content, nameof(Homepage.Modules), this.CreateRoomCarouselModule(serviceProvider, context));

            // Text content
            var textContent = EmbeddedContent.New(ContentTypeDefinition.Lookup<TextWithImage>());
            var imageId = await CreateImageAsync(serviceProvider, context, Path.Combine(Directory.GetCurrentDirectory(), @"../seed-assets/home/home-text-image.jpg".ToSafeFilePath()), "Homepage Image");
            this.AddContentValue(textContent, nameof(TextWithImage.Image), NodeReference.New(imageId));
            this.AddContentValue(textContent, nameof(TextWithImage.HeadingCaption), "Welcome to The Redakt", "Welkom bij The Redakt");
            this.AddContentValue(textContent, nameof(TextWithImage.Heading), "Demonstration website", "Demonstratie website");
            this.AddContentValue(textContent, nameof(TextWithImage.BodyText), "The Redakt Hotel & Resort is a demo website for the <a href=\"https://www.redaktcms.com\" target=\"_blank\">Redakt Content Management System</a>. Therefore most content is fictional and automatically generated. You can visit the back office application at <a href=\"/redakt\">/redakt</a>. In the back office and application code you can play with many of the features that are part of the Redakt system. Please note, as this website is meant as a showcase for Redakt CMS only, and it does not necessarily reflect best web software development practices. Parts of the website may be functionally incomplete or missing.", "The Redakt Hotel & Resort is een demo website voor het <a href=\"https://www.redaktcms.com\" target=\"_blank\">Redakt Content Management System</a>. Om deze reden is de meeste content fictioneel en automatisch gegenereerd. Je kan de back office applicatie bezoeken op <a href=\"/redakt\">/redakt</a>. In the back office en applicatie code kan je veel van de functionaliteiten uitproberen die onderdeel maken van het Redakt systeem. Let wel, aangezien deze website alleen bedoeld is als showcase voor Redakt CMS, is het niet per se een goed voorbeeld van web software ontwikkeling. Sommige onderdelen van de website kunnen functioneel incompleet zijn.");
            this.AddContentValue(content, nameof(Homepage.Modules), textContent);

            // Facilities grid
            var facilitiesContent = this.CreateFacilitiesModule(serviceProvider, context);
            this.AddContentValue(facilitiesContent, nameof(FacilitiesGrid.HeadingCaption), "Our Facilities", "Onze Faciliteiten");
            this.AddContentValue(facilitiesContent, nameof(FacilitiesGrid.Heading), "Explore The Redakt", "Ontdek The Redakt");
            this.AddContentValue(facilitiesContent, nameof(FacilitiesGrid.IntroText), Lorem.Paragraph(5, 10, 4, 8), Lorem.Paragraph(5, 10, 4, 8));
            this.AddContentValue(content, nameof(Homepage.Modules), facilitiesContent);

            // Offers mosaic
            var offersContent = await this.CreateOffersModuleAsync(serviceProvider, context);
            this.AddContentValue(offersContent, nameof(OffersMosaic.HeadingCaption), "Our Offers", "Onze Aanbiedingen");
            this.AddContentValue(offersContent, nameof(OffersMosaic.Heading), "The Redakt Special Offers", "The Redakt Speciale Aanbiedingen");
            this.AddContentValue(offersContent, nameof(OffersMosaic.IntroText), Lorem.Paragraph(5, 10, 4, 8), Lorem.Paragraph(5, 10, 4, 8));
            this.AddContentValue(content, nameof(Homepage.Modules), offersContent);

            // Latest Blog Articles
            var blogContent = EmbeddedContent.New(ContentTypeDefinition.Lookup<LatestBlogArticles>());
            this.AddContentValue(blogContent, nameof(LatestBlogArticles.Parent), new Link { PageId = context.BlogParentId });
            this.AddContentValue(blogContent, nameof(LatestBlogArticles.HeadingCaption), "The Redakt Blog", "Het Blog");
            this.AddContentValue(blogContent, nameof(LatestBlogArticles.Heading), "Latest News", "Laatste Nieuw");
            this.AddContentValue(blogContent, nameof(LatestBlogArticles.IntroText), Lorem.Paragraph(5, 10, 4, 8), Lorem.Paragraph(5, 10, 4, 8));
            this.AddContentValue(content, nameof(Homepage.Modules), blogContent);

            var versionId = NodeVersionId.New;
            var contentId = await contentService.CreateContentAsync(contentType, content).NoSync();
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.AddNodeVersion(homepageId, versionId, contentId, "Version 1", NodeVersionStateKey.New));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(homepageId, _englishCulture, versionId, null));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(homepageId, _dutchCulture, versionId, null));
        }

        private async Task CreateFacilitiesPagesAsync(IServiceProvider serviceProvider, SeederContext context, NodeId parentId)
        {
            // Create Facilities list
            var content = EmbeddedContent.New(ContentTypeDefinition.Lookup<SimplePage>());
            this.AddContentValue(content, nameof(ContentPage.NavigationTitle), "Facilities", "Faciliteiten");

            var facilitiesPage = await this.CreatePageAsync(serviceProvider, context, parentId, content, "Facilities", "Faciliteiten", 2, "FacilitiesList");

            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "buffet-restaurants", "Buffet Restaurants", "Buffetten", 0);
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "private-dining", "A-la-carte & Private Dining", "A-la-carte & Prive Dineren", 1);
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "skybar-lounge", "Bars & Lounges", "Bars & Lounges", 2);
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "spa-wellness", "Spa & Beauty Center", "Spa & Wellness", 3);
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "pools-waterpark", "Pools & Waterpark", "Zwembaden & Waterpark", 4);
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "games-recreation", "Games & Recreation", "Spel & Ontspanning", 5);
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "sport-fitness", "Sport & Fitness", "Sport & Fitness", 6);
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "conference-events", "Conference & Events", "Conferentie & Evenementen", 7);
            await this.CreateFacilityPageAsync(serviceProvider, context, facilitiesPage, "wedding", "Wedding Venue", "Trouwlocatie", 8);
        }

        private async Task CreateFacilityPageAsync(IServiceProvider serviceProvider, SeederContext context, NodeId facilitiesParentId, string imageFolder, string englishName, string dutchName, int sortOrder)
        {
            var content = EmbeddedContent.New(ContentTypeDefinition.Lookup<FacilityPage>());
            var images = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), $"../seed-assets/facilities/{imageFolder}".ToSafeFilePath()));

            var imageAssets = new List<string>();
            for (var i = 0; i < images.Length; i++)
            {
                var imageId = await CreateImageAsync(serviceProvider, context, images[i], $"{englishName} Image {i + 1}");
                this.AddContentValue(content, nameof(FacilityPage.Images), NodeReference.New(imageId));
                imageAssets.Add(imageId);
            }

            this.AddContentValue(content, nameof(FacilityPage.ListDescription), Lorem.Sentence(8, 14), Lorem.Sentence(8, 14));

            var facilityPageId = await this.CreatePageAsync(serviceProvider, context, facilitiesParentId, content, englishName, dutchName, sortOrder);

            context.Facilities.Add(facilityPageId);
        }

        private async Task CreateBlogPagesAsync(IServiceProvider serviceProvider, SeederContext context, NodeId parentId)
        {
            // Create Blog overview
            var blogContent = EmbeddedContent.New(ContentTypeDefinition.Lookup<SimplePage>());
            this.AddContentValue(blogContent, nameof(ContentPage.NavigationTitle), "Blog", "Blog");

            context.BlogParentId = await this.CreatePageAsync(serviceProvider, context, parentId, blogContent, "Blog", "Blog", 4, "BlogOverview");

            var articleDate = DateTime.Today.AddDays(-_rnd.Next(0, 7));
            var imageFiles = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), $"../seed-assets/blog".ToSafeFilePath()));
            var sortIndex = 0;
            foreach (var imageFile in imageFiles)
            {
                var imageId = await CreateImageAsync(serviceProvider, context, imageFile, $"Blog Article Image {articleDate.ToShortDateString()}");
                var categories = Path.GetFileNameWithoutExtension(imageFile).Split('-').Skip(1);

                var content = EmbeddedContent.New(ContentTypeDefinition.Lookup<BlogArticle>());
                this.AddContentValue(content, nameof(BlogArticle.Image), NodeReference.New(imageId));
                this.AddContentValues(content, nameof(BlogArticle.Categories), categories);
                this.AddContentValue(content, nameof(BlogArticle.PublicationDate), articleDate);
                this.AddContentValue(content, nameof(BlogArticle.Author), Lorem.Words(2, 3));
                this.AddContentValue(content, nameof(BlogArticle.ListDescription), Lorem.Sentence(8, 14), Lorem.Sentence(8, 14));
                this.AddContentValue(content, nameof(BlogArticle.Body), "<p>" + string.Join("</p><p>", Lorem.Paragraphs(6, 16, 5, 10, 3)) + "</p>", "<p>" + string.Join("</p><p>", Lorem.Paragraphs(6, 16, 5, 10, 3)) + "</p>");

                await this.CreatePageAsync(serviceProvider, context, context.BlogParentId, content, Lorem.Words(4, 8), Lorem.Words(4, 8), sortIndex++);

                articleDate = articleDate.AddDays(-_rnd.Next(0, 7));
            }
        }

        private async Task CreateRoomsPagesAsync(IServiceProvider serviceProvider, SeederContext context, NodeId parentId)
        {
            // Create Rooms list
            var content = EmbeddedContent.New(ContentTypeDefinition.Lookup<SimplePage>());
            this.AddContentValue(content, nameof(ContentPage.NavigationTitle), "Rooms", "Kamers");

            var roomsPageId = await this.CreatePageAsync(serviceProvider, context, parentId, content, "Rooms", "Kamers", 0, "RoomList");

            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "twin-sea-view", "Twin Sea View", "Tweepersoonskamer Zeezicht", 180, 0);
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "twin-private-pool", "Twin Private Pool", "Tweepersoonskamer met Privezwembad", 220, 1);
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "bungalow-garden-view", "Bungalow Garden View", "Bungalow Tuinzicht", 210, 2);
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "bungalow-sea-view", "Bungalow Sea View", "Bungalow Zeezicht", 240, 3);
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "bungalow-sharing-pool", "Bungalow Sharing Pool", "Bungalow met gedeeld zwembad", 260, 4);
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "bungalow-family-garden-view", "Bungalow Family Garden View", "Familiebungalow Tuinzicht", 240, 5);
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "bungalow-family-side-sea-view", "Bungalow Family Side Sea View", "Familiebungalow Zeezicht", 280, 6);
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "maisonette-garden-view", "Maisonette Garden View", "Maisonette Tuinzicht", 250, 7);
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "suite-sea-view", "Suite Sea View", "Suite Zeezicht", 320, 8);
            await this.CreateRoomPageAsync(serviceProvider, context, roomsPageId, "villa-sea-view-private-pool", "Villa Sea View Private Pool", "Villa Zeezicht met Privezwembad", 400, 9);
        }

        private async Task CreateRoomPageAsync(IServiceProvider serviceProvider, SeederContext context, NodeId roomsPageId, string imageFolder, string englishName, string dutchName, int nightlyRate, int sortIndex)
        {
            var content = EmbeddedContent.New(ContentTypeDefinition.Lookup<RoomDetail>());
            var images = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), $"../seed-assets/rooms/{imageFolder}".ToSafeFilePath()));

            var mainImageId = await CreateImageAsync(serviceProvider, context, images.First(), $"{englishName} Image 1");
            this.AddContentValue(content, nameof(RoomDetail.MainImage), NodeReference.New(mainImageId));

            for (var i = 1; i < images.Length; i++)
            {
                var additionalImageId = await CreateImageAsync(serviceProvider, context, images[i], $"{englishName} Image {i + 1}");
                this.AddContentValue(content, nameof(RoomDetail.AdditionalImages), NodeReference.New(additionalImageId));
            }

            this.AddContentValue(content, nameof(RoomDetail.NightlyRate), nightlyRate);
            this.AddContentValue(content, nameof(RoomDetail.DiscountedFrom), new Random().Next(nightlyRate, Convert.ToInt32(nightlyRate * 1.2)));
            this.AddContentValue(content, nameof(RoomDetail.ListDescription), Lorem.Sentence(8, 14), Lorem.Sentence(8, 14));
            this.AddContentValue(content, nameof(RoomDetail.LongDescription), "<p>" + string.Join("</p><p>", Lorem.Paragraphs(6, 16, 5, 10, 3)) + "</p>", "<p>" + string.Join("</p><p>", Lorem.Paragraphs(6, 16, 5, 10, 3)) + "</p>");

            var roomPage = await this.CreatePageAsync(serviceProvider, context, roomsPageId, content, englishName, dutchName, sortIndex);

            context.Rooms.Add(roomPage);
        }

        private async Task<NodeId> CreatePageAsync(IServiceProvider serviceProvider, SeederContext context, NodeId parentId, ILocalizableContentModel content, string englishName, string dutchName, int sortOrder, string viewName = null)
        {
            var commandBus = serviceProvider.GetRequiredService<ICommandBus>();
            var contentService = serviceProvider.GetRequiredService<IContentService>();

            this.AddContentValue(content, nameof(PageBase.PageTitle), englishName, dutchName);
            if (typeof(SimplePage).IsAssignableFrom(content.ContentType.ViewModelType))
            {
                this.AddContentValue(content, nameof(ContentPage.HeadingIntro), Lorem.Paragraph(8, 16, 3, 5), Lorem.Paragraph(8, 16, 3, 5));
                this.AddContentValue(content, nameof(ContentPage.HeadingCaption), Lorem.Words(3, 6), Lorem.Words(3, 6));
            }

            // Events
            var nodeId = NodeId.New;
            var versionId = NodeVersionId.New;
            var contentId = await contentService.CreateContentAsync(content.ContentType, content).NoSync();
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.CreateNode(nodeId, content.ContentType, englishName, parentId, sortOrder));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.SetNodeView(nodeId, viewName ?? content.ContentType.AllowedViewNames.First()));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.AddNodeVersion(nodeId, versionId, contentId, "Version 1", NodeVersionStateKey.New));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _englishCulture, versionId, englishName.UrlFriendly()));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _dutchCulture, versionId, dutchName.UrlFriendly()));

            return nodeId;
        }

        // Modules
        private EmbeddedContent CreateRoomCarouselModule(IServiceProvider serviceProvider, SeederContext context)
        {
            var content = EmbeddedContent.New(ContentTypeDefinition.Lookup<RoomCarousel>());

            foreach (var roomPageId in context.Rooms.Take(6))
            {
                this.AddContentValue(content, nameof(RoomCarousel.Rooms), new Link { PageId = roomPageId });
            }

            return content;
        }

        // Modules
        private EmbeddedContent CreateFacilitiesModule(IServiceProvider serviceProvider, SeederContext context)
        {
            var content = EmbeddedContent.New(ContentTypeDefinition.Lookup<FacilitiesGrid>());

            foreach (var facilitiesPageId in context.Facilities.Take(9))
            {
                this.AddContentValue(content, nameof(FacilitiesGrid.Facilities), new Link { PageId = facilitiesPageId });
            }

            return content;
        }

        private async Task<EmbeddedContent> CreateOffersModuleAsync(IServiceProvider serviceProvider, SeederContext context)
        {
            var content = EmbeddedContent.New(ContentTypeDefinition.Lookup<OffersMosaic>());

            foreach (var imageFile in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), $"../seed-assets/offers".ToSafeFilePath())))
            {
                var imageId = await CreateImageAsync(serviceProvider, context, imageFile, "Offer");
                var offer = EmbeddedContent.New(ContentTypeDefinition.Lookup<OfferItem>());

                this.AddContentValue(offer, nameof(OfferItem.Image), NodeReference.New(imageId));
                this.AddContentValue(offer, nameof(OfferItem.Title), Lorem.Words(3, 6), Lorem.Words(3, 6));
                this.AddContentValue(offer, nameof(OfferItem.Text), Lorem.Paragraph(5, 8, 2, 4), Lorem.Paragraph(5, 8, 2, 4));

                this.AddContentValue(content, nameof(OffersMosaic.Offers), offer);
            }

            return content;
        }

        private void AddContentValue(ILocalizableContentModel content, string propertyKey, object englishValue, object dutchValue)
        {
            content.ViewFor(_englishCulture).AddValue(propertyKey, englishValue);
            content.ViewFor(_dutchCulture).AddValue(propertyKey, dutchValue);
        }

        private void AddContentValue(ILocalizableContentModel content, string propertyKey, object invariantValue)
        {
            content.ViewFor(CultureInfo.InvariantCulture).AddValue(propertyKey, invariantValue);
        }

        private void AddContentValues(ILocalizableContentModel content, string propertyKey, IEnumerable<object> invariantValues)
        {
            var view = content.ViewFor(CultureInfo.InvariantCulture);
            foreach (var value in invariantValues) view.AddValue(propertyKey, value);
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
            var commandBus = serviceProvider.GetRequiredService<ICommandBus>();
            var contentService = serviceProvider.GetRequiredService<IContentService>();

            var fileId = FileDescriptorId.New;
            await using (var fileStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), @$"../seed-assets/staff/staff-{imageNumber}.jpg".ToSafeFilePath())))
            {
                await commandBus.PublishAsync(new CreateFile(fileId, fileStream, $"staff-{imageNumber}.jpg", "image/jpeg"));
            }

            var contentType = ContentTypeDefinition.Lookup<StaffMember>();
            var content = EmbeddedContent.New(contentType);

            this.AddContentValue(content, nameof(StaffMember.Picture), new MediaFile(fileId));
            this.AddContentValue(content, nameof(StaffMember.Name), name);
            this.AddContentValue(content, nameof(StaffMember.Role), roleEnglish, roleDutch);

            // Events
            var nodeId = NodeId.New;
            var versionId = NodeVersionId.New;

            var contentId = await contentService.CreateContentAsync(contentType, content).NoSync();
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.CreateNode(nodeId, contentType, name, context.StaffMembersFolderId, 0));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.AddNodeVersion(nodeId, versionId, contentId, "Version 1", NodeVersionStateKey.New));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _englishCulture, versionId, null));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _dutchCulture, versionId, null));
        }

        private async Task<string> CreateImageAsync(IServiceProvider serviceProvider, SeederContext context, string imageFile, string nodeName)
        {
            var fileService = serviceProvider.GetRequiredService<IFileService>();
            var commandBus = serviceProvider.GetRequiredService<ICommandBus>();
            var contentService = serviceProvider.GetRequiredService<IContentService>();

            var fileId = FileDescriptorId.New;
            await using (var fileStream = File.OpenRead(imageFile))
            {
                await commandBus.PublishAsync(new CreateFile(fileId, fileStream, Path.GetFileName(imageFile), "image/jpeg"));
            }

            var contentType = ContentTypeDefinition.Lookup<Image>();
            var content = EmbeddedContent.New(contentType);

            this.AddContentValue(content, nameof(Image.File), new MediaFile(fileId));

            // Events
            var nodeId = NodeId.New;
            var versionId = NodeVersionId.New;
            var contentId = await contentService.CreateContentAsync(contentType, content).NoSync();
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.CreateNode(nodeId, contentType, nodeName, context.ImagesFolderId, 0));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.AddNodeVersion(nodeId, versionId, contentId, "Version 1", NodeVersionStateKey.New));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _englishCulture, versionId, null));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _dutchCulture, versionId, null));

            context.AllImages.Add(nodeId);

            return nodeId;
        }
    }
}
