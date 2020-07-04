using Redakt.BackOffice.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedaktHotel.BackOfficeExtensions.Models
{
    public class RoomFeatureOptions : ISelectOptionsProvider
    {
        /// <summary>
        /// In this demo, the list of select options is statically generated. In a real scenario, this list could be retrieved from a database. The options provider can be registered with the DI container to allow for injected services.
        /// </summary>
        public ValueTask<IEnumerable<SelectOption>> GetOptionsAsync()
        {
            var options = new List<SelectOption>();

            options.Add(new SelectOption("breakfast", "Breakfast included"));
            options.Add(new SelectOption("wifi", "High-Speed Internet"));
            options.Add(new SelectOption("hairdryer", "Hair Dryer"));
            options.Add(new SelectOption("tv", "LCD TV"));
            options.Add(new SelectOption("bath", "Shower and Bath"));
            options.Add(new SelectOption("locker", "Secured Locker"));
            options.Add(new SelectOption("airco", "Air Conditioning"));
            options.Add(new SelectOption("minibar", "Mini Bar"));
            options.Add(new SelectOption("tea-coffee", "Tea and Coffee maker"));
            options.Add(new SelectOption("late-checkout", "Late Checkout"));
            options.Add(new SelectOption("ironing", "Ironing Facilities"));
            options.Add(new SelectOption("balcony", "Balcony"));
            options.Add(new SelectOption("xl-room", "Extra large room"));
            options.Add(new SelectOption("xl-bed", "Extra large bed"));
            options.Add(new SelectOption("desk", "Desk"));
            options.Add(new SelectOption("alarmclock", "Alarm Clock"));
            options.Add(new SelectOption("slippers", "Complimentary Slippers"));
            options.Add(new SelectOption("bathrobe", "Bathrobe"));

            return new ValueTask<IEnumerable<SelectOption>>(options);
        }
    }
}
