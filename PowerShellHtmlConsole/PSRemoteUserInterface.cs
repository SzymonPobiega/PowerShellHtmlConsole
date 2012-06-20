// <copyright file="MyHostUserInterface.cs" company="Microsoft Corporation">
// Copyright (c) 2009 Microsoft Corporation. All rights reserved.
// </copyright>
// DISCLAIMER OF WARRANTY: The software is licensed “as-is.” You 
// bear the risk of using it. Microsoft gives no express warranties, 
// guarantees or conditions. You may have additional consumer rights 
// under your local laws which this agreement cannot change. To the extent 
// permitted under your local laws, Microsoft excludes the implied warranties 
// of merchantability, fitness for a particular purpose and non-infringement.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;
using System.Threading;
using log4net;
using Size = System.Management.Automation.Host.Size;

namespace PowerShellHtmlConsole
{
    public class PSRemoteUserInterface : PSHostUserInterface
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PSWrapper));

        private static readonly Dictionary<ConsoleColor, Color> ColorMap
            = new Dictionary<ConsoleColor, Color>()
                  {
                      {ConsoleColor.Black, Color.Black},
                      {ConsoleColor.Blue, Color.Blue},
                      {ConsoleColor.Cyan, Color.Cyan},
                      {ConsoleColor.DarkBlue, Color.DarkBlue},
                      {ConsoleColor.DarkCyan, Color.DarkCyan},
                      {ConsoleColor.DarkGray, Color.DarkGray},
                      {ConsoleColor.DarkGreen, Color.DarkGreen},
                      {ConsoleColor.DarkMagenta, Color.DarkMagenta},
                      {ConsoleColor.DarkRed, Color.DarkRed},
                      {ConsoleColor.DarkYellow, Color.YellowGreen},
                      {ConsoleColor.Gray, Color.Gray},
                      {ConsoleColor.Green, Color.Green},
                      {ConsoleColor.Magenta, Color.Magenta},
                      {ConsoleColor.Red, Color.Red},
                      {ConsoleColor.White, Color.White},
                      {ConsoleColor.Yellow, Color.Yellow},
                  };

        private readonly InputOutputBuffers _buffers;
        private readonly PSRemoteRawUserInterface _rawUI = new PSRemoteRawUserInterface();

        public PSRemoteUserInterface(InputOutputBuffers buffers)
        {
            _buffers = buffers;
            _buffers.InterceptInCommand(cmd =>
                                            {
                                                _rawUI.BufferSize = new Size(cmd.Columns, 25);
                                            });
        }

        /// <summary>
        /// Gets an instance of the PSRawUserInterface object for this host
        /// application.
        /// </summary>
        public override PSHostRawUserInterface RawUI
        {
            get { return _rawUI; }
        }

        /// <summary>
        /// Prompts the user for input.
        /// </summary>
        /// <param name="caption">Text that preceeds the prompt (a title).</param>
        /// <param name="message">Text of the prompt.</param>
        /// <param name="descriptions">A collection of FieldDescription objects 
        /// that contains the user input.</param>
        /// <returns>A dictionary object that contains the results of the user prompts.</returns>
        public override Dictionary<string, PSObject> Prompt(
                       string caption,
                       string message,
                       Collection<FieldDescription> descriptions)
        {
            if (!string.IsNullOrEmpty(message))
            {
                WriteLine(message);
            }
            var results = new Dictionary<string, PSObject>();
            foreach (var fd in descriptions)
            {
                var label = GetHotkeyAndLabel(string.IsNullOrEmpty(fd.Label) ? fd.Name : fd.Label);
                var userData = ReadLineInternal(false, label.Item2 + ": ");
                results[fd.Name] = PSObject.AsPSObject(userData);
            }
            return results;
        }

        /// <summary>
        /// Provides a set of choices that enable the user to choose a single option 
        /// from a set of options. 
        /// </summary>
        /// <param name="caption">A title that proceeds the choices.</param>
        /// <param name="message">An introduction  message that describes the 
        /// choices.</param>
        /// <param name="choices">A collection of ChoiceDescription objects that describ 
        /// each choice.</param>
        /// <param name="defaultChoice">The index of the label in the Choices parameter 
        /// collection that indicates the default choice used if the user does not specify 
        /// a choice. To indicate no default choice, set to -1.</param>
        /// <returns>The index of the Choices parameter collection element that corresponds 
        /// to the option that is selected by the user.</returns>
        public override int PromptForChoice(
                                            string caption,
                                            string message,
                                            Collection<ChoiceDescription> choices,
                                            int defaultChoice)
        {
            if (!string.IsNullOrEmpty(message))
            {
                WriteLine(message);
            }

            var hotKeysAndOptions = BuildHotkeysAndPlainLabels(choices).ToList();

            var optionsText = string.Join("   ",
                                          hotKeysAndOptions.Select(
                                              (x, i) => FormatWithColor(String.Format("<{0}> {1}   ", x.Item1, x.Item2),
                                                                        i == defaultChoice ? Color.Yellow : Color.White,
                                                                        null)));

            var defaultOptionHint = "";
            if (defaultChoice >= 0 && defaultChoice < choices.Count)
            {
                defaultOptionHint = " (default is \"" + hotKeysAndOptions[defaultChoice].Item1 + "\")";
            }
            
            while (true)
            {
                _buffers.QueueOutCommand(OutCommand.CreatePrint(optionsText + defaultOptionHint));
                _buffers.QueueOutCommand(OutCommand.CreateReadLine(false, null));

                var data = ReadLineInternal(false).ToUpper();
                if (data.Length == 0)
                {
                    return defaultChoice;
                }
                var optionIndex = hotKeysAndOptions.FindIndex(x => x.Item1 == data);
                if (optionIndex >= 0)
                {
                    return optionIndex;
                }
                optionIndex = hotKeysAndOptions.FindIndex(x => x.Item2 == data);
                if (optionIndex >= 0)
                {
                    return optionIndex;
                }
                WriteErrorLine("Invalid choice: " + data);
            }
        }

        /// <summary>
        /// Prompts the user for credentials with a specified prompt window 
        /// caption, prompt message, user name, and target name.
        /// </summary>
        /// <param name="caption">The caption of the message window.</param>
        /// <param name="message">The text of the message.</param>
        /// <param name="userName">The user name whose credential is to be prompted for.</param>
        /// <param name="targetName">The name of the target for which the credential is collected.</param>
        /// <returns>Throws a NotImplementException exception.</returns>
        public override PSCredential PromptForCredential(
                                     string caption,
                                     string message,
                                     string userName,
                                     string targetName)
        {
            throw new NotImplementedException(
              "The method PromptForCredential() is not implemented by MyHost.");
        }

        /// <summary>
        /// Prompts the user for credentials by using a specified prompt window 
        /// caption, prompt message, user name and target name, credential types 
        /// allowed to be returned, and UI behavior options.
        /// </summary>
        /// <param name="caption">The caption of the message window.</param>
        /// <param name="message">The text of the message.</param>
        /// <param name="userName">The user name whose credential is to be prompted for.</param>
        /// <param name="targetName">The name of the target for which the credential is collected.</param>
        /// <param name="allowedCredentialTypes">PSCredentialTypes cconstants that identify the type of 
        /// credentials that can be returned.</param>
        /// <param name="options">A PSCredentialUIOptions constant that identifies the UI behavior 
        /// when it gathers the credentials.</param>
        /// <returns>Throws a NotImplementException exception.</returns>
        public override PSCredential PromptForCredential(
                               string caption,
                               string message,
                               string userName,
                               string targetName,
                               PSCredentialTypes allowedCredentialTypes,
                               PSCredentialUIOptions options)
        {
            Log.DebugFormat("Prompting for credentials for {0} at {1}", userName, targetName);
            _buffers.QueueOutCommand(OutCommand.CreatePrint(string.Format("Enter password for user {0} at {1}", userName, targetName)));
            var password = ReadLineAsSecureString();
            return new PSCredential(userName, password);
        }

        /// <summary>
        /// Reads characters that are entered by the user until a 
        /// newline (carriage return) is encountered.
        /// </summary>
        /// <returns>The characters entered by the user.</returns>
        public override string ReadLine()
        {
            Log.DebugFormat("Waiting for user input");
            return ReadLineInternal(false, "");
        }

        private string ReadLineInternal(bool secure, string overrideProppt = null)
        {
            var waitEvent = new ManualResetEventSlim();
            string result = "";
            _buffers.RegisterForInCommand((cmd, scope) =>
                                                         {
                                                             scope.Dispose();
                                                             waitEvent.Set();
                                                             result = cmd.TextLine;
                                                         });
            _buffers.QueueOutCommand(OutCommand.CreateReadLine(secure, overrideProppt));
            waitEvent.Wait();
            return result;
        }

        /// <summary>
        /// Reads characters entered by the user until a newline (carriage return) 
        /// is encountered and returns the characters as a secure string.
        /// </summary>
        /// <returns>A secure string of the characters entered by the user.</returns>
        public override SecureString ReadLineAsSecureString()
        {
            Log.DebugFormat("Waiting for user input (password)");
            var unsecure = ReadLineInternal(true);
            var result = new SecureString();
            foreach (char c in unsecure)
            {
                result.AppendChar(c);
            }
            return result;
        }

        /// <summary>
        /// Writes a line of characters to the output display of the host 
        /// and appends a newline (carriage return).
        /// </summary>
        /// <param name="value">The characters to be written.</param>
        public override void Write(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            Log.DebugFormat("Echo: {0}",value);
            _buffers.QueueOutCommand(OutCommand.CreatePrint(value));
        }

        /// <summary>
        /// Writes characters to the output display of the host with possible 
        /// foreground and background colors. 
        /// </summary>
        /// <param name="foregroundColor">The color of the characters.</param>
        /// <param name="backgroundColor">The backgound color to use.</param>
        /// <param name="value">The characters to be written.</param>
        public override void Write(
                                   ConsoleColor foregroundColor,
                                   ConsoleColor backgroundColor,
                                   string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            Log.DebugFormat("Echo: {0}", value);
            var foregroundColorValue = MapColor(foregroundColor);
            var backgroundColorValue = MapColor(backgroundColor);

            string formattedValue = FormatWithColor(value, foregroundColorValue, backgroundColorValue);

            _buffers.QueueOutCommand(OutCommand.CreatePrint(formattedValue));
        }

        private static string FormatWithColor(string value, Color? foregroundColorValue, Color? backgroundColorValue)
        {
            return string.Format("[[;{0};{1}]{2}]",
                                 FormatColor(foregroundColorValue),
                                 FormatColor(backgroundColorValue),
                                 value.Replace("[", "&#91;").Replace("]", "&#93;"));
        }

        private static string FormatColor(Color? color)
        {
            return color.HasValue
                       ? string.Format("#{0:x2}{1:x2}{2:x2}", color.Value.R, color.Value.G, color.Value.B)
                       : "";
        }

        private static Color MapColor(ConsoleColor consoleColor)
        {
            return ColorMap[consoleColor];
        }

        /// <summary>
        /// Writes a line of characters to the output display of the host 
        /// with foreground and background colors and appends a newline (carriage return). 
        /// </summary>
        /// <param name="foregroundColor">The forground color of the display. </param>
        /// <param name="backgroundColor">The background color of the display. </param>
        /// <param name="value">The line to be written.</param>
        public override void WriteLine(
                                       ConsoleColor foregroundColor,
                                       ConsoleColor backgroundColor,
                                       string value)
        {
            this.Write(foregroundColor, backgroundColor, value);
        }

        /// <summary>
        /// Writes a debug message to the output display of the host.
        /// </summary>
        /// <param name="message">The debug message that is displayed.</param>
        public override void WriteDebugLine(string message)
        {
            this.WriteLine(
                           ConsoleColor.DarkYellow,
                           ConsoleColor.Black,
                           String.Format(CultureInfo.CurrentCulture, "DEBUG: {0}", message));
        }

        /// <summary>
        /// Writes an error message to the output display of the host.
        /// </summary>
        /// <param name="value">The error message that is displayed.</param>
        public override void WriteErrorLine(string value)
        {
            this.WriteLine(ConsoleColor.Red, ConsoleColor.Black, value);
        }

        /// <summary>
        /// Writes a newline character (carriage return) 
        /// to the output display of the host. 
        /// </summary>
        public override void WriteLine()
        {
            //_buffers.QueueOutCommand(OutCommand.CreatePrint(""));
        }

        /// <summary>
        /// Writes a line of characters to the output display of the host 
        /// and appends a newline character(carriage return). 
        /// </summary>
        /// <param name="value">The line to be written.</param>
        public override void WriteLine(string value)
        {
            Log.DebugFormat("Echo: {0}",value);
            _buffers.QueueOutCommand(OutCommand.CreatePrint(value+"\n"));
        }

        /// <summary>
        /// Writes a verbose message to the output display of the host.
        /// </summary>
        /// <param name="message">The verbose message that is displayed.</param>
        public override void WriteVerboseLine(string message)
        {
            this.WriteLine(
                           ConsoleColor.Green,
                           ConsoleColor.Black,
                           String.Format(CultureInfo.CurrentCulture, "VERBOSE: {0}", message));
        }

        /// <summary>
        /// Writes a warning message to the output display of the host.
        /// </summary>
        /// <param name="message">The warning message that is displayed.</param>
        public override void WriteWarningLine(string message)
        {
            this.WriteLine(
                           ConsoleColor.Yellow,
                           ConsoleColor.Black,
                           String.Format(CultureInfo.CurrentCulture, "WARNING: {0}", message));
        }

        /// <summary>
        /// Writes a progress report to the output display of the host. 
        /// Wrinting a progress report is not required for the cmdlet to 
        /// work so it is better to do nothing instead of throwing an 
        /// exception.
        /// </summary>
        /// <param name="sourceId">Unique identifier of the source of the record. </param>
        /// <param name="record">A ProgressReport object.</param>
        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            // Do nothing.
        }

        /// <summary>
        /// Parse a string containing a hotkey character.
        /// Take a string of the form
        ///    Yes to &amp;all
        /// and returns a two-dimensional array split out as
        ///    "A", "Yes to all".
        /// </summary>
        /// <param name="input">The string to process</param>
        /// <returns>
        /// A two dimensional array containing the parsed components.
        /// </returns>
        private static Tuple<string, string> GetHotkeyAndLabel(string input)
        {
            int indexOfAmpersand = input.IndexOf("&");
            if (indexOfAmpersand >= 0 && indexOfAmpersand < input.Length - 1)
            {
                var hotKey = input[indexOfAmpersand+1].ToString().ToUpper();
                var label = input.Replace("&", "");
                return new Tuple<string, string>(hotKey, label);
            }
            return new Tuple<string, string>("", input);
        }

        /// <summary>
        /// This is a private worker function splits out the
        /// accelerator keys from the menu and builds a two
        /// dimentional array with the first access containing the
        /// accelerator and the second containing the label string
        /// with the &amp; removed.
        /// </summary>
        /// <param name="choices">The choice collection to process</param>
        /// <returns>
        /// A two dimensional array containing the accelerator characters
        /// and the cleaned-up labels</returns>
        private static IEnumerable<Tuple<string, string>> BuildHotkeysAndPlainLabels(IEnumerable<ChoiceDescription> choices)
        {
            return choices.Select(x => GetHotkeyAndLabel(x.Label));
        }
    }
}

