using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OllamaSharp;
using VectorDataSearch;


// WeatherResult weatherResult = await WeatherService.GetWeatherAndAirQualityAsync("Kolkata");

// // Json Log Weather Result
// Console.WriteLine("Weather Result:");
// Console.WriteLine($"Location: {weatherResult.Location.Name}, {weatherResult.Location.Admin1}, {weatherResult.Location.Country}");
// Console.WriteLine($"Coordinates: {weatherResult.Location.Latitude}, {weatherResult.Location.Longitude}");
// Console.WriteLine($"Current Weather: {weatherResult.Weather.Current.Temperature}°C, {weatherResult.Weather.Current.Condition}, Feels Like: {weatherResult.Weather.Current.FeelsLike}°C, Humidity: {weatherResult.Weather.Current.Humidity}%, Wind Speed: {weatherResult.Weather.Current.WindSpeed} m/s");

// return;


Console.WriteLine("Your local Ollama AI assistant is ready to help you with movie recommendations and weather information");
Console.WriteLine("I remember the last 20 messages in our conversation.");
Console.WriteLine("Type 'exit' to end the conversation.\n");

Console.WriteLine("Generating embeddings for movie descriptions and storing them in the vector store...");

Console.WriteLine("--------------------------------------------------------------------------------------------------------");


var ollamaClient = new OllamaApiClient("http://localhost:11434")
{
    // SelectedModel = "qwen2.5:7b"
    SelectedModel = "gemma4:latest"
};

IChatClient chatClient = ((IChatClient)ollamaClient).AsBuilder()
    .UseFunctionInvocation()
    .Build();


IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = new OllamaApiClient(new Uri("http://localhost:11434"), "nomic-embed-text");

var vectorStore = new InMemoryVectorStore();

var moviesStore = vectorStore.GetCollection<int, Movie>("movies");
await moviesStore.EnsureCollectionExistsAsync();

foreach (var movie in MovieData.GetMovies())
{
    var embedding = await embeddingGenerator.GenerateVectorAsync(movie.Description);
    movie.DescriptionVector = embedding.ToArray();
    await moviesStore.UpsertAsync(movie);
}

var tools = new List<AITool>
{
    //AIFunctionFactory.Create(GetWeather, "get_weather", "Get the current weather for a city in India"),
    AIFunctionFactory.Create(WeatherService.GetWeatherAndAirQualityAsync, "get_weather", "Get the current weather and air quality for a city."),
    AIFunctionFactory.Create(GetMovieRecommendations, "get_movie_recommendations", "Get movie recommendations based on a description"),
};

var history = new List<ChatMessage>
{
    new (ChatRole.System,
        @"You are a helpful assistant with access to tools.
        Your name is Ollama AI (Gemma 4).
        When the user asks about weather or movie recommendations, you MUST use the appropriate tool.
        Do not guess or answer directly. Always call the tool.")
};

// Embedding generation and vector store setup

static string GetWeather(string city)
{
    Console.WriteLine($" [TOOL CALLED] Weather → {city}");
    return $"Current weather in {city} is 29°C with partly cloudy skies.";
}

async Task<string> GetMovieRecommendations(string query)
{
    Console.WriteLine($" [TOOL CALLED] Movie Recommendations → {query}");
    if (moviesStore is null)
    {
        return "Movie recommendations are currently unavailable.";
    }

    var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(query);
    var movieRecommendation = moviesStore.SearchAsync(queryEmbedding, top: 3);

    var recommendations = new List<string>();
    await foreach (var result in movieRecommendation)
    {
        recommendations.Add($"Movie: {result.Record.Title}, Score: {result.Score:F2}, Description: {result.Record.Description}");
    }

    return recommendations.Count > 0
        ? string.Join("\n", recommendations)
        : "I could not find movie recommendations for that query.";
}

// var query = "A movie about a hacker who discovers the truth about reality";
// var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(query);
// var searchResults = moviesStore.SearchAsync(queryEmbedding, top: 2);

// await foreach (var result in searchResults)
// {
//     Console.WriteLine($"Movie: {result.Record.Title}, Score: {result.Score}, Description: {result.Record.Description}");
// }


Console.WriteLine("Let's start the conversation! You can ask me about movie recommendations or the weather in any city.");

// Generate a chat screen here
while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(userInput) || userInput.ToLower() == "exit") break;

    history.Add(new ChatMessage(ChatRole.User, userInput));

    Console.Write("Ollama AI (Gemma 4): ");
    string fullResponse = string.Empty;

    var options = new ChatOptions
    {
        Tools = tools,
        ToolMode = ChatToolMode.Auto   // Important
    };

    await foreach (var update in chatClient.GetStreamingResponseAsync(history, options))
    {
        if (!string.IsNullOrEmpty(update.Text))
        {
            Console.Write(update.Text);
            fullResponse += update.Text;
        }
    }

    Console.WriteLine("\n");
    history.Add(new ChatMessage(ChatRole.Assistant, fullResponse));

    if (history.Count > 20) history.RemoveAt(0);
}