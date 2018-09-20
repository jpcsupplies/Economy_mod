namespace EconomyConfigurationEditor.Support
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class FrameworkExtension
    {
        public static decimal ToDecimal(this float value)
        {
            return Convert.ToDecimal(value.ToString("G9", null));
        }

        public static double ToDouble(this float value)
        {
            return Convert.ToDouble(value.ToString("G9", null));
        }

        public static byte RoundUpToNearest(this byte value, int scale)
        {
            return (byte)(Math.Min(0xff, Math.Ceiling((double)value / scale) * scale));
        }

        
        /// <summary>
        /// Adds an element with the provided key and value to the System.Collections.Generic.IDictionary&gt;TKey,TValue&lt;.
        /// If the provide key already exists, then the existing key is updated with the newly supplied value.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="System.ArgumentNullException">key is null</exception>
        /// <exception cref="System.NotSupportedException">The System.Collections.Generic.IDictionary&gt;TKey,TValue&lt; is read-only.</exception>
        public static void Update<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        /// <summary>
        /// Concatenates the Message portion of each exception and inner exception together into a string, in much the same manner as .ToString() except without the stack.
        /// </summary>
        public static string AllMessages(this Exception exception)
        {
            Exception ex = exception;

            StringBuilder text = new StringBuilder();
            text.Append(ex.Message);
            while (ex.InnerException != null)
            {
                text.AppendLine();
                text.Append(" ---> ");
                text.AppendLine(ex.InnerException.Message);

                if (ex.InnerException is InvalidOperationException)
                {
                    text.AppendLine("Stack:");
                    text.AppendLine(ex.InnerException.StackTrace);
                }

                ex = ex.InnerException;
            }

            return text.ToString();
        }
    }
}