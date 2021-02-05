using System;
using Unity.Entities;
using UnityEngine;
using kcp2k;

namespace DOTSNET.kcp2k
{
    [ClientWorld]
    [AlwaysUpdateSystem]
    // use SelectiveSystemAuthoring to create it selectively
    [DisableAutoCreation]
    public class KcpTransportClientSystem : TransportClientSystem
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

        // kcp
        internal KcpClient client;

        // all except WebGL
        public override bool Available() =>
            Application.platform != RuntimePlatform.WebGLPlayer;

        // MTU
        public override int GetMaxPacketSize() => Kcp.MTU_DEF;

        public override bool IsConnected() => client.connected;

        public override void Connect(string hostname)
        {
            client.Connect(hostname, Port, NoDelay, Interval, FastResend, CongestionWindow, SendWindowSize, ReceiveWindowSize);
        }

        public override bool Send(ArraySegment<byte> segment, Channel channel)
        {
            client.Send(segment);
            return true;
        }

        public override void Disconnect()
        {
            client?.Disconnect();
        }

        // statistics
        public int GetSendQueue() =>
            client.connection.kcp.snd_queue.Count;
        public int GetReceiveQueue() =>
            client.connection.kcp.rcv_queue.Count;
        public int GetSendBuffer() =>
            client.connection.kcp.snd_buf.Count;
        public int GetReceiveBuffer() =>
            client.connection.kcp.rcv_buf.Count;

        // ECS /////////////////////////////////////////////////////////////////
        protected override void OnStartRunning()
        {
            client = new KcpClient(
                OnConnected,
                OnData,
                OnDisconnected
            );
            Debug.Log("KCP client created");
        }

        protected override void OnUpdate()
        {
            client.Tick();
        }

        protected override void OnStopRunning()
        {
            client = null;
        }
    }
}