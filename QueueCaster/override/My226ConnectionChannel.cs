﻿using Sharpcaster.Channels;
using Sharpcaster.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueCaster
{
    public class My226ConnectionChannel :ConnectionChannel {
        public override Task OnMessageReceivedAsync(IMessage message) {
            if (message is CloseMessage) {
                _ = Task.Run(async () => {
                    await Client.DisconnectAsync();
                    await Task.Delay(2000);
                });
            }
            return Task.CompletedTask;
        }

    }
}
