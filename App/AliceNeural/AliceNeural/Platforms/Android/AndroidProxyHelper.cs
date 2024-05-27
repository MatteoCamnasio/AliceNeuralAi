using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using HttpProxyControl;

namespace AliceNeural.Utils
{
    public class AndroidProxyHelper
    {
        public static ProxyParams? GetProxyParams(Context context)
        {
            //WifiManager serve se si vuole accedere ai parametri della connessione wifi - richiede, se usato, il permesso android.permission.ACCESS_WIFI_STATE
            //WifiManager wifiManager = (WifiManager)context.GetSystemService(Context.WifiService);
            ConnectivityManager? connectivityManager = (ConnectivityManager?)context.GetSystemService(Context.ConnectivityService);
            if (connectivityManager != null)
            {
                if (Android.OS.Build.VERSION.SdkInt > Android.OS.BuildVersionCodes.M)
                {
                    // Esegui il codice solo se la versione di Android è superiore a Marshmallow (versione 23)
                    ProxyInfo? proxyInfo = connectivityManager.DefaultProxy;
                    if (proxyInfo != null)
                    {
                        return new ProxyParams() { ProxyAddress = proxyInfo.Host, ProxyPort = proxyInfo.Port };
                    }
                }
            }
            return null;
        }
    }
}
