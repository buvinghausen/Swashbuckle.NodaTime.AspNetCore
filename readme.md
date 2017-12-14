# Swashbuckle.NodaTime.AspNetCore

*Easily configure [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) to generate correct documentation for [NodaTime](https://github.com/nodatime/nodatime) types.*

NodaTime is an alternative date and time API for .NET which is often used to replace built in types for handling date and time. It can be easily configured to work nicely with ASP.NET Core MVC using [NodaTime.Serialization.JsonNet](https://www.nuget.org/packages/NodaTime.Serialization.JsonNet) package.

Swashbuckle.AspNetCore is a library to seamlesly add swagger generation and UI to ASP.NET Core MVC projects.

The problem is that by default swagger generated by Swashbuckle.AspNetCore doesn't show NodaTime types nicely as can be seen on the following picture:

<img src="https://raw.githubusercontent.com/buvinghausen/Swashbuckle.NodaTime.AspNetCore/master/images/not-enabled.png" width="700">

Swashbuckle.NodaTime.AspNetCore configures Swashbuckle.AspNetCore to show NodaTime types as they will be really deserialized:

<img src="https://raw.githubusercontent.com/buvinghausen/Swashbuckle.NodaTime.AspNetCore/master/images/enabled.png" width="700">

## Installation

Install from NuGet: https://www.nuget.org/packages/Swashbuckle.NodaTime.AspNetCore.

Run the following command in the Package Manager Console:

```
Install-Package Swashbuckle.NodaTime.AspNetCore
```

## Usage

Call `ConfigureForNodaTime` method on swagger configuration when setting up swagger using EnableSwagger method.

```csharp
public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		// This example is using JSON.NETs default settings function with some sample overrides
		// You may also pass the settings object directly into the ConfigureForNodaTime function
		JsonConvert.DefaultSettings = () =>
		{
			var settings = new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				Converters =
				{
					new StringEnumConverter()
				},
				NullValueHandling = NullValueHandling.Ignore
			};
			settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
			return settings;
		}

		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new Info
			{
				Title = "My NodaTime & ASP.NET Core API",
				Version = "v1"
			});
			c.ConfigureForNodaTime();
		});
	}
}
```
