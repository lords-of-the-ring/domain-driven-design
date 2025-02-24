using NSubstitute;

namespace Testing.Infrastructure;

public static class NSubstituteExtensions
{
    public static T GetFirstArgument<T>(this object obj)
    {
        return (T)obj.ReceivedCalls().Single().GetArguments()[0]!;
    }
}
