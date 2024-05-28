# Hexa.Protobuf

`Hexa.Protobuf` is a powerful inline code generator for Protobuf, designed to work via Roslyn analyzers. It enables seamless generation and integration of Protobuf serialization code directly in your .NET projects, enhancing build-time type checking and reducing runtime errors. The library targets `netstandard2.0`, ensuring broad compatibility across different .NET implementations.

## Features

- **Inline Code Generation:** Generates Protobuf serialization code directly within your codebase without the need for external scripts or tools.
- **Build-Time Analysis:** Leverages Roslyn analyzers to provide immediate feedback on potential serialization issues at compile time.
- **Easy Integration:** Works out of the box with existing .NET projects using Protobuf, with minimal setup required.
- **Customizable:** Offers options to customize code generation to fit different project needs and constraints.

## Getting Started

### Prerequisites

Ensure you have the following installed:
- .NET SDK supporting `netstandard2.0` (e.g., .NET Core 2.0 or later, .NET Framework 4.6.1 or later)
- An IDE that supports Roslyn analyzers (e.g., Visual Studio, Visual Studio Code with OmniSharp)

### Installation

To install `Hexa.Protobuf`, add the NuGet package to your project:

```bash
dotnet add package Hexa.Protobuf
