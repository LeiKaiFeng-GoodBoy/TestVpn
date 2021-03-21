using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TestVpn
{
    public sealed class MainPageInfo
    {
        public MainPageInfo(Action<IPEndPoint> action, Action<IPEndPoint> startUdp)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            StartUdp = startUdp ?? throw new ArgumentNullException(nameof(startUdp));
        }

        public Action<IPEndPoint> Action { get; }


        public Action<IPEndPoint> StartUdp { get; }
    }


    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageInfo info)
        {
            InitializeComponent();

            m_start.Clicked += (obj, e) => info.Action(IPEndPoint());

            m_startudp.Clicked += (obj, e) => info.StartUdp(IPEndPoint());
        }


        IPEndPoint IPEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(m_ip.Text), int.Parse(m_port.Text));
        }
    }
}
