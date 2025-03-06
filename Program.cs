using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

class MyBot
{
    private static readonly string GitHubToken = "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"; // Замінити на твій токен
    private static readonly string RepoOwner = "werwerwerwre"; // Замінити на твоє ім'я користувача на GitHub
    private static readonly string RepoName = "Telegram-dot"; // Замінити на назву твого репозиторію
    private static readonly string FilePath = "Text.txt"; // Шлях до файлу, який хочеш відправити
    private static readonly string TelegramToken = "8197638246:AAHRc3q3WlYAQKKEvZDqC60zuSmJ6YWvnjE"; // Замінити на токен твого бота

    private static TelegramBotClient botClient;

    static async Task Main(string[] args)
    {
        // Ініціалізація Telegram бота
        botClient = new TelegramBotClient(TelegramToken);
        botClient.OnMessage += BotClient_OnMessage;
        botClient.StartReceiving();

        Console.WriteLine("Бот запущений. Чекаємо на повідомлення...");

        // Цикл для підтримки бота в роботі
        await Task.Delay(-1);
    }

    // Обробка повідомлень, що надходять на бот
    private static async void BotClient_OnMessage(object sender, MessageEventArgs e)
    {
        if (e.Message.Text != null)
        {
            string receivedText = e.Message.Text;
            Console.WriteLine($"Отримано повідомлення: {receivedText}");

            // Зберігаємо текст у файл
            File.AppendAllText(FilePath, receivedText + Environment.NewLine);

            // Після цього відправляємо вміст файлу на GitHub
            await SendToGitHub();
        }
    }

    static async Task SendToGitHub()
    {
        try
        {
            // Читання вмісту з файлу
            string fileContent = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                Console.WriteLine("Файл порожній.");
                return;
            }

            using (var client = new HttpClient())
            {
                // Заголовки для авторизації
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {GitHubToken}");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

                // Створюємо JSON для запиту до GitHub API
                var jsonContent = new
                {
                    message = "Update Text.txt with new messages",
                    content = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent)),
                    committer = new
                    {
                        name = "MyBot",
                        email = "mybot@example.com"
                    }
                };

                // Запит до API для додавання файлу або оновлення існуючого
                var response = await client.PutAsJsonAsync(
                    $"https://api.github.com/repos/{RepoOwner}/{RepoName}/contents/Text.txt",
                    jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Інформація успішно передана в GitHub.");
                }
                else
                {
                    Console.WriteLine($"Сталася помилка при відправці: {response.ReasonPhrase}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Сталася помилка: {ex.Message}");
        }
    }
}
