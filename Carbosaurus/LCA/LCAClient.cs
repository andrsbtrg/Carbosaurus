using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using Carbosaurus.Utils;

namespace Carbosaurus
{
    public static class LCAClient
    {
        private static readonly HttpClient _client = new HttpClient(new HttpClientHandler { UseProxy = false, UseCookies = false });
        private static readonly string url = "http://localhost:5000";
        public static bool saveResults = false;
        public static string TestConnection()
        {
            using (var client = new HttpClient(new HttpClientHandler
            {
                UseProxy = false
            }))
            {
                var endpoint = new Uri(url);

                var response = client.GetAsync(endpoint).Result;
                var json = response.Content.ReadAsStringAsync().Result;

                return json;
            }
        }

        public static string GetMaterials()
        {
            string json = "";
            using (var client = new HttpClient(new HttpClientHandler
            {
                UseProxy = false,
                UseCookies = false
            }))
            {
                var endpoint = new Uri(url + "/materials");
                try
                {
                    var response = client.GetAsync(endpoint).Result;
                    json = response.Content.ReadAsStringAsync().Result;
                }
                catch (Exception)
                {
                    return json;
                }

                return json;
            }
        }

        public static string SearchMaterial(string query)
        {
            string json = "";
            using (var client = new HttpClient(new HttpClientHandler
            {
                UseProxy = false,
                UseCookies = false
            }))
            {

                var endpoint = new Uri(url + "/materials/name-en/" + query);
                try
                {
                    var response = client.GetAsync(endpoint).Result;

                    if (response.StatusCode == HttpStatusCode.OK)
                        json = response.Content.ReadAsStringAsync().Result;
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException)
                        return "Something went wrong...";
                    else if (ex is WebException)
                        return ex.Message;
                    else if (ex is System.Net.Sockets.SocketException)
                        return "Server error...";
                    else if (ex is WebException)
                        return "Server error...";
                    else
                        return "Something went terribly wrong";
                }
                return json;
            }
        }

        public static List<LCA.BuildingMaterial> GetMaterial(string query)
        {
            string json = "";
            using (var client = new HttpClient(new HttpClientHandler
            {
                UseProxy = false,
                UseCookies = false
            }))
            {

                var endpoint = new Uri(url + "/materials/name-en/" + query);
                try
                {
                    var response = client.GetAsync(endpoint).Result;

                    if (response.StatusCode == HttpStatusCode.OK)
                        json = response.Content.ReadAsStringAsync().Result;

                    return JsonConvert.DeserializeObject<List<LCA.BuildingMaterial>>(json);

                }
                catch (Exception ex)
                {
                    if (ex is AggregateException)
                        Console.WriteLine("Something went wrong...");
                    else if (ex is WebException)
                        Console.WriteLine(ex.Message);
                    else if (ex is System.Net.Sockets.SocketException)
                        Console.WriteLine("Server error...");
                    else if (ex is WebException)
                        Console.WriteLine("Server error");
                    else
                        Console.WriteLine("Something went terribly wrong");
                    return null;
                }
            }

        }
        public static List<LCA.LandscapeMaterial> GetLandscapeMaterials()
        {
            string json = "";
            using (var client = new HttpClient(new HttpClientHandler{ UseProxy = false,UseCookies = false }))
            {

                var endpoint = new Uri(url + "/landscape");
                try
                {
                    var response = client.GetAsync(endpoint).Result;

                    if (response.StatusCode == HttpStatusCode.OK)
                        json = response.Content.ReadAsStringAsync().Result;

                    return JsonConvert.DeserializeObject<List<LCA.LandscapeMaterial>>(json);

                }
                catch (Exception ex)
                {
                    if (ex is AggregateException)
                        Console.WriteLine("Something went wrong...");
                    else if (ex is WebException)
                        Console.WriteLine(ex.Message);
                    else if (ex is System.Net.Sockets.SocketException)
                        Console.WriteLine("Server error...");
                    else if (ex is WebException)
                        Console.WriteLine("Server error");
                    else
                        Console.WriteLine("Something went terribly wrong");
                    return null;
                }
            }

        }

        public static LCA.TreeNode GetLandscapeCollection()
        {
            string json = "";
            using (var client = new HttpClient(new HttpClientHandler { UseProxy = false, UseCookies = false }))
            {
                var endpoint = new Uri(url + "/lsmaterials/collection");
                try
                {
                    var response = client.GetAsync(endpoint).Result;

                    if (response.StatusCode == HttpStatusCode.OK)
                        json = response.Content.ReadAsStringAsync().Result;
                    //return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    return JsonConvert.DeserializeObject<LCA.TreeNode>(json);

                }
                catch (Exception ex)
                {
                    if (ex is AggregateException)
                        Console.WriteLine("Something went wrong...");
                    else if (ex is WebException)
                        Console.WriteLine(ex.Message);
                    else if (ex is System.Net.Sockets.SocketException)
                        Console.WriteLine("Server error...");
                    else if (ex is WebException)
                        Console.WriteLine("Server error");
                    else
                        Console.WriteLine("Something went terribly wrong");
                    return null;
                }
            }
        }

        //public static void SendBOM()
        //{
        //    var bom = Inventory.GetBOM();
        //    using (var client = new HttpClient(new HttpClientHandler { UseProxy = false, UseCookies = false }))
        //    {
        //        var content = new StringContent(JsonConvert.SerializeObject(bom), Encoding.UTF8, "application/json");
        //        var endpoint = new Uri("http://localhost:5000/simulation");
        //        try
        //        {
        //            var response = client.PostAsync(endpoint,content);
        //            response.Wait();
        //            Rhino.RhinoApp.Write(response.Result.Content.ToString());
        //        }
        //        catch (WebException ex)
        //        {
        //            Console.WriteLine("Some web error");
        //        }

        //    }
        //    return;
        //}


        public static async Task SendBOM()
        {
            var bom = Inventory.GetBOM();
            string write = "False";
            if (saveResults)
                write = "True";

            using (var client = new HttpClient(new HttpClientHandler { UseProxy = false, UseCookies = false }))
            {
                var content = new StringContent(JsonConvert.SerializeObject(bom), Encoding.UTF8, "application/json");
                var endpoint = new Uri($"http://localhost:5000/simulation/write={write}");
                try
                {
                    var response = client.PostAsync(endpoint, content);
                    await response;

                    var responseJson = "";
                    if (response.IsCompleted)
                        {

                        responseJson = await response.Result.Content.ReadAsStringAsync().ConfigureAwait(false);
                        //Rhino.RhinoApp.WriteLine(responseJson);
                        Utils.ResultsPreview.Instance.SaveValues(responseJson);
                        Utils.ResultsPreview.Instance.PreviewValues("Correlation");
                        Rhino.RhinoApp.WriteLine("Simulation completed");
                    }

                    //Rhino.RhinoApp.Write(response.Result.Content.ToString());
                }
                catch (WebException ex)
                {
                    Console.WriteLine("Some web error");
                }

            }
            return;
        }
    }
}
