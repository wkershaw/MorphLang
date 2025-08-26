# MorphLang

**Morph** is a domain-specific language (DSL) for transforming data. It is capable of accepting the JSON bodies and URLS of a HTTP request, querying them and writing a response, useful for mock servers or backend logic prototyping.

## Features

- Parse and query JSON request bodies
- Parse and query URL query strings
- Define custom transformation logic
- Generate dynamic HTTP responses
- Easy to script and extend

## Example

### URL:
**/test?name=Bob&greeting=greeting2"**

### Body:
```
{
    "greeting1" : "hi",
    "greeting2" : "hello"
}
```
### Morph:
```
// A test morph file

in JSON body;
in URL query;

var name = query["name"];

var greetingToUse = query["greeting"];
var greeting = body[greetingToUse];

var response = $"
{
    \"response\" : \"[greeting] [name]!\"
}";

WriteLine(response);
```

### Outputs:
```
{
    "response" : "hello Bob!"
}
```


## Get started

1) Clone the repository
2) Start the test server in the `Morph.Api` folder using `dotnet run`
3) Within a seperate terminal, send a test request to the server using the `Morph.Test` project (can also be run with `dotnet run`)

The test server checks for a .mor file within the folder corresponding to the URL of the request, starting at the `morph` folder, an example .mor file is included in `Morph.Api\morph\test.mor`
