using Redakt.BackOffice;
using RedaktHotel.BackOfficeExtensions.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedaktHotel.BackOfficeExtensions
{
    public class CustomBackOfficeEventHandler: IBackOfficeEventHandler
    {
        public Task PostAuthenticateAsync(BackOfficeContext context)
        {
            return context.ModalDialog.ShowAsync<DisclaimerDialog>();
        }
    }
}
