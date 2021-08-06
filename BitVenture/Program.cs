using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace BitVenture
{
    public enum Test { regex, identifier};
    class Program
    {
        static void Main(string[] args)
        {

            var client = new RestClient();
            client.UseJson();            
            doBasic(client);


        }
        static void doBasic(RestClient client)
        {
            //I made a change to the context of the the basic endpoints json provided I do not
            //understand how I was meant to hand indentifier and 
#if DEBUG
            string input = File.ReadAllText("basic_endpoints.json");
#endif
#if BONUS
            string input = File.ReadAllText("bonus_endpoints.json");
#endif

            JObject obj =  (JObject)JsonConvert.DeserializeObject(input);
            
            IEnumerable<Service> services = ((JArray)obj["services"]).ToObject<IList<Service>>();
            foreach (var service in services)
            {
                if (service.enabled)
                {
                    client.BaseUrl = new Uri(service.baseUrl);
                    foreach (var endpoint in service.endpoints)
                    {
                        if (endpoint.enabled)
                        {
                            IRestRequest request = new RestRequest(endpoint.resource);
                            var response = client.Get(request);
                            if (response.ResponseStatus == ResponseStatus.Completed && response.StatusCode != System.Net.HttpStatusCode.NotFound)
                            {
                                JToken content = (JToken)JsonConvert.DeserializeObject(response.Content);
                                if (content.HasValues)
                                {
                                    foreach (var expect in endpoint.response)
                                    {

#if DEBUG
                                       Console.WriteLine($"{expect["element"]}: {expect["regex"]} = {recurseToken(content, expect["element"], expect["regex"])}");
#endif
#if BONUS
            string input = File.ReadAllText("bonus_endpoints.json");
#endif
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
        }
        
        static bool recurseToken(JToken token, string element, string test)
        {
            if (token.Path == element)
            {
                //I Really don't like that this works just seems like a hack
                //why is the value property not exposed?
                dynamic tkn = token;
                return Regex.IsMatch(Convert.ToString(tkn.Value), test);

            }
            else if (token.HasValues)
            {
                foreach (var child in token.Children())
                {
                    if (recurseToken(child, element, test))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (token.Next != null)
            {
                return recurseToken(token.Next, element, test);
            }
            else
            {
                return false;
            }


        }
        
    }
}
