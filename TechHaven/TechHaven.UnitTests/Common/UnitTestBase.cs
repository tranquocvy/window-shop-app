
//Class cơ sở chứa Mock Setup
using AutoMapper;
//Thêm vào để lấp đầy khoảng trống ở hàm constructor MapperConfiguration
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Interfaces;
using TechHaven.Application.Mappings; // Namespace chứa MappingProfile của bạn
using TechHaven.Domain.Interfaces;

namespace TechHaven.UnitTests.Common;
[ExcludeFromCodeCoverage] //Đây chỉ là Mock Base - không cần test

public abstract class UnitTestBase
{
    protected readonly Mock<IUnitOfWork> MockUow;

    // Mock các Repository con
    protected readonly Mock<IOrderRepository> MockOrderRepo;
    protected readonly Mock<IProductRepository> MockProductRepo;
    protected readonly Mock<ICustomerRepository> MockCustomerRepo;
    protected readonly Mock<IUserRepository> MockUserRepo;


    // Auth & Common Services Mocks
    protected readonly Mock<IPasswordHasher> MockHasher;
    protected readonly Mock<IOtpService> MockOtpService;
    protected readonly Mock<IEmailService> MockEmailService;
    protected readonly Mock<IJwtTokenService> MockJwtTokenService;
    // External Auth Services Mocks
    protected readonly Mock<IExternalAuthService> MockExternalAuthService;
    protected readonly Mock<IStringEncryptionHelper> MockEncryptionHelper;

    protected readonly IMapper Mapper;
    protected readonly CancellationToken CancellationToken;

    protected UnitTestBase()
    {
        MockOrderRepo = new Mock<IOrderRepository>();
        MockProductRepo = new Mock<IProductRepository>();
        MockCustomerRepo = new Mock<ICustomerRepository>();
        MockUserRepo = new Mock<IUserRepository>();
        // Init Auth & Common Services Mocks
        MockHasher = new Mock<IPasswordHasher>();
        MockOtpService = new Mock<IOtpService>();
        MockEmailService = new Mock<IEmailService>();
        MockJwtTokenService = new Mock<IJwtTokenService>();
        // Init External Auth Services Mocks
        MockExternalAuthService = new Mock<IExternalAuthService>();
        MockEncryptionHelper = new Mock<IStringEncryptionHelper>();

        // 1. Setup Mock Unit of Work
        MockUow = new Mock<IUnitOfWork>();

        // Setup để khi gọi uow.Orders thì trả về Mock tương ứng
        MockUow.Setup(u => u.Orders).Returns(MockOrderRepo.Object);
        MockUow.Setup(u => u.Products).Returns(MockProductRepo.Object);
        MockUow.Setup(u => u.Customers).Returns(MockCustomerRepo.Object);
        MockUow.Setup(u => u.Users).Returns(MockUserRepo.Object);

        // 2. Setup Real AutoMapper (nên dùng mapper thật thay vì mock)
        var configuration = new MapperConfiguration(cfg =>
        {
            // Thay vì AddProfile<MappingProfile>() (không tồn tại),
            // ta dùng AddMaps để quét toàn bộ Assembly chứa class AppSettingProfile.
            // Nó sẽ tự động load OrderProfile, UserProfile, v.v.
            cfg.AddMaps(typeof(AppSettingProfile).Assembly);
        }, NullLoggerFactory.Instance); //Tạo null Logger để tránh lỗi null trong constructor của MapperConfiguration 

        Mapper = configuration.CreateMapper();

        CancellationToken = CancellationToken.None;
    }

    // Helper: Mock một Repository cụ thể khi cần
    protected void SetupRepo<TRepo>(Func<IUnitOfWork, TRepo> repoSelector, TRepo mockRepo) where TRepo : class
    {
        MockUow.Setup(u => repoSelector(u)).Returns(mockRepo);
    }
}