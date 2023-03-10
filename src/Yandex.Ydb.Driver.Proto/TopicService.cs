using Grpc.Core;

namespace Ydb.Topic.V1;

public static partial class TopicService
{
    public static Method<CreateTopicRequest, CreateTopicResponse> CreateTopicMethod => __Method_CreateTopic;
}