using System.Text;

var url = "/test?name=will&greeting=greeting2";

var body = """
{
    "greeting1" : "hi",
    "greeting2" : "hello",
    "nested" : {
        "value1" : 123,
        "value2" : "abc"
    }
}
""";


var cdpApiKey = "123";

using var client = new HttpClient();
client.DefaultRequestHeaders.Add("CDP-Api-Key", cdpApiKey);

var requestContent = new StringContent(body, Encoding.UTF8, "application/json");
var response = await client.PostAsync("http://localhost:5257" + url, requestContent);
var responseString = await response.Content.ReadAsStringAsync();

Console.WriteLine(((int)response.StatusCode).ToString() + ": " + response.StatusCode.ToString());
Console.WriteLine(response.Headers.ToString());
Console.WriteLine(responseString);