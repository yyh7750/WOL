using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WOL.Data;
using WOL.Data.Repositories;
using WOL.Data.Repositories.Interface;
using WOL.Services;
using WOL.Services.Interface;
using WOL.ViewModels;
using System.Windows;

namespace WOL
{
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            // 클라이언트로부터 핑 수신 시작
            var wakeOnLanService = ServiceProvider.GetRequiredService<IWakeOnLanService>();
            wakeOnLanService.StartHeartbeatListener();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 클라이언트로부터 핑 수신 중지
            var wakeOnLanService = ServiceProvider.GetRequiredService<IWakeOnLanService>();
            wakeOnLanService.StopHeartbeatListener();
            base.OnExit(e);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var connectionString = "Server=localhost;Database=wol;Uid=root;Pwd=str123;";
            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            );

            // 서비스 등록
            services.AddSingleton<IWakeOnLanService, WakeOnLanService>();
            services.AddSingleton<IDataService, DataService>();
            services.AddSingleton<IDeviceService, DeviceService>();
            services.AddSingleton<IIniService, IniService>();

            // Repository 등록
            services.AddSingleton<IProjectRepository, ProjectRepository>();
            services.AddSingleton<IDeviceRepository, DeviceRepository>();
            services.AddSingleton<IProgramRepository, ProgramRepository>();

            // --- ViewModel 등록 ---
            services.AddTransient<MainViewModel>();
            services.AddTransient<DeviceViewModel>();
            services.AddTransient<ProgramViewModel>();
            services.AddTransient<NewProjectViewModel>();
            services.AddTransient<NewDeviceViewModel>();

            // --- View 등록 ---
            services.AddTransient<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });
        }
    }
}
