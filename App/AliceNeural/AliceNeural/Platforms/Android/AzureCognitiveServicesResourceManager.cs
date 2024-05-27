using HttpProxyControl;
using Microsoft.CognitiveServices.Speech;

namespace AliceNeural.Utils
{
    public partial class AzureCognitiveServicesResourceManager
    {
         static partial void SetSpeechConfigProxy(SpeechConfig speechConfig)
        {
            Android.Content.Context androidContext = Microsoft.Maui.ApplicationModel.Platform.AppContext;
            ProxyParams? androidProxyParams = AndroidProxyHelper.GetProxyParams(androidContext);
            if (androidProxyParams != null)
            {
                speechConfig.SetProxy(androidProxyParams.Value.ProxyAddress, androidProxyParams.Value.ProxyPort);
            }
        }

        partial void GetNewProxyParams()
        {
            Android.Content.Context androidContext = Microsoft.Maui.ApplicationModel.Platform.AppContext;
            ProxyParams? androidProxyParams = AndroidProxyHelper.GetProxyParams(androidContext);
            _newProxyParams = androidProxyParams;

        }
    }
}
