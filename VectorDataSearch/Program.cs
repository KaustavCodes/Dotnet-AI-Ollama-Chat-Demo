using System.Globalization;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OllamaSharp;
using VectorDataSearch;


//const string AiModel = "qwen2.5-coder:7b";
const string AiModel = "gemma4:e4b";

const string EmbeddingModel = "nomic-embed-text";

const string AiName = "Local AI (Ollama)";

const int MaxConversationHistory = 5;

// WeatherResult weatherResult = await WeatherService.GetWeatherAndAirQualityAsync("Kolkata");

// // Json Log Weather Result
// Console.WriteLine("Weather Result:");
// Console.WriteLine($"Location: {weatherResult.Location.Name}, {weatherResult.Location.Admin1}, {weatherResult.Location.Country}");
// Console.WriteLine($"Coordinates: {weatherResult.Location.Latitude}, {weatherResult.Location.Longitude}");
// Console.WriteLine($"Current Weather: {weatherResult.Weather.Current.Temperature}°C, {weatherResult.Weather.Current.Condition}, Feels Like: {weatherResult.Weather.Current.FeelsLike}°C, Humidity: {weatherResult.Weather.Current.Humidity}%, Wind Speed: {weatherResult.Weather.Current.WindSpeed} m/s");

// return;


Console.WriteLine($"Your local {AiName} assistant is ready to help you with movie recommendations and weather information");
Console.WriteLine($"I remember the last {MaxConversationHistory} messages in our conversation.");
Console.WriteLine("Type 'exit' to end the conversation.\n");

Console.WriteLine("Generating embeddings for movie descriptions and storing them in the vector store...");

Console.WriteLine("--------------------------------------------------------------------------------------------------------");


var ollamaClient = new OllamaApiClient("http://localhost:11434")
{
    // SelectedModel = "qwen2.5:7b"
    SelectedModel = AiModel
};

IChatClient chatClient = ((IChatClient)ollamaClient).AsBuilder()
    .UseFunctionInvocation()
    .Build();


IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = new OllamaApiClient(new Uri("http://localhost:11434"), EmbeddingModel);

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
    AIFunctionFactory.Create(GetTodaysDateTime, "get_todays_date_time", "Get today's date, time and day of the week"),
    AIFunctionFactory.Create(WeatherService.GetWeatherAndAirQualityAsync, "get_weather", "Get the current weather and air quality for a city."),
    AIFunctionFactory.Create(GetMovieRecommendations, "get_movie_recommendations", "Get movie recommendations based on a description"),
    AIFunctionFactory.Create(WebScrapperService.DuckDuckGoSearchAsync, "duckduckgo_search", "Search the web using DuckDuckGo and return the top results"),
    
    // Math Tools - Both are useful
    AIFunctionFactory.Create(CalculatorService.Calculate, "calculate", "Perform accurate mathematical calculations. Use this for ANY math, numbers, percentages, roots, trig, etc."),
    AIFunctionFactory.Create(PythonCodeExecutionService.ExecutePythonCode, "execute_python_code", "Execute safe Python code for complex calculations, logic, algorithms, or when calculator is not enough"),

    AIFunctionFactory.Create(FileService.ReadFile, "read_file", "Read the full content of a local text file"),
    AIFunctionFactory.Create(FileService.ListDirectory, "list_directory", "List files and folders in a directory"),
    AIFunctionFactory.Create(GetCurrentDirectoryPath, "get_current_directory_path", "Get the current working directory path"),
    AIFunctionFactory.Create(RememberService.AddToRemember, "remember", "Add a string to the assistant's memory"),
    AIFunctionFactory.Create(RememberService.GetAllRememberedItems, "get_remembered_items", "Retrieve all strings that the assistant has remembered. Use this for things where the user asked you to remember something and what was questions?"),
    AIFunctionFactory.Create(RememberService.ForgetItem, "forget_item", "Forget a specific string that the assistant has remembered"),
    AIFunctionFactory.Create(RememberService.ForgetAll, "forget_all", "Forget all strings that the assistant has remembered"),
};

// ==================== SYSTEM PROMPT ====================
var history = new List<ChatMessage>
{
    new (ChatRole.System,
    @"You are a helpful, friendly, and accurate assistant named Ollama AI.

You have access to powerful tools. **You MUST use tools for all mathematical calculations.**

### Tool Usage Rules (Follow Strictly):

- **Simple math** (basic arithmetic, percentages, square roots, simple formulas) → use `calculate`
- **Complex math**, quadratic equations, systems of equations, symbolic math, integrals, or when `calculate` fails → use `execute_python`
- Weather → `get_weather`
- Movie recommendations → `get_movie_recommendations`
- Current date & time → `get_todays_date_time`
- Remember things → `remember`
- Retrieve remembered items → `get_remembered_items`
- Read files → `read_file`
- List files → `list_directory`
- Web search → `duckduckgo_search`
- For delete remember item, first call GetAllRememberedItems and then call ForgetItem with the exact string of the item to be forgotten.

### Important Instructions for Math:
- Never solve math problems yourself.
- For quadratic equations, always prefer `execute_python` and write clean Python code using `import math` or `import sympy as sp` if needed.
- You can call multiple tools if necessary.

Be concise, natural, and helpful in your final response.")
};

// Embedding generation and vector store setup

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



string GetTodaysDateTime()
{
    Console.WriteLine($" [TOOL CALLED] Get Today's Date and Time");

    var now = DateTime.Now;

    // Get local timezone info
    var timeZone = TimeZoneInfo.Local;
    string tzName = timeZone.DisplayName;
    string tzAbbr = timeZone.StandardName;

    // If it's India, make it user-friendly
    if (tzAbbr.Contains("India") || tzName.Contains("Kolkata") || tzAbbr == "India Standard Time")
    {
        tzAbbr = "IST";
    }

    string dayName = now.ToString("dddd", CultureInfo.CurrentCulture);
    string fullDate = now.ToString("MMMM dd, yyyy", CultureInfo.CurrentCulture);
    string time = now.ToString("hh:mm tt", CultureInfo.CurrentCulture);

    return $"Today is {dayName}, {fullDate} at {time} ({tzAbbr} / {tzName}).";
}

string GetCurrentDirectoryPath()
{
    Console.WriteLine($" [TOOL CALLED] Get Current Directory Path");
    return Directory.GetCurrentDirectory();
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

    Console.Write($"{AiName}: ");
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

    if (history.Count > MaxConversationHistory) history.RemoveAt(0);
}