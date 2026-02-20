namespace markit.Application.Models.Authentication.Demo
{
    public record DemoStatus(
        bool Available,
        bool LimitReached,
        string Message
    ) {}
}
