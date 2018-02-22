namespace Polkovnik.DroidInjector
{
    internal interface IInjectAttribute
    {
        int ResourceId { get; }
        bool CanBeNull { get; }
    }
}