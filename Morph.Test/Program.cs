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


using var client = new HttpClient();
var requestContent = new StringContent(body, Encoding.UTF8, "application/json");
var response = await client.PostAsync("http://localhost:5257" + url, requestContent);
var responseString = await response.Content.ReadAsStringAsync();

Console.WriteLine(response.StatusCode);
Console.WriteLine(responseString);