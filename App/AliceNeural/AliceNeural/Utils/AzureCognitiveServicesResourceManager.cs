using HttpProxyControl;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Intent;

namespace AliceNeural.Utils
{
    public partial class AzureCognitiveServicesResourceManager
    {
        AzureSpeechServiceStore azureSpeechServiceStore=null!;
        AzureIntentRecognitionByCLUStore azureIntentRecognitionByCLUStore=null!;
        SpeechConfig _currentSpeechConfig;
        ConversationalLanguageUnderstandingModel _currentCluModel;
        LanguageUnderstandingModelCollection _currentPatternMatchingModelCollection;
        string _cluProjectName = string.Empty;
        string _cluDeploymentName = string.Empty;

        ProxyParams? _proxyParams;
        ProxyParams? _newProxyParams;
        public SpeechConfig CurrentSpeechConfig { get => _currentSpeechConfig; set => _currentSpeechConfig = value; }
        public ConversationalLanguageUnderstandingModel CurrentCluModel { get => _currentCluModel; set => _currentCluModel = value; }
        public string CluProjectName { get => _cluProjectName; set => _cluProjectName = value; }
        public string CluDeploymentName { get => _cluDeploymentName; set => _cluDeploymentName = value; }
        public LanguageUnderstandingModelCollection CurrentPatternMatchingModel { get => _currentPatternMatchingModelCollection; set => _currentPatternMatchingModelCollection = value; }
        public ProxyParams? ProxyParams { get => _proxyParams; set => _proxyParams = value; }

        public AzureCognitiveServicesResourceManager(string cluProjectName, string cluDeploymentName)
        {
            CluProjectName = cluProjectName;
            CluDeploymentName = cluDeploymentName;
            azureSpeechServiceStore = StoreManager.GetSpeechDataFromStore();
            azureIntentRecognitionByCLUStore = StoreManager.GetCLUDataFromStore();
            _currentCluModel = CreateConversationalLanguageUnderstandingModel(
                        azureIntentRecognitionByCLUStore.KeyValue,
                        azureIntentRecognitionByCLUStore.LanguageEndpoint,
                        CluProjectName,
                        CluDeploymentName);
            _currentSpeechConfig = CreateSpeechConfig(
                        azureSpeechServiceStore.APIKeyValue,
                        azureSpeechServiceStore.LocationRegion);
            _currentPatternMatchingModelCollection = CreatePatternMatchingModelCollection();
            GetNewProxyParams();//aggiorna _newProxyParams
            _proxyParams=_newProxyParams;//mantengo allineati i due valori alla partenza
        }

        public bool ShouldRecreateSpeechConfigForProxyChange(string speechRecognitionLanguage = "it-IT")
        {
            //legge i nuovi valori del proxy
            GetNewProxyParams();
            //verifica se i parametri del proxy sono cambiati
            if ((_newProxyParams is null && ProxyParams is not null)||
                (_newProxyParams is not null && ProxyParams is null)||
                (_newProxyParams is not null && ProxyParams is not null) &&
                ((_newProxyParams.Value.ProxyAddress!=ProxyParams.Value.ProxyAddress)||
                (_newProxyParams.Value.ProxyPort != ProxyParams.Value.ProxyPort)))
            {
                //i parametri del proxy sono cambiati e _currentSpeechConfig va ricreato
                _currentSpeechConfig = CreateSpeechConfig(
                       azureSpeechServiceStore.APIKeyValue,
                       azureSpeechServiceStore.LocationRegion,
                       speechRecognitionLanguage);
                //dopo aver ricreato il nuovo speechConfig tengo allineati i due ProxyParams
                ProxyParams = _newProxyParams;
                return true;
            }
           return false;
        }
        public static SpeechConfig CreateSpeechConfig(
            string azureSpeechServiceKey,
            string azureSpeechServiceRegion,
            string speechRecognitionLanguage = "it-IT")
        {
            var speechConfig = SpeechConfig.FromSubscription(azureSpeechServiceKey, azureSpeechServiceRegion);
            speechConfig.SpeechRecognitionLanguage = speechRecognitionLanguage;

            //usiamo le classi parziali per implementare versioni differenti del metodo  ConfigureSpeechConfigProxy
            //https://learn.microsoft.com/en-us/dotnet/maui/user-interface/handlers/customize#partial-classes
            //la versione di SetSpeechConfigProxy che verrà usata dipende dal tipo dal target framework di compilazione
            SetSpeechConfigProxy(speechConfig);
            //l'esempio commentato sotto è la versione che utilizza la compilazione condizionale che è in alternativa alla scrittura
            //delle classi parziali
            //https://learn.microsoft.com/en-us/dotnet/maui/user-interface/handlers/customize#conditional-compilation
            //#if ANDROID
            //            Android.Content.Context androidContext = Microsoft.Maui.ApplicationModel.Platform.AppContext;
            //            AndroidProxyParams? androidProxyParams = AndroidProxyHelper.GetProxyParams(androidContext);
            //            if (androidProxyParams != null)
            //            {
            //                speechConfig.SetProxy(androidProxyParams.Value.ProxyAddress, androidProxyParams.Value.ProxyPort);
            //            }
            //#elif WINDOWS
            //            ProxyParams? proxyParams = HttpProxyHelper.GetHttpClientProxyParams();
            //            if (proxyParams != null)
            //            {
            //                speechConfig.SetProxy(proxyParams.Value.ProxyAddress, proxyParams.Value.ProxyPort);
            //            }
            //#endif
            return speechConfig;
        }

