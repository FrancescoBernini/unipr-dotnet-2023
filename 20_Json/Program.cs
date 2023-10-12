using System.Text;
using System.Text.Json;

// Fino a qualche anno fa, per interagire con dati serializzati in JSON in C#
// era comune utilizzare Json.NET (libreria nota anche come "Newtonsoft JSON").
//
// Recentemente la Microsoft ha sviluppato una serie di strumenti inclusi nel
// runtime di .NET per interagire con dati serializati in JSON.
//
// È ancora frequente trovare su internet esempi che fanno uso della libreria
// Json.NET, ma è consigiabile usare il JSON nativo per tutti i nuovi sviluppi.



// La classe JsonSerializer permette di serializzre classi .NET in JSON.

Example example1 = new()
{
    Hello = "bonjour",
    World = 42,
};

string json1 = JsonSerializer.Serialize(example1);
string jsonOptions = JsonSerializer.Serialize(example1, new JsonSerializerOptions()
{
    WriteIndented = true // con questo verra' identato
});

// Output:
// {"Hello":"bonjour","World":42}
Console.WriteLine(json1);



// La classe JsonSerializer permette anche di deserializzre dal JSON.

string json2 = """{ "Hello": "hola", "World": 123 }"""; // piu' comodo di mettere \ ogni volta prima di "

Example? example2 = JsonSerializer.Deserialize<Example>(json2);

// Output:
// Hello: hola
// World: 123
Console.WriteLine($"Hello: {example2?.Hello}"); // se e' null non leggere .hello
Console.WriteLine($"World: {example2?.World}");

// Il valore restituito è nullabile in quanto il JSON potrebbe essere "null".
Example? example3 = JsonSerializer.Deserialize<Example>("null");

// Output:
// Valore null? True
Console.WriteLine($"Valore null? {example3 == null}");

// in questo modo in ram potrebbe finire un json enorme e non va bene piu' questo modo
// c'e' una classe migliore che vediamo poi che si chiama UTF8-json qualcosa

class Example
{
    public string? Hello { get; set; }

    public int World { get; set; }
}
