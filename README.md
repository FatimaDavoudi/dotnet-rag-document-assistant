RagDemo — RAG Document Assistant with C# and .NET

A hands-on AI engineering project that demonstrates how to build a Retrieval-Augmented Generation (RAG) application using C#, .NET, and locally hosted language models with Ollama.

The application reads a technical support document, splits it into smaller chunks, generates embeddings, retrieves relevant information using semantic search, and uses a local LLM to answer questions based on the retrieved context.

Features

Document ingestion: Reads technical documentation from a local text file.

Text chunking: Splits the document into smaller sections for retrieval.

Embeddings: Generates vector embeddings using Ollama and the all-minilm model.

Semantic search: Uses cosine similarity to find relevant document chunks.

Configurable retrieval: Supports a relevance threshold and a configurable Top-K value.

Context-based generation: Uses qwen2.5:3b to generate answers from retrieved context.

Insufficient-context handling: Avoids calling the LLM when no relevant chunks are found and instructs the model to acknowledge insufficient information.

Technology Stack

C#

.NET

Ollama

all-minilm:latest — embedding model

qwen2.5:3b — language model

System.Net.Http

System.Text.Json

How It Works

Load the support guide from a text file.

Split the document into chunks.

Generate and store an embedding for each chunk.

Convert the user's question into an embedding.

Calculate cosine similarity and retrieve the most relevant chunks.

Send the question and retrieved context to the LLM.

Display the generated answer.

Example Questions

Which technology helps access a database?

Where is relational data stored?

What is the difference between authentication and authorization?

What does the support guide say about database connection timeouts?

If no relevant chunks are found, the application reports that no relevant information was retrieved.

Prerequisites

A compatible .NET SDK

Ollama

The required local models

Pull the models:

ollama pull all-minilm
ollama pull qwen2.5:3b

Make sure Ollama is running before starting the application.

Run Locally

Clone the repository and navigate to the project directory.

dotnet restore
dotnet run

Ensure the support guide is available at the path expected by the application.

Project Goals

This project was built to gain practical experience with:

Integrating local LLMs into .NET applications

Embeddings and semantic search

Retrieval-Augmented Generation

Prompt design and context-grounded responses

Retrieval parameters and their impact on answer quality

Current Limitations

The current document ingestion supports plain-text files.

The application uses a simple chunking strategy tailored to the support guide's structure.

Retrieval quality depends on the embedding model, chunking strategy, and relevance threshold.

Prompt instructions can reduce hallucinations but cannot guarantee that every answer is correct.

The project uses a small local document collection and is not yet designed for production deployment.

Future Improvements

Automated tests and retrieval evaluation

Source references in generated answers

An ASP.NET Core Web API

Tool calling for controlled access to application data

Improved logging and error handling

Author

Developed as a practical AI engineering portfolio project using C#/.NET and locally hosted LLMs.