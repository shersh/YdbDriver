using System.CodeDom.Compiler;
using Grpc.Core;

namespace Ydb.Topic.V1;

public static partial class TopicService
{
    [field: GeneratedCode("grpc_csharp_plugin", null)]
    public static Method<CreateTopicRequest, CreateTopicResponse> CreateTopicMethod { get; } =
        new Method<CreateTopicRequest, CreateTopicResponse>(
            MethodType.Unary,
            __ServiceName,
            "CreateTopic",
            __Marshaller_Ydb_Topic_CreateTopicRequest,
            __Marshaller_Ydb_Topic_CreateTopicResponse);
}