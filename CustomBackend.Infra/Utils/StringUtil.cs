using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CustomBackend.Infra.Utils
{
    public static class StringUtil
    {
        public static CultureInfo brCulture = CreateBrCulture();
        public static CultureInfo usCulture = CultureInfo.CreateSpecificCulture("en-US");

        public static CultureInfo CreateBrCulture()
        {
            var culture = CultureInfo.CreateSpecificCulture("pt-BR");
            culture.NumberFormat.NumberDecimalSeparator = ",";
            culture.NumberFormat.CurrencyDecimalSeparator = ",";
            culture.NumberFormat.NumberGroupSeparator = ".";
            culture.NumberFormat.CurrencyGroupSeparator = ".";

            return culture;
        }

        public static bool IsValidString(this string? value) => !string.IsNullOrWhiteSpace(value?.Trim());

        public static bool IsValidMail(this string? mailAdress) => mailAdress.IsValidString() && new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").Match(mailAdress).Success;

        public static string JoinInString<T>(this IEnumerable<T> values, string separador) => string.Join(separador, values);



        public static string ExtractDigits(this string value) => Regex.Replace(value, @"[^\d]", string.Empty);

        public static int GetDigitsFromString(this string value, int defaultValue = 0)
        {
            try
            {
                return new Regex(@"[0-9]+").Match(value).Value.ToInt32(0);
            }
            catch
            {
                return defaultValue;
            }
        }



        public static string ToTitleCase(this string value) => brCulture.TextInfo.ToTitleCase(value);

        public static string ToMoneyFormat(this decimal value) => value.ToString("C", brCulture);

        public static string ToUsaMoneyFormat(this decimal value) => value.ToString(usCulture);

        public static string ToBrMoneyFormat(this decimal value) => value.ToString(brCulture);

        public static string FormatarBoolean(this bool value) => value ? "Sim" : "Não";

        public static string RemoveAccents(this string text)
        {
            if (!text.IsValidString())
                return text;

            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }

        public static string Slugfy(this string value)
        {
            value = value.RemoveAccents().ToLower();

            // invalid chars           
            value = Regex.Replace(value, @"[^a-z0-9\s-]", string.Empty);

            // convert multiple spaces into one space   
            value = Regex.Replace(value, @"\s+", " ").Trim();

            // cut and trim 
            value = Regex.Replace(value, @"\s", "-"); // hyphens   

            // Replace multiple - with single -
            value = Regex.Replace(value, @"\-\-+", "-");

            // Remove trailing -
            value = Regex.Replace(value, @"\-$", string.Empty);

            // after removing number and "W" from start, we need to check again
            // because scenarios like theese : 123www123wwwaaaa or www123wwww123aaa

            var previous = string.Empty;
            do
            {
                previous = value;
                // Remove numbers from start
                value = Regex.Replace(value, @"^\d+", string.Empty);

                // Remove "W" from start
                value = Regex.Replace(value, @"^w+", string.Empty);

                // Remove - from start
                value = Regex.Replace(value, @"^\-", string.Empty);
            } while (value != previous);

            return value;
        }

        public static string Slugfy(this string value, string defaultValue, bool sumOneToEndNumber)
        {
            if (!value.IsValidString())
                value = string.Empty;

            value = value.Slugfy();

            if (!value.IsValidString())
                value = defaultValue.Slugfy();

            if (sumOneToEndNumber)
            {
                var number = Regex.Match(value, @"\d+$").Value;

                if (!number.IsValidString())
                    value += "-1";
                else
                    value = Regex.Replace(value, @"\d+$", (number.ToInt32() + 1).ToString());
            }

            return value;
        }

        public static string DecodeBase64ToString(this string encodedString, Encoding encoding)
        {
            byte[] data = Convert.FromBase64String(encodedString);
            string decodedString = encoding.GetString(data);
            return decodedString;
        }

        public static T JsonTo<T>(this string? json, int? maxDepth = null)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.PropertyNameCaseInsensitive = true;
            options.Converters.Add(new JsonStringEnumConverter());

            if (maxDepth.HasValue)
                options.MaxDepth = maxDepth.Value;

            return JsonSerializer.Deserialize<T>(json, options);
        }

        public static string ToJson(this object value, int? maxDepth = null, bool camelCase = false)
        {
            var options = new JsonSerializerOptions();
            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

            if (maxDepth.HasValue)
                options.MaxDepth = maxDepth.Value;

            if (camelCase)
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            options.Converters.Add(new JsonStringEnumConverter());

            return JsonSerializer.Serialize(value, options);
        }



        public static bool IsCnpj(this string cnpj)
        {
            if (!cnpj.IsValidString())
                return false;

            cnpj = cnpj.Trim();
            cnpj = cnpj.Replace(".", string.Empty).Replace("-", string.Empty).Replace("/", string.Empty);
            if (cnpj.Length != 14 || cnpj.Length == cnpj.Count(e => e == cnpj[0]))
                return false;

            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma;
            int resto;
            string digito;
            string tempCnpj;

            tempCnpj = cnpj.Substring(0, 12);
            soma = 0;
            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCnpj = tempCnpj + digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = digito + resto.ToString();
            return cnpj.EndsWith(digito);
        }

        public static bool IsCpf(this string cpf)
        {
            if (!cpf.IsValidString())
                return false;

            cpf = cpf.Trim();
            cpf = cpf.Replace(".", string.Empty).Replace("-", string.Empty);
            if (cpf.Length != 11 || cpf.Length == cpf.Count(e => e == cpf[0]))
                return false;

            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;

            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = digito + resto.ToString();
            return cpf.EndsWith(digito);
        }

        public static string NormalizarCnpj(this string cnpj)
        {
            if (!cnpj.IsValidString())
                return cnpj;

            if (cnpj.Length > 14)
                return cnpj.Substring(cnpj.Length - 14);

            if (cnpj.Length < 14)
                return cnpj.PadLeft(14, '0');

            return cnpj;
        }



        public static bool HasAnyInvalidInString(params string[] values)
        {
            var validQuantity = values.Count(e => e.IsValidString());

            if (validQuantity == values.Length) // -> todas válidas
                return false;

            if (validQuantity == 0) // -> todas null ou vazias
                return false;

            return true; // -> algumas preenchidas, outras não
        }
    }
}
