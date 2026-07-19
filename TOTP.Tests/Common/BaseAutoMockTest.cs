using Moq;
using Moq.AutoMock;

namespace TOTP.Tests.Common;

public abstract class BaseAutoMockTest
{
    protected BaseAutoMockTest()
    {
        AutoMocker = new AutoMocker(MockBehavior.Loose);
    }

    protected AutoMocker AutoMocker { get; }

    protected Mock<T> FreezeMock<T>() where T : class => AutoMocker.GetMock<T>();

    protected T CreateWithAutoMocker<T>() where T : class => AutoMocker.CreateInstance<T>();

    protected Mock<T> GetMockFromAutoMocker<T>() where T : class => AutoMocker.GetMock<T>();
}
