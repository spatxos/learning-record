// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: LuCat.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace GrpcService1 {
  /// <summary>
  ///定义服务
  /// </summary>
  public static partial class LuCat
  {
    static readonly string __ServiceName = "LuCat.LuCat";

    static readonly grpc::Marshaller<global::GrpcService1.BathTheCatReq> __Marshaller_LuCat_BathTheCatReq = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::GrpcService1.BathTheCatReq.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::GrpcService1.BathTheCatResp> __Marshaller_LuCat_BathTheCatResp = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::GrpcService1.BathTheCatResp.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Google.Protobuf.WellKnownTypes.Empty> __Marshaller_google_protobuf_Empty = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.Empty.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::GrpcService1.CountCatResult> __Marshaller_LuCat_CountCatResult = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::GrpcService1.CountCatResult.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::GrpcService1.Heartbeat> __Marshaller_LuCat_Heartbeat = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::GrpcService1.Heartbeat.Parser.ParseFrom);

    static readonly grpc::Method<global::GrpcService1.BathTheCatReq, global::GrpcService1.BathTheCatResp> __Method_BathTheCat = new grpc::Method<global::GrpcService1.BathTheCatReq, global::GrpcService1.BathTheCatResp>(
        grpc::MethodType.DuplexStreaming,
        __ServiceName,
        "BathTheCat",
        __Marshaller_LuCat_BathTheCatReq,
        __Marshaller_LuCat_BathTheCatResp);

    static readonly grpc::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::GrpcService1.CountCatResult> __Method_Count = new grpc::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::GrpcService1.CountCatResult>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Count",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_LuCat_CountCatResult);

    static readonly grpc::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::GrpcService1.Heartbeat> __Method_Heartbeats = new grpc::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::GrpcService1.Heartbeat>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Heartbeats",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_LuCat_Heartbeat);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::GrpcService1.LuCatReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of LuCat</summary>
    [grpc::BindServiceMethod(typeof(LuCat), "BindService")]
    public abstract partial class LuCatBase
    {
      /// <summary>
      ///定义给猫洗澡双向流rpc
      /// </summary>
      /// <param name="requestStream">Used for reading requests from the client.</param>
      /// <param name="responseStream">Used for sending responses back to the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>A task indicating completion of the handler.</returns>
      public virtual global::System.Threading.Tasks.Task BathTheCat(grpc::IAsyncStreamReader<global::GrpcService1.BathTheCatReq> requestStream, grpc::IServerStreamWriter<global::GrpcService1.BathTheCatResp> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      ///定义统计猫数量简单rpc
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      public virtual global::System.Threading.Tasks.Task<global::GrpcService1.CountCatResult> Count(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::GrpcService1.Heartbeat> Heartbeats(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(LuCatBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_BathTheCat, serviceImpl.BathTheCat)
          .AddMethod(__Method_Count, serviceImpl.Count)
          .AddMethod(__Method_Heartbeats, serviceImpl.Heartbeats).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the  service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static void BindService(grpc::ServiceBinderBase serviceBinder, LuCatBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_BathTheCat, serviceImpl == null ? null : new grpc::DuplexStreamingServerMethod<global::GrpcService1.BathTheCatReq, global::GrpcService1.BathTheCatResp>(serviceImpl.BathTheCat));
      serviceBinder.AddMethod(__Method_Count, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Google.Protobuf.WellKnownTypes.Empty, global::GrpcService1.CountCatResult>(serviceImpl.Count));
      serviceBinder.AddMethod(__Method_Heartbeats, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Google.Protobuf.WellKnownTypes.Empty, global::GrpcService1.Heartbeat>(serviceImpl.Heartbeats));
    }

  }
}
#endregion
