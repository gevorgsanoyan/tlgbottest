using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json;
using System.IO;

namespace tlgBotTest
{
    class Program
    {
        static TelegramBotClient bot = new TelegramBotClient("");

        private class Result1
        {
            public string Value1 { get; set; }
            public string Value2 { get; set; }

            public Result1()
            { }
            public Result1(string Val1, string Val2)
            {
                Value1 = Val1;
                Value2 = Val2;
            }
        }

        private class Result2
        {
            public string Value3 { get; set; }
        }

        private class fQueue
        {
            public string fName { get; set; }
            public long userId { get; set; }            
            public Telegram.Bot.Types.Message message { get; set; }
            public Result1 Result1 { get; set; }
            public Result2 Result2 { get; set; }
        }

        private static List<fQueue> qList = new List<fQueue>();

        private static void AddToQueue(fQueue cQueue)
        {
            fQueue lastQ = qList.LastOrDefault();
            if(lastQ != null)
            {
                if(cQueue.Result1 == null)
                    cQueue.Result1 = lastQ.Result1;
                if(cQueue.Result2 == null)
                    cQueue.Result2 = lastQ.Result2;
            }//if(lastQ != null)
            qList.Add(cQueue);

            using (StreamWriter w = File.AppendText(cQueue.userId.ToString()))
            {
                w.WriteLine(JsonConvert.SerializeObject(cQueue));
            }

            Console.WriteLine("-----------------------------------");
            Console.WriteLine(JsonConvert.SerializeObject(cQueue));
            Console.WriteLine("-----------------------------------");
        }//private static void AddToQueue(fQueue cQueue)

        private static fQueue GetLastUpdate(long userId)
        {
            fQueue tQ = new fQueue();
            tQ.message = new Telegram.Bot.Types.Message();
            tQ.message.Text = "not updated";
            try
            {
                var lastFromFile = File.ReadLines(userId.ToString()).Last();
                if (lastFromFile.Length > 0)
                {
                    tQ = JsonConvert.DeserializeObject<fQueue>(lastFromFile);
                    tQ.message.Text = "updated";
                }
            }
            catch { }
            
            if (tQ.fName == null)
                tQ.fName = "f1";

            return tQ;
        }//private static void GetLastUpdate(string userId)

        static void Main(string[] args)
        {
            var me = bot.GetMeAsync().Result;
            Console.Title = me.Username;

            bot.OnMessage += BotOnMessageReceived;
            bot.OnCallbackQuery += BotOnCallbackQueryReceived;

            bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            bot.StopReceiving();

        }


        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text) return;

            fQueue curQueue = new fQueue();
            curQueue.message = messageEventArgs.Message;
            curQueue.userId = messageEventArgs.Message.Chat.Id;

            if(message.Text.Split(' ').First() == "/start")
            {
                
                string Value1 = "A";
                string Value2 = "5";
                string Value3 = "D";
                curQueue.Result1 = new Result1(Value2, Value1);
                curQueue.Result2 = new Result2();
                curQueue.Result2.Value3 = Value3;
                curQueue.fName = "f1";

                fQueue fromDbQueue = GetLastUpdate(curQueue.userId);
                if (fromDbQueue.message.Text == "updated")
                {
                    curQueue = fromDbQueue;
                    curQueue.message = messageEventArgs.Message;
                }//if (fromDbQueue.message.Text == "updated")

            }
            else
            {
                fQueue lastInQueue = qList.LastOrDefault();
                if (lastInQueue != null)
                {
                    if (lastInQueue.fName == "f3fb")
                    {
                        curQueue.fName = "f3fb";
                    }
                }//if (curQueue == null)
            }//if(message.Text.Split(' ').First() == "/start")


            AddToQueue(curQueue);            
            gFunction(curQueue);

        }//private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            //await bot.AnswerCallbackQueryAsync(
            //    callbackQuery.Id,
            //    $"Received {callbackQuery.Data}");

            fQueue curQueue = qList.LastOrDefault();

            if (curQueue == null)
                return;

            switch(curQueue.fName)
            {
                case "f1":
                    switch(callbackQuery.Data)
                    {
                        case "Result1":
                            curQueue = new fQueue();
                            curQueue.fName = "f2";
                            curQueue.userId = callbackQuery.Message.Chat.Id;
                            curQueue.message = callbackQuery.Message;
                            AddToQueue(curQueue);
                            //gFunction(curQueue);
                            break;
                        case "Result2":
                            curQueue = new fQueue();
                            curQueue.fName = "f4";
                            curQueue.userId = callbackQuery.Message.Chat.Id;
                            curQueue.message = callbackQuery.Message;
                            AddToQueue(curQueue);
                            //gFunction(curQueue);
                            break;
                        case "Next":
                            string selectedValue = "Results : " + JsonConvert.SerializeObject(curQueue.Result1) + "; " + JsonConvert.SerializeObject(curQueue.Result2);
                            Console.WriteLine(selectedValue);
                            await bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id,selectedValue);
                            curQueue = new fQueue();
                            curQueue.fName = "f1";
                            curQueue.userId = callbackQuery.Message.Chat.Id;
                            curQueue.message = callbackQuery.Message;
                            AddToQueue(curQueue);                            
                            break;
                    }//switch(callbackQuery.Data)
                    await bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    gFunction(curQueue);
                    break;
                case "f2":
                    curQueue = new fQueue();
                    if (callbackQuery.Data != "Previous")                    
                        curQueue.fName = "f3";
                    else
                        curQueue.fName = "f1";
                    curQueue.userId = callbackQuery.Message.Chat.Id;
                    curQueue.message = callbackQuery.Message;
                    AddToQueue(curQueue);
                    await bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    gFunction(curQueue);
                    break;
                case "f3":
                    curQueue = new fQueue();
                    curQueue.fName = "f3fb";
                    curQueue.userId = callbackQuery.Message.Chat.Id;
                    curQueue.message = callbackQuery.Message;
                    AddToQueue(curQueue);                    
                    gFunction(curQueue);
                    break;
                case "f4":
                    curQueue = new fQueue();
                    if (callbackQuery.Data != "Previous")
                    {
                        Console.WriteLine("Selected: " + callbackQuery.Data);
                        curQueue.fName = "f4";
                    }
                    else
                        curQueue.fName = "f1";
                    curQueue.userId = callbackQuery.Message.Chat.Id;
                    curQueue.message = callbackQuery.Message;
                    AddToQueue(curQueue);
                    await bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    gFunction(curQueue);
                    break;
            }//switch(curQueue.fName)

        }//private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)

        private static async void gFunction(fQueue q)
        {
            var message = q.message;
            fQueue curQueue = new fQueue();

            switch (q.fName)
            {
                case "f1":
                    
                    var kButtons = new InlineKeyboardMarkup(new[] {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Result1"),
                            InlineKeyboardButton.WithCallbackData("Result2"),
                            InlineKeyboardButton.WithCallbackData("Next")
                        }
                    });                    
                    await bot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Select",
                        replyMarkup: kButtons);
                    break;
                case "f2":
                    kButtons = new InlineKeyboardMarkup(new[] {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("A"),
                            InlineKeyboardButton.WithCallbackData("B"),
                            InlineKeyboardButton.WithCallbackData("C"),
                            InlineKeyboardButton.WithCallbackData("Previous")
                        }
                    });
                    await bot.SendTextMessageAsync(
                       message.Chat.Id,
                       "A, B or C ?",
                       replyMarkup: kButtons);
                    break;
                case "f3":
                    string selectedValue = string.Empty;
                    switch(message.Text)
                    {
                        case "A":
                            selectedValue = "A";
                            break;
                        case "B":
                            selectedValue = "B";
                            break;
                        case "C":
                            selectedValue = "C";
                            break;
                    }//switch(message)
                    
                    curQueue.fName = "f3fb";
                    curQueue.userId = message.Chat.Id;
                    curQueue.message = message;
                    AddToQueue(curQueue);

                    await bot.SendTextMessageAsync(
                       message.Chat.Id,
                       "Enter the Quantity in " + selectedValue);
                    break;
                case "f3fb":
                    try {
                        double rVal = double.Parse(message.Text);
                        q.Result1.Value2 = rVal.ToString();
                        curQueue.fName = "f1";
                        curQueue.userId = message.Chat.Id;
                        curQueue.message = message;
                        AddToQueue(curQueue);
                        await bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        gFunction(curQueue);
                    }
                    catch {
                        curQueue.fName = "f3";
                        curQueue.userId = message.Chat.Id;
                        curQueue.message = message;
                        AddToQueue(curQueue);
                        gFunction(curQueue);
                    }
                    break;
                case "f4":
                    kButtons = new InlineKeyboardMarkup(new[] {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("D"),
                            InlineKeyboardButton.WithCallbackData("E"),
                            InlineKeyboardButton.WithCallbackData("Previous")
                        }
                    });
                    Console.WriteLine("Value3 = " + q.Result2.Value3);
                    await bot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Result2",
                        replyMarkup: kButtons);
                    break;
            }            
        }//private static async void gFunction(string fName, object rData)

    }
}
