using System;
using System.IO;
using Amazon.CloudWatch.EMF.Model;

namespace Amazon.CloudWatch.EMF.Sink
{
    // public class AgentSink : ISink
    // {
    //     private static string _logGroupName;
    //     private static string _logStreamName;
    //     private static SocketClient _client;
    //
    //     public AgentSink(
    //         string logGroupName,
    //         string logStreamName,
    //         Endpoint endpoint,
    //         SocketClientFactory clientFactory) {
    //         _logGroupName = logGroupName;
    //         _logStreamName = logStreamName;
    //         _client = clientFactory.GetClient(endpoint);
    //     }
    //
    //     public void Accept(MetricsContext context) {
    //         if (!String.IsNullOrEmpty(_logGroupName)) {
    //             context.putMetadata("LogGroupName", _logGroupName);
    //         }
    //         
    //         if (!String.IsNullOrEmpty(logStreamName)) {
    //             context.putMetadata("LogStreamName", _logStreamName);
    //         }
    //         
    //         try {
    //             for (String event : context.serialize()) {
    //                 _client.SendMessage(event + "\n");
    //             }
    //         } catch (JsonProcessingException e) {
    //             log.error("Failed to serialize the metrics with the exception: ", e);
    //         }
    //     }
    // }
    //
    // public class Endpoint {
    //
    //     public static Endpoint DEFAULT_TCP_ENDPOINT =
    //     new Endpoint("127.0.0.1", 25888, Protocol.TCP);
    //
    //     private static string host;
    //
    //     private static int port;
    //
    //     private static Protocol protocol;
    //
    //     public static Endpoint fromURL(String endpoint) {
    //         Uri parsedURI = null;
    //
    //         try {
    //             parsedURI = new Uri(endpoint);
    //         } catch (URISyntaxException ex) {
    //             log.warn("Failed to parse the endpoint: {} ", endpoint);
    //             return DEFAULT_TCP_ENDPOINT;
    //         }
    //
    //         if (parsedURI.Host == null
    //             || parsedURI.Port < 0
    //             || parsedURI.Scheme == null) {
    //             return DEFAULT_TCP_ENDPOINT;
    //         }
    //
    //         Protocol protocol;
    //         try {
    //             protocol = Protocol.GetProtocol(parsedURI.Scheme);
    //         } catch (IllegalArgumentException e) {
    //             log.warn(
    //                 "Unsupported protocol: {}. Would use default endpoint: {}",
    //                 parsedURI.Scheme,
    //                 DEFAULT_TCP_ENDPOINT);
    //         
    //             return DEFAULT_TCP_ENDPOINT;
    //         }
    //
    //         return new Endpoint(parsedURI.Host, parsedURI.Port, protocol);
    //     }
    //
    //     public String toString() {
    //         return protocol.ToString().ToLower() + "://" + host + ":" + port;
    //     }
    // }
}