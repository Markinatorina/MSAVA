using System;

namespace MSAVA_API.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class TaintedPathCheckAttribute : Attribute
    {
    }
}
