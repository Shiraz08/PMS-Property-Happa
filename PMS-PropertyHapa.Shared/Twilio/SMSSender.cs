using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;


namespace PMS_PropertyHapa.Shared.Twilio
{
    public class SMSSender
    {
        public async Task SmsSender(string reciever,string message,string accountSid,string authToken,string phoneNumber)
        {
            try
            {
                TwilioClient.Init(accountSid, authToken);
                await MessageResource.CreateAsync(
                    to: new PhoneNumber(reciever),
                    from: new PhoneNumber(phoneNumber),
                    body: message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending SMS: {ex.Message}");
                throw;
            }
        }
    }
}
