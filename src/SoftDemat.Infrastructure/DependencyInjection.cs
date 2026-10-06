using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Context;
using SoftDemat.Infrastructure.Mail;
using SoftDemat.Infrastructure.Options;
using SoftDemat.Infrastructure.Repositories;
using SoftDemat.Infrastructure.Security;
using SoftDemat.Infrastructure.Services;
using SoftDemat.Infrastructure.Storage;

namespace SoftDemat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<LegacyEncryptionOptions>(configuration.GetSection(LegacyEncryptionOptions.SectionName));
        services.Configure<PayslipStorageOptions>(configuration.GetSection(PayslipStorageOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

        services.AddDbContext<SdtDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("Sdt")));
        services.AddDbContext<SageDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("Sage")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthSessionRepository, AuthSessionRepository>();
        services.AddScoped<IUserSecurityRepository, UserSecurityRepository>();
        services.AddScoped<IMailTemplateRepository, MailTemplateRepository>();
        services.AddScoped<IGeneralParameterRepository, GeneralParameterRepository>();
        services.AddScoped<IMailSenderSettingRepository, MailSenderSettingRepository>();
        services.AddScoped<ISageConnectionRepository, SageConnectionRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEstablishmentRepository, EstablishmentRepository>();
        services.AddScoped<IDispatchRepository, DispatchRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<AuthStores>();
        services.AddScoped<AuthCrypto>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ILegacyPasswordProtector, LegacyPasswordProtector>();
        services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();
        services.AddSingleton<PayslipStorageLocation>();
        services.AddSingleton<PayslipUploadStore>();
        services.AddSingleton<IPayslipDirectory, PayslipDirectory>();
        services.AddSingleton<IPayslipArchiver, PayslipArchiver>();
        services.AddSingleton<IMailSender, SmtpMailSender>();
        services.AddSingleton<ILocalMailbox, OutlookMailbox>();
        services.AddSingleton<IArchiveFolderPicker, WindowsArchiveFolderPicker>();
        services.AddScoped<IPayslipMailer, PayslipMailer>();
        services.AddSingleton<ISageConnectionTester, SageConnectionTester>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGeneralParameterService, GeneralParameterService>();
        services.AddScoped<IWorkstationToolService, WorkstationToolService>();
        services.AddScoped<IMailTemplateService, MailTemplateService>();
        services.AddScoped<ISageConnectionService, SageConnectionService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IEstablishmentService, EstablishmentService>();
        services.AddScoped<IPayslipFileService, PayslipFileService>();
        services.AddScoped<IPayslipUploadService, PayslipUploadService>();
        services.AddScoped<IDispatchService, DispatchService>();
        return services;
    }
}
