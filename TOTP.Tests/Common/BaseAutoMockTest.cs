using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using Moq.AutoMock;

namespace TOTP.Tests.Common;

public abstract class BaseAutoMockTest
{
    protected BaseAutoMockTest()
    {
        Fixture = new Fixture();
        Fixture.Customize(new AutoMoqCustomization
        {
            ConfigureMembers = true,
            GenerateDelegates = true
        });

        AutoMocker = new AutoMocker(MockBehavior.Loose);
    }

    protected IFixture Fixture { get; }

    protected AutoMocker AutoMocker { get; }

    protected T CreateWithFixture<T>() where T : class => Fixture.Create<T>();

    protected Mock<T> FreezeMock<T>() where T : class => Fixture.Freeze<Mock<T>>();

    protected T CreateWithAutoMocker<T>() where T : class => AutoMocker.CreateInstance<T>();

    protected Mock<T> GetMockFromAutoMocker<T>() where T : class => AutoMocker.GetMock<T>();
}
