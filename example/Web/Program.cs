using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Swashbuckle.NodaTime.AspNetCore.Web.Filters;

JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
	ContractResolver = new CamelCasePropertyNamesContractResolver(),
	Converters = { new StringEnumConverter() },
	NullValueHandling = NullValueHandling.Ignore,
	Formatting = Formatting.Indented
}.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

var builder = WebApplication.CreateBuilder(args);
builder.Services
	.AddSwaggerGen(o =>
	{
		o.SwaggerDoc("v1",
			new OpenApiInfo
			{
				Title = builder.Environment.ApplicationName,
				Version = "v1"
			});
		o.OperationFilter<OperationFilter>();
		o.ConfigureForNodaTime(); //Note: you can pass JsonSerializerSettings in directly here I just leverage JsonConvert.DefaultSettings which is what gets called when no settings are passed
	})
	.AddControllers()
	.AddNewtonsoftJson(options =>
	{
		options.AllowInputFormatterExceptionMessages = true;
		options.SerializerSettings.ContractResolver =
			new CamelCasePropertyNamesContractResolver();
		options.SerializerSettings.Converters =
			new List<JsonConverter> { new StringEnumConverter() };
		options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
		options.SerializerSettings.Formatting = Formatting.Indented;
		options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
	});
var app = builder.Build();
_ = app
	.UseStaticFiles()
	.UseSwagger(o => o.PreSerializeFilters.Add((apiDoc, httpReq) => apiDoc.Servers = new List<OpenApiServer> { new() { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" } }))
	.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v1/swagger.json", builder.Environment.ApplicationName))
	.UseAuthentication()
	.UseRouting()
	.UseAuthorization();
_ = app.MapControllers();

// Run the app
var cts = new CancellationTokenSource();
#if DEBUG // Only really need to process ctrl+c in debug mode
var sigintReceived = false;
Console.CancelKeyPress += (_, e) =>
{
	sigintReceived = true;
	cts.Cancel();
	e.Cancel = true;
};
#endif
AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
#if DEBUG
	if (sigintReceived) return;
#endif
	cts.Cancel();
};

await app
	.RunAsync(cts.Token)
	.ConfigureAwait(false);
