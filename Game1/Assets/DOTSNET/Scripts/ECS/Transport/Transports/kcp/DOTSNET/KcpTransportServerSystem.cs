using System;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using kcp2k;

namespace DOTSNET.kcp2k
{
    [ServerWorld]
    [AlwaysUpdateSystem]
    // use SelectiveSystemAuthoring to create it selectively
    [DisableAutoCreation]
    public class KcpTransportServerSystem : TransportServerSystem
    {
        // configuration
        public ushort Port = 7777;
        public bool NoDelay = true;
        public uint Interval = 10;

        // advanced configuration
        public int FastResend = 0;
        public bool CongestionWindow = true; // KCP 'NoCongestionWindow' is false by default. here we negate it for ease of use.
        public uint SendWindowSize = 128; //Kcp.WND_SND; 32 by default. 128 is better for 4k Benchmark etc.
        public uint ReceiveWindowSize = Kcp.WND_RCV;

        // kcp server
        internal KcpServer server;

        // all except WebGL
        public override bool Available() =>
            Application.platform != RuntimePlatform.WebGLPlayer;

        // MTU
        public override int GetMaxPacketSize() => Kcp.MTU_DEF;

        public override bool IsActive() => server.IsActive();

        public override void Start()
        {
            server.Start(Port);
        }

        // note: DOTSNET already packs messages. Transports don't need to.
        public override bool Send(int connectionId, ArraySegment<byte> segment, Channel channel)
        {
            server.Send(connectionId, segment);
            return true;
        }

        public override bool Disconnect(int connectionId)
        {
            server.Disconnect(connectionId);
            return true;
        }

        public override string GetAddress(int connectionId)
        {
            return server.GetClientAddress(connectionId);
        }

        public override void Stop()
        {
            server?.Stop();
        }

        // statistics
        public int GetTotalSendQueue() =>
            server.connections.Values.Sum(conn => conn.kcp.snd_queue.Count);
        public int GetTotalReceiveQueue() =>
            server.connections.Values.Sum(conn => conn.kcp.rcv_queue.Count);
        public int GetTotalSendBuffer() =>
            server.connections.Values.Sum(conn => conn.kcp.snd_buf.Count);
        public int GetTotalReceiveBuffer() =>
            server.connections.Values.Sum(conn => conn.kcp.rcv_buf.Count);

        // ECS /////////////////////////////////////////////////////////////////
        protected override void OnStartRunning()
        {
            server = new KcpServer(
                OnConnected,
                OnData,
                OnDisconnected,
                NoDelay,
                Interval,
                FastResend,
                CongestionWindow,
                SendWindowSize,
                ReceiveWindowSize
            );
            Debug.Log("KCP server created");
        }

        protected override void OnUpdate()
        {
            server.Tick();
        }

        protected override void OnStopRunning()
        {
            server = null;
        }
    }
}