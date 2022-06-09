using System;
using System.IO;
using System.Runtime.InteropServices;

#if !TURN_OFF_ELMAH
using ElmahCore.Mvc;
using ElmahCore.Sql;
#endif

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OCC.HSM.Analysis;
using OCC.HSM.EPC;
using OCC.HSM.Model.Interfaces;
using OCC.HSM.Persistence;
using OCC.HSM.Services;

namespace OCC.HSM.UI
{
    /// <summary>
    /// The class used by the host builder to initialise the application with dependency
    /// injection and the setting up of the request handling pipeline.
    /// </summary>
    public class Startup
	{
		/// <summary>
		/// The path relative to the application root where images are located.
		/// </summary>
		public const string IMG_PATH = "img";

		/// <summary>
		/// A copy of the configuration which has been loaded by the runtime.
		/// </summary>
		private readonly IConfiguration configuration_;

		/// <summary>
		/// A copy of the hosting environment passed to the constructor to be used when
		/// configuring the services.
		/// </summary>
		public IWebHostEnvironment Env { get; set; }

		/// <summary>
		/// Grabs a copy of the <see cref="IConfiguration"/> passed by ASP.NET
		/// </summary>
		/// <param name="config">The application configuration contents.</param>
		/// <param name="env">The hosting environment.</param>
		public Startup(IConfiguration config, IWebHostEnvironment env)
		{
			configuration_ = config;
			Env = env;
		}

		/// <summary>
		/// Add the required services to the DI mechanism.
		/// </summary>
		/// <param name="services">Collects the available services.</param>
		public void ConfigureServices(IServiceCollection services)
		{
			IMvcBuilder builder = services.AddRazorPages();
			builder.AddRazorPagesOptions(options => options.Conventions.AddPageRoute("/PostCode", ""));

			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential 
				// cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				// requires using Microsoft.AspNetCore.Http;
				options.MinimumSameSitePolicy = SameSiteMode.None;
				options.Secure = CookieSecurePolicy.Always;
			});

			services.AddSingleton<IEohMemoryCache>(x =>
			{
				// manually construct EohContext early, so that its data can be cached

				//Sqlite db file directory
				string path = Path.Combine(Env.ContentRootPath, "data");

				var dbOptions = new DbContextOptionsBuilder<EohContext>();
				// give it the connection string from config?
				dbOptions.UseSqlite(configuration_.GetConnectionString("SqliteDatabase").Replace("[DataDirectory]", path));

				using var db = new EohContext(dbOptions.Options);
				var cache = new EohMemoryCache();
				cache.LoadEohTableFromDb(db);
				return cache;
			});
			
			services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

#if DEBUG
			if (Env.IsDevelopment())
				builder.AddRazorRuntimeCompilation();			

			services.AddControllersWithViews().AddRazorRuntimeCompilation();
#endif			

			// Enable session state.
			services.AddSession(opts =>
			{
				opts.IdleTimeout = TimeSpan.FromDays(1D);
				opts.Cookie.Name = "HSMSession";				
				opts.Cookie.IsEssential = true;
			});
			services.AddMemoryCache();

			services.AddMvc(option => option.EnableEndpointRouting = false);

			var appSettings = configuration_.GetSection("AppConfigSettings");
#if !TURN_OFF_ELMAH
			services.AddElmah<SqlErrorLog>(opts => {
				opts.ConnectionString = appSettings["ElmahDatabaseConnectionString"];
				opts.Path = "esmah";
			});
#endif
			services.AddSingleton<IApplicationConfiguration>(LoadConfiguration(Env.WebRootPath));
			services.AddSingleton<IEPCService>(new EPCService(
				(string)appSettings.GetValue(typeof(String), "EPCServiceUrl"),
				(string)appSettings.GetValue(typeof(String), "EPCServiceEmail"),
				(string)appSettings.GetValue(typeof(String), "EPCServiceKey")));
			services.AddSingleton<ILogger, Logger>();
						
			services.AddTransient<IAnalysisService, OCC.HSM.AnalysisService.AnalysisService>();

            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
			services.AddSession(opts => opts.IdleTimeout = TimeSpan.FromMinutes(30));
		}

		/// <summary>
		/// Add the required middle-ware components.
		/// </summary>
		/// <param name="app">The application builder to which components are added.</param>
		/// <param name="env">The hosting environment, useful for further configuration.</param>
		/// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance used
		/// to access the injected <see cref="ILogger"/></param>
		public static void Configure(IApplicationBuilder app, IWebHostEnvironment env,
			IServiceProvider serviceProvider)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseStatusCodePagesWithRedirects("/ServiceError?code={0}");
				app.UseHsts();
			}						

			// TODO: which of these is necessary?
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();

			app.UseSession();
			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller}/{action=Index}/{id?}"
					);
			});

			app.UseRouting();

			app.UseAuthorization();


			app.Use(next => context => {
				context.Request.EnableBuffering();
				return next(context);
			});

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
			});

#if !TURN_OFF_ELMAH
			app.UseElmah();
#endif
			if (!(serviceProvider is null))
			{
				var logger = serviceProvider.GetService(typeof(ILogger)) as ILogger;
				logger.Information($"HSM started on {RuntimeInformation.FrameworkDescription}," +
					$" {RuntimeInformation.OSDescription}" +
					$" ({RuntimeInformation.OSArchitecture})");
			}
		}

		/// <summary>
		/// Create an instance of the <see cref="ApplicationConfiguration"/> using what got
		/// loaded from the "AppConfigSettings" settings section of the configuration.
		/// </summary>
		/// <param name="rootPath">The root path for the application.</param>
		/// <returns>A new instance of the configuration.</returns>
		private ApplicationConfiguration LoadConfiguration(string rootPath)
		{
			var appSettings = configuration_.GetSection("AppConfigSettings");

			var appConfig = new ApplicationConfiguration(
				rootDir: rootPath.Trim(),
				logDir: (string)appSettings.GetValue(typeof(String), "LogDirectory", String.Empty),
				loggingLevel: (string)appSettings.GetValue(typeof(String), "LogLevel", "Warning"),
				questionsXmlFile: (string)appSettings.GetValue(typeof(String), "QuestionsXML"));

			LoadIllustrations(appConfig.Questions, appConfig.RootDirectory);

			return appConfig;
		}

		/// <summary>
		/// Determines if there are illustrations to be displayed on each answer choice or if there is a common illustration for all the answers.
		/// If a single image needs to be used for all the answer choices then the individual images needs to be removed, otherwise priority will be given to the individual ones.
		/// </summary>
		/// <param name="questions">The set of questions loaded from the configuration.</param>
		/// <param name="rootDirectory">The root directory of the application.</param>
		static void LoadIllustrations(IQuestionCollection questions, string rootDirectory)
		{
			foreach (var question in questions)
			{
				bool imageNotFound = false;

				// If all answers have a corresponding png file then images can be drawn against the options 
				var hasImages = false;

				foreach (var answer in question.AnswerChoices)
				{
					var imgPath = Path.Combine(rootDirectory, IMG_PATH, $"{answer.Key}.png");

					if (File.Exists(imgPath))
					{
						hasImages = true;
					}
					else
					{
						imageNotFound = true;
					}
				}

				if (imageNotFound)
				{
					hasImages = false;

					// If NOT all answers have a corresponding png file then a common single image can be drawn against the options 
					// The single image file name has to be using the question.Key value
					var imgPath = Path.Combine(rootDirectory, IMG_PATH, $"{question.Key}.png");

					if (File.Exists(imgPath))
					{
						question.SetSingleImage(question.Key); 
					}
				}

				question.SetHasImages(hasImages);
			}
		}
	}
}
