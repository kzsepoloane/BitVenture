using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Xml;

namespace BitVenture
{
    public enum Test { regex, identifier};
    class Program
    {
        static void Main(string[] args)
        {

            var client = new RestClient();
            
            doBasic(client);


        }
        static void doBasic(RestClient client)
        {
            //I made a change to the context of the the basic endpoints json provided I do not
            //understand how I was meant to hand indentifier and 
#if DEBUG
            string input = File.ReadAllText("basic_endpoints.json");
            client.UseJson();            
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
                    switch (service.datatype)
                    {
                        case "JSON":
                            client.UseJson();
                            break;
                        case "XML":
                            client.UseXml();
                            break;
                    }
                    foreach (var endpoint in service.endpoints)
                    {
                        if (endpoint.enabled)
                        {
                            IRestRequest request = new RestRequest(endpoint.resource);
                            var response = client.Get(request);
                            if (response.ResponseStatus == ResponseStatus.Completed && (response.StatusCode != System.Net.HttpStatusCode.NotFound || response.StatusCode != System.Net.HttpStatusCode.InternalServerError))
                            {
                                JToken content = default;
                                switch (service.datatype)
                                {
                                    case "JSON":
                                        content = (JToken)JsonConvert.DeserializeObject(response.Content);
                                        break;
                                    case "XML":
                                        XmlDocument doc = new XmlDocument();
                                        doc.LoadXml(response.Content);

                                        string json = JsonConvert.SerializeXmlNode(doc);
                                        content = (JToken)JsonConvert.DeserializeObject(json);
                                        break;
                                }
                                
                                if (content != null && content.HasValues)
                                {
                                    foreach (var expect in endpoint.response)
                                    {

#if DEBUG
                                       Console.WriteLine($"{expect["element"]}: {expect["regex"]} = {recurseToken(content, expect["element"], expect["regex"])}");
#endif
#if BONUS
                                        if (expect.ContainsKey("regex") || expect.ContainsKey("identifier"))
                                        {
                                            var element = expect["element"]; 
                                            expect.Remove("element");
                                            Console.WriteLine($"{element}: {expect.First().Key} = {recurseToken(content, element, expect.First(),service)}");
                                        }
                                        
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
         static JToken recurseToken(JToken token, string element)
        {
            if (token.Path.Contains(element))
            {
                return token;

            }
            else if (token.HasValues)
            {
                foreach (var child in token.Children())
                {
                    var tkn = recurseToken(child, element);
                    if ( tkn != null)
                    {
                        return tkn;
                    }
                }
                if (token.Next != null)
                {
                    return recurseToken(token.Next, element);
                }
                return null;
            }
            else if (token.Next != null)
            {
                return recurseToken(token.Next, element);
            }
            else
            {
                return null;
            }
        }
        static bool recurseToken(JToken token, string element, KeyValuePair<string,string> keyValue, Service service)
        {
            dynamic tkn = recurseToken(token, element);
            if (tkn != null)
            {
                if (keyValue.Key == "regex")
                {

                    return Regex.IsMatch(Convert.ToString(tkn.Value), keyValue.Value);
                }
                else
                {
                    if (service.identifiers != null)
                    {
                        foreach (var identifier in service.identifiers)
                        {
                            if (identifier.Key == keyValue.Value)
                            {
                                return Regex.IsMatch(Convert.ToString(tkn.Value), identifier.Value);
                            }
                        }
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

        }

    }
}
