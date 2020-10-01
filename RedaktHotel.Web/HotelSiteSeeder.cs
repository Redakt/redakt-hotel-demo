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
using Redakt.Files;
using Redakt.ContentManagement.NestedContent;
using RedaktHotel.Web.Models.Assets;
using Microsoft.AspNetCore.Http;
using RedaktHotel.Web.Models.Pages;
using RedaktHotel.Web.Models.Embedded;
using LoremNET;
using Redakt.ContentManagement.Content.Aggregates;
using Redakt.Data;
using Redakt.ContentManagement.NodeCollections;
using Redakt.ContentManagement.NodeCollections.Aggregates;
using Redakt.ContentManagement.NodeCollections.Commands;
using Redakt.Dictionary.Commands;
using Redakt.ContentManagement.Models;
using Redakt.Files.Commands;
using Redakt.ContentManagement.Files;
using Redakt.ContentManagement.Nodes.Aggregates;
using Redakt.Dictionary.Aggregates;
using Redakt.EventSourcing;
using Redakt.Files.Aggregates;

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
            await commandBus.PublishAsync(new SetNodeCollectionMapping(context.AssetLibraryId, new NodeCollectionMapping { Id = NodeCollectionMappingId.New, Culture = _englishCulture }));
            await commandBus.PublishAsync(new SetNodeCollectionMapping(context.AssetLibraryId, new NodeCollectionMapping { Id = NodeCollectionMappingId.New, Culture = _dutchCulture }));
            //await commandBus.PublishAsync(new AddNodeCollectionMapping(context.AssetLibraryId, IdGenerator.GenerateId(), CultureInfo.GetCultureInfo("en-US"), _englishCulture));

            // Create staff library
            var staffLibraryId = NodeCollectionId.New;
            await commandBus.PublishAsync(new CreateNodeCollection(staffLibraryId, NodeCollectionTypes.AssetLibrary, "Staff", 0));
            await commandBus.PublishAsync(new SetNodeCollectionMapping(staffLibraryId, new NodeCollectionMapping { Id = NodeCollectionMappingId.New, Culture = _englishCulture }));
            await commandBus.PublishAsync(new SetNodeCollectionMapping(staffLibraryId, new NodeCollectionMapping { Id = NodeCollectionMappingId.New, Culture = _dutchCulture }));

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
            await commandBus.PublishAsync(new SetNodeCollectionMapping(context.SiteId, new NodeCollectionMapping { Id = NodeCollectionMappingId.New, Culture = _englishCulture }));
            await commandBus.PublishAsync(new SetNodeCollectionMapping(context.SiteId, new NodeCollectionMapping { Id = NodeCollectionMappingId.New, Culture = _dutchCulture }));

            await commandBus.PublishAsync(new SetNodeCollectionHost(context.SiteId, new NodeCollectionHost { Id = NodeCollectionHostId.New, Hostname = hostname, RootPath = "/en", Cultures = new List<CultureInfo> { _englishCulture } }));
            await commandBus.PublishAsync(new SetNodeCollectionHost(context.SiteId, new NodeCollectionHost { Id = NodeCollectionHostId.New, Hostname = hostname, RootPath = "/nl", Cultures = new List<CultureInfo> { _dutchCulture } }));
        }

        private async Task CreatePagesAsync(IServiceProvider serviceProvider, SeederContext context)
        {
            var homepageId = await this.CreateHomepageAsync(serviceProvider, context);

            // Rooms & Suites 
            await this.CreateRoomsPagesAsync(serviceProvider, context, homepageId);

            // The Hotel
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<ContentPage>());
            this.SetProperty(content, nameof(ContentPage.NavigationTitle), "The Hotel", "Het Hotel");
            await this.CreatePageAsync(serviceProvider, context, homepageId, content, "The Hotel", "Het Hotel", 1);

            // Facilities
            await this.CreateFacilitiesPagesAsync(serviceProvider, context, homepageId);

            // Booking
            content = new LocalizedContent(ContentTypeDefinition.Lookup<BookingPage>());
            this.SetProperty(content, nameof(ContentPage.NavigationTitle), "Bookings", "Reserveringen");
            this.SetProperty(content, nameof(ContentPage.HideInNavigation), true);
            await this.CreatePageAsync(serviceProvider, context, homepageId, content, "Bookings", "Reserveringen", 3);

            // Blog
            await this.CreateBlogPagesAsync(serviceProvider, context, homepageId);

            // Contact
            content = new LocalizedContent(ContentTypeDefinition.Lookup<ContactPage>());
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

            var contentType = ContentTypeDefinition.Lookup<Homepage>();
            var content = new LocalizedContent(contentType);
            this.SetProperty(content, nameof(Homepage.PageTitle), "Welcome", "Welkom");
            this.SetProperty(content, nameof(Homepage.NavigationTitle), "Home", "Home");

            // Header slider items
            var sliderContentType = ContentTypeDefinition.Lookup<SliderItem>();

            var sliderContent1 = new LocalizedContent(sliderContentType);
            var sliderImageId = await CreateImageAsync(serviceProvider, context, Path.Combine(Directory.GetCurrentDirectory(), @"..\seed-assets\hotel\slider-item-1.jpg"), "Slider Image 1");
            this.SetProperty(sliderContent1, nameof(SliderItem.BackgroundImage), new NodeReference(sliderImageId));
            this.SetProperty(sliderContent1, nameof(SliderItem.Caption), "Welcome to The Redakt Hotel and Resort", "Welkom bij het Redakt Hotel en Resort");
            this.SetProperty(sliderContent1, nameof(SliderItem.Title), "Experience the Luxury", "Ervaar de Luxe");
            this.SetProperty(sliderContent1, nameof(SliderItem.Subtitle), "in our Hotel", "in ons Hotel");
            content.AddValue(nameof(Homepage.SliderItems), CultureInfo.InvariantCulture, ContentReference.New(sliderContent1));

            var sliderContent2 = new LocalizedContent(sliderContentType);
            this.SetProperty(sliderContent2, nameof(SliderItem.BackgroundImage), new NodeReference(sliderImageId));
            this.SetProperty(sliderContent2, nameof(SliderItem.Caption), "Welcome to The Redakt Hotel and Resort", "Welkom bij het Redakt Hotel en Resort");
            this.SetProperty(sliderContent2, nameof(SliderItem.Title), "Experience Exceptional Dining", "Ervaar Uitzonderlijk Dineren");
            this.SetProperty(sliderContent2, nameof(SliderItem.Subtitle), "in our Restaurants", "in onze Restaurants");
            content.AddValue(nameof(Homepage.SliderItems), CultureInfo.InvariantCulture, ContentReference.New(sliderContent2));

            var sliderContent3 = new LocalizedContent(sliderContentType);
            this.SetProperty(sliderContent3, nameof(SliderItem.BackgroundImage), new NodeReference(sliderImageId));
            this.SetProperty(sliderContent3, nameof(SliderItem.Caption), "Welcome to The Redakt Hotel and Resort", "Welkom bij het Redakt Hotel en Resort");
            this.SetProperty(sliderContent3, nameof(SliderItem.Title), "Experience Ultimate Relaxation", "Ervaar Ultieme Ontspanning");
            this.SetProperty(sliderContent3, nameof(SliderItem.Subtitle), "in our Spa & Wellness", "in onze Spa & Wellness");
            content.AddValue(nameof(Homepage.SliderItems), CultureInfo.InvariantCulture, ContentReference.New(sliderContent3));

            // Room carousel
            content.AddValue(nameof(Homepage.Modules), CultureInfo.InvariantCulture, ContentReference.New(this.CreateRoomCarouselModule(serviceProvider, context)));

            // Text content
            var textContent = new LocalizedContent(ContentTypeDefinition.Lookup<TextWithImage>());
            var imageId = await CreateImageAsync(serviceProvider, context, Path.Combine(Directory.GetCurrentDirectory(), @"..\seed-assets\home\home-text-image.jpg"), "Homepage Image");
            textContent.SetValue(nameof(TextWithImage.Image), CultureInfo.InvariantCulture, new NodeReference(imageId));
            textContent.SetValue(nameof(TextWithImage.HeadingCaption), _englishCulture, "Welcome to The Redakt");
            textContent.SetValue(nameof(TextWithImage.Heading), _englishCulture, "Demonstration website");
            textContent.SetValue(nameof(TextWithImage.BodyText), _englishCulture, "The Redakt Hotel & Resort is a demo website for the <a href=\"https://www.redaktcms.com\" target=\"_blank\">Redakt Content Management System</a>. Therefore most content is fictional and automatically generated. You can visit the back office application at <a href=\"/redakt\">/redakt</a>. In the back office and application code you can play with many of the features that are part of the Redakt system. Please note, as this website is meant as a showcase for Redakt CMS only, and it does not necessarily reflect best web software development practices. Parts of the website may be functionally incomplete or missing.");
            textContent.SetValue(nameof(TextWithImage.HeadingCaption), _dutchCulture, "Welkom bij The Redakt");
            textContent.SetValue(nameof(TextWithImage.Heading), _dutchCulture, "Demonstratie website");
            textContent.SetValue(nameof(TextWithImage.BodyText), _dutchCulture, "The Redakt Hotel & Resort is een demo website voor het <a href=\"https://www.redaktcms.com\" target=\"_blank\">Redakt Content Management System</a>. Om deze reden is de meeste content fictioneel en automatisch gegenereerd. Je kan de back office applicatie bezoeken op <a href=\"/redakt\">/redakt</a>. In the back office en applicatie code kan je veel van de functionaliteiten uitproberen die onderdeel maken van het Redakt systeem. Let wel, aangezien deze website alleen bedoeld is als showcase voor Redakt CMS, is het niet per se een goed voorbeeld van web software ontwikkeling. Sommige onderdelen van de website kunnen functioneel incompleet zijn.");
            content.AddValue(nameof(Homepage.Modules), CultureInfo.InvariantCulture, ContentReference.New(textContent));

            // Facilities grid
            var facilitiesContent = this.CreateFacilitiesModule(serviceProvider, context);
            facilitiesContent.SetValue(nameof(FacilitiesGrid.HeadingCaption), _englishCulture, "Our Facilities");
            facilitiesContent.SetValue(nameof(FacilitiesGrid.Heading), _englishCulture, "Explore The Redakt");
            facilitiesContent.SetValue(nameof(FacilitiesGrid.IntroText), _englishCulture, Lorem.Paragraph(5, 10, 4, 8));
            facilitiesContent.SetValue(nameof(FacilitiesGrid.HeadingCaption), _dutchCulture, "Onze Faciliteiten");
            facilitiesContent.SetValue(nameof(FacilitiesGrid.Heading), _dutchCulture, "Ontdek The Redakt");
            facilitiesContent.SetValue(nameof(FacilitiesGrid.IntroText), _dutchCulture, Lorem.Paragraph(5, 10, 4, 8));
            content.AddValue(nameof(Homepage.Modules), CultureInfo.InvariantCulture, ContentReference.New(facilitiesContent));

            // Offers mosaic
            var offersContent = await this.CreateOffersModuleAsync(serviceProvider, context);
            offersContent.SetValue(nameof(OffersMosaic.HeadingCaption), _englishCulture, "Our Offers");
            offersContent.SetValue(nameof(OffersMosaic.Heading), _englishCulture, "The Redakt Special Offers");
            offersContent.SetValue(nameof(OffersMosaic.IntroText), _englishCulture, Lorem.Paragraph(5, 10, 4, 8));
            offersContent.SetValue(nameof(OffersMosaic.HeadingCaption), _dutchCulture, "Onze Aanbiedingen");
            offersContent.SetValue(nameof(OffersMosaic.Heading), _dutchCulture, "The Redakt Speciale Aanbiedingen");
            offersContent.SetValue(nameof(OffersMosaic.IntroText), _dutchCulture, Lorem.Paragraph(5, 10, 4, 8));
            content.AddValue(nameof(Homepage.Modules), CultureInfo.InvariantCulture, ContentReference.New(offersContent));

            // Latest Blog Articles
            var blogContent = new LocalizedContent(ContentTypeDefinition.Lookup<LatestBlogArticles>());
            blogContent.SetValue(nameof(LatestBlogArticles.Parent), CultureInfo.InvariantCulture, new Link { PageId = context.BlogParentId });
            blogContent.SetValue(nameof(LatestBlogArticles.HeadingCaption), _englishCulture, "The Redakt Blog");
            blogContent.SetValue(nameof(LatestBlogArticles.Heading), _englishCulture, "Latest News");
            blogContent.SetValue(nameof(LatestBlogArticles.IntroText), _englishCulture, Lorem.Paragraph(5, 10, 4, 8));
            blogContent.SetValue(nameof(LatestBlogArticles.HeadingCaption), _dutchCulture, "Het Blog");
            blogContent.SetValue(nameof(LatestBlogArticles.Heading), _dutchCulture, "Laatste Nieuws");
            blogContent.SetValue(nameof(LatestBlogArticles.IntroText), _dutchCulture, Lorem.Paragraph(5, 10, 4, 8));
            content.AddValue(nameof(Homepage.Modules), CultureInfo.InvariantCulture, ContentReference.New(blogContent));

            var versionId = NodeVersionId.New;
            var contentId = ContentId.New;
            await commandBus.PublishAsync(new Redakt.ContentManagement.Content.Commands.CreateContent(contentId, contentType));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Content.Commands.AddContentRevision(contentId, ContentRevisionId.New, content.Properties));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.AddNodeVersion(homepageId, versionId, contentId, "Version 1", NodeVersionStateKey.New));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(homepageId, _englishCulture, versionId, null));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(homepageId, _dutchCulture, versionId, null));
        }

        private async Task CreateFacilitiesPagesAsync(IServiceProvider serviceProvider, SeederContext context, NodeId parentId)
        {
            // Create Facilities list
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<SimplePage>());
            this.SetProperty(content, nameof(ContentPage.NavigationTitle), "Facilities", "Faciliteiten");

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
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<FacilityPage>());
            var images = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), @$"..\seed-assets\facilities\{imageFolder}"));

            var imageAssets = new List<string>();
            for (var i = 0; i < images.Length; i++)
            {
                var imageId = await CreateImageAsync(serviceProvider, context, images[i], $"{englishName} Image {i + 1}");
                content.AddValue(nameof(FacilityPage.Images), CultureInfo.InvariantCulture, new NodeReference(imageId));
                imageAssets.Add(imageId);
            }

            this.SetProperty(content, nameof(FacilityPage.ListDescription), Lorem.Sentence(8, 14), Lorem.Sentence(8, 14));

            var facilityPageId = await this.CreatePageAsync(serviceProvider, context, facilitiesParentId, content, englishName, dutchName, sortOrder);

            context.Facilities.Add(facilityPageId);
        }

        private async Task CreateBlogPagesAsync(IServiceProvider serviceProvider, SeederContext context, NodeId parentId)
        {
            // Create Blog overview
            var blogContent = new LocalizedContent(ContentTypeDefinition.Lookup<SimplePage>());
            this.SetProperty(blogContent, nameof(ContentPage.NavigationTitle), "Blog", "Blog");

            context.BlogParentId = await this.CreatePageAsync(serviceProvider, context, parentId, blogContent, "Blog", "Blog", 4, "BlogOverview");

            var articleDate = DateTime.Today.AddDays(-_rnd.Next(0, 7));
            var imageFiles = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), @$"..\seed-assets\blog"));
            var sortIndex = 0;
            foreach (var imageFile in imageFiles)
            {
                var imageId = await CreateImageAsync(serviceProvider, context, imageFile, $"Blog Article Image {articleDate.ToShortDateString()}");
                var categories = Path.GetFileNameWithoutExtension(imageFile).Split('-').Skip(1);

                var content = new LocalizedContent(ContentTypeDefinition.Lookup<BlogArticle>());
                this.SetProperty(content, nameof(BlogArticle.Image), new NodeReference(imageId));
                content.AddValues(nameof(BlogArticle.Categories), CultureInfo.InvariantCulture, categories.Select(x => ContentValue.New(x)));
                this.SetProperty(content, nameof(BlogArticle.PublicationDate), articleDate);
                this.SetProperty(content, nameof(BlogArticle.Author), Lorem.Words(2, 3));
                this.SetProperty(content, nameof(BlogArticle.ListDescription), Lorem.Sentence(8, 14), Lorem.Sentence(8, 14));
                this.SetProperty(content, nameof(BlogArticle.Body), "<p>" + string.Join("</p><p>", Lorem.Paragraphs(6, 16, 5, 10, 3)) + "</p>", "<p>" + string.Join("</p><p>", Lorem.Paragraphs(6, 16, 5, 10, 3)) + "</p>");

                await this.CreatePageAsync(serviceProvider, context, context.BlogParentId, content, Lorem.Words(4, 8), Lorem.Words(4, 8), sortIndex++);
                
                articleDate = articleDate.AddDays(-_rnd.Next(0, 7));
            }
        }

        private async Task CreateRoomsPagesAsync(IServiceProvider serviceProvider, SeederContext context, NodeId parentId)
        {
            // Create Rooms list
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<SimplePage>());
            this.SetProperty(content, nameof(ContentPage.NavigationTitle), "Rooms", "Kamers");

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
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<RoomDetail>());
            var images = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), @$"..\seed-assets\rooms\{imageFolder}"));

            var mainImageId = await CreateImageAsync(serviceProvider, context, images.First(), $"{englishName} Image 1");
            this.SetProperty(content, nameof(RoomDetail.MainImage), new NodeReference(mainImageId));

            for (var i = 1; i < images.Length; i++)
            {
                var additionalImageId = await CreateImageAsync(serviceProvider, context, images[i], $"{englishName} Image {i + 1}");
                content.AddValue(nameof(RoomDetail.AdditionalImages), CultureInfo.InvariantCulture, new NodeReference(additionalImageId));
            }

            this.SetProperty(content, nameof(RoomDetail.NightlyRate), nightlyRate);
            this.SetProperty(content, nameof(RoomDetail.DiscountedFrom), new Random().Next(nightlyRate, Convert.ToInt32(nightlyRate * 1.2)));
            this.SetProperty(content, nameof(RoomDetail.ListDescription), Lorem.Sentence(8, 14), Lorem.Sentence(8, 14));
            this.SetProperty(content, nameof(RoomDetail.LongDescription), "<p>" + string.Join("</p><p>", Lorem.Paragraphs(6, 16, 5, 10, 3)) + "</p>", "<p>" + string.Join("</p><p>", Lorem.Paragraphs(6, 16, 5, 10, 3)) + "</p>");

            var roomPage = await this.CreatePageAsync(serviceProvider, context, roomsPageId, content, englishName, dutchName, sortIndex);

            context.Rooms.Add(roomPage);
        }

        private async Task<NodeId> CreatePageAsync(IServiceProvider serviceProvider, SeederContext context, NodeId parentId, ILocalizedContent content, string englishName, string dutchName, int sortOrder, string viewName = null)
        {
            var commandBus = serviceProvider.GetRequiredService<ICommandBus>();
            var contentType = content.ContentType();

            this.SetProperty(content, nameof(PageBase.PageTitle), englishName, dutchName);
            if (typeof(SimplePage).IsAssignableFrom(contentType.ViewModelType))
            {
                this.SetProperty(content, nameof(ContentPage.HeadingIntro), Lorem.Paragraph(8, 16, 3, 5), Lorem.Paragraph(8, 16, 3, 5));
                this.SetProperty(content, nameof(ContentPage.HeadingCaption), Lorem.Words(3, 6), Lorem.Words(3, 6));
            }

            // Events
            var nodeId = NodeId.New;
            var versionId = NodeVersionId.New;
            var contentId = ContentId.New;
            await commandBus.PublishAsync(new Redakt.ContentManagement.Content.Commands.CreateContent(contentId, contentType));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Content.Commands.AddContentRevision(contentId, ContentRevisionId.New, content.Properties));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.CreateNode(nodeId, contentType, englishName, parentId, sortOrder));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.SetNodeView(nodeId, viewName ?? contentType.AllowedViewNames.First()));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.AddNodeVersion(nodeId, versionId, contentId, "Version 1", NodeVersionStateKey.New));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _englishCulture, versionId, englishName.UrlFriendly()));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _dutchCulture, versionId, dutchName.UrlFriendly()));

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

                this.SetProperty(offer, nameof(OfferItem.Image), new NodeReference(imageId));
                this.SetProperty(offer, nameof(OfferItem.Title), Lorem.Words(3, 6), Lorem.Words(3, 6));
                this.SetProperty(offer, nameof(OfferItem.Text), Lorem.Paragraph(5, 8, 2, 4), Lorem.Paragraph(5, 8, 2, 4));

                content.AddValue(nameof(OffersMosaic.Offers), CultureInfo.InvariantCulture, ContentReference.New(offer));
            }

            return content;
        }

        private LocalizedContent CreateTextWithImageModule(CultureInfo culture, string imageId = null, string heading = null, string headingCaption = null)
        {
            var content = new LocalizedContent(ContentTypeDefinition.Lookup<TextWithImage>());

            if (headingCaption != null) content.SetValue(nameof(TextWithImage.HeadingCaption), culture, headingCaption);
            if (heading != null) content.SetValue(nameof(TextWithImage.Heading), culture, heading);
            content.SetValue(nameof(TextWithImage.BodyText), culture, Lorem.Paragraph(8, 16, 5, 10));
            if (imageId != null) content.SetValue(nameof(TextWithImage.Image), culture, new NodeReference(imageId));

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
            content.AddValues(nameof(ImageGallery.Images), CultureInfo.InvariantCulture, imageIds.Select(x => ContentValue.New(new NodeReference(x))));

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
            var commandBus = serviceProvider.GetRequiredService<ICommandBus>();

            var fileId = FileDescriptorId.New;
            await using (var fileStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), @$"..\seed-assets\staff\staff-{imageNumber}.jpg")))
            {
                await commandBus.PublishAsync(new CreateFile(fileId, fileStream, $"staff-{imageNumber}.jpg", "image/jpeg"));
            }

            var contentType = ContentTypeDefinition.Lookup<StaffMember>();
            var content = new LocalizedContent(contentType);

            this.SetProperty(content, nameof(StaffMember.Picture), new MediaFile(fileId));
            this.SetProperty(content, nameof(StaffMember.Name), name);
            this.SetProperty(content, nameof(StaffMember.Role), roleEnglish, roleDutch);

            // Events
            var nodeId = NodeId.New;
            var versionId = NodeVersionId.New;
            var contentId = ContentId.New;
            await commandBus.PublishAsync(new Redakt.ContentManagement.Content.Commands.CreateContent(contentId, contentType));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Content.Commands.AddContentRevision(contentId, ContentRevisionId.New, content.Properties));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.CreateNode(nodeId, contentType, name, context.StaffMembersFolderId, 0));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.AddNodeVersion(nodeId, versionId, contentId, "Version 1", NodeVersionStateKey.New));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _englishCulture, versionId, null));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _dutchCulture, versionId, null));
        }

        private async Task<string> CreateImageAsync(IServiceProvider serviceProvider, SeederContext context, string imageFile, string nodeName)
        {
            var fileService = serviceProvider.GetRequiredService<IFileService>();
            var commandBus = serviceProvider.GetRequiredService<ICommandBus>();

            var fileId = FileDescriptorId.New;
            await using (var fileStream = File.OpenRead(imageFile))
            {
                await commandBus.PublishAsync(new CreateFile(fileId, fileStream, Path.GetFileName(imageFile), "image/jpeg"));
            }

            var contentType = ContentTypeDefinition.Lookup<Image>();
            var content = new LocalizedContent(contentType);

            this.SetProperty(content, nameof(Image.File), new MediaFile(fileId));

            // Events
            var nodeId = NodeId.New;
            var versionId = NodeVersionId.New;
            var contentId = ContentId.New;
            await commandBus.PublishAsync(new Redakt.ContentManagement.Content.Commands.CreateContent(contentId, contentType));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Content.Commands.AddContentRevision(contentId, ContentRevisionId.New, content.Properties));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.CreateNode(nodeId, contentType, nodeName, context.ImagesFolderId, 0));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.AddNodeVersion(nodeId, versionId, contentId, "Version 1", NodeVersionStateKey.New));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _englishCulture, versionId, null));
            await commandBus.PublishAsync(new Redakt.ContentManagement.Nodes.Commands.PublishNode(nodeId, _dutchCulture, versionId, null));

            context.AllImages.Add(nodeId);

            return nodeId;
        }
    }
}
