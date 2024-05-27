using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Intent;
using AliceNeural.Utils;
using AliceNeural.Models;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Web;
using HttpProxyControl;
namespace AliceNeural
{

    public partial class MainPage : ContentPage
    {
        string bingKey = "Al9R--iN9109sHWX2LhuvoL_1nXSwVqA7oBSXGAc1HhcBI3nEO-zVxr_fBmD8Zxm"; 
        SpeechRecognizer? speechRecognizer;
        IntentRecognizer? intentRecognizerByPatternMatching;
        IntentRecognizer? intentRecognizerByCLU;
        SpeechSynthesizer? speechSynthesizer;
        TaskCompletionSourceManager<int>? taskCompletionSourceManager;
        AzureCognitiveServicesResourceManager? serviceManager;
        bool buttonToggle = false;
        Brush? buttonToggleColor;
         readonly HttpClient _client = HttpProxyHelper.CreateHttpClient(setProxy: true);

        private  readonly JsonSerializerOptions? jsonSerializationOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
        public MainPage()
        {
            InitializeComponent();
            serviceManager = new AzureCognitiveServicesResourceManager("MyResponseDemo", "Progetto");
            taskCompletionSourceManager = new TaskCompletionSourceManager<int>();
            (intentRecognizerByPatternMatching, speechSynthesizer, intentRecognizerByCLU) =
                ConfigureContinuousIntentPatternMatchingWithMicrophoneAsync(
                    serviceManager.CurrentSpeechConfig,
                    serviceManager.CurrentCluModel,
                    serviceManager.CurrentPatternMatchingModel,
                    taskCompletionSourceManager);
            speechRecognizer = new SpeechRecognizer(serviceManager.CurrentSpeechConfig);
        }
        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            if (speechSynthesizer != null)
            {
                await speechSynthesizer.StopSpeakingAsync();
                speechSynthesizer.Dispose();
            }

            if (intentRecognizerByPatternMatching != null)
            {
                await intentRecognizerByPatternMatching.StopContinuousRecognitionAsync();
                intentRecognizerByPatternMatching.Dispose();
            }

            if (intentRecognizerByCLU != null)
            {
                await intentRecognizerByCLU.StopContinuousRecognitionAsync();
                intentRecognizerByCLU.Dispose();
            }
        }
        
        private async void ContentPage_Loaded(object sender, EventArgs e)
        {
            await CheckAndRequestMicrophonePermission();
        }

