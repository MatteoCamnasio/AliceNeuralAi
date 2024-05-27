using HttpProxyControl;
using Microsoft.CognitiveServices.Speech;

namespace AliceNeural.Utils
{
    public partial class AzureCognitiveServicesResourceManager
    {
        static partial void SetSpeechConfigProxy(SpeechConfig speechConfig)
        {
            ProxyParams? proxyParams = HttpProxyHelper.GetHttpClientProxyParams();
            if (proxyParams != null)
            {
                speechConfig.SetProxy(proxyParams.Value.ProxyAddress, proxyParams.Value.ProxyPort);
            }
        }
        partial void GetNewProxyParams()
        {
            ProxyParams? proxyParams = HttpProxyHelper.GetHttpClientProxyParams();
            _newProxyParams = proxyParams;
        }
    }
}
