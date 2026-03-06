using FluentAssertions;
using FluentAssertions.Events;

namespace TOTP.Tests.Common;

public static class FluentAssertionsEventExtensions
{
    public static IMonitor<TSubject> MonitorEvents<TSubject>(this TSubject subject)
        where TSubject : class
        => subject.Monitor();
}
