// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: StreamExChange.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace GrpcService1 {
  /// <summary>
  ///定义服务
  /// </summary>
  public static partial class StreamExChange
  {
    static readonly string __ServiceName = "StreamExChange.StreamExChange";

    static readonly grpc::Marshaller<global::GrpcService1.StreamReqData> __Marshaller_StreamExChange_StreamReqData = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::GrpcService1.StreamReqData.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::GrpcService1.StreamResData> __Marshaller_StreamExChange_StreamResData = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::GrpcService1.StreamResData.Parser.ParseFrom);

    static readonly grpc::Method<global::GrpcService1.StreamReqData, global::GrpcService1.StreamResData> __Method_GetStream = new grpc::Method<global::GrpcService1.StreamReqData, global::GrpcService1.StreamResData>(
        grpc::MethodType.ServerStreaming,
        __ServiceName,
        "GetStream",
        __Marshaller_StreamExChange_StreamReqData,
        __Marshaller_StreamExChange_StreamResData);

    static readonly grpc::Method<global::GrpcService1.StreamReqData, global::GrpcService1.StreamResData> __Method_PutStream = new grpc::Method<global::GrpcService1.StreamReqData, global::GrpcService1.StreamResData>(
        grpc::MethodType.ClientStreaming,
        __ServiceName,
        "PutStream",
        __Marshaller_StreamExChange_StreamReqData,
        __Marshaller_StreamExChange_StreamResData);

    static readonly grpc::Method<global::GrpcService1.StreamReqData, global::GrpcService1.StreamResData> __Method_AllStream = new grpc::Method<global::GrpcService1.StreamReqData, global::GrpcService1.StreamResData>(
        grpc::MethodType.DuplexStreaming,
        __ServiceName,
        "AllStream",
        __Marshaller_StreamExChange_StreamReqData,
        __Marshaller_StreamExChange_StreamResData);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::GrpcService1.StreamExChangeReflection.Descriptor.Services[0]; }
    }

    /// <summary>Client for StreamExChange</summary>
    public partial class StreamExChangeClient : grpc::ClientBase<StreamExChangeClient>
    {
      /// <summary>Creates a new client for StreamExChange</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public StreamExChangeClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for StreamExChange that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public StreamExChangeClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected StreamExChangeClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected StreamExChangeClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      /// <summary>
      ///
      ///以下 分别是 服务端 推送流， 客户端 推送流 ，双向流。
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncServerStreamingCall<global::GrpcService1.StreamResData> GetStream(global::GrpcService1.StreamReqData request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetStream(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///
      ///以下 分别是 服务端 推送流， 客户端 推送流 ，双向流。
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncServerStreamingCall<global::GrpcService1.StreamResData> GetStream(global::GrpcService1.StreamReqData request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncServerStreamingCall(__Method_GetStream, null, options, request);
      }
      public virtual grpc::AsyncClientStreamingCall<global::GrpcService1.StreamReqData, global::GrpcService1.StreamResData> PutStream(grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return PutStream(new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncClientStreamingCall<global::GrpcService1.StreamReqData, global::GrpcService1.StreamResData> PutStream(grpc::CallOptions options)
      {
        return CallInvoker.AsyncClientStreamingCall(__Method_PutStream, null, options);
      }
      public virtual grpc::AsyncDuplexStreamingCall<global::GrpcService1.StreamReqData, global::GrpcService1.StreamResData> AllStream(grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return AllStream(new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncDuplexStreamingCall<global::GrpcService1.StreamReqData, global::GrpcService1.StreamResData> AllStream(grpc::CallOptions options)
      {
        return CallInvoker.AsyncDuplexStreamingCall(__Method_AllStream, null, options);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override StreamExChangeClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new StreamExChangeClient(configuration);
      }
    }

  }
}
#endregion
