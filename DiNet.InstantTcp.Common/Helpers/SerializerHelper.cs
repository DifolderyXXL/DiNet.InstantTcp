using ASiNet.Data.Serialization;
using DiNet.InstantTcp.Core;

namespace DiNet.InstantTcp.Common.Helpers;
public static class SerializerHelper
{
    public static void RegModel<TType>()
    {
        BinarySerializer.SerializeContext.GetOrGenerate<TType>();
    }
    public static void RegInstantResponseFor<TType>()
    {
        BinarySerializer.SerializeContext.GetOrGenerate<InstantResponse<TType>>();
    }
}
