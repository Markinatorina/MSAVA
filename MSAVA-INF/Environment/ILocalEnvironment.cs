using M_SAVA_INF.Models;
using System;
using System.Collections.Generic;

namespace M_SAVA_INF.Environment
{
    public interface ILocalEnvironment
    {
        LocalEnvironmentValues Values { get; }
        byte[] GetSigningKeyBytes();
    }
}