        private async Task<PermissionStatus> CheckAndRequestMicrophonePermission()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (status == PermissionStatus.Granted)
            {
                return status;
            }
            if (Permissions.ShouldShowRationale<Permissions.Microphone>())
            {
                // Prompt the user with additional information as to why the permission is needed
                await DisplayAlert("Permission required", "Microphone permission is necessary", "OK");
            }
            status = await Permissions.RequestAsync<Permissions.Microphone>();
            return status;
        }

        private  async Task ContinuousIntentPatternMatchingWithMicrophoneAsync(
            IntentRecognizer intentRecognizer, TaskCompletionSourceManager<int> stopRecognition)
        {
            await intentRecognizer.StartContinuousRecognitionAsync();
            // Waits for completion. Use Task.WaitAny to keep the task rooted.
            Task.WaitAny(new[] { stopRecognition.TaskCompletionSource.Task });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="cluModel"></param>
        /// <param name="patternMatchingModelCollection"></param>
        /// <param name="stopRecognitionManager"></param>
        /// <returns>una tupla contentente nell'ordine un intent recognizer basato su Patter Matching, un sintetizzatore vocale e un intent recognizer basato su un modello di Conversational Language Understanding </returns>
        private  (IntentRecognizer, SpeechSynthesizer, IntentRecognizer) ConfigureContinuousIntentPatternMatchingWithMicrophoneAsync(
            SpeechConfig config,
            ConversationalLanguageUnderstandingModel cluModel,
            LanguageUnderstandingModelCollection patternMatchingModelCollection,
            TaskCompletionSourceManager<int> stopRecognitionManager)
        {
            //creazione di un intent recognizer basato su pattern matching
            var intentRecognizerByPatternMatching = new IntentRecognizer(config);
            intentRecognizerByPatternMatching.ApplyLanguageModels(patternMatchingModelCollection);

            //creazione di un intent recognizer basato su CLU
            var intentRecognizerByCLU = new IntentRecognizer(config);
            var modelsCollection = new LanguageUnderstandingModelCollection { cluModel };
            intentRecognizerByCLU.ApplyLanguageModels(modelsCollection);

            //creazione di un sitetizzatore vocale
            var synthesizer = new SpeechSynthesizer(config);

            //gestione eventi
            intentRecognizerByPatternMatching.Recognized += async (s, e) =>
            {
                switch (e.Result.Reason)
                {
                    case ResultReason.RecognizedSpeech:
                        Debug.WriteLine($"PATTERN MATCHING - RECOGNIZED SPEECH: Text= {e.Result.Text}");
                        break;
                    case ResultReason.RecognizedIntent:
                        {
                            Debug.WriteLine($"PATTERN MATCHING - RECOGNIZED INTENT: Text= {e.Result.Text}");
                            Debug.WriteLine($"       Intent Id= {e.Result.IntentId}.");
                            if (e.Result.IntentId == "Ok")
                            {
                                Debug.WriteLine("Stopping current speaking if any...");
                                await synthesizer.StopSpeakingAsync();
                                Debug.WriteLine("Stopping current intent recognition by CLU if any...");
                                await intentRecognizerByCLU.StopContinuousRecognitionAsync();
                                await HandleOkCommand(synthesizer, intentRecognizerByCLU).ConfigureAwait(false);
                            }
                            else if (e.Result.IntentId == "Stop")
                            {
                                Debug.WriteLine("Stopping current speaking...");
                                await synthesizer.StopSpeakingAsync();
                            }
                        }

                        break;
                    case ResultReason.NoMatch:
                        Debug.WriteLine($"NOMATCH: Speech could not be recognized.");
                        var noMatch = NoMatchDetails.FromResult(e.Result);
                        switch (noMatch.Reason)
                        {
                            case NoMatchReason.NotRecognized:
                                Debug.WriteLine($"PATTERN MATCHING - NOMATCH: Speech was detected, but not recognized.");
                                break;
                            case NoMatchReason.InitialSilenceTimeout:
                                Debug.WriteLine($"PATTERN MATCHING - NOMATCH: The start of the audio stream contains only silence, and the service timed out waiting for speech.");
                                break;
                            case NoMatchReason.InitialBabbleTimeout:
                                Debug.WriteLine($"PATTERN MATCHING - NOMATCH: The start of the audio stream contains only noise, and the service timed out waiting for speech.");
                                break;
                            case NoMatchReason.KeywordNotRecognized:
                                Debug.WriteLine($"PATTERN MATCHING - NOMATCH: Keyword not recognized");
                                break;
                        }
                        break;

                    default:
                        break;
                }
            };
            intentRecognizerByPatternMatching.Canceled += (s, e) =>
            {
                Debug.WriteLine($"PATTERN MATCHING - CANCELED: Reason={e.Reason}");

                if (e.Reason == CancellationReason.Error)
                {
                    Debug.WriteLine($"PATTERN MATCHING - CANCELED: ErrorCode={e.ErrorCode}");
                    Debug.WriteLine($"PATTERN MATCHING - CANCELED: ErrorDetails={e.ErrorDetails}");
                    Debug.WriteLine($"PATTERN MATCHING - CANCELED: Did you update the speech key and location/region info?");
                }
                stopRecognitionManager.TaskCompletionSource.TrySetResult(0);
            };
            intentRecognizerByPatternMatching.SessionStopped += (s, e) =>
            {
                Debug.WriteLine("\n    Session stopped event.");
                stopRecognitionManager.TaskCompletionSource.TrySetResult(0);
            };

            return (intentRecognizerByPatternMatching, synthesizer, intentRecognizerByCLU);

        }
        private  async Task HandleOkCommand(SpeechSynthesizer synthesizer, IntentRecognizer intentRecognizer)
        {
            MainThread.BeginInvokeOnMainThread(() => Test.Text = " ");
            await synthesizer.SpeakTextAsync("Sono in ascolto");
            //avvia l'intent recognition su Azure
            string? jsonResult = await RecognizeIntentAsync(intentRecognizer);
            if (jsonResult != null)
            {
                //process jsonResult
                //deserializzo il json
                CLUResponse cluResponse = JsonSerializer.Deserialize<CLUResponse>(jsonResult, jsonSerializationOptions) ?? new CLUResponse();
                await synthesizer.SpeakTextAsync($"La tua richiesta è stata {cluResponse.Result?.Query}");
                var topIntent = cluResponse.Result?.Prediction?.TopIntent;

                if (topIntent != null)
                {
                    switch (topIntent)
                    {
                        case string intent when intent.Contains("Wiki"):
                            await synthesizer.SpeakTextAsync("Vuoi fare una ricerca su Wikipedia");
                            string ricerca = cluResponse.Result.Prediction.Entities[0].Text;
                            string SubItem = "";
                            string MainItem = "";
                            if (cluResponse.Result.Prediction.Entities.Count == 2)
                            {
                                SubItem = cluResponse.Result.Prediction.Entities[0].Text;
                                MainItem = cluResponse.Result.Prediction.Entities[1].Text;
                                string chiave = await SearchKeyText(ricerca);
                                string summary = await ExtractSummaryByKey(chiave, SubItem);
                                await SearchSections(MainItem, SubItem, synthesizer);
                            }
                            else
                            {
                                MainItem = cluResponse.Result.Prediction.Entities[0].Text;
                                string chiave = await SearchKeyText(MainItem);
                                string summary = await ExtractSummaryByKey(chiave, SubItem);
                                string wikiFinal = await FormattaStringa(summary);
                                MainThread.BeginInvokeOnMainThread(() => Test.Text = wikiFinal);
                                await synthesizer.SpeakTextAsync(wikiFinal);
                                
                            }
                            break;

                        case string intent when intent.Contains("Weather"):
                            await synthesizer.SpeakTextAsync("Vuoi sapere come è il tempo");
                            string? città = cluResponse.Result?.Prediction?.Entities?[2].Text;
                            string? data = cluResponse.Result?.Prediction?.Entities?[1].Text;
                            await PrevisioniMeteo(synthesizer,città,data);

                            break;
                        case string intent when intent.Contains("Places"):
                            await synthesizer.SpeakTextAsync("Vuoi informazioni geolocalizzate");
                                switch (cluResponse.Result?.Prediction?.Intents?[0].Category)
                                {
                                    case "Places.GetDistance":
                                    string? wp1 = cluResponse.Result?.Prediction?.Entities?[0].Text;

                                    string? wp2 = cluResponse.Result?.Prediction?.Entities?[1].Text;
                                    await RouteWp1ToWp2(wp1, wp2, synthesizer);
                                    break;

                                    case "Places.FindPlace":
                                    string? luogo = cluResponse?.Result?.Prediction?.Entities?[0].Text;
                                    string? citta = cluResponse?.Result?.Prediction?.Entities?[1].Text;
                                        await FindPointOfInterest(citta, luogo, synthesizer);
                                    break;
                                 }
                            
                            break;
                        case string intent when intent.Contains("None"):
                            await synthesizer.SpeakTextAsync("Non ho capito");
                            break;
                    }

                }
                //determino l'action da fare, eventualmente effettuando una richiesta GET su un endpoint remoto scelto in base al topScoringIntent
                //ottengo il risultato dall'endpoit remoto
                //effettuo un text to speech per descrivere il risultato
            }
            else
            {
                //è stato restituito null - ad esempio quando il processo è interrotto prima di ottenre la risposta dal server
                Debug.WriteLine("Non è stato restituito nulla dall'intent reconition sul server");
            }
        }

        public  async Task<string?> RecognizeIntentAsync(IntentRecognizer recognizer)
        {
            // Starts recognizing.
            Debug.WriteLine("Say something...");

            // Starts intent recognition, and returns after a single utterance is recognized. The end of a
            // single utterance is determined by listening for silence at the end or until a maximum of 15
            // seconds of audio is processed.  The task returns the recognition text as result. 
            // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
            // shot recognition like command or query. 
            // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
            var result = await recognizer.RecognizeOnceAsync();
            string? languageUnderstandingJSON = null;

            // Checks result.
            switch (result.Reason)
            {
                case ResultReason.RecognizedIntent:
                    Debug.WriteLine($"RECOGNIZED: Text={result.Text}");
                    Debug.WriteLine($"    Intent Id: {result.IntentId}.");
                    languageUnderstandingJSON = result.Properties.GetProperty(PropertyId.LanguageUnderstandingServiceResponse_JsonResult);
                    Debug.WriteLine($"    Language Understanding JSON: {languageUnderstandingJSON}.");
                    CLUResponse cluResponse = JsonSerializer.Deserialize<CLUResponse>(languageUnderstandingJSON, jsonSerializationOptions) ?? new CLUResponse();
                    Debug.WriteLine("Risultato deserializzato:");
                    Debug.WriteLine($"kind: {cluResponse.Kind}");
                    Debug.WriteLine($"result.query: {cluResponse.Result?.Query}");
                    Debug.WriteLine($"result.prediction.topIntent: {cluResponse.Result?.Prediction?.TopIntent}");
                    Debug.WriteLine($"result.prediction.Intents[0].Category: {cluResponse.Result?.Prediction?.Intents?[0].Category}");
                    Debug.WriteLine($"result.prediction.Intents[0].ConfidenceScore: {cluResponse.Result?.Prediction?.Intents?[0].ConfidenceScore}");
                    Debug.WriteLine($"result.prediction.entities: ");
                    cluResponse.Result?.Prediction?.Entities?.ForEach(s => Debug.WriteLine($"\tcategory = {s.Category}; text= {s.Text};"));
                    break;
                case ResultReason.RecognizedSpeech:
                    Debug.WriteLine($"RECOGNIZED: Text={result.Text}");
                    Debug.WriteLine($"    Intent not recognized.");
                    break;
                case ResultReason.NoMatch:
                    Debug.WriteLine($"NOMATCH: Speech could not be recognized.");
                    break;
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(result);
                    Debug.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Debug.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Debug.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Debug.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                    break;
            }
            return languageUnderstandingJSON;
        }
        private async void OnRecognitionButtonClicked2(object sender, EventArgs e)
        {
            if(serviceManager != null && taskCompletionSourceManager != null)
            {
                buttonToggle = !buttonToggle;
                if (buttonToggle)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        buttonToggleColor = RecognizeSpeechBtn.Background;
                    });

                    RecognizeSpeechBtn.Background = Colors.Yellow;
                    //creo le risorse
                    //su un dispositivo mobile potrebbe succedere che cambiando rete cambino i parametri della rete, ed in particolare il proxy
                    //In questo caso, per evitare controlli troppo complessi, si è scelto di ricreare lo speechConfig ad ogni richiesta se cambia il proxy
                    if (serviceManager.ShouldRecreateSpeechConfigForProxyChange())
                    {
                        (intentRecognizerByPatternMatching, speechSynthesizer, intentRecognizerByCLU) =
                       ConfigureContinuousIntentPatternMatchingWithMicrophoneAsync(
                           serviceManager.CurrentSpeechConfig,
                           serviceManager.CurrentCluModel,
                           serviceManager.CurrentPatternMatchingModel,
                           taskCompletionSourceManager);
                    }

                    _ = Task.Factory.StartNew(async () =>
                    {
                        taskCompletionSourceManager.TaskCompletionSource = new TaskCompletionSource<int>();
                        await ContinuousIntentPatternMatchingWithMicrophoneAsync(
                            intentRecognizerByPatternMatching!, taskCompletionSourceManager)
                        .ConfigureAwait(false);
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        RecognizeSpeechBtn.Background = buttonToggleColor;
                    });
                    //la doppia chiamata di StopSpeakingAsync è un work-around a un problema riscontrato in alcune situazioni:
                    //se si prova a fermare il task mentre il sintetizzatore sta parlando, in alcuni casi si verifica un'eccezione. 
                    //Con il doppio StopSpeakingAsync non succede.
                    await speechSynthesizer!.StopSpeakingAsync();
                    await speechSynthesizer.StopSpeakingAsync();
                    await intentRecognizerByCLU!.StopContinuousRecognitionAsync();
                    await intentRecognizerByPatternMatching!.StopContinuousRecognitionAsync();
                    //speechSynthesizer.Dispose();
                    //intentRecognizerByPatternMatching.Dispose();
                }
            }
        }
        private async void OnRecognitionButtonClicked(object sender, EventArgs e)
        {
            try
            {
                //accedo ai servizi
                //AzureCognitiveServicesResourceManager serviceManager =(Application.Current as App).AzureCognitiveServicesResourceManager;
                // Creates a speech recognizer using microphone as audio input.
                // Starts speech recognition, and returns after a single utterance is recognized. The end of a
                // single utterance is determined by listening for silence at the end or until a maximum of 15
                // seconds of audio is processed.  The task returns the recognition text as result.
                // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
                // shot recognition like command or query.
                // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                var result = await speechRecognizer!.RecognizeOnceAsync().ConfigureAwait(false);

                // Checks result.
                StringBuilder sb = new();
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    sb.AppendLine($"RECOGNIZED: Text={result.Text}");
                    await speechSynthesizer!.SpeakTextAsync(result.Text);
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    sb.AppendLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    sb.AppendLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        sb.AppendLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        sb.AppendLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        sb.AppendLine($"CANCELED: Did you update the subscription info?");
                    }
                }
                UpdateUI(sb.ToString());
            }
            catch (Exception ex)
            {
                UpdateUI("Exception: " + ex.ToString());
            }
        }
        private void UpdateUI(String message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                RecognitionText.Text = message;
            });
        }
        public  async Task<(double? lat, double? lon)?>
          GetCoordinate(string? città, string language = "it", int count = 1)
        {
            string? cittaCod = HttpUtility.UrlEncode(città);
            string urlCoordinate = $"https://geocoding-api.open-meteo.com/v1/search?name={cittaCod}&count={count}&language={language}";
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{urlCoordinate}");
                if (response.IsSuccessStatusCode)
                {
                    //await Console.Out.WriteLineAsync(await response.Content.ReadAsStringAsync());
                    GeoCoding? geoCoding = await response.Content.ReadFromJsonAsync<GeoCoding>();
                    if (geoCoding != null && geoCoding.Results?.Count > 0)
                    {
                        return (geoCoding.Results[0].Latitude, geoCoding.Results[0].Longitude);
                    }
                }
                return null;
            }
            catch (Exception)
            {

                Console.WriteLine("Errore");
            }
            return null;
        }
        public  async Task PrevisioniMeteo(SpeechSynthesizer synthesizer, string città, string data)
        {
            const string datoNonFornitoString = "";
            var geo = await GetCoordinate(città);
            if (geo != null)
            {
                FormattableString addressUrlFormattable = $"https://api.open-meteo.com/v1/forecast?latitude={geo?.lat}&longitude={geo?.lon}&current=temperature_2m,weather_code,wind_speed_10m,wind_direction_10m&hourly=temperature_2m,relative_humidity_2m,dew_point_2m,apparent_temperature,precipitation_probability,precipitation,rain,showers,weather_code,wind_speed_10m,wind_direction_10m&daily=weather_code,temperature_2m_max,temperature_2m_min,apparent_temperature_max,apparent_temperature_min&timeformat=unixtime&timezone=auto";
                string addressUrl = FormattableString.Invariant(addressUrlFormattable);
                var response = await _client.GetAsync($"{addressUrl}");
                if (response.IsSuccessStatusCode)
                {
                    OpenMeteoForecast? forecast = await response.Content.ReadFromJsonAsync<OpenMeteoForecast>();
                    if (forecast != null && data == "oggi")
                    {
                        MainThread.BeginInvokeOnMainThread(() => Test.Text = $"\nCondizioni meteo attuali per {città}" +
                        $"\nData e ora previsione: {UtilsMeteo.Display(UtilsMeteo.UnixTimeStampToDateTime(forecast.Current.Time), datoNonFornitoString)}" +
                        $"\nTemperatura : {UtilsMeteo.Display(forecast.Current.Temperature2m, datoNonFornitoString)} °C" +
                        $"\nDirezione del vento: {UtilsMeteo.Display(forecast.Current.WindDirection10m, datoNonFornitoString)} °" +
                        $"\nVelocità del vento: {UtilsMeteo.Display(forecast.Current.WindSpeed10m, datoNonFornitoString)} Km/h");
                         
                        await synthesizer.SpeakTextAsync($"\nCondizioni meteo attuali per {città}");
                        await synthesizer.SpeakTextAsync($"Data e ora previsione: {UtilsMeteo.Display(UtilsMeteo.UnixTimeStampToDateTime(forecast.Current.Time), datoNonFornitoString)}");
                        await synthesizer.SpeakTextAsync($"Temperatura : {UtilsMeteo.Display(forecast.Current.Temperature2m, datoNonFornitoString)} °C");
                        await synthesizer.SpeakTextAsync($"Direzione del vento: {UtilsMeteo.Display(forecast.Current.WindDirection10m, datoNonFornitoString)} °");
                        await synthesizer.SpeakTextAsync($"Velocità del vento: {UtilsMeteo.Display(forecast.Current.WindSpeed10m, datoNonFornitoString)} Km/h");
                    }
                    
                    if (forecast.Daily != null && data != "oggi")
                    {
                        await synthesizer.SpeakTextAsync($"\nPrevisioni meteo giornaliere per {città}");
                        int? numeroGiorni = forecast.Daily.Time?.Count;
                        if (data == "domani")
                        {
                            MainThread.BeginInvokeOnMainThread(() => Test.Text = $"Data e ora = {UtilsMeteo.Display(UtilsMeteo.UnixTimeStampToDateTime(forecast.Daily?.Time?[1]), datoNonFornitoString)}" +
                            $"\nTemperatura massima = {UtilsMeteo.Display(forecast.Daily?.Temperature2mMax?[1], datoNonFornitoString)} °C" +
                            $"\nTemperatura minima = {UtilsMeteo.Display(forecast.Daily?.Temperature2mMin?[1], datoNonFornitoString)} °C" +
                            $"  previsione = {UtilsMeteo.Display(UtilsMeteo.WMOCodesIntIT(forecast.Daily?.WeatherCode?[1]), datoNonFornitoString)}");

                            await synthesizer.SpeakTextAsync($"Data e ora = {UtilsMeteo.Display(UtilsMeteo.UnixTimeStampToDateTime(forecast.Daily?.Time?[1]), datoNonFornitoString)};" +
                                    $"\nTemperatura massima = {UtilsMeteo.Display(forecast.Daily?.Temperature2mMax?[1], datoNonFornitoString)} °C;" +
                                    $"\nTemperatura minima = {UtilsMeteo.Display(forecast.Daily?.Temperature2mMin?[1], datoNonFornitoString)} °C; " +
                                    $"\nprevisione = {UtilsMeteo.Display(UtilsMeteo.WMOCodesIntIT(forecast.Daily?.WeatherCode?[1]), datoNonFornitoString)}");
                        }
                        else if (data == "dopodomani")
                        {
                            MainThread.BeginInvokeOnMainThread(() => Test.Text = $"Data e ora = {UtilsMeteo.Display(UtilsMeteo.UnixTimeStampToDateTime(forecast.Daily?.Time?[2]), datoNonFornitoString)}" +
                            $"\nTemperatura massima = {UtilsMeteo.Display(forecast.Daily?.Temperature2mMax?[2], datoNonFornitoString)} °C" +
                            $"\nTemperatura minima = {UtilsMeteo.Display(forecast.Daily?.Temperature2mMin?[2], datoNonFornitoString)} °C" +
                            $"\nprevisione = {UtilsMeteo.Display(UtilsMeteo.WMOCodesIntIT(forecast.Daily?.WeatherCode?[2]), datoNonFornitoString)}");

                            await synthesizer.SpeakTextAsync($"Data e ora = {UtilsMeteo.Display(UtilsMeteo.UnixTimeStampToDateTime(forecast.Daily?.Time?[2]), datoNonFornitoString)};" +
                                    $"Temperatura massima = {UtilsMeteo.Display(forecast.Daily?.Temperature2mMax?[2], datoNonFornitoString)} °C;" +
                                    $"Temperatura minima = {UtilsMeteo.Display(forecast.Daily?.Temperature2mMin?[2], datoNonFornitoString)} °C; " +
                                    $"previsione = {UtilsMeteo.Display(UtilsMeteo.WMOCodesIntIT(forecast.Daily?.WeatherCode?[2]), datoNonFornitoString)}");
                        }
                        else
                        {
                            string[] split = data.Split(" ");
                            int giorni = Converti(split[0]);

                            if (giorni != 0)
                            {
                                MainThread.BeginInvokeOnMainThread(() => Test.Text = $"Data e ora = {UtilsMeteo.Display(UtilsMeteo.UnixTimeStampToDateTime(forecast.Daily?.Time?[giorni]), datoNonFornitoString)}" +
                            $"\nTemperatura massima = {UtilsMeteo.Display(forecast.Daily?.Temperature2mMax?[giorni], datoNonFornitoString)} °C" +
                            $"\nTemperatura minima = {UtilsMeteo.Display(forecast.Daily?.Temperature2mMin?[giorni], datoNonFornitoString)} °C" +
                            $"\nprevisione = {UtilsMeteo.Display(UtilsMeteo.WMOCodesIntIT(forecast.Daily?.WeatherCode?[giorni]), datoNonFornitoString)}");

                                await synthesizer.SpeakTextAsync($"Data e ora = {UtilsMeteo.Display(UtilsMeteo.UnixTimeStampToDateTime(forecast.Daily?.Time?[giorni]), datoNonFornitoString)};" +
                                    $"Temperatura massima = {UtilsMeteo.Display(forecast.Daily?.Temperature2mMax?[giorni], datoNonFornitoString)} °C;" +
                                    $"Temperatura minima = {UtilsMeteo.Display(forecast.Daily?.Temperature2mMin?[giorni], datoNonFornitoString)} °C; " +
                                    $"previsione = {UtilsMeteo.Display(UtilsMeteo.WMOCodesIntIT(forecast.Daily?.WeatherCode?[giorni]), datoNonFornitoString)}");
                            }
                            else
                            {
                                await synthesizer.SpeakTextAsync("Condizioni meteo non trovate su open meteo, probabilmente non saranno state ancora caricate");
                            }
                        }
                        
                    }
                   

                }
            }
        }
        public  int Converti(string n)
        {
            switch (n)
            {
                case "due":
                    return 2;
                    break;
                case "tre":
                    return 3;
                    break;
                case "quattro":
                    return 4;
                    break;
                case "cinque":
                    return 5;
                    break;
                case "sei":
                    return 6;
                    break;
                case "sette":
                    return 7;
                    break;
                default:
                    return 0;
            }
        }
         async Task RouteWp1ToWp2(string wp1, string wp2, SpeechSynthesizer synthesizer)
        {
            string wp1Encode = HttpUtility.UrlEncode(wp1);
            string wp2Encode = HttpUtility.UrlEncode(wp2);
            string urlCompleto = $"https://dev.virtualearth.net/REST/v1/Routes?wp.1={wp1Encode}&wp.2={wp2Encode}&optimize=time&tt=departure&dt=2024-04-11%2019:35:00&distanceUnit=km&c=it&ra=regionTravelSummary&key={bingKey}";
            HttpResponseMessage response = await _client.GetAsync(urlCompleto);
            if (response.IsSuccessStatusCode)
            {
                LocalRoute? localRoute = await response.Content.ReadFromJsonAsync<LocalRoute>();
                if (localRoute != null)
                {
                    // distanza in km
                    double distanza = localRoute.ResourceSets[0].Resources[0].TravelDistance;
                    double durata = localRoute.ResourceSets[0].Resources[0].TravelDuration;
                    double durataConTraffico = localRoute.ResourceSets[0].Resources[0].TravelDurationTraffic;
                    string modViaggio = localRoute.ResourceSets[0].Resources[0].TravelMode;
                    MainThread.BeginInvokeOnMainThread(() => Test.Text = $"La distanza da {wp1} a {wp2}  è di {Math.Round(distanza , 2)} KM" +
                        $"\ncon una durata di {Math.Round(durata / 60, 2)} minuti e " +
                        $"con {Math.Round( durataConTraffico / 60 , 2)} minuti con traffico attuale utilizzando" +
                        $"la {modViaggio} ");
                    await synthesizer.SpeakTextAsync($"La distanza da {wp1} a {wp2}  è di {Math.Round(distanza, 2)} KM" +
                        $"\ncon una durata di {Math.Round(durata /60 , 2) } minuti e " +
                        $"con { Math.Round(durataConTraffico / 60 , 2)} minuti con traffico attuale utilizzando" +
                        $"la {modViaggio} ");
                    // regioni attraversate
                    var regioni = localRoute.ResourceSets[0].Resources[0].RouteLegs[0].RegionTravelSummary[0].Subregions;
                    if (regioni != null)
                    {
                        // con il count so qunante regioni attraverso
                        for (int i = 0; i < regioni.Count; i++)
                        {
                            MainThread.BeginInvokeOnMainThread(() => Test.Text = $"Regione {i + 1} = {regioni[i].Name}" +
                                $" per {regioni[i].TravelDistance} KM");
                            await synthesizer.SpeakTextAsync($"Regione {i + 1} = {regioni[i].Name}" +
                                $" per {Math.Round( regioni[i].TravelDistance, 2) } KM");
                        }
                    }

                }
            }
        }
         async Task<Models.Point?> FindLocationByQuery(string queryString, SpeechSynthesizer synthesizer)
        {
            //https://docs.microsoft.com/en-us/bingmaps/rest-services/locations/find-a-location-by-query
            //esempio: https://dev.virtualearth.net/REST/v1/Locations/{locationQuery}?includeNeighborhood={includeNeighborhood}&maxResults={maxResults}&include={includeValue}&key={BingMapsAPIKey}
            //https://docs.microsoft.com/en-us/bingmaps/rest-services/locations/find-a-location-by-query#api-parameters
            int includeNeighborhood = 1;
            string includeValue = "queryParse,ciso2";
            int maxResults = 1;
            string locationQuery = HttpUtility.UrlEncode(queryString);
            string addressUrl = $"https://dev.virtualearth.net/REST/v1/Locations/{locationQuery}?includeNeighborhood={includeNeighborhood}&maxResults={maxResults}&include={includeValue}&key={bingKey}";
            Models.Point? point = null;
            try
            {
                HttpResponseMessage response = await _client.GetAsync(addressUrl);
                Locations? data = await response.Content.ReadFromJsonAsync<Locations>();
                point =data.ResourceSets[0].Resources[0].Point;
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException || ex is ArgumentException)
                {
                    await synthesizer.SpeakTextAsync(ex.Message + "\nIl recupero dei dati dal server non è riuscito");
                }

            }
            return point;
        }
         async Task FindPointOfInterest( string citta, string luogo, SpeechSynthesizer synthesizer)
        {
            // recupara coordinate geografiche
            var point = await FindLocationByQuery(citta, synthesizer);
            double? lat = point.Coordinates[0];
            double? lng = point.Coordinates[1];
            //string locationQuery = HttpUtility.UrlEncode(url);
            FormattableString urlComplete = $"https://dev.virtualearth.net/REST/v1/LocationRecog/{lat},{lng}?radius=1&top=15&datetime=2024-04-11%2018:50:42Z&distanceunit=km&verboseplacenames=true&includeEntityTypes=businessAndPOI,naturalPOI,address&includeNeighborhood=1&include=ciso2&key={bingKey} ";
            // convere le virgole in punti per la latituine e longitudine
            string addressUrl = FormattableString.Invariant(urlComplete);
            HttpResponseMessage response = await _client.GetAsync(addressUrl);
            if (response.IsSuccessStatusCode)
            {
                string luoghi = await Trasforma(luogo);
                LocalRecognition? data = await response.Content.ReadFromJsonAsync<LocalRecognition>();
                int numeroPunti = data.ResourceSets[0].Resources[0].BusinessesAtLocation.Count;
                var resources = data.ResourceSets[0].Resources[0];
                List<string > lista = new List<string>();
                if (luogo !=null)
                {
                    for (int i = 0; i < numeroPunti; i++)
                    {
                        if (resources.BusinessesAtLocation[i].BusinessInfo.Type == luoghi)
                        {
                            lista.Add(resources.BusinessesAtLocation[i].BusinessInfo.EntityName);
                        }

                    }
                }
                if (lista != null)
                {
                    await synthesizer.SpeakTextAsync($"Questi sono i {luogo} che ho trovato a {citta}");
                    foreach (var item in lista)
                    {
                        MainThread.BeginInvokeOnMainThread(() => Test.Text = (string)item + "\n");
                        await synthesizer.SpeakTextAsync(item);
                    }
                }
            }
        }
        public  async Task<string> Trasforma(string luogo)
        {
            switch (luogo)
            {
                case "bar":
                    return "Bars";
                    break;
                case "hotel":
                    return "Hotels";
                    break;
                case "motel":
                    return "Motels";
                    break;
                case "aziende":
                    return "Business-to-Business";
                    break;
                case "scuole":
                    return "Education";
                    break;
                case "chiese":
                    return "Religion";
                    break;
                case "banche":
                    return "Banking & finance";
                    break;
                case "palestre":
                    return "Sports & recreation";
                    break;
                case "ristoranti":
                    return "Restaurants";
                    break;
                case "negozi":
                    return "Shop";
                    break;
                case "parrucchieri":
                    return "Barbers";
                    break;
                case "officine":
                    return "Cars and Trucks";
                    break;
                case "servizi taxi":
                    return "Taxi Services";
                    break;
                case "agenzie di viaggio":
                    return "Travel Agencies";
                    break;
                case "servizi governativi":
                    return "Government Services";
                    break;
                case "gioiellerie":
                    return "B2B Jewelers";
                    break;
                case "musei":
                    return "Museums";
                    break;
                default:
                    return null;
                    break;
            }
        }
         async Task<string> SearchKeyText(string argument)
        {
            string argumentClean = HttpUtility.UrlEncode(argument);
            string wikiUrl = $"https://it.wikipedia.org/w/rest.php/v1/search/page?q={argumentClean}&limit=1";
            // recupero la chiave di ricerca con il parsing del dominio
            var response = await _client.GetAsync(wikiUrl);
            //Console.WriteLine(wikiResult);
            if (response.IsSuccessStatusCode)
            {
                Models.KeyModel? model = await response.Content.ReadFromJsonAsync<KeyModel>();
                if (model != null)
                {
                    string? keySearch = model.Pages[0].Key;

                    return keySearch;
                }
            }
            return null;
        }
         async Task<string> ExtractSummaryByKey(string keySearch, string subItem)
        {
            string wikiUrl = $"https://it.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro&explaintext&exsectionformat=plain&redirects=1&titles={keySearch}";
            string wikiSummaryJSON = await _client.GetStringAsync(wikiUrl);
            using JsonDocument document = JsonDocument.Parse(wikiSummaryJSON);
            JsonElement root = document.RootElement;
            JsonElement query = root.GetProperty("query");
            JsonElement pages = query.GetProperty("pages");
            JsonElement.ObjectEnumerator enumerator = pages.EnumerateObject();
            if (enumerator.MoveNext())
            {
                JsonElement target = enumerator.Current.Value;
                if (target.TryGetProperty("extract", out JsonElement extract))
                {
                    return extract.GetString() ?? string.Empty;
                }
            }
            return string.Empty;
        }

         async Task SearchSections(string mainItem, string subItem, SpeechSynthesizer synthesizer)
        {
            string urlSection = $"https://it.wikipedia.org/w/api.php?action=parse&format=json&page={mainItem}&prop=sections&disabletoc=1";
            var response = await _client.GetAsync(urlSection);
            // parso le sezioni e recupero la key e l'indice di sezione
            if (response.IsSuccessStatusCode)
            {
                SectionModel? sectionModel = await response.Content.ReadFromJsonAsync<SectionModel>();
                if (sectionModel != null)
                {
                    List<SectionWiki> sections = sectionModel.Parse.Sections;
                    foreach (SectionWiki section in sections)
                    {
                        if (section.LinkAnchor.ToLower().Contains(subItem))
                        {
                            await SearchDentroSezioni(mainItem, section.Index, synthesizer);
                        }
                    }
                }
            }
        }
         async Task SearchDentroSezioni(string mainItem, string subItem, SpeechSynthesizer synthesizer)
        {
            await synthesizer.SpeakTextAsync(subItem);
            MainThread.BeginInvokeOnMainThread(() => Test.Text = subItem);
            string urlSection = $"https://it.wikipedia.org/w/api.php?action=parse&format=json&page={mainItem}&prop=wikitext&section={subItem}&disabletoc=1";
            var response = await _client.GetAsync(urlSection);
            if (response.IsSuccessStatusCode)
            {
                WikiSection? wikiSezioni = await response.Content.ReadFromJsonAsync<WikiSection>();
                if (wikiSezioni != null)
                {
                    string sections = wikiSezioni.Parses.Wikitext.Risposta;
                    if (sections != null)
                    {
                        string wiki = await FormattaStringa(sections);
                        MainThread.BeginInvokeOnMainThread(() => Test.Text = wiki);
                        await synthesizer.SpeakTextAsync(wiki);
                        
                    }
                }
            }
        }
        static async Task<string> FormattaStringa(string summary)
{
    char[] caratteri = summary.ToCharArray();
    string risposta = "";
    foreach (var n in caratteri)
    {
        if (n != '=' && n != '*' && n != '{' && n != '}' && n != '[' && n != ']' && n != '|' && n != '\'')
        {
            risposta += n;
        }
    }
    return risposta;
    MainPage mainPage = new MainPage();
    mainPage.Testo.Text = risposta;
                
        }
    }
}
