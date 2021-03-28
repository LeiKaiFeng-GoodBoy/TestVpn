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
        public MainPageInfo(Action<IPEndPoint> action)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public Action<IPEndPoint> Action { get; }
    }


    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageInfo info)
        {
            InitializeComponent();

            m_start.Clicked += (obj, e) => info.Action(IPEndPoint());
        }


        IPEndPoint IPEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(m_ip.Text), int.Parse(m_port.Text));
        }
    }
}
