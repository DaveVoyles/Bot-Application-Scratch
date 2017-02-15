using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.FormFlow;

namespace Bot_Application
{

    public enum GeneralOptions
    {
        ThingOne,
        ThingTwo,
        ThingThr
    }

    // It must be serializable so the bot can be stateless.
    [Serializable]
    public class OptionsOrder
    {
        public GeneralOptions? Options;

        public static IForm<OptionsOrder> BuildForm()
        {
            return new FormBuilder<OptionsOrder>()
                    .Message("Welcome to the simple sandwich order bot!")
                    .Build();
        }

    }
}