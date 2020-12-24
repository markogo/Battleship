using System;

namespace MenuSystem
{
    public class MenuItem
    {
        private string? _label;

        private string? _userChoice;
        public virtual Func<string>? MethodToExecute { get; set; }

        protected virtual string Label
        {
            get => _label ?? "";
            set => _label = Validate(value, 1, 100, true);
        }

        public virtual string UserChoice
        {
            get => _userChoice ?? "";
            set => _userChoice = Validate(value, 1, 1, true);
        }

        public MenuItem(string label, string userChoice, Func<string>? methodToExecute)
        {
            Label = label;
            UserChoice = userChoice;
            MethodToExecute = methodToExecute;
        }

        private static string Validate(string item, int minLength, int maxLength, bool toUpper)
        {
            item = item.Trim();
            if (toUpper)
            {
                item = item.ToUpper();
            }

            if (item.Length < minLength || item.Length > maxLength)
            {
                throw new ArgumentException(
                    $"String is not correct length (" +
                    $"{minLength}-{maxLength})! Got " +
                    $"{item.Length} characters.");
            }

            return item;
        }

        public override string ToString()
        {
            return UserChoice + ") " + Label;
        }
    }
}