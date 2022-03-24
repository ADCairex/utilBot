// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Util_Bot.Bots
{
    public class Binary
    {
        public string base64;
        public string subType;

        public Binary(string binary = null)
        {
            if (binary == null)
            {
                Guid guid = Guid.NewGuid();
                this.base64 = Convert.ToBase64String(guid.ToByteArray());
                this.subType = "03";
            }
            else
            {
                Guid guid = Guid.Parse(binary);
                this.base64 = Convert.ToBase64String(guid.ToByteArray());
                this.subType = "03";
            }
        }
    }

    public class IdMongo
    {
        [JsonProperty(PropertyName = "$binary")]
        public Binary binary;

        public IdMongo(string binary = null)
        {
            if (binary == null)
            {
                this.binary = new Binary();
            }
            else
            {
                this.binary = new Binary(binary);
            }
        }
    }
    public class UtilBot : ActivityHandler
    {
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hola bienvendio al chat"+member.Name), cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userOption = turnContext.Activity.Text;
            if (userOption == "utilBot genera un guid")
            {
                Guid guid = Guid.NewGuid();
                string guidStr = guid.ToString();
                var idObject = new IdMongo(guidStr);
                string json = JsonConvert.SerializeObject(idObject);
                await turnContext.SendActivityAsync("Guid: " + guidStr);
                await turnContext.SendActivityAsync("Base64: \"_id\": " + json);
            } 
            else if (userOption.Contains("utilBot codifica el guid"))
            {
                string[] optionSplited = userOption.Split(' ');
                string guidStr = optionSplited[optionSplited.Length - 1];

                try
                {
                    Guid guid = Guid.Parse(guidStr);
                    //string guidB64 = Convert.ToBase64String(guid.ToByteArray());
                    var idObject = new IdMongo(guidStr);
                    string json = JsonConvert.SerializeObject(idObject);
                    await turnContext.SendActivityAsync("Base64: \"_id\": " + json);
                }
                catch
                {
                    string response = "El guid no es correcto";
                    await turnContext.SendActivityAsync(response);
                }
            }
            else if (userOption.Contains("utilBot codifica la apiKey"))
            {
                string[] optionSplited = userOption.Split(' ');
                string apiStr = optionSplited[optionSplited.Length - 1];

                var bytes = Encoding.UTF8.GetBytes(apiStr);
                await turnContext.SendActivityAsync(Convert.ToBase64String(bytes));
            }
            else if (userOption.Contains("utilBot decodifica el guid"))
            {
                string[] optionSplited = userOption.Split(' ');
                string codedGuid = optionSplited[optionSplited.Length - 1];

                var bytes = Convert.FromBase64String(codedGuid);
                try
                {
                    Guid guid = new Guid(bytes);
                    await turnContext.SendActivityAsync(guid.ToString());
                }
                catch
                {
                    await turnContext.SendActivityAsync("No se ha podido decodificar el guid");
                }

            }
            else if (userOption.Contains("utilBot decodifica la apiKey"))
            {
                string[] optionSplited = userOption.Split(' ');
                string codedGuid = optionSplited[optionSplited.Length - 1];

                var bytes = Convert.FromBase64String(codedGuid);
                string finalStr = Encoding.UTF8.GetString(bytes);
                if (finalStr.Contains(":"))
                {
                    string[] separator = { ":" };
                    string[] apiKey = finalStr.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    await turnContext.SendActivityAsync("ClientId: " + apiKey[0]);
                    await turnContext.SendActivityAsync("Secret: " + apiKey[1]);
                }
                else
                {
                    await turnContext.SendActivityAsync("No se ha podido decodificar");
                }
            }
        }
    }
}
