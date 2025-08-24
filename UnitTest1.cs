using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoil.Models;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace StorySpoiler
{
    internal class StorySpoilerAttribute : System.Attribute
    {
        private readonly int _id;
        public StorySpoilerAttribute(int id)
        {
            _id = id;
        }
    }

    [TestFixture]
    public class StorySpoilerTests
    {
        private RestClient client;
        private static string createdStoryId;
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("viktor1", "viktor1");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

        [Test, StorySpoiler(1)]
        [Order(1)]
        public async Task CreateNewStorySpoiler_ShouldReturnCreated()
        {
            var story = new
            {
                Title = "New Story",
                Description = "Test story description",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = await client.ExecuteAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Content, Does.Contain("Successfully created!"));


            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            createdStoryId = json.GetProperty("storyId").GetString();
        }




        [Test, StorySpoiler(2)]
        [Order(2)]
        public async Task EditStorySpoilerTitle_ShouldReturnOk()
        {
            var changes = new
            {
                Title = "Updated Title",
                Description = "Updated description",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
            request.AddJsonBody(changes);

            var response = await client.ExecuteAsync(request);

            TestContext.WriteLine($"Status: {response.StatusCode}");
            TestContext.WriteLine($"Content: {response.Content ?? "EMPTY"}");

            
            Assert.That(response.StatusCode, Is.AnyOf(HttpStatusCode.OK, HttpStatusCode.NoContent));

            
            if (!string.IsNullOrWhiteSpace(response.Content))
            {
                var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
                Assert.That(json.GetProperty("title").GetString(), Is.EqualTo("Updated Title"));
                Assert.That(json.GetProperty("description").GetString(), Is.EqualTo("Updated description"));
            }
        }



        [Test, StorySpoiler(3)]
        [Order(3)]
        public void GetAllStorySpoilers_ShouldReturnList()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var storySpoilers = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(storySpoilers, Is.Not.Empty);
        }

        [Test, StorySpoiler(4)]
        [Order(4)]
        public void DeleteStorySpoiler_ShoudReturnOk()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }

        [Test, StorySpoiler(5)]
        [Order(5)]
        public void CreateStorySpoilerWithoutRequiredFields_ShouldReturnBadRequest()
        {
            var story = new
            {
                Title = "",
                Description = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }


        [Test, StorySpoiler(6)]
        [Order(6)]
        public void EditNonExistingStorySpoiler_ShouldReturnNoSpoilers()
        {
            string fakeId = "123";
            var changes = new[]
            {
          new { Title = "New Story22",
                  Description = "Test story description22",
                  Url = "" }
        };

            var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);
            request.AddJsonBody(changes);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, StorySpoiler(7)]
        [Order(7)]
        public void DeleteNonExistingStorySpoiler_ShouldReturnBadRequest()
        {
            string fakeId = "123";
            var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [OneTimeTearDown]
public void Cleanup()
{
    client?.Dispose();
}
    }
}
    