using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WOL.Data;
using WOL.Data.Repositories;
using WOL.Data.Repositories.Interface;
using WOL.Services;
using WOL.Services.Interface;
using WOL.ViewModels;
using System.Windows;
using WOL.Helpers;
using WOL.Helpers.Interface;
using System;

namespace WOL
{
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            ServiceCollection serviceCollection = new();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 데이터베이스가 생성되었는지 확인
            var contextFactory = ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using (var dbContext = contextFactory.CreateDbContext())
            {
                dbContext.Database.EnsureCreated();
            }

            MainWindow mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            // 클라이언트로부터 핑 수신 시작
            IWakeOnLanService wakeOnLanService = ServiceProvider.GetRequiredService<IWakeOnLanService>();
            wakeOnLanService.StartHeartbeatListener();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            // 클라이언트로부터 핑 수신 중지
            IWakeOnLanService wakeOnLanService = ServiceProvider.GetRequiredService<IWakeOnLanService>();
            wakeOnLanService.StopHeartbeatListener();

            if (ServiceProvider is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else
            {
                ServiceProvider?.Dispose();
            }

            base.OnExit(e);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            string connectionString = "Server=localhost;Database=wol;Uid=root;Pwd=str123;";
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            );

            // 서비스 등록
            services.AddSingleton<IWakeOnLanService, WakeOnLanService>();
            services.AddTransient<IDataService, DataService>();
            services.AddTransient<IDeviceService, DeviceService>();
            services.AddSingleton<IIniService, IniService>();
            services.AddSingleton<IRemoteExplorerService, RemoteExplorerService>();
            services.AddTransient<IProgramService, ProgramService>();
            services.AddTransient<IProgramStatusService, ProgramStatusService>();
            services.AddSingleton<IUdpService, UdpService>();

            // TCP Client
            services.AddScoped<ITcpJsonClient, TcpJsonClient>();

            // Repository 등록
            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<IDeviceRepository, DeviceRepository>();
            services.AddTransient<IProgramRepository, ProgramRepository>();

            // --- ViewModel 등록 ---
            services.AddTransient<MainViewModel>();
            services.AddTransient<DeviceViewModel>();
            services.AddTransient<ProgramViewModel>();
            services.AddTransient<NewProjectViewModel>();
            services.AddTransient<NewDeviceViewModel>();
            services.AddTransient<RemoteExplorerViewModel>();

            // --- View 등록 ---
            services.AddTransient<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });
        }
    }
}
