using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using consulting_telegram_bot.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using System;
using System.IO;
using Microsoft.VisualBasic;

namespace consulting_telegram_bot
{
    class Program
    {
        private class UserRequest
        {
            public string FIO { get; set; }
            public string Email { get; set; }
            public string RequestText { get; set; }
            public RegistrationStage RegistrationStage { get; set; } = RegistrationStage.None;
        }

        private enum RegistrationStage
        {
            None,
            Name,
            Email,
            Request,
            Completed
        }

        private static ITelegramBotClient botClient;
        private static ReceiverOptions receiverOptions;
        private static ConsultingDataApi context = new ConsultingDataApi();
        private static UserRequest userRequest = new UserRequest();
        static async Task Main()
        {
            botClient = new TelegramBotClient("TOKEN");
            receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery
                }
                ,
                ThrowPendingUpdates = true,
            };

            using var cts = new CancellationTokenSource();
            botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);
            var me = await botClient.GetMeAsync();
            Console.WriteLine($"{me.FirstName} запущен!");
            Console.ReadLine();
        }

        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            var message = update.Message;
                            var user = message.From;
                            var chat = message.Chat;

                            Console.WriteLine($"{user.FirstName} ({user.Id}) написал сообщение: {message.Text}");

                            switch (message.Type)
                            {
                                case MessageType.Text:
                                    {
                                        if (message.Text == "/start")
                                        {
                                            //меню через нижние кнопки
                                            var replyKeyboard = new ReplyKeyboardMarkup(

                                                new List<KeyboardButton[]>()
                                                {
                                                    new KeyboardButton[]
                                                {
                                                    new KeyboardButton("Подать заявку")
                                                },

                                                new KeyboardButton[]
                                                {
                                                    new KeyboardButton("Услуги"),
                                                    new KeyboardButton("Проекты")
                                                },

                                                new KeyboardButton[]
                                                {
                                                    new KeyboardButton("Блог"),
                                                    new KeyboardButton("Реквизиты")
                                                },

                                                new KeyboardButton[]
                                                {
                                                    new KeyboardButton("Ссылки")
                                                }

                                                })
                                            {
                                                ResizeKeyboard = true,
                                            };

                                            await botClient.SendTextMessageAsync(
                                                chat.Id,
                                                "Добро пожаловать к телеграм-боту SkillProfi! Выберите раздел меню. Для повторного вывода меню наберите /start",
                                                replyMarkup: replyKeyboard);
                                            return;
                                        }

                                        //нижняя кнопка Подать заявку
                                        if (message.Text == "Подать заявку")
                                        {
                                            userRequest.RegistrationStage = RegistrationStage.Name;
                                            await botClient.SendTextMessageAsync(
                                                chatId: chat.Id,
                                                text: "Введите свои ФИО"
                                            );

                                            return;
                                        }

                                        switch (userRequest.RegistrationStage)
                                        {
                                            case RegistrationStage.Name:
                                                userRequest.FIO = message.Text;
                                                userRequest.RegistrationStage = RegistrationStage.Email;

                                                await botClient.SendTextMessageAsync(
                                                    chatId: chat.Id,
                                                    text: "Введите свой E-mail"
                                                );
                                                break;

                                            case RegistrationStage.Email:
                                                userRequest.Email = message.Text;
                                                userRequest.RegistrationStage = RegistrationStage.Request;

                                                await botClient.SendTextMessageAsync(
                                                    chatId: chat.Id,
                                                    text: "Введите запрос"
                                                );
                                                break;

                                            case RegistrationStage.Request:
                                                userRequest.RequestText = message.Text;
                                                userRequest.RegistrationStage = RegistrationStage.Completed;                                                

                                                Console.WriteLine($"FIO {userRequest.FIO}, Email: {userRequest.Email}, Request: {userRequest.RequestText}");
                                                string request_status = await context.CreateRequest(userRequest.FIO, userRequest.Email, userRequest.RequestText);

                                                if(request_status != "ok")
                                                {
                                                    await botClient.SendTextMessageAsync(chatId: chat.Id, request_status);
                                                    userRequest.FIO = "";
                                                    userRequest.Email = "";
                                                    userRequest.RequestText = "";
                                                    userRequest.RegistrationStage = RegistrationStage.None;
                                                    break;
                                                }

                                                await botClient.SendTextMessageAsync(chatId: chat.Id, text: "Ваша заявка принята. Спасибо за обращение, с Вами скоро свяжутся.");

                                                userRequest.FIO = "";
                                                userRequest.Email = "";
                                                userRequest.RequestText = "";
                                                userRequest.RegistrationStage = RegistrationStage.None;
                                                break;
                                        }

                                        //нижняя кнопка Услуги
                                        if (message.Text == "Услуги")
                                        {
                                            var services = await context.GetAllServices() as List<Service>;
                                            int count = services.Count;
                                            for (int i = 0; i < count; i++)
                                            {
                                                await botClient.SendTextMessageAsync(chat.Id, $"<b>{services[i].ServiceTitle}</b> \n <i>{services[i].ServiceDescription}</i>", parseMode: ParseMode.Html);
                                            }
                                            return;
                                        }

                                        //нижняя кнопка Проекты
                                        if (message.Text == "Проекты")
                                        {
                                            var projects = await context.GetAllProjects() as List<Project>;
                                            int count = projects.Count;
                                            List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>>(count);
                                            for (int i = 0; i < count; i++)
                                            {
                                                buttons.Add(new List<InlineKeyboardButton>(1)
                                                {
                                                    InlineKeyboardButton.WithCallbackData($"{projects[i].ProjectTitle}", $"project_{projects[i].Id}"),
                                                });
                                            }
                                            var inlineKeyboard = new InlineKeyboardMarkup(buttons);
                                            await botClient.SendTextMessageAsync(chat.Id, "Выберите проект из списка", replyMarkup: inlineKeyboard);
                                            return;
                                        }

                                        //нижняя кнопка Блог
                                        if (message.Text == "Блог")
                                        {
                                            var blogs = await context.GetAllBlogs() as List<Blog>;
                                            int count = blogs.Count;
                                            List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>>(count);
                                            for (int i = 0; i < count; i++)
                                            {
                                                buttons.Add(new List<InlineKeyboardButton>(1)
                                                {
                                                    InlineKeyboardButton.WithCallbackData($"{blogs[i].BlogTitle}", $"blog_{blogs[i].Id}"),
                                                });
                                            }
                                            var inlineKeyboard = new InlineKeyboardMarkup(buttons);
                                            await botClient.SendTextMessageAsync(chat.Id, "Выберите новость из блога", replyMarkup: inlineKeyboard);
                                            return;
                                        }

                                        //нижняя кнопка Реквизиты
                                        if (message.Text == "Реквизиты")
                                        {
                                            MainClass mc = context.GetRequisitesTexts();
                                            await botClient.SendTextMessageAsync(message.Chat.Id,
                                                $"Адрес: {mc.siteText.ContactsPage_Address}" +
                                                $"\nТелефон: {mc.siteText.ContactsPage_ContactsPhone}" +
                                                $"\nФакс: {mc.siteText.ContactsPage_ContactsFax}" +
                                                $"\nE-mail: {mc.siteText.ContactsPage_ContactsEmail}" +
                                                $"\nФИО: {mc.siteText.ContactsPage_ContactsFIO}");

                                            string mapURl = "https://localhost:44380/api/application/getpixbyname/yandex_map.png";
                                            var req = System.Net.WebRequest.Create(mapURl);
                                            using (Stream stream = req.GetResponse().GetResponseStream())
                                            {
                                                await botClient.SendPhotoAsync(chatId: chat.Id, InputFile.FromStream(stream));
                                            }
                                            return;
                                        }

                                        //нижняя кнопка Ссылки
                                        if (message.Text == "Ссылки")
                                        {
                                            var links = await context.GetAllContacts() as List<consulting_telegram_bot.Models.Contact>;
                                            int count = links.Count;
                                            for (int i = 0; i < count; i++)
                                            {
                                                await botClient.SendTextMessageAsync(chat.Id, $"{links[i].ContactText}\n{links[i].ContactLink}");
                                            }
                                            return;
                                        }
                                        return;
                                    }
                            }
                            return;
                        }

                    case UpdateType.CallbackQuery:
                        {
                            var callbackQuery = update.CallbackQuery;
                            var user = callbackQuery.From;
                            Console.WriteLine($"{user.FirstName} ({user.Id}) нажал на кнопку: {callbackQuery.Data}");
                            var chat = callbackQuery.Message.Chat;

                            //строка данных (услуга, проект, блог)
                            string selectedData = callbackQuery.Data;

                            //строка типа данных до разделителя
                            string dataType = selectedData.Substring(0, selectedData.IndexOf('_'));

                            //строка ID данных после разделителя
                            int dataId = Int32.Parse(selectedData.Substring(selectedData.IndexOf('_') + 1));

                            Console.WriteLine($"selectedData = {selectedData} | dataType = {dataType} | dataId = {dataId}");

                            //проверка на тип данных
                            switch (dataType)
                            {
                                case "project":
                                    {
                                        Project project = await context.GetProjectById(dataId);
                                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                        var req = System.Net.WebRequest.Create(project.PicturePath);
                                        using (Stream stream = req.GetResponse().GetResponseStream())
                                        {
                                            await botClient.SendPhotoAsync(chatId: chat.Id, InputFile.FromStream(stream), caption: project.ProjectTitle);
                                            await botClient.SendTextMessageAsync(chat.Id, $"{project.ProjectDescription}");
                                        }
                                        return;
                                    }
                                case "blog":
                                    {
                                        Blog blog = await context.GetBlogById(dataId);
                                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                        var req = System.Net.WebRequest.Create(blog.PicturePath);
                                        using (Stream stream = req.GetResponse().GetResponseStream())
                                        {
                                            await botClient.SendPhotoAsync(chatId: chat.Id, InputFile.FromStream(stream), caption: $"{blog.PublishTime}\n{blog.BlogTitle}");
                                            await botClient.SendTextMessageAsync(chat.Id, $"{blog.BlogDescription}");
                                        }
                                        return;
                                    }
                            }
                            return;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}