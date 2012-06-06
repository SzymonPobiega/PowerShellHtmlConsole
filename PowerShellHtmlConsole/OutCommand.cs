using System;
using System.Collections.Generic;

namespace PowerShellHtmlConsole
{
    public class OutCommand
    {
        public PrintOutCommand Print { get; set; }
        public PromptOutCommand Prompt { get; set; }
        public PromptForChoiceOutCommand PromptForChoice { get; set; }
        public PromptForCredentialsOutCommand PromptForCredentials { get; set; }
        public ReadLineOutCommand ReadLine { get; set; }
        public ClearOutCommand Clear { get; set; }
        public ExitOutCommand Exit { get; set; }

        public static OutCommand CreatePrint(string message)
        {
            return new OutCommand()
                       {
                           Print = new PrintOutCommand()
                                       {
                                           Text = message
                                       }
                       };
        }


        public static OutCommand CreatePromptForChoice(string caption, string message, List<string > options)
        {
            return new OutCommand()
                       {
                           PromptForChoice = new PromptForChoiceOutCommand()
                                                 {
                                                     Caption = caption,
                                                     Message = message,
                                                     Options = options
                                                 }
                       };
        }

        public static OutCommand CreatePrompt(string caption, string message, List<PromptField> fields)
        {
            return new OutCommand()
                       {
                           Prompt = new PromptOutCommand()
                                        {
                                            Caption = caption,
                                            Message = message,
                                            Fields = fields
                                        }
                       };
        }

        public static OutCommand CreateReadLine(bool secure)
        {
            return new OutCommand()
                       {
                           ReadLine = new ReadLineOutCommand()
                                          {
                                              Secure = secure
                                          }
                       };
        }

        public static OutCommand CreateClear()
        {
            return new OutCommand()
                       {
                           Clear = new ClearOutCommand()
                       };
        }

        public static OutCommand CreateExit()
        {
            return new OutCommand()
                       {
                           Exit = new ExitOutCommand()
                       };
        }
    }

    public class ExitOutCommand
    {        
    }

    public class ClearOutCommand
    {        
    }

    public class PrintOutCommand
    {
        public string Text { get; set; }
    }

    public class ReadLineOutCommand
    {
        public bool Secure { get; set; }
    }

    public class PromptOutCommand
    {
        public string Caption { get; set; }
        public string Message { get; set; }
        public List<PromptField> Fields { get; set; }
    }

    public class PromptForCredentialsOutCommand
    {
        public string Caption { get; set; }
        public string Message { get; set; }
        public string User { get; set; }
        public string Target { get; set; }
    }

    public class PromptForChoiceOutCommand
    {
        public string Caption { get; set; }
        public string Message { get; set; }
        public List<string> Options { get; set; }
    }

    public class PromptField
    {
        public string Name { get; set; }
        public string Label { get; set; }
    }
}