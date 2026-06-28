# Dotnet AI Ollama Chat Demo

A personal playground project demonstrating local AI chat, tool/function calling, and vector database search in .NET 10. The project integrates Microsoft.Extensions.AI, Ollama, and Microsoft Semantic Kernel's in-memory vector store.

## Features

- Local LLM Integration: Chat client configured to run with a local Ollama instance using the Gemma 4 model (or other local models like Qwen 2.5).
- Function Calling (Tools): The assistant can dynamically call local tools to fetch real-time information.
  - Weather: Retrieves current weather and air quality for any city using Open-Meteo APIs.
  - Movie Recommendations: Performs a semantic vector search over a local collection of movies.
- In-Memory Vector Store: Seeds a collection of movies into Microsoft Semantic Kernel's `InMemoryVectorStore`, generating embeddings locally via `nomic-embed-text` and searching them using cosine similarity.
- Interactive Streaming: Interactive console loop with streaming output and message history management (remembers the last 20 messages).

## Prerequisites

To run this project, you need the following installed:

1. .NET 10.0 SDK
2. Ollama running locally on port 11434

Pull the required models in Ollama:

```bash
# Pull the LLM for chat and function calling
ollama pull gemma4:latest

# Pull the embedding model for vector search
ollama pull nomic-embed-text
```

## Running the Application

1. Make sure your local Ollama service is active.
2. Navigate to the project folder and run:
   ```bash
   dotnet run --project VectorDataSearch
   ```
3. Talk to the assistant in the command line interface. Try asking questions like:
   - "How is the weather in Kolkata?"
   - "Recommend me a movie about space travel or wormholes."
   - Type "exit" to quit.

## Project Structure

- VectorDataSearch/Program.cs: Application entry point. Configures Ollama clients, builds the IChatClient with function invocation support, seeds the in-memory vector database, and hosts the console chat loop.
- VectorDataSearch/Movie.cs: Movie data model decorated with Semantic Kernel vector store attributes, along with a seed dataset of 40 movies.
- VectorDataSearch/WeatherService.cs: Core service responsible for calling the Open-Meteo geocoding, forecast, and air quality REST APIs.
- VectorDataSearch/WeatherModel.cs: DTOs and data models representing Open-Meteo responses.

## Key Dependencies

- Microsoft.Extensions.AI: Standardized abstractions for chat and embedding generation.
- Microsoft.SemanticKernel.Connectors.InMemory: Provides the in-memory vector database functionality.
- OllamaSharp: Client library to interface with the local Ollama instance.
