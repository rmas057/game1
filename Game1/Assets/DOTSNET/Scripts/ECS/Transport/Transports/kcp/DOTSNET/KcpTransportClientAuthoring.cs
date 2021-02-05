using System;
using UnityEngine;
using kcp2k;

namespace DOTSNET.kcp2k
{
    public class KcpTransportClientAuthoring : MonoBehaviour, SelectiveSystemAuthoring
    {
        // find NetworkClientSystem in ECS world
        KcpTransportClientSystem client =>
            Bootstrap.ClientWorld.GetExistingSystem<KcpTransportClientSystem>();

        // common
        [Header("Configuration")]
        public ushort Port = 7777;
        [Tooltip("NoDelay is recommended to reduce latency. This also scales better without buffers getting full.")]
        public bool NoDelay = true;
        [Tooltip("KCP internal update interval. 100ms is KCP default, but a lower interval is recommended to minimize latency and to scale to more networked entities.")]
        public uint Interval = 10;

        [Header("Advanced")]
        [Tooltip("KCP fastresend parameter. Faster resend for the cost of higher bandwidth.")]
        public int FastResend = 0;
        [Tooltip("KCP congestion window can be disabled. This is necessary to Mirror 10k Benchmark. Disable this for high scale games if connections get chocked regularly.")]
        public bool CongestionWindow = true; // KCP 'NoCongestionWindow' is false by default. here we negate it for ease of use.
        [Tooltip("KCP window size can be modified to support higher loads. For example, Mirror Benchmark requires 128 for 4k monsters, 256 for 10k monsters (if CongestionWindow is disabled.)")]
        public uint SendWindowSize = 128; //Kcp.WND_SND; 32 by default. 128 is better for 4k Benchmark etc.
        [Tooltip("KCP window size can be modified to support higher loads. For example, Mirror Benchmark requires 128 for 4k monsters, 256 for 10k monsters (if CongestionWindow is disabled.)")]
        public uint ReceiveWindowSize = Kcp.WND_RCV;

        // debugging
        [Header("Debug")]
        public bool debugGUI;

        // add to selectively created systems before Bootstrap is called
        public Type GetSystemType() => typeof(KcpTransportClientSystem);

        // apply configuration in awake
        void Awake()
        {
            client.Port = Port;
            client.NoDelay = NoDelay;
            client.Interval = Interval;
            client.FastResend = FastResend;
            client.CongestionWindow = CongestionWindow;
            client.SendWindowSize = SendWindowSize;
            client.ReceiveWindowSize = ReceiveWindowSize;
        }

        void OnGUI()
        {
            if (!debugGUI) return;

            GUILayout.BeginArea(new Rect(5, 300, 300, 300));

            if (client.IsConnected())
            {
                GUILayout.BeginVertical("Box");
                GUILayout.Label("CLIENT");
                GUILayout.Label("  SendQueue: " + client.GetSendQueue());
                GUILayout.Label("  ReceiveQueue: " + client.GetReceiveQueue());
                GUILayout.Label("  SendBuffer: " + client.GetSendBuffer());
                GUILayout.Label("  ReceiveBuffer: " + client.GetReceiveBuffer());
                GUILayout.EndVertical();
            }

            GUILayout.EndArea();
        }
    }
}