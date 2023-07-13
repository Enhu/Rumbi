namespace Rumbi.Utils
{
    public static class BotMessage
    {
        // General Messages
        public static string InternalError { get; } =
            $"❌ An error ocurred, please contact enhu on discord.";

        public static string AboutBot { get; } = $"Made by and maintaned by Enhu. \n\n " +
            $"__GitHub repository:__ https://github.com/Enhu/Rumbi \n";

        // FAQ Module

        public static string NoQuestions { get; } = "⚠️ No questions found yet, please wait until an moderator adds some!";

        public static string ManagerEmpty { get; } = "⚠️ No questions found, please add some.\n\n" +
            "Please **REFRESH** once you add a question.";

        public static string ManagerMessage { get; } = "**Questions Manager**\n\n" +
            "Please **REFRESH** once you add/edit/remove a question.";

        public static string ManagerQuestionSelected(string identifier)
        {
            return $"\n\nSelected question with identifier '**{identifier}**'\n";
        }

        public static string QuestionNotFound(string identifier)
        {
            return $"⚠️ Question with identifier '**{identifier}**' not found";
        }

        public static string QuestionAlreadyExists(string identifier)
        {
            return $"⚠️ Question with identifier '**{identifier}**' already exists";
        }

        public static string QuestionEdited { get; } = "✅ Question Edited";
        public static string QuestionAdded { get; } = "✅ Question added";
        public static string QuestionDeleted { get; } = "✅ Question deleted";
        public static string QuestionSelect { get; } = "Select a question...";
        public static string QuestionModalAdd { get; } = "Add a question";
        public static string QuestionModalEdit { get; } = "Edit a question";

        public static string StratsWelcomeMessage { get; } =
            "These are the most frequenlty asked questions about strats.\n"
            + "If you don't understand something in here, or your specific doubt/question"
            + "isn't covered please **don't hesitate to ask!**\n\n";

        public static string GeneralWelcomeMessage { get; } =
            "These are the most frequenlty asked questions.\n"
            + "If you don't understand something in here, or your specific doubt/question"
            + "isn't covered please **don't hesitate to ask!**\n\n";
    }
}