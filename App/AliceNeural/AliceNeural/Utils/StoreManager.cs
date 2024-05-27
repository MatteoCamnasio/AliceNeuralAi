using System.Diagnostics;
using System.Text.Json;

namespace AliceNeural.Utils
{
    public class StoreManager
    {
        public static AzureSpeechServiceStore GetSpeechDataFromStore()
        {
            //il file _secretMyAzureRoboVoiceStore.json deve essere creato con le coppie chiave-valore contenenti i secrets
            //il .gitignore è stato modificato, aggiungendo una regola che ignora i file del tipo _secret*.json
            using var stream = FileSystem.Current.OpenAppPackageFileAsync("_secretRoboVoiceStore.json").Result;
            using var reader = new StreamReader(stream);
            string store = reader.ReadToEnd();
            AzureSpeechServiceStore? azureSpeechServiceStore = JsonSerializer.Deserialize<AzureSpeechServiceStore>(store);
            return azureSpeechServiceStore ?? new AzureSpeechServiceStore();
        }

        public static AzureIntentRecognitionByCLUStore GetCLUDataFromStore()
        {
            //il file _secretMyAzureIntentRecognitionByCLUStore.json deve essere creato con le coppie chiave-valore contenenti i secrets
            using var stream = FileSystem.Current.OpenAppPackageFileAsync("_secretCLUStore.json").Result;
            using var reader = new StreamReader(stream);
            string store = reader.ReadToEnd();
            AzureIntentRecognitionByCLUStore? azureSpeechServiceStore = JsonSerializer.Deserialize<AzureIntentRecognitionByCLUStore>(store);
            return azureSpeechServiceStore ?? new AzureIntentRecognitionByCLUStore();
        }
    }
}
