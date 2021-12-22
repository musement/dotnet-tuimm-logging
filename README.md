[![.github/workflows/ci.yml](https://github.com/musement/dotnet-tuimm-logging/actions/workflows/ci.yml/badge.svg)](https://github.com/musement/dotnet-tuimm-logging/actions/workflows/ci.yml) [![Nuget package](https://img.shields.io/nuget/vpre/Musement.Extensions.Logging.svg)](https://www.nuget.org/packages/Musement.Extensions.Logging)

## Overview

This repository contains a Serilog log formatter and a convenience extension
method to set it up. The formatter is an implementation of TUI Musement's
standard logging specifications, the convenience method automatically sets
Serilog up as an ILogger implementation and uses the formatter when not in
Development mode (so to have human-readable logs at dev-time).

Using it is as simple as:

```csharp
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder()
    .UseMusementLogging();
```

or (when using Minimal APIs in .NET 6 and `WebApplication`)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseMusementLogging();
```

Logs will be written to stdout unless the `LOG_PATH` environment variable is
set, in that case logs will be written to the file specified in the variable.
