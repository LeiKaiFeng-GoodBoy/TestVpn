using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Net;
using Android.Content;
using System.Net.Sockets;
using System.Net;
using Xamarin.Essentials;
using Android.Support.V4.App;
using System.Threading.Tasks;
using Java.IO;
using System.IO;
using System.Threading;
using System.Text;
using AndroidX.Core.App;

namespace TestVpn.Droid
{

    


    [Service(Permission = "android.permission.BIND_VPN_SERVICE")]
    [IntentFilter(actions: new string[] { VpnService.ServiceInterface })]
    public sealed class MyVpnService : VpnService
    {
        public static IPEndPoint IPEndPoint { get; set; }

        private static readonly object s_lock = new object();

        public static void Log(object e)
        {
            string s = System.Environment.NewLine;

            lock (s_lock)
            {
                System.IO.File.AppendAllText($"/storage/emulated/0/textvpn.txt", $"{s}{s}{s}{s}{DateTime.Now}{s}{e}", System.Text.Encoding.UTF8);
            }         
        }


        void Init()
        {
            const string CHNNEL_ID = "456784343";
            const string CHNNEL_NAME = "545765554";

            const int ID = 3435;

            ServerHelper.CreateNotificationChannel(this, CHNNEL_ID, CHNNEL_NAME);


            var func = ServerHelper.CreateServerNotificationFunc("title", this, CHNNEL_ID);

            this.StartForeground(ID, func("run"));
        }

        public override void OnCreate()
        {
            Init();


            var handle = new VpnService.Builder(this)
                .AddAddress("192.168.2.2", 24)
                .AddRoute("0.0.0.0", 0)
                .AddAllowedApplication("com.android.chrome")
                .AddAllowedApplication("com.companyname.yande.re")
                .AddAllowedApplication("com.companyname.testvpn")
                .Establish();

            var inputStream = new ParcelFileDescriptor.AutoCloseInputStream(handle);

            var oustream = new ParcelFileDescriptor.AutoCloseOutputStream(handle);


            try
            {

                Socket socket = new Socket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, ProtocolType.Udp);

                
                this.Protect(socket.Handle.ToInt32());

                socket.Bind(new IPEndPoint(IPAddress.Any, IPEndPoint.Port));

                socket.Connect(IPEndPoint);



                Copy(oustream, socket)
                    .ContinueWith((t) =>
                    {
                        if (t.Exception != null)
                        {
                            Log(t.Exception);
                        }
                    });

                Copy(inputStream, socket)
                    .ContinueWith((t) =>
                    {
                        if (t.Exception != null)
                        {
                            Log(t.Exception);
                        }
                    });
            }
            catch(Exception e)
            {
                MyVpnService.Log(e);
            }

        }


        public static void Catch(Action action)
        {
            while (true)
            {

                try
                {
                    action();
                    return;
                }
                catch (SocketException e)
                {
                    Thread.Sleep(100);
                }
            }
        }

        static Task Copy(OutputStream outputStream, Socket socket)
        {
            return Task.Run(() =>
            {
                byte[] buffer = new byte[75536];

                while (true)
                {

                    int n = 0;

                    Catch(() =>
                    {
                        n = socket.Receive(buffer);
                    });

                    outputStream.Write(buffer, 0, n);
                }
            });



        }

        static Task Copy(InputStream inputStream, Socket socket)
        {
            return Task.Run(() =>
            {
                byte[] buffer = new byte[75536];

                while (true)
                {
                    int n = inputStream.Read(buffer, 0, buffer.Length);


                    if (n == 0)
                    {
                        Thread.Sleep(100);
                    }
                    else
                    {
                        Catch(() => socket.Send(buffer, 0, n, SocketFlags.None));
                    }
                }
            });
        }
    }

    public static class ServerHelper
    {
        public static void CreateNotificationChannel(ContextWrapper context, string channelID, string channelName)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                ((NotificationManager)context.GetSystemService(Context.NotificationService))
                            .CreateNotificationChannel(new NotificationChannel(channelID, channelName, NotificationImportance.Max) { LockscreenVisibility = NotificationVisibility.Public });
            }
        }


        public static void StartServer(ContextWrapper context, Intent intent)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                context.StartForegroundService(intent);
            }
            else
            {
                context.StartService(intent);
            }
        }

        public static Action<string> CreateUpServerNotificationFunc(ContextWrapper context, int notificationID, Func<string, Notification> func)
        {
            return (contentText) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ((NotificationManager)context.GetSystemService(Context.NotificationService))
                            .Notify(notificationID, func(contentText));
                });



            };


        }


        public static Func<string, Notification> CreateServerNotificationFunc(string contentTitle, Context context, string channelID)
        {
            return (contentText) =>
            {
                return new NotificationCompat.Builder(context, channelID)
                               .SetContentTitle(contentTitle)
                               .SetContentText(contentText)
                               .SetSmallIcon(Resource.Mipmap.icon)
                               .SetOngoing(true)
                               .Build();
            };
        }
    }



    [Activity(Label = "TestVpn", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            Xamarin.Essentials.Permissions.RequestAsync<Permissions.StorageWrite>();

            LoadApplication(new App(new MainPageInfo(CreateVpn)));
        }

        void CreateVpn(IPEndPoint endPoint)
        {
            MyVpnService.IPEndPoint = endPoint;

            Intent inter = VpnService.Prepare(this);

            
            if (inter is null)
            {
                //null已经有权限

                this.OnActivityResult(0, Result.Ok, null);
            }
            else
            {
                this.StartActivityForResult(inter, 0);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            if (resultCode == Result.Ok)
            {

                ServerHelper.StartServer(this, new Intent(this, typeof(MyVpnService)));
            }

        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}