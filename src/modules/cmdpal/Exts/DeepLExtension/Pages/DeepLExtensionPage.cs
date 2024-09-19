// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
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
        if (query == string.Empty)
        {
            return [
                new ListSection()
                {
                    Title = "DeepL Instructions",
                    Items = [
                    new ListItem(new NoOpCommand()) { Title = "Type your sentence to get an English translation. Use \"> TargetLanguage\" to specify a target language." },
                    ],
                }
             ];
        }

        var authKey = "<YOUR KEY HERE>";
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

        var defaultTranslation = await translator.TranslateTextAsync(queryString, null, targetLanguage, new TextTranslateOptions { Formality = Formality.Default });
        var moreFormalTranslation = await translator.TranslateTextAsync(queryString, null, targetLanguage, new TextTranslateOptions { Formality = Formality.PreferMore });
        var lessFormalTranslation = await translator.TranslateTextAsync(queryString, null, targetLanguage, new TextTranslateOptions { Formality = Formality.PreferLess });

        return [
           new ListSection()
            {
                Title = "DeepL Responses",
                Items = [
                   new ListItem(new NoOpCommand())
                   {
                       Title = string.IsNullOrEmpty(query) ? "dynamic item" : defaultTranslation.ToString(), Subtitle = "TO-DO: How do I make a copy button? LOL", Tags = [new Tag()
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
            }
           ];
    }
}
