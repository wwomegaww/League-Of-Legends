using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using System.Net;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using MainMenu = EloBuddy.SDK.Menu.MainMenu;
using CheckBox = EloBuddy.SDK.Menu.Values.CheckBox;
using Slider = EloBuddy.SDK.Menu.Values.Slider;
using KeyBind = EloBuddy.SDK.Menu.Values.KeyBind;


namespace ChatTranslator
{
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    internal class Program
    {
        public static EloBuddy.SDK.Menu.Menu Config,
            TranslatorMenu,
            IncomingText,
            OutgoingText,
            Position,
            Logger,
            CopyPaste;

        public static String[] FromArray = new[]
        {
            "auto", "af", "sq", "ar", "hy", "az", "eu", "be", "bs", "bg", "ca", "zh", "hr", "cs", "da", "nl", "en", "et",
            "fi", "fr", "gl", "ka", "de", "el", "ht", "hu", "is", "id", "ga", "it", "kk", "ko", "ky", "la", "lv", "lt",
            "mk", "mg", "ms", "mt", "mn", "no", "fa", "pl", "pt", "ro", "ru", "sr", "sk", "sl", "es", "sw", "sv", "tl",
            "tg", "tt", "th", "tr", "uk", "uz", "vi", "cy", "he"
        };

        public static String[] FromArrayMenu = new[]
        {
            "auto", "Afrikaans", "Albanian", "Arabic", "Armenian", "Azerbaijan", "Basque", "Belarusian", "Bosnian",
            "Bulgarian", "Catalan", "Chinese", "Croatian", "Czech", "Danish", "Dutch", "English", "Estonian", "Finish",
            "French", "Galician", "Georgian", "German", "Greek", "Haitian", "Hungarian", "Icelandic", "Indonesian",
            "Irish", "Italian", "Kazakh", "Korean", "Kyrgyz", "Latin", "Latvian", "Lithuanian", "Macedonian", "Malagasy",
            "Malay", "Maltese", "Mongolian", "Norwegian", "Persian", "Polish", "Portuguese", "Romanian", "Russian",
            "Serbian", "Slovakian", "Slovenian", "Spanish", "Swahili", "Swedish", "Tagalog", "Tajik", "Tatar", "Thai",
            "Turkish", "Ukrainian", "Uzbek", "Vietnamese", "Welsh", "Yiddish"
        };

        public static String[] ToArray = new[]
        {
            "af", "sq", "ar", "hy", "az", "eu", "be", "bs", "bg", "ca", "zh", "hr", "cs", "da", "nl", "en", "et", "fi",
            "fr", "gl", "ka", "de", "el", "ht", "hu", "is", "id", "ga", "it", "kk", "ko", "ky", "la", "lv", "lt", "mk",
            "mg", "ms", "mt", "mn", "no", "fa", "pl", "pt", "ro", "ru", "sr", "sk", "sl", "es", "sw", "sv", "tl", "tg",
            "tt", "th", "tr", "uk", "uz", "vi", "cy", "he"
        };

        public static String[] ToArrayMenu = new[]
        {
            "Afrikaans", "Albanian", "Arabic", "Armenian", "Azerbaijan", "Basque", "Belarusian", "Bosnian", "Bulgarian",
            "Catalan", "Chinese", "Croatian", "Czech", "Danish", "Dutch", "English", "Estonian", "Finish", "French",
            "Galician", "Georgian", "German", "Greek", "Haitian", "Hungarian", "Icelandic", "Indonesian", "Irish",
            "Italian", "Kazakh", "Korean", "Kyrgyz", "Latin", "Latvian", "Lithuanian", "Macedonian", "Malagasy", "Malay",
            "Maltese", "Mongolian", "Norwegian", "Persian", "Polish", "Portuguese", "Romanian", "Russian", "Serbian",
            "Slovakian", "Slovenian", "Spanish", "Swahili", "Swedish", "Tagalog", "Tajik", "Tatar", "Thai", "Turkish",
            "Ukrainian", "Uzbek", "Vietnamese", "Welsh", "Yiddish"
        };

        public static String[] SpecChars = new[] {"bg", "zh-CN", "zh-TW", "ru", "ko", "uk"};
        public static ObservableCollection<Message> LastMessages = new ObservableCollection<Message>();

        public const string YandexUrl = "https://translate.yandex.net/api/v1.5/tr.json/translate";

        public static List<string> YandexApiKeys =
            new List<string>(
                new[]
                {
                    "?key=trnsl.1.1.20151027T151706Z.16f6a75f2f1b2aa4.d670690d98ed95429c11e25d871b7c2e05e81cbb",
                    "?key=trnsl.1.1.20160104T204216Z.fafe170e32096852.9e3884fe5a4c00881dbf6781534fee74664c68ec",
                    "?key=trnsl.1.1.20160104T204235Z.bbf140ab21cf34a8.7b5b3ff936297932f10250966e112f963370e675",
                    "?key=trnsl.1.1.20160104T204435Z.4ee06ec4b7bf42ed.a6408bbb10e00b0480d516d7c8b2368d8f369710",
                    "?key=trnsl.1.1.20160104T204502Z.776f8a7e2c4d9d42.b1ee11db59b64410c6402df651cb1e156e0fe1d3",
                    "?key=trnsl.1.1.20160104T204457Z.986a06123cc06620.f114139f32732c33ba127de92e8a14961a080edd",
                    "?key=trnsl.1.1.20160104T204437Z.d766324c28c39ddb.25569303b37b7132212831048bbc0db2476eebbb",
                });

        public static string YandexApiKey;
        public static bool ShowMessages, Sent, Copied, Translate;
        public static string Path, FileName, ClipBoard, LastInput;
        public static List<string> ClipBoardLines;
        public static float Gamestart;

        private static void Main()
        {
            Loading.OnLoadingComplete += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            CreateMenu();

            Chat.Print("ChatTranslator", Color.Purple);
            Game.OnUpdate += Game_OnUpdate;
            Chat.OnInput += Chat_OnInput;
            Chat.OnMessage += Chat_OnMessage;
            Drawing.OnDraw += Drawing_OnDraw;
            LastMessages.CollectionChanged += OnMessage;

            Path = string.Format(
                @"{0}\ChatLogs\{1}\{2}\{3}\{4}\", EloBuddy.Sandbox.SandboxConfig.DataDirectory,
                DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MMMM"), DateTime.Now.ToString("dd"),
                Game.MapId);
            FileName = ObjectManager.Player.BaseSkinName + "_" + Game.MapId + ".txt";
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            SetApiKey();

            if (Logger["EnabledLog"].Cast<CheckBox>().CurrentValue)
            {
                InitText();
            }
            LastInput = "null";
        }

        private static void Chat_OnInput(ChatInputEventArgs args)
        {
            var message = "";
            message += args.Input;
            if (OutgoingText["EnabledOut"].Cast<CheckBox>().CurrentValue &&
                (ToArray[OutgoingText["OutFrom"].Cast<Slider>().CurrentValue] !=
                 ToArray[OutgoingText["OutTo"].Cast<Slider>().CurrentValue]))
            {
                TranslateAndSend(message);
                args.Process = false;
            }
            LastInput = message;
        }

        private static void Chat_OnMessage(AIHeroClient sender, ChatMessageEventArgs args)
        {
            if (Logger["EnabledLog"].Cast<CheckBox>().CurrentValue)
            {
                try
                {
                    AddToLog(args.Message, args.Sender);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error at adding log");
                }
            }

            if (IncomingText["Enabled"].Cast<CheckBox>().CurrentValue && !args.Sender.IsMe)
            {
                AddMessage(args.Message, args.Sender);
            }
            else
            {
                AddMessage(args.Message, args.Sender, false);
            }
        }

        private static void SetApiKey()
        {
            var apiKeyPath = EloBuddy.Sandbox.SandboxConfig.DataDirectory + @"\yandexApiKey.txt";
            if (!File.Exists(apiKeyPath))
            {
                File.Create(apiKeyPath);
            }

            Core.DelayAction(() =>
            {
                YandexApiKey = File.ReadLines(apiKeyPath).FirstOrDefault();
                if (YandexApiKey != null && YandexApiKey.Contains("trns"))
                {
                    YandexApiKey = "?key=" + YandexApiKey.Trim();
                }
                else
                {
                    YandexApiKey = "";
                }
                Test("hello");
            }, 100);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (TranslatorMenu["Check"].Cast<KeyBind>().CurrentValue || ShowMessages)
            {
                var posX = Position["Horizontal"].Cast<Slider>().CurrentValue;
                var posY = Position["Vertical"].Cast<Slider>().CurrentValue;
                var line = 0;
                foreach (var message in LastMessages)
                {
                    var textSize = TextRenderer.MeasureText(
                        message.Sender.Name + ":", new Font(FontFamily.GenericSansSerif, 10));

                    if (!message.Sender.IsAlly)
                    {
                        Drawing.DrawText(posX, posY + line, Color.Red, message.Sender.Name + ":");
                    }
                    else
                    {
                        if (message.Sender.IsMe)
                        {
                            Drawing.DrawText(posX, posY + line, Color.Goldenrod, message.Sender.Name + ":");
                        }
                        else
                        {
                            Drawing.DrawText(posX, posY + line, Color.DeepSkyBlue, message.Sender.Name + ":");
                        }
                    }
                    Drawing.DrawText(
                        posX + textSize.Width + message.Sender.Name.Length, posY + line, Color.White,
                        (message.Translated != message.Original ? message.Original : "") + " " + message.Translated);
                    line += 15;
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (CopyPaste["DisablePaste"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }
            if (Sent)
            {
                return;
            }
            var delay = CopyPaste["Delay"].Cast<Slider>().CurrentValue;
            if (CopyPaste["Paste"].Cast<KeyBind>().CurrentValue)
            {
                SetClipBoardData();
                if (!ClipBoard.Contains("\n"))
                {
                    Chat.Say(ClipBoard);
                    Sent = true;
                    Core.DelayAction(() => Sent = false, delay);
                }
                else
                {
                    foreach (var s in ClipBoardLines)
                    {
                        Chat.Say(s);
                    }
                    Sent = true;
                    Core.DelayAction(() => Sent = false, delay);
                    ClipBoardLines.Clear();
                }
            }
            if (CopyPaste["PasteForAll"].Cast<KeyBind>().CurrentValue)
            {
                SetClipBoardData();
                if (!ClipBoard.Contains("\n"))
                {
                    Chat.Say("/all " + ClipBoard);
                    Sent = true;
                    Core.DelayAction(() => Sent = false, delay);
                }
                else
                {
                    foreach (var s in ClipBoardLines)
                    {
                        Chat.Say("/all " + s);
                    }
                    Sent = true;
                    Core.DelayAction(() => Sent = false, delay);
                    ClipBoardLines.Clear();
                }
            }
        }

        private static void SetClipBoardData()
        {
            if (Clipboard.ContainsText())
            {
                ClipBoard = SetEncodingDefault(Clipboard.GetText());
                if (ClipBoard.Contains("\n"))
                {
                    ClipBoardLines = ClipBoard.Split('\n').ToList();
                }
            }
        }

        private static void OnMessage(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Position["AutoShow"].Cast<CheckBox>().CurrentValue && Translate)
            {
                ShowMessages = true;
                Core.DelayAction(() => ShowMessages = false, Position["Duration"].Cast<Slider>().CurrentValue);
            }
        }

/*
        private static string SetEncodingUtf8(string data)
        {
            var bytes = Encoding.Default.GetBytes(data);
            return Encoding.UTF8.GetString(bytes);
        }
*/

        private static string SetEncodingDefault(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            return Encoding.Default.GetString(bytes);
        }

        private static void InitText()
        {
            if (!File.Exists(Path + FileName))
            {
                var initText = "";
                var team = "";
                foreach (var hero in ObjectManager.Get<AIHeroClient>())
                {
                    if (team != hero.Team.ToString())
                    {
                        initText += "\t" + hero.Team.ToString() + "\n";
                        team = hero.Team.ToString();
                    }
                    initText += hero.Name + " (" + hero.ChampionName + ")\n";
                }
                initText += "------------------------\n";
                File.AppendAllText(Path + FileName, initText, Encoding.Default);
            }
        }

        private static void AddToLog(string message, AIHeroClient sender)
        {
            if (sender == null || message == null)
            {
                return;
            }
            InitText();
            var line = sender.Name + " (" + sender.ChampionName + ")" + ": " + message + "\n";
            File.AppendAllText(Path + FileName, line, Encoding.Default);
        }

        private static async void AddMessage(string message, Obj_AI_Base sender, bool shouldTranslate = true)
        {
            var from = FromArray[IncomingText["From"].Cast<Slider>().CurrentValue];
            var to = ToArray[IncomingText["To"].Cast<Slider>().CurrentValue];
            var translated = message;
            if (shouldTranslate)
            {
                translated = await TranslateYandex(message, from, to, true);
            }
            if (from != to && !sender.IsMe && translated != message)
            {
                Translate = true;
                Core.DelayAction(() => Translate = false, 500);
                LastMessages.Add(new Message(translated, sender, message));

                if (IncomingText["ShowInChat"].Cast<CheckBox>().CurrentValue)
                {
                    Chat.Print(translated);
                }
            }
            else
            {
                var last = new TestPerAll(LastInput);
                if (from != to && sender.IsMe && last.Output != message)
                {
                    LastMessages.Add(
                        new Message(String.Format("({0} => {1}) {2}", from, to, message), sender, last.Output));
                    if (IncomingText["ShowInChat"].Cast<CheckBox>().CurrentValue && translated != message)
                    {
                        Chat.Print(translated);
                    }
                }
                else
                {
                    LastMessages.Add(new Message(message, sender, message));
                    if (IncomingText["ShowInChat"].Cast<CheckBox>().CurrentValue && translated != message)
                    {
                        Chat.Print(translated);
                    }
                }
            }
            if (LastMessages.Count > 8)
            {
                LastMessages.RemoveAt(0);
            }
        }

        private static async void Test(string text)
        {
            if (text.Length > 1)
            {
                var from = ToArray[OutgoingText["OutFrom"].Cast<Slider>().CurrentValue];
                var to = ToArray[OutgoingText["OutTo"].Cast<Slider>().CurrentValue];
                var x = "";
                x += await TranslateYandex(text, from, to, false);
                Console.WriteLine(x);
            }
        }

        private static async void TranslateAndSend(string text)
        {
            if (text.Length > 1)
            {
                var all = false;
                var test = new TestPerAll(text);
                if (test.ContainsPerAll)
                {
                    text = test.Output;
                    all = true;
                }
                var from = ToArray[OutgoingText["OutFrom"].Cast<Slider>().CurrentValue];
                var to = ToArray[OutgoingText["OutTo"].Cast<Slider>().CurrentValue];
                var x = "";
                x += await TranslateYandex(text, from, to, false);
                x = SetEncodingDefault(x);
                if (all)
                {
                    Chat.Say("/all " + x);
                }
                else
                {
                    Chat.Say(x);
                }
            }
        }

        private static async Task<string> TranslateYandex(string text, string fromCulture, string toCulture, bool langs)
        {
            var lang = fromCulture == "auto" ? toCulture : fromCulture + "-" + toCulture;
            string key;
            tryAgain:
            var keyIndex = new Random().Next(0, YandexApiKeys.Count - 1);
            if (YandexApiKey != "")
            {
                key = YandexApiKey;
            }
            else
            {
                key = YandexApiKeys[keyIndex];
            }
            var strServerUrl = YandexUrl + key + "&lang=" + lang + "&text=" + text;
            var url = string.Format(strServerUrl, fromCulture, toCulture, text.Replace(' ', '+'));
            var bytessss = Encoding.Default.GetBytes(url);
            url = Encoding.UTF8.GetString(bytessss);
            var html = "";
            var uri = new Uri(url);
            try
            {
                html = await DownloadStringAsync(uri);
            }
            catch (Exception)
            {
                if (YandexApiKey != "")
                {
                    YandexApiKey = "";
                }
                else
                {
                    if (keyIndex == 0)
                    {
                        return text;
                    }
                    YandexUrl.Remove(keyIndex);
                }
                if (YandexUrl.Any())
                {
                    Console.WriteLine("One of the API key is wrong!");
                    goto tryAgain;
                }
            }

            var result = "";
            if (langs)
            {
                result += "(" + fromCulture + " => " + toCulture + ") ";
            }
            var code = Regex.Matches(html, "([0-9])\\d+", RegexOptions.IgnoreCase)[0].ToString();
            switch (int.Parse(code))
            {
                case 200:
                    var trans = Regex.Matches(html, "\\\".*?\\\"", RegexOptions.IgnoreCase)[4].ToString();
                    trans = trans.Substring(1, trans.Length - 2);
                    if (trans.Length == 0)
                    {
                        return "";
                    }
                    if (trans.Trim() == text.Trim())
                    {
                        return text;
                    }
                    result += trans;
                    return result;
                case 401:
                    Console.WriteLine("Invalid API key");
                    break;
                case 402:
                    Console.WriteLine("Blocked API key");
                    break;
                case 403:
                    Console.WriteLine("Exceeded the daily limit on the number of requests");
                    break;
                case 404:
                    Console.WriteLine("Exceeded the daily limit on the amount of translated text");
                    break;
                case 413:
                    Console.WriteLine("Exceeded the maximum text size");
                    break;
                case 422:
                    Console.WriteLine("The text cannot be translated");
                    break;
                case 501:
                    Console.WriteLine("The specified translation direction is not supported");
                    break;
                default:
                    return text;
            }
            if (YandexApiKey != "")
            {
                YandexApiKey = "";
            }
            else
            {
                if (keyIndex == 0)
                {
                    return text;
                }
                YandexUrl.Remove(keyIndex);
            }
            if (YandexUrl.Any())
            {
                Console.WriteLine("One of the API key is wrong2");
                goto tryAgain;
            }
            return text;
        }

        public static byte[] FromHex(string hex)
        {
            hex = hex.Replace(" ", "");
            var raw = new byte[hex.Length/2];
            for (var i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i*2, 2), 16);
            }
            return raw;
        }

        public static Task<string> DownloadStringAsync(Uri url)
        {
            var tcs = new TaskCompletionSource<string>();
            var wc = new WebClient();
            wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0");
            wc.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");
            wc.Encoding = Encoding.UTF8;
            wc.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    tcs.TrySetException(e.Error);
                }
                else if (e.Cancelled)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(e.Result);
                }
            };
            wc.DownloadStringAsync(url);

            return tcs.Task;
        }

/*
        private static double StringCompare(string a, string b)
        {
            if (a == b)
            {
                return 100;
            }
            if ((a.Length == 0) || (b.Length == 0))
            {
                return 0;
            }
            double maxLen = a.Length > b.Length ? a.Length : b.Length;
            var minLen = a.Length < b.Length ? a.Length : b.Length;
            var sameCharAtIndex = 0;
            for (var i = 0; i < minLen; i++)
            {
                if (a[i] == b[i])
                {
                    sameCharAtIndex++;
                }
            }
            return sameCharAtIndex/maxLen*100;
        }
*/

        private static void CreateMenu()
        {
            Config = MainMenu.AddMenu("Chat Translator", "Config");
            TranslatorMenu = Config.AddSubMenu("Translator", "Translator");
            IncomingText = Config.AddSubMenu("IncomingText", "IncomingText");
            OutgoingText = Config.AddSubMenu("OutgoingText", "OutgoingText");
            Position = Config.AddSubMenu("Position", "Position");
            Logger = Config.AddSubMenu("Logger", "Logger");
            CopyPaste = Config.AddSubMenu("Paste", "Paste");

            #region Translator Menu
            
            TranslatorMenu.Add("Check", new KeyBind("Check", true, KeyBind.BindTypes.HoldActive, 32));

            #endregion

            #region Incoming Text Menu

            IncomingText.AddLabel("From: ");
            var incomingFrom = IncomingText.Add("From", new Slider("From: ", 0, 0, 63));
            incomingFrom.DisplayName = FromArrayMenu[incomingFrom.CurrentValue];
            incomingFrom.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
            {
                sender.DisplayName = FromArrayMenu[changeArgs.NewValue];
            };

            IncomingText.AddLabel("To: ");
            var incomingTo = IncomingText.Add("To", new Slider("To: ", 0, 0, 62));
            incomingTo.DisplayName = ToArrayMenu[incomingFrom.CurrentValue];
            incomingTo.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
            {
                sender.DisplayName = ToArrayMenu[changeArgs.NewValue];
            };
            IncomingText.Add("ShowInChat", new CheckBox("Show in chat", false));
            IncomingText.Add("Enabled", new CheckBox("Enabled"));

            #endregion

            #region Outgoing Text Menu

            OutgoingText.AddLabel("From: ");
            var outgoingFrom = OutgoingText.Add("OutFrom", new Slider("From: ", 0, 0, 63));
            outgoingFrom.DisplayName = FromArrayMenu[outgoingFrom.CurrentValue];
            outgoingFrom.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
            {
                sender.DisplayName = FromArrayMenu[changeArgs.NewValue];
            };

            OutgoingText.AddLabel("To: ");
            var outgoingTo = OutgoingText.Add("OutTo", new Slider("To: ", 0, 0, 62));
            outgoingTo.DisplayName = ToArrayMenu[outgoingTo.CurrentValue];
            outgoingTo.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
            {
                sender.DisplayName = ToArrayMenu[changeArgs.NewValue];
            };
            OutgoingText.Add("EnabledOut", new CheckBox("Enabled", false));

            #endregion

            #region Position

            Position.Add("Horizontal", new Slider("Horizontal", 15, 1, 2000));
            Position.Add("Vertical", new Slider("Vertical", 500, 1, 2000));
            Position.Add("AutoShow", new CheckBox("Show on message"));
            Position.Add("Duration", new Slider("Duration", 3000, 1000, 8000));

            #endregion

            #region Logger

            Logger.Add("EnabledLog", new CheckBox("Enable"));

            #endregion

            #region Copy Paste

            CopyPaste.Add("Paste", new KeyBind("Paste", false, KeyBind.BindTypes.PressToggle, 'P'));
            CopyPaste.Add("PasteForAll", new KeyBind("Paste for all", false, KeyBind.BindTypes.PressToggle, 'O'));
            CopyPaste.Add("Delay", new Slider("Spam delay", 2000, 0, 2000));
            CopyPaste.Add("DisablePaste", new CheckBox("Disable this section"));
            Config.AddLabel("You can use your own API key");
            Config.AddLabel(
                "AppData\\Roaming\\EloBuddy\\yandexApiKey.txt, copy into the first line \"trnsl.1.1.201...\"");

            #endregion
        }
    }

    internal class TestPerAll
    {
        public string Input;
        public string Output;
        public bool ContainsPerAll;

        public TestPerAll(string input)
        {
            var regex = new Regex("/all", RegexOptions.IgnoreCase);
            var contains = regex.IsMatch(input);
            var replaced = regex.Replace(input, "");
            Input = input;
            Output = replaced.Trim();
            ContainsPerAll = contains;
        }
    }
}