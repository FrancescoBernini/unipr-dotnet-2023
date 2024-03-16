
// In C# .NET è possibile avviare nuovi thread, ma generalmente il codice in un
// singolo metodo scritto in C# viene eseguito su un singolo thread.

// Con C# 5 sono state introdotte le nuove parole chiave async ed await, introducendo
// il vasto universo dei task in contrapposizione al concetto di thread.
//
// Un thread è un filo logico eseguito su un processore che può essere eseguito
// su uno o più core logici del processore. È un concetto fortemente legato
// all'hardware ed al kernel del sistema operativo.
//
// Un task è semplicemente un'operazione, che in base alla configurazione
// potrebbe essere vincolata ad essere eseguita su un determinato thread,
// oppure liberamente sul primo thread libero in una thread-pool.
//
// Come un thread può essere interrotto dal kernel e ripreso su un altro core
// logico della CPU, un task può essere interrotto e ripreso su un altro thread.
// Questi cambi possono avvenire solo in corrispondenza della parola chiave await.

// Il vantaggio di usare task asincroni è che il runtime di .NET è libero di
// riciclare thread, piuttosto che doverne creare uno nuovo per ogni operazione.
// Creare un nuovo thread è un'operazione potenzialmente molto pesante, che
// prevede una chiamata al kernel del sistema operativo ed una modifica alla
// coda di esecuzione globale.

// È bene usare async in qualsiasi metodo che esegue operazioni di input/output.
// Non è utile convertire in async un metodo che fa solo uso di CPU e memoria.

// Per ulteriori informazioni:
// https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/async-scenarios



// Un metodo è considerato "asincrono" se usa la parola chiave async e/o se
// restituisce un Task oppure un ValueTask.

// i metodi async 

// mio esempio
async Task<int> SumAsync(int a, int b)
{
    await Task.Delay(5000); // come la sleep ma asyncrona
    return a + b;
}

Task<int> sum = SumAsync(1, 1);
Console.WriteLine("ciao");
Console.WriteLine(sum); // senza await scrive qualcosa che non ha ancora.

void Example1()
{
    // Questo non è un metodo asincrono.
}

async void Example1Async() // convenzione "Async" alla fine del nome del metodo
{
    // Questo è un metodo asincrono.

    // Nota: non è MAI considerato buona pratica fare un metodo asincrono con
    // tipo di ritorno void. Questo metodo è solo di esempio. Dovra' restituire un tipo che implemnta
    // l'interfaccia IAsyncResult
}

double Example2()
{
    return 12.34;
}

async Task<double> Example2Async()
{
    return 12.34;
}

int Example3()
{
    return 123;
}

Task<int> Example3Async()
{
    return Task.FromResult(123);
}

string Example4()
{
    return "hello";
}

ValueTask<string> Example4Async()
{
    return ValueTask.FromResult("hello");
}

// valuetask e' una struct quindi va sullo stack ma cmq se si lavora
// in modo async verra' creato un Task e quindi in genere si usa Task
// viene usato quando l'operazione potrebbe essere sia in modo sincrono
// che in modo asincrono. viene usato anche quando le operazioni vengono
// chiamate molto frequentemente. ATTENZIONE: A differenza di Task, che
// può essere atteso più volte, un ValueTask è pensato per essere atteso
// una sola volta. Se hai bisogno di attendere lo stesso risultato più
// volte, potresti dover convertire il ValueTask in un Task con .AsTask()
// o gestire diversamente la logica.



// I metodi asincroni possono essere chiamati all'interno di un contesto "async"
// tramite la parola chiave "await".
string DoSomething()
{
    double test1 = Example2();
    int test2 = Example3();
    string test3 = Example4();

    return $"{test1} {test2} {test3}";
}

async Task<string> DoSomethingAsync()
{
    Task<double> test0 = Example2Async();
    double test0r = await test0; // aspetto che finisca il task test0

    double test1 = await Example2Async(); // chiama e aspetto ma e' come al solito
    int test2 = await Example3Async();
    string test3 = await Example4Async();

    return $"{test1} {test2} {test3}";
}



// Il vantaggio di usare metodi asincroni è quando sono presenti operazioni che
// non restituiscono immediatamente un valore, per esempio letture da file,
// socket e networking, interazioni con periferiche di sistema, ecc...
string GetPage(string url)
{
    using HttpClient client = new();

    // La riga seguente fa una chiamata HTTP; mentre attende risposta il thread
    // chiamante resta bloccato, occupando inutilmente risorse del sistema.
    HttpResponseMessage response = client.Send(new(HttpMethod.Get, url)); // interfaccia bloccata

    // Le righe seguenti leggono la risposta in modo sincrono.
    using Stream stream = response.Content.ReadAsStream();
    using StreamReader reader = new(stream, leaveOpen: true);
    string content = reader.ReadToEnd();

    return content;
}

