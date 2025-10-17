using MSAVA_INF.Models;
using System;
using System.Collections.Generic;

namespace MSAVA_INF.Environment
{
    public interface ILocalEnvironment
    {
        LocalEnvironmentValues Values { get; }
        byte[] GetSigningKeyBytes();
    }
}
