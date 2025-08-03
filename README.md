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