// Il cancellation token permette di annullare un'operazione asincrona
// dall'esterno se il chiamante non è più interessato ad una risposta.
// È buona pratica aggiungerne uno ai metodi asincroni.
async Task<string> GetPageAsync(string url, CancellationToken cancellationToken = default)
{
    // Nota: la versione asincrona del codice seguente potrebbe essere scritta
    // in modo più semplice, ma ho cercato di tenerla il più simile possibile
    // alla versione sincrona, per permettere un confronto riga per riga.

    using HttpClient client = new();

    // La riga seguente fa una chiamata HTTP; mentre attende risposta viene
    // rilasciato il thread chiamante, che nel frattempo può fare altro.
    HttpResponseMessage response = await client.SendAsync(new(HttpMethod.Get, url), cancellationToken);

    // Le righe seguenti leggono la risposta in modo asincrono.
    await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken); 
    // L'await prima della using viene messo perche' Stream implementa l'interfaccia IDisposableAsync
    // e quindi non vogliamo bloccare il thread mentre fa la DisposeAsync

    using StreamReader reader = new(stream, leaveOpen: true); // leaveOpen ci lascia aperto lo stream
    string content = await reader.ReadToEndAsync(cancellationToken);

    return content;
}
// se il processore ha 8 thread c# ne alloca 16


// In un contesto sincrono potrebbe risultare difficile eseguire più operazioni
// in parallelo, ma in un contesto asincrono è semplice.
async Task<string> GetPagesSequentialAsync(CancellationToken cancellationToken = default)
{
    // Fa la prima chiamata HTTP ed attende una risposta.
    string home = await GetPageAsync("https://example.com", cancellationToken);

    // Fa la seconda chiamata HTTP ed attende una risposta.
    string test = await GetPageAsync("https://example.com/test", cancellationToken);

    return $"{home}\n\n{test}";
}

async Task<string> GetPagesParallelAsync(CancellationToken cancellationToken = default)
{
    // Fa due chiamate HTTP in parallelo.
    Task<string> homeTask = GetPageAsync("https://example.com", cancellationToken);
    Task<string> testTask = GetPageAsync("https://example.com/test", cancellationToken);

    // attende che entrambe le chiamate abbiano ricevuto una risposta in maniera sincrona
    // Task.WaitAll(homeTask, testTask); 

    // attende che entrambe le chiamate abbiano ricevuto una risposta in maniera asyncrona
    await Task.WhenAll(homeTask, testTask);

    // Legge la risposta ad entrambe le chiamate.
    string home = await homeTask; // senza la await non compila infatti e' come se assegnassimo della roba che non abbiamo ancora
    string test = await testTask;

    return $"{home}\n\n{test}";
}


// NON E' BUONA PRATICA
// È possibile eseguire codice sincrono in un contesto asincrono:
Task<string> SomeLegacyMethodAsync(CancellationToken cancellationToken = default)
{
    // Nota: non è MAI necessario e non è considerato buona pratica aggiungere
    // un metodo asincrono che fa il wrap della versione sincrona.
    //
    // La buona pratica vuole che il codice legacy venga implementato in modo
    // asincrono, ed eventualmente che la versione sincrona chiami quella
    // asincrona.
    //
    // Ove ciò non fosse possibile, è consigliato lasciare che sia il chiamante
    // a passare il metodo sincrono a Task.Run() solo se lo ritiene necessario.

    // Nota 2: il cancellation token non è in grado di cancellare codice
    // sincrono, il massimo che può fare è non lanciare l'operazione se rileva
    // che la richiesta è già stata cancellata quando questo codice parte.

    Task<string> task = Task.Run(SomeLegacyMethod, cancellationToken); // affido ad un thread che si blocchera' del codice sincrono per non bloccare il main thread

    return Task.Run(SomeLegacyMethod, cancellationToken);
}

string SomeLegacyMethod()
{
    // codice...
    return "test";
}



// È anche possibile eseguire codice asincrono in un contesto sincrono:
string GetPagesSequential()
{
    Task<string> result = GetPagesParallelAsync();
    return result.GetAwaiter().GetResult();
}



// Microsoft consiglia di usare async/await in ASP.NET Core, di non usare
// Task.Run(), Task.Wait() o Task.Result, e di non usare lock in codice
// condiviso.
// https://learn.microsoft.com/aspnet/core/fundamentals/best-practices#avoid-blocking-calls




// È possibile usare il cancellation token per cancellare un'operazione
// asincrona se il chiamante non è più interessato:
async Task ProcessNumbersAsync(int[] numbers, CancellationToken cancellationToken = default)
{
    foreach (int number in numbers)
    {
        // Lancia "OperationCancelledException" se richiesta la cancellazione.
        cancellationToken.ThrowIfCancellationRequested();

        Console.WriteLine($"Processing {number}...");

        // Simula operazione di lunga durata...
        await Task.Delay(1000, cancellationToken);
    }
}

int[] numbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Sorgente di cancellation token, che viene cancellato dopo 3 secondi.
using CancellationTokenSource cts = new();
cts.CancelAfter(TimeSpan.FromSeconds(3));

try
{
    await ProcessNumbersAsync(numbers, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation cancelled.");
}

// Nota: il cancellation token indica una "richiesta da parte del chiamante,
// di cancellare l'operazione in corso". Trattasi solo di una richiesta, il
// metodo chiamato non garantisce che la richiesta venga soddisfatta
// immediatamente, anzi, non garantisce affatto alcuna cancellazione.
//
// È buona pratica accettare un cancellation token in tutti i metodi async, e
// tenerne conto internamente per interrompere l'operazione in corso, ma il
// chiamante non può dare per scontato che il chiamato adotti questa pratica.