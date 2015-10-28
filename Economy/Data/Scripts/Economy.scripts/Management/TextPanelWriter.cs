namespace Economy.scripts.Management
{
    // Avoid using Linq if possible, then this can be copied into an Ingame Programmable block script.
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;

    public class TextPanelWriter
    {
        #region fields

        private const bool ForceRedraw = true;

        // Spacing looks to be something Keen planned to implement as configurable to the Font, but haven't and left Spacing defaulted as 1.
        private const int Spacing = 1;

        private static readonly int WhitespaceWidth;

        private const int DefaultCharWidth = 40;

        public const int LcdLineWidth = 652; // this is approximate.

        private const string Elipse = "\x2026"; // the ... symbol.
        private static readonly int ElipseSize;

        private readonly IMyTextPanel _panel;
        private readonly StringBuilder _publicString;
        private readonly StringBuilder _privateString;
        private readonly bool _isWide;

        // AdvanceWidth is defined as Byte in VRageRender.MyFont.
        private static readonly Dictionary<char, byte> FontCharWidth = new Dictionary<char, byte>();

        private static readonly Dictionary<IMyTextPanel, TextPanelWriter> TextPanelWriterCache = new Dictionary<IMyTextPanel, TextPanelWriter>();

        #endregion

        #region ctor

        /// <summary>
        /// This will find an existing TextPanelWriter for the specified IMyTextPanel, or create one if one doesn't already exist.
        /// </summary>
        public static TextPanelWriter Create(IMyTextPanel textPanel)
        {
            TextPanelWriter writer;
            if (TextPanelWriterCache.TryGetValue(textPanel, out writer))
            {
                writer.Clear();
                return writer;
            }

            writer = new TextPanelWriter(textPanel);
            TextPanelWriterCache.Add(textPanel, writer);
            return writer;
        }

        private TextPanelWriter(IMyTextPanel panel)
        {
            _panel = panel;
            _isWide = _panel.DefinitionDisplayNameText.Contains("Wide") || _panel.DefinitionDisplayNameText == "Computer Monitor";
            _publicString = new StringBuilder();
            _privateString = new StringBuilder();
            Clear();
        }

        ~TextPanelWriter()
        {
            if (TextPanelWriter.TextPanelWriterCache.ContainsKey(_panel))
                TextPanelWriter.TextPanelWriterCache.Remove(_panel);
        }

        static TextPanelWriter()
        {
            LoadCharWidths();
            WhitespaceWidth = MeasureString(" ");
            ElipseSize = MeasureString(Elipse);
        }

        // TODO: needs to be called routinely at long intervals, to clean up the cache.
        private static void CleanupCache()
        {
            var list = new List<IMyTextPanel>();
            foreach (var kvp in TextPanelWriterCache)
            {
                if (kvp.Key.Closed)
                    list.Add(kvp.Key);
            }

            foreach (var item in list)
                TextPanelWriterCache.Remove(item);
        }

        #endregion

        #region properties

        public float WidthModifier
        {
            get { return (_isWide ? 2f : 1f) / FontSize; }
        }

        public int DisplayLines
        {
            get { return (int)Math.Round(17.6f / FontSize); }
        }

        public float FontSize { get; private set; }

        #endregion

        #region Set Public text

        public void AddPublicText(string text, params object[] args)
        {
            _publicString.AppendFormat(text, args);
        }

        public void AddPublicLine()
        {
            _publicString.AppendLine();
        }

        public void AddPublicLine(string text, params object[] args)
        {
            _publicString.AppendLine(StringFormatter(text, args));
        }

        public void AddPublicLeftTrim(float desiredWidth, string text, params object[] args)
        {
            _publicString.Append(GetStringTrimmed(desiredWidth * WidthModifier, StringFormatter(text, args)));
        }

        public void AddPublicRightText(float rightEdgePosition, string text, params object[] args)
        {
            AddRightAlign(_publicString, rightEdgePosition, StringFormatter(text, args));
        }

        public void AddPublicRightLine(float rightEdgePosition, string text, params object[] args)
        {
            AddRightAlign(_publicString, rightEdgePosition, StringFormatter(text, args));
            _publicString.AppendLine();
        }

        public void AddPublicCenterText(float centerPosition, string text, params object[] args)
        {
            AddCenterAlign(_publicString, centerPosition, StringFormatter(text, args));
        }

        public void AddPublicCenterLine(float centerPosition, string text, params object[] args)
        {
            AddCenterAlign(_publicString, centerPosition, StringFormatter(text, args));
            _publicString.AppendLine();
        }

        public void AddPublicFill(string left, char fill, string right)
        {
            AddFillText(_publicString, left, fill, right);
            _publicString.AppendLine();
        }

        public void ClearPublicText()
        {
            _publicString.Clear();
        }

        public string GetPublicString()
        {
            return _publicString.ToString();
        }

        // TODO: display n to m lines.
        public void UpdatePublic(bool show = true)
        {
            _panel.SetValueFloat("FontSize", FontSize);
            _panel.WritePublicText(_publicString.ToString());

            if (show)
            {
                if (ForceRedraw)
                    _panel.ShowTextureOnScreen();
                _panel.ShowPublicTextOnScreen();
            }
        }

        #endregion

        #region Set Private text

        public void AddPrivateText(string text, params object[] args)
        {
            _privateString.AppendFormat(text, args);
        }

        public void AddPrivateLine(string text)
        {
            _privateString.AppendLine(text);
        }

        public void ClearPrivateText()
        {
            _privateString.Clear();
        }

        public string GetPrivateString()
        {
            return _privateString.ToString();
        }

        public void UpdatePrivate(bool show = false)
        {
            _panel.SetValueFloat("FontSize", FontSize);
            _panel.WritePublicText(_privateString.ToString());

            if (show)
            {
                if (ForceRedraw)
                    _panel.ShowTextureOnScreen();
                _panel.ShowPrivateTextOnScreen();
            }
        }

        #endregion

        #region set image

        public void UpdateImage(float interval, List<string> images)
        {
            _panel.ClearImagesFromSelection();
            _panel.ShowPublicTextOnScreen();
            _panel.SetValueFloat("ChangeIntervalSlider", interval);
            _panel.AddImagesToSelection(images, true); // This truely acts weird.
            _panel.ShowTextureOnScreen();
        }

        #endregion

        #region methods

        public void Clear()
        {
            FontSize = _panel.GetValueFloat("FontSize");
            _publicString.Clear();
            _privateString.Clear();
        }

        public void SetFontSize(float size)
        {
            FontSize = size;
        }

        #endregion

        #region panel helper methods

        // This protects us from stupidity, like forgetting arguments or passing null.
        private static string StringFormatter(string text, params object[] args)
        {
            if (args == null || args.Length == 0)
                return text;

            return string.Format(text, args);
        }

        private static string LastLine(StringBuilder stringBuilder)
        {
            var lastIndex = stringBuilder.ToString().LastIndexOf("\n", StringComparison.Ordinal);
            return lastIndex == -1 ? stringBuilder.ToString() : stringBuilder.ToString().Substring(lastIndex + 1);
        }

        private void AddRightAlign(StringBuilder stringBuilder, float rightEdgePosition, string text)
        {
            var curWidth = MeasureString(LastLine(stringBuilder));
            float textWidth = MeasureString(text);
            rightEdgePosition *= WidthModifier;
            rightEdgePosition -= curWidth;

            if (rightEdgePosition < textWidth)
            {
                stringBuilder.Append(text);
                return;
            }

            rightEdgePosition -= textWidth;
            int fillchars = (int)Math.Round(rightEdgePosition / (Spacing + WhitespaceWidth), MidpointRounding.AwayFromZero);
            string filler = new string(' ', fillchars);
            stringBuilder.Append(filler + text);
        }

        private void AddCenterAlign(StringBuilder stringBuilder, float centerPosition, string text)
        {
            var curWidth = MeasureString(LastLine(stringBuilder));
            float textWidth = MeasureString(text);
            centerPosition *= WidthModifier;
            centerPosition -= curWidth;

            if (centerPosition < textWidth / 2)
            {
                stringBuilder.Append(text);
                return;
            }

            centerPosition -= textWidth / 2;
            int fillchars = (int)Math.Round(centerPosition / (Spacing + WhitespaceWidth), MidpointRounding.AwayFromZero);
            string filler = new string(' ', fillchars);
            stringBuilder.Append(filler + text);
        }

        private void AddFillText(StringBuilder stringBuilder, string left, char fill, string right)
        {
            var curWidth = MeasureString(LastLine(stringBuilder)) + MeasureString(left) + MeasureString(right);
            var fillSpace = (LcdLineWidth - curWidth) * WidthModifier;
            var fillWidth = MeasureChar(fill);
            int fillchars = (int)Math.Round(fillSpace / (Spacing + fillWidth), MidpointRounding.AwayFromZero);
            string filler = new string(fill, fillchars);
            stringBuilder.Append(left + filler + right);
        }

        public static string GetStringTrimmed(float desiredWidth, string text)
        {
            float stringSize = MeasureString(text);
            if (stringSize <= desiredWidth)
                return text;

            int trimLength = text.Length;

            while (stringSize > desiredWidth - ElipseSize)
            {
                stringSize -= MeasureString(text.Substring(trimLength));
                text = text.Substring(0, trimLength);
                trimLength--;
            }

            return text + Elipse;
        }

        #endregion

        #region LoadCharWidths

        private static void LoadCharWidths()
        {
            if (FontCharWidth.Count > 0)
                return;

            AddCharWidth(" !I`ijl\xa0\xa1\xa8\xaf\xb4\xb8\xcc\xcd\xce\xcf\xec\xed\xee\xef\x128\x129\x12a\x12b\x12e\x12f\x130\x131\x135\x13a\x13c\x13e\x142\x2c6\x2c7\x2d8\x2d9\x2da\x2db\x2dc\x2dd\x406\x407\x456\x457\x2039\x203a\x2219", 8);
            AddCharWidth("\"-r\xaa\xad\xba\x140\x155\x157\x159", 10);
            AddCharWidth("#0245689CXZ\xa4\xa5\xc7\xdf\x106\x108\x10a\x10c\x179\x17b\x17d\x192\x401\x40c\x410\x411\x412\x414\x418\x419\x41f\x420\x421\x422\x423\x425\x42c\x20ac", 19);
            AddCharWidth("$&GHPUVY\xa7\xd9\xda\xdb\xdc\xde\x100\x11c\x11e\x120\x122\x124\x126\x168\x16a\x16c\x16e\x170\x172\x41e\x424\x426\x42a\x42f\x436\x44b\x2020\x2021", 20);
            AddCharWidth("%\x132\x42b", 24);
            AddCharWidth("'|\xa6\x2c9\x2018\x2019\x201a", 6);
            AddCharWidth("(),.1:;[]ft{}\xb7\x163\x165\x167\x21b", 9);
            AddCharWidth("*\xb2\xb3\xb9", 11);
            AddCharWidth("+<=>E^~\xac\xb1\xb6\xc8\xc9\xca\xcb\xd7\xf7\x112\x114\x116\x118\x11a\x404\x40f\x415\x41d\x42d\x2212", 18);
            AddCharWidth("/\x133\x442\x44d\x454", 14);
            AddCharWidth("3FKTabdeghknopqsuy\xa3\xb5\xdd\xe0\xe1\xe2\xe3\xe4\xe5\xe8\xe9\xea\xeb\xf0\xf1\xf2\xf3\xf4\xf5\xf6\xf8\xf9\xfa\xfb\xfc\xfd\xfe\xff\x101\x103\x105\x10f\x111\x113\x115\x117\x119\x11b\x11d\x11f\x121\x123\x125\x127\x136\x137\x144\x146\x148\x149\x14d\x14f\x151\x15b\x15d\x15f\x161\x162\x164\x166\x169\x16b\x16d\x16f\x171\x173\x176\x177\x178\x219\x21a\x40e\x417\x41a\x41b\x431\x434\x435\x43a\x440\x443\x446\x44f\x451\x452\x45b\x45e\x45f", 17);
            AddCharWidth("7?Jcz\xa2\xbf\xe7\x107\x109\x10b\x10d\x134\x17a\x17c\x17e\x403\x408\x427\x430\x432\x438\x439\x43d\x43e\x43f\x441\x44a\x44c\x453\x455\x45c", 16);
            AddCharWidth("@\xa9\xae\x43c\x448\x45a", 25);
            AddCharWidth("ABDNOQRS\xc0\xc1\xc2\xc3\xc4\xc5\xd0\xd1\xd2\xd3\xd4\xd5\xd6\xd8\x102\x104\x10e\x110\x143\x145\x147\x14c\x14e\x150\x154\x156\x158\x15a\x15c\x15e\x160\x218\x405\x40a\x416\x444\x25a1", 21);
            AddCharWidth("L_vx\xab\xbb\x139\x13b\x13d\x13f\x141\x413\x433\x437\x43b\x445\x447\x490\x2013\x2022", 15);
            AddCharWidth("M\x41c\x428", 26);
            AddCharWidth("W\xc6\x152\x174\x2014\x2026\x2030", 31);
            AddCharWidth("\\\xb0\x201c\x201d\x201e", 12);
            AddCharWidth("mw\xbc\x175\x42e\x449", 27);
            AddCharWidth("\xbd\x429", 29);
            AddCharWidth("\xbe\xe6\x153\x409", 28);
            AddCharWidth("\x44e", 23);
            AddCharWidth("\x458", 7);
            AddCharWidth("\x459", 22);
            AddCharWidth("\x491", 13);
            AddCharWidth("\x2122", 30);
            AddCharWidth("\xe001\xe002\xe003\xe004\xe009\xe00a\xe00d\xe00e\xe00f\xe014\xe015\xe016\xe017\xe018\xe019\xe020\xe021", 40);
            AddCharWidth("\xe005\xe006\xe010\xe012", 41);
            AddCharWidth("\xe007\xe008\xe011\xe013", 32);
            AddCharWidth("\xe00b\xe00c", 34);
        }

        private static void BuildFontWidthCatalog()
        {
            // This is commented out, as it's only used to generate the LoadCharWidths() content.

            //var definition = @"C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers\Content\Fonts\white\FontData.xml";

            //StringBuilder str = new StringBuilder();

            //var doc = new System.Xml.XmlDocument();
            //doc.Load(definition);
            //var ns = new System.Xml.XmlNamespaceManager(doc.NameTable);
            //ns.AddNamespace("p", "http://xna.microsoft.com/bitmapfont");
            //var nav = doc.CreateNavigator();

            //var charSize = new Dictionary<byte, List<char>>();

            //var itemsNode = nav.Select("/p:font/p:glyphs/p:glyph", ns);
            //while (itemsNode.MoveNext())
            //{
            //    var strChar = itemsNode.Current.GetAttribute("ch", "");
            //    var strCode = itemsNode.Current.GetAttribute("code", "");
            //    var strAW = itemsNode.Current.GetAttribute("aw", "");
            //    var pxAdvanceWidth = Byte.Parse(strAW);

            //    if (!charSize.ContainsKey(pxAdvanceWidth))
            //        charSize.Add(pxAdvanceWidth, new List<char>());
            //    charSize[pxAdvanceWidth].Add(strChar[0]);
            //}

            ////charSize = charSize.OrderBy(e => e.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            //foreach (KeyValuePair<byte, List<char>> kvp in charSize)
            //{
            //    str.Append("AddCharWidth(\"");

            //    foreach (char c in kvp.Value)
            //    {
            //        var i = (int)c;
            //        if (i == 34)
            //            str.Append("\\\"");
            //        else if (i == 92)
            //            str.Append("\\\\");
            //        else if (i >= 32 && i <= 126)
            //            str.Append(c);
            //        else
            //            str.AppendFormat("\\x{0:x}", i);
            //    }
            //    str.AppendFormat("\", {0});\r\n", kvp.Key);
            //}

            ////AddCharWidth("ABDNOQRSÀÁÂÃÄÅÐÑÒÓÔÕÖØĂĄĎĐŃŅŇŌŎŐŔŖŘŚŜŞŠȘЅЊЖф□", 21);

            //// You are going to want to pause here and read the output of str.ToString();
            //Console.ReadKey();
        }

        private static void AddCharWidth(string chars, byte size)
        {
            for (int i = 0; i < chars.Length; i++)
                FontCharWidth.Add(chars[i], size);
        }

        #endregion

        #region character helper methods

        private static byte MeasureChar(char c)
        {
            byte width;
            if (!FontCharWidth.TryGetValue(c, out width))
                width = DefaultCharWidth;
            return width;
        }

        // Derived from VRageRender.MyFont.MeasureString
        public static int MeasureString(string str)
        {
            int sum = 0;
            for (int i = 0; i < str.Length; i++)
                sum += MeasureChar(str[i]);

            //  Spacing is applied to every character, except the last.
            sum += Spacing * (str.Length > 1 ? str.Length - 1 : 0);
            return sum;
        }

        #endregion
    }
}
