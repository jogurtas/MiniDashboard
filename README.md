<p align="center">
  <img src="./icon.png" width="128" height="128" />
</p>

<h1 align="center">Mini Dashboard</h1>

<h4 align="center">
  <a href="https://github.com/jogurtas/MiniDashboard#-documentation">Documentation</a> |
  <a href="https://i.imgur.com/WpgB3vt.png">UI Screenshot</a> | 
  <a href="https://www.nuget.org/packages/MiniDashboard/">NuGet</a>
</h4>

<p align="center">
  <a href="https://github.com/jogurtas/MiniDashboard/actions"><img src="https://github.com/jogurtas/MiniDashboard/workflows/Deploy/badge.svg" alt="Github Workflow Status"></a>
  <a href="https://www.nuget.org/packages/MiniDashboard/"><img src="https://img.shields.io/nuget/v/MiniDashboard" alt="NuGet"></a>
  <a href="https://github.com/jogurtas/MiniDashboard/blob/master/LICENSE"><img src="https://img.shields.io/badge/license-MIT-informational" alt="License"></a>
</p>

<p align="center">⚡ Zero configuration dashboard for .NET</p>


## Table of Contents

- [🔧 Installation](#-installation)
- [🚀 Getting Started](#-getting-started)
- [📖 Documentation](#-documentation)
  - [Cards](#cards)
  - [Charts](#charts)
  - [Tables](#tables)
  - [Routes](#routes)
  - [Authentication](#authentication)


## 🔧 Installation

Using the [.NET Core command-line interface (CLI) tools](https://docs.microsoft.com/en-us/dotnet/core/tools/):

```bash
$ dotnet add package MiniDashboard
```

or with the [Package Manager Console](https://docs.microsoft.com/en-us/nuget/tools/package-manager-console):

```bash
$ Install-Package MiniDashboard
```

## 🚀 Getting Started

Add `MiniDashMiddleware` & `MiniDashUiMidlleware` to `Startup.cs`

```c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // ...
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // ...

        app.UseEndpoints(endpoints => 
        { 
            endpoints.MapControllers();
            endpoints.MapMiniDashboard();
        });

        app.UseMiniDashboardUi();
    }
}
```

That's it! Dashboard should be accessible at http://localhost:5000/dash/index.html

## 📖 Documentation

It is possible to add custom [Cards](https://i.imgur.com/WpgB3vt.png), [Charts](https://i.imgur.com/WpgB3vt.png) and [Tables](https://i.imgur.com/WpgB3vt.png) to dashboard via C# code.

### Cards

```c#
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseEndpoints(endpoints => 
    { 
        endpoints.MapControllers();
        endpoints.MapMiniDashboard(cfg => 
        {
            cfg
                .EnrichWithStaticCard(title: "Branch", value: "master", data: "Some data (optional)")
                // Data will be dynamically evaluated on dashboard refresh or on card click
                .EnrichWithDynamicCard(title: "Dynamic", value: "Number", onUpdate: () => new Random().Next(1000).ToString());
        });
    });

    app.UseMiniDashboardUi();
}
```

### Charts

```c#
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ...

    app.UseEndpoints(endpoints => 
    { 
        endpoints.MapControllers();
        endpoints.MapMiniDashboard(cfg => 
        {
            cfg.EnrichWithChart(title: "Weather", type: ChartType.Line, WeatherChart.SampleWeatherData);
        });
    });

    app.UseMiniDashboardUi();
}

// WeatherChart.cs
public class WeatherChart : IChartData // Chart data point (must implment IChartData)
{
    public IConvertible Label { get; init; } // Data point label
    public float Temperature { get; init; } // Each property is separate chart dataset
    public int Wind { get; init; }
    public int Precipitation { get; init; }

    public WeatherChart(IConvertible label, float temperature, int wind, int precipitation)
    {
        Label = label;
        Temperature = temperature;
        Wind = wind;
        Precipitation = precipitation;
    }

    public static List<IChartData> SampleWeatherData()
    {
        return new()
        {
            new WeatherChart("MON", 6.1f, 7, 25); 
            new WeatherChart("TUE", 7.3f, 2, 19); 
            new WeatherChart("WED", 6.5f, 6, 1); 
            new WeatherChart("THU", 3.0f, 2, 0); 
            new WeatherChart("FRI", 4.4f, 9, 2); 
            new WeatherChart("SAT", 5.6f, 6, 3); 
            new WeatherChart("SUN", 3.2f, 3, 0); 
        };
    }
}
```

### Tables

```c#
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ...

    app.UseEndpoints(endpoints => 
    { 
        endpoints.MapControllers();
        endpoints.MapMiniDashboard(cfg => 
        {
            cfg.EnrichWithTable(title: "Weather", WeatherChart.SampleWeatherData);
        });
    });

    app.UseMiniDashboardUi();
}

// WeatherTable.cs
public class WeatherTable : ITableRowData // Chart data point (must implment IChartData)
{
    public IConvertible RowId { get; init; } // Unique row ID
    public float Temperature { get; init; } // Each property represents table row
    public int Wind { get; init; }
    public int Precipitation { get; init; }

    public WeatherChart(IConvertible rowId, float temperature, int wind, int precipitation)
    {
        RowId = rowId;
        Temperature = temperature;
        Wind = wind;
        Precipitation = precipitation;
    }

    public static List<IChartData> SampleWeatherData()
    {
        return new()
        {
            new WeatherChart("MON", 6.1f, 7, 25); 
            new WeatherChart("TUE", 7.3f, 2, 19); 
            new WeatherChart("WED", 6.5f, 6, 1); 
            new WeatherChart("THU", 3.0f, 2, 0); 
            new WeatherChart("FRI", 4.4f, 9, 2); 
            new WeatherChart("SAT", 5.6f, 6, 3); 
            new WeatherChart("SUN", 3.2f, 3, 0); 
        };
    }
}
```

### Routes

Default routes:
  - `/dash/data` - Data
  - `/dash` - UI

```c#
// Customize routes
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ...

    app.UseEndpoints(endpoints => 
    { 
        endpoints.MapControllers();
        endpoints.MapMiniDashboard(cfg => 
        {
            cfg.Route = "/data"; // Data
        });
    });

    app.UseMiniDashboardUi(cfg =>
    {
        cfg.Route = "/dashboard"; // UI
    });
}
```

### Authentication

Currently `MiniDashboard` supports [Basic authentication](https://tools.ietf.org/html/rfc7617).

```json
// appsettings.json
{
    // ...
    "DashAuthSecrets": {
        "username": "admin",
        "password": "secure_password"
    }
}
```

```c#
// Load credentials
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ...

    app.UseEndpoints(endpoints => 
    { 
        endpoints.MapControllers();
        endpoints.MapMiniDashboard(cfg => 
        {
            cfg.BasicAuthUserName = Configuration.GetValue<string>("DashAuthSecrets:username");
            cfg.BasicAuthPassword = Configuration.GetValue<string>("DashAuthSecrets:password");
        });
    });

    app.UseMiniDashboardUi();
}
```