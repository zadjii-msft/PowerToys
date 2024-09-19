// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace DeepLExtension;

internal sealed partial class DeepLExtensionPage : DynamicListPage
{
    public DeepLExtensionPage()
    {
        Icon = new("https://www.deepl.com/favicon.ico");
        Name = "DeepL";
    }

    public override ISection[] GetItems(string query)
    {
        var t = DoGetItems(query);
        t.ConfigureAwait(false);
        return t.Result;
    }

    private async Task<ISection[]> DoGetItems(string query)
    {
        var authKey = Environment.GetEnvironmentVariable("DEEPL_KEY");
        if (string.IsNullOrEmpty(authKey))
        {
            return [
                new ListSection()
                {
                    Title = "No DeepL API key detected!",
                    Items = [
                    new ListItem(new NoOpCommand()) { Title = "Set DEEPL_KEY in your Environment Variables." },
                    ],
                }
             ];
        }

        if (query == string.Empty)
        {
            return [
                new ListSection()
                {
                    Title = "Type a sentence for an English translation",
                    Items = [
                    new ListItem(new NoOpCommand()) { Title = "Type \"> Language\" to translate to a different language." },
                    ],
                }
             ];
        }

        var translator = new Translator(authKey);
        var characterToSplit = '>';

        var queryString = query;
        var targetLanguage = LanguageCode.EnglishAmerican;

        if (query.Contains(characterToSplit))
        {
            var parts = query.Split(characterToSplit);
            queryString = parts[0];
            var targetLanguageKey = parts[1];

            targetLanguage = targetLanguageKey switch
        {
            " English" => LanguageCode.EnglishAmerican,
            " Chinese" => LanguageCode.Chinese,
            " Japanese" => LanguageCode.Japanese,
            " Korean" => LanguageCode.Korean,
            " German" => LanguageCode.German, // supports formality
            " French" => LanguageCode.French, // supports formality
            " Spanish" => LanguageCode.Spanish, // supports formality
            _ => LanguageCode.EnglishAmerican, // Default to American English if the language is not found
        };
        }

        var targetLanguages = await translator.GetTargetLanguagesAsync();
        var selectedLanguage = targetLanguages.FirstOrDefault(lang => lang.Code == targetLanguage);

        var defaultTranslation = await translator.TranslateTextAsync(queryString, null, targetLanguage, new TextTranslateOptions { Formality = Formality.Default });

        ListSection finalOutput = new ListSection()
        {
            Title = "DeepL Responses",
            Items = [
                   new ListItem(new NoOpCommand())
                   {
                       Title = string.IsNullOrEmpty(query) ? "dynamic item" : defaultTranslation.ToString(),
                   },
                ],
        };

        var moreFormalTranslation = await translator.TranslateTextAsync(queryString, null, targetLanguage, new TextTranslateOptions { Formality = Formality.PreferMore });
        var lessFormalTranslation = await translator.TranslateTextAsync(queryString, null, targetLanguage, new TextTranslateOptions { Formality = Formality.PreferLess });

        if (selectedLanguage?.SupportsFormality == true)
        {
            finalOutput = new ListSection()
            {
                Title = "DeepL Responses",
                Items = [
                   new ListItem(new NoOpCommand())
                   {
                       Title = string.IsNullOrEmpty(query) ? "dynamic item" : defaultTranslation.ToString(), Tags = [new Tag()
                               {
                                   Text = "Default Form",
                               }
                        ],
                   },
                    new ListItem(new NoOpCommand())
                    {
                       Title = string.IsNullOrEmpty(query) ? "dynamic item" : moreFormalTranslation.ToString(), Tags = [new Tag()
                               {
                                   Text = "More Formal",
                               }
                         ],
                    },
                   new ListItem(new NoOpCommand())
                    {
                       Title = string.IsNullOrEmpty(query) ? "dynamic item" : lessFormalTranslation.ToString(), Tags = [new Tag()
                               {
                                   Text = "Less Formal",
                               }
                         ],
                    }
                ],
            };
        }

        return [
            finalOutput
           ];
    }
}
