using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceNeural.Utils
{
    public static class UtilsMeteo
    {
        public static string Display(object? value, string? stringIfNull)
        {
            if (value is null)
            {
                if (stringIfNull is null)
                {
                    return string.Empty;
                }
                return stringIfNull;
            }
            else
            {
                if (stringIfNull is null)
                {
                    return value.ToString() ?? string.Empty;
                }
                return value.ToString() ?? stringIfNull;
            }
        }
        public static DateTime? UnixTimeStampToDateTime(double? unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            if (unixTimeStamp != null)
            {
                DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds((double)unixTimeStamp).ToLocalTime();
                return dateTime;
            }
            return null;
        }

        /// <summary>
        /// Restituisce la descrizione testuale della previsione meteo a partire dal codice di previsione
        /// </summary>
        /// <param name="code">Codice di previsione meteo</param>
        /// <returns></returns>
        public static string? WMOCodesInt(int? code)
        {
            string? result = code switch
            {
                0 => "clear sky",
                1 => "mainly clear",
                2 => "partly cloudy",
                3 => "overcast",
                45 => "fog",
                48 => "depositing rime fog",
                51 => "drizzle: light intensity",
                53 => "drizzle: moderate intensity",
                55 => "drizzle: dense intensity",
                56 => "freezing drizzle: light intensity",
                57 => "freezing drizzle: dense intensity",
                61 => "rain: slight intensity",
                63 => "rain: moderate intensity",
                65 => "rain: heavy intensity",
                66 => "freezing rain: light intensity",
                67 => "freezing rain: heavy intensity",
                71 => "snow fall: slight intensity",
                73 => "snow fall: moderate intensity",
                75 => "snow fall: heavy intensity",
                77 => "snow grains",
                80 => "rain showers: slight",
                81 => "rain showers: moderate",
                82 => "rain showers: violent",
                85 => "snow showers slight",
                86 => "snow showers heavy",
                95 => "thunderstorm: slight or moderate",
                96 => "thunderstorm with slight hail",
                99 => "thunderstorm with heavy hail",
                _ => null,
            };
            return result;
        }

        /// <summary>
        /// Restituisce la descrizione testuale della previsione meteo a partire dal codice di previsione in italiano
        /// </summary>
        /// <param name="code">Codice di previsione meteo</param>
        /// <returns></returns>
        public static string? WMOCodesIntIT(int? code)
        {
            string? result = code switch
            {
                0 => "cielo sereno",
                1 => "prevalentemente limpido",
                2 => "parzialmente nuvoloso",
                3 => "coperto",
                45 => "nebbia",
                48 => "nebbia con brina",
                51 => "pioggerellina di scarsa intensità",
                53 => "pioggerellina di moderata intensità",
                55 => "pioggerellina intensa",
                56 => "pioggerellina gelata di scarsa intensità",
                57 => "pioggerellina gelata intensa",
                61 => "pioggia di scarsa intensità",
                63 => "pioggia di moderata intensità",
                65 => "pioggia molto intensa",
                66 => "pioggia gelata di scarsa intensità",
                67 => "pioggia gelata intensa",
                71 => "nevicata di lieve entità",
                73 => "nevicata di media entità",
                75 => "nevicata intensa",
                77 => "granelli di neve",
                80 => "deboli rovesci di pioggia",
                81 => "moderati rovesci di pioggia",
                82 => "violenti rovesci di pioggia",
                85 => "leggeri rovesci di neve",
                86 => "pesanti rovesci di neve",
                95 => "temporale lieve o moderato",
                96 => "temporale con lieve grandine",
                99 => "temporale con forte grandine",
                _ => null,
            };
            return result;
        }
    }
    }
