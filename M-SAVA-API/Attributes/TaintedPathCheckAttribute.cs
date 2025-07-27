using System;

namespace M_SAVA_API.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class TaintedPathCheckAttribute : Attribute
    {
    }
}