        public static ConversationalLanguageUnderstandingModel CreateConversationalLanguageUnderstandingModel(
            string azureCluLanguageServiceKey,
            string azureCluLanguageServiceEndpoint,
            string cluProjectName,
            string cluDeploymentName)
        {
            var cluModel = new ConversationalLanguageUnderstandingModel(
                azureCluLanguageServiceKey,
                azureCluLanguageServiceEndpoint,
                cluProjectName,
                cluDeploymentName);
            return cluModel;
        }

        public static LanguageUnderstandingModelCollection CreatePatternMatchingModelCollection()
        {
            //creazione del modello di Pattern Matching
            // Creates a Pattern Matching patternMatchingModel and adds specific intents from your patternMatchingModel. The
            // Id is used to identify this patternMatchingModel from others in the collection.
            var patternMatchingModel = new PatternMatchingModel("MyBasicPatternMatchingId");
            // Creates a pattern that uses groups of optional words.
            //https://learn.microsoft.com/en-us/azure/ai-services/speech-service/pattern-matching-overview
            //tra parentesi {} ci sono le Any Entity --> catturano qualsiasi cosa
            //https://learn.microsoft.com/en-us/azure/ai-services/speech-service/pattern-matching-overview#grouping-required-and-optional-items
            //tra parentesi [] ci sono le entity opzionali. Con il simbolo | si fa la OR. Ad esempio "[Go | Take me]" will match either "Go", "Take me", or "".
            //tra parentesi () ci sono le entity obbligatorie. Con il simbolo | si fa la OR
            var patternWithOptionalAndMandatoryWordsOk = "({startCommand}) [,] ([A|Da])lice";
            var patternWithOptionalAndMandatoryWordsStop = "[{anything3}] ([A|Da])lice [,] ({stopCommand})";
            var patternNone = "[{anything4}] Alice [{anything5}]";
            //intent recognition con custom entity pattern matching
            //https://learn.microsoft.com/en-us/azure/ai-services/speech-service/how-to-use-custom-entity-pattern-matching
            patternMatchingModel.Entities.Add(PatternMatchingEntity.CreateListEntity("startCommand", EntityMatchMode.Basic, "ok", "ehi", "ah", "dai", "vai"));
            patternMatchingModel.Entities.Add(PatternMatchingEntity.CreateListEntity("stopCommand", EntityMatchMode.Basic, "stop", "basta", "fermati"));
            //configurazione degli intent per il Pattern Matching
            //"ok Alice" ascolta per far partire una ricerca; "Alice stop" per fermare lo speech in corso
            //lo speech in corso è fermato sia quando viene riconosciuto l'intent di stop,
            //sia quando viene riconosciuta la frase "stop" nel testo pronunciato
            //Infatti, quando c'è il text to speech attivo, potrebbe succedere che il pattern matching non 
            //riesca a riconoscere l'intent di stop. In questo caso interviene la funzione di speech to text in loop
            //continuo che trascrive tutto quello che viene detto
            patternMatchingModel.Intents.Add(new PatternMatchingIntent("Ok", patternWithOptionalAndMandatoryWordsOk));
            patternMatchingModel.Intents.Add(new PatternMatchingIntent("Stop", patternWithOptionalAndMandatoryWordsStop));
            patternMatchingModel.Intents.Add(new PatternMatchingIntent("None", patternNone));
            var patternMatchingModelCollection = new LanguageUnderstandingModelCollection
                {
                    patternMatchingModel
                };
            return patternMatchingModelCollection;
        }
        //I metodi SetSpeechConfigProxy e  GetNewProxyParams sono implementati nelle classi parziali
        //AzureCognitiveServicesResourceManager per ogni piattaforma
        static partial void SetSpeechConfigProxy(SpeechConfig speechConfig);

        partial void GetNewProxyParams();
        
    }
}
