namespace EconomyConfigurationEditor.Interop
{
    using EconomyConfigurationEditor.Support;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using VRage;
    using VRage.FileSystem;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    /// <summary>
    /// Helper api for accessing and interacting with Space Engineers content.
    /// </summary>
    public static class SpaceEngineersApi
    {
        #region Serializers

        /// <returns>True if it sucessfully deserialized the file.</returns>
        public static bool TryReadSpaceEngineersFile<T>(string filename, out T outObject, out bool isCompressed, out string errorInformation, bool snapshot = false, bool specificExtension = false) where T : MyObjectBuilder_Base
        {
            string protoBufFile = null;
            if (specificExtension)
            {
                if ((Path.GetExtension(filename) ?? string.Empty).EndsWith(SpaceEngineersConsts.ProtobuffersExtension, StringComparison.OrdinalIgnoreCase))
                    protoBufFile = filename;
            }
            else
            {
                if ((Path.GetExtension(filename) ?? string.Empty).EndsWith(SpaceEngineersConsts.ProtobuffersExtension, StringComparison.OrdinalIgnoreCase))
                    protoBufFile = filename;
                else
                    protoBufFile = filename + SpaceEngineersConsts.ProtobuffersExtension;
            }

            if (protoBufFile != null && File.Exists(protoBufFile))
            {
                var tempFilename = protoBufFile;

                if (snapshot)
                {
                    // Snapshot used for Report on Dedicated servers to prevent locking of the orginal file whilst reading it.
                    tempFilename = TempfileUtil.NewFilename();
                    File.Copy(protoBufFile, tempFilename);
                }

                using (var fileStream = new FileStream(tempFilename, FileMode.Open, FileAccess.Read))
                {
                    var b1 = fileStream.ReadByte();
                    var b2 = fileStream.ReadByte();
                    isCompressed = (b1 == 0x1f && b2 == 0x8b);
                }

                bool retCode;
                try
                {
                    // A failure to load here, will only mean it falls back to try and read the xml file instead.
                    // So a file corruption could easily have been covered up.
                    retCode = MyObjectBuilderSerializer.DeserializePB<T>(tempFilename, out outObject);
                }
                catch (InvalidCastException ex)
                {
                    outObject = null;
                    errorInformation = $@"Failed to load file: {filename}
Reason: {ex.AllMessages()}";
                    return false;
                }
                if (retCode && outObject != null)
                {
                    errorInformation = null;
                    return true;
                }
                return TryReadSpaceEngineersFileXml(filename, out outObject, out isCompressed, out errorInformation, snapshot);
            }

            return TryReadSpaceEngineersFileXml(filename, out outObject, out isCompressed, out errorInformation, snapshot);
        }

        private static bool TryReadSpaceEngineersFileXml<T>(string filename, out T outObject, out bool isCompressed, out string errorInformation, bool snapshot = false) where T : MyObjectBuilder_Base
        {
            isCompressed = false;

            if (File.Exists(filename))
            {
                var tempFilename = filename;

                if (snapshot)
                {
                    // Snapshot used for Report on Dedicated servers to prevent locking of the orginal file whilst reading it.
                    tempFilename = TempfileUtil.NewFilename();
                    File.Copy(filename, tempFilename);
                }

                using (var fileStream = new FileStream(tempFilename, FileMode.Open, FileAccess.Read))
                {
                    var b1 = fileStream.ReadByte();
                    var b2 = fileStream.ReadByte();
                    isCompressed = (b1 == 0x1f && b2 == 0x8b);
                }

                return DeserializeXml<T>(tempFilename, out outObject, out errorInformation);
            }

            errorInformation = null;
            outObject = null;
            return false;
        }

        public static T Deserialize<T>(string xml) where T : MyObjectBuilder_Base
        {
            T outObject;
            using (var stream = new MemoryStream())
            {
                StreamWriter sw = new StreamWriter(stream);
                sw.Write(xml);
                sw.Flush();
                stream.Position = 0;

                MyObjectBuilderSerializer.DeserializeXML(stream, out outObject);
            }
            return outObject;
        }

        public static string Serialize<T>(MyObjectBuilder_Base item) where T : MyObjectBuilder_Base
        {
            using (var outStream = new MemoryStream())
            {
                if (MyObjectBuilderSerializer.SerializeXML(outStream, item))
                {
                    outStream.Position = 0;

                    StreamReader sw = new StreamReader(outStream);
                    return sw.ReadToEnd();
                }
            }
            return null;
        }

        public static bool WriteSpaceEngineersFile<T>(T myObject, string filename)
            where T : MyObjectBuilder_Base
        {
            bool ret;
            using (StreamWriter sw = new StreamWriter(filename))
            {
                ret = MyObjectBuilderSerializer.SerializeXML(sw.BaseStream, myObject);
                if (ret)
                {
                    var xmlTextWriter = new XmlTextWriter(sw.BaseStream, null);
                    xmlTextWriter.WriteString("\r\n");
                    xmlTextWriter.WriteComment($" Saved '{DateTime.Now:o}' with SEToolbox version '{GlobalSettings.GetAppVersion()}' ");
                    xmlTextWriter.Flush();
                }
            }

            return true;
        }

        public static bool WriteSpaceEngineersFilePB<T>(T myObject, string filename, bool compress)
            where T : MyObjectBuilder_Base
        {
            return MyObjectBuilderSerializer.SerializePB(filename, compress, myObject);
        }

        /// <returns>True if it sucessfully deserialized the file.</returns>
        public static bool DeserializeXml<T>(string filename, out T objectBuilder, out string errorInformation) where T : MyObjectBuilder_Base
        {
            bool result = false;
            objectBuilder = null;
            errorInformation = null;

            using (var fileStream = MyFileSystem.OpenRead(filename))
            {
                if (fileStream != null)
                    using (var readStream = fileStream.UnwrapGZip())
                    {
                        if (readStream != null)
                        {
                            try
                            {
                                XmlSerializer serializer = MyXmlSerializerManager.GetSerializer(typeof(T));

                                XmlReaderSettings settings = new XmlReaderSettings { CheckCharacters = true };
                                MyXmlTextReader xmlReader = new MyXmlTextReader(readStream, settings);

                                objectBuilder = (T)serializer.Deserialize(xmlReader);
                                result = true;
                            }
                            catch (Exception ex)
                            {
                                objectBuilder = null;
                                errorInformation = $@"Failed to load file: {filename}
Reason: {ex.AllMessages()}";
                            }
                        }
                    }
            }

            return result;
        }

        #endregion

        #region GenerateEntityId

        public static long GenerateEntityId(VRage.MyEntityIdentifier.ID_OBJECT_TYPE type)
        {
            return MyEntityIdentifier.AllocateId(type);
        }

        public static bool ValidateEntityType(VRage.MyEntityIdentifier.ID_OBJECT_TYPE type, long id)
        {
            return MyEntityIdentifier.GetIdObjectType(id) == type;
        }

        #endregion

        #region GetResourceName

        public static string GetResourceName(string value)
        {
            if (value == null)
                return null;

            MyStringId stringId = MyStringId.GetOrCompute(value);
            return MyTexts.GetString(stringId);
        }

        // Reflection copy of MyTexts.AddLanguage
        private static void AddLanguage(MyLanguagesEnum id, string cultureName, string subcultureName = null, string displayName = null, float guiTextScale = 1f, bool isCommunityLocalized = true)
        {
            // Create an empty instance of LanguageDescription.
            MyTexts.LanguageDescription languageDescription = ReflectionUtil.ConstructPrivateClass<MyTexts.LanguageDescription>(
                new Type[] { typeof(MyLanguagesEnum), typeof(string), typeof(string), typeof(string), typeof(float), typeof(bool) },
                new object[] { id, displayName, cultureName, subcultureName, guiTextScale, isCommunityLocalized });

            Dictionary<MyLanguagesEnum, MyTexts.LanguageDescription> m_languageIdToLanguage = typeof(MyTexts).GetStaticField<Dictionary<MyLanguagesEnum, MyTexts.LanguageDescription>>("m_languageIdToLanguage");
            Dictionary<string, MyLanguagesEnum> m_cultureToLanguageId = typeof(MyTexts).GetStaticField<Dictionary<string, MyLanguagesEnum>>("m_cultureToLanguageId");

            if (!m_languageIdToLanguage.ContainsKey(id))
            {
                m_languageIdToLanguage.Add(id, languageDescription);
                m_cultureToLanguageId.Add(languageDescription.FullCultureName, id);
            }
        }

        #endregion
    }
}