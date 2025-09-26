namespace Datalake.PrivateApi.Attributes;


[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class TransientAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ScopedAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SingletonAttribute : Attribute
{
}
