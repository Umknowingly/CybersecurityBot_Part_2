using System;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CyberSecurity_Bot
{
    public partial class MainWindow : Window
    {
        // USER DATA STORAGE - these variables store information about the user and the conversation context
        string userName = "";           // stores the user's name
        string lastTopic = "";          // remembers the last topic (for "tell me more")
        string userInterest = "";       // remembers what the user is interested in
        int emptyInputCount = 0;        // counts blank messages
        Random random = new Random();   // used to pick random responses

        // RESPONSE DATABASE - these arrays store multiple tips for each topic, which the bot can randomly choose from to keep the conversation fresh and engaging
        string[] passwordResponses =
        {
            "Use a password with at least 12 characters. Mix uppercase, lowercase, numbers and symbols. Never reuse the same password on different websites! ",
            "Try a passphrase — three random words joined together like 'BloodMoon$Castle9'. It is long and easy to remember! ",
            "A password manager like Bitwarden stores all your passwords safely. You only need to remember one master password! "
        };

        string[] phishingResponses =
        {
            "Phishing is when scammers send fake emails pretending to be real companies. Never click links in emails you did not expect — go to the website directly! 🎣",
            "Watch out for spelling mistakes, urgent warnings, or requests for personal info in emails. These are classic signs of a phishing attack! ⚠️",
            "Always check the sender's actual email address. A scammer might show 'FNB Support' but the real email is randomname@fakesite.com. 🔍"
        };

        string[] scamResponses =
        {
            "If someone asks you to pay with gift cards or Bitcoin — it is a scam! No real company or government ever asks for payment this way. 🚨",
            "Scams often make you feel rushed or scared with messages like 'Act now or lose your account!'. Take a breath and verify before doing anything! 🛑",
            "If a deal seems too good to be true, it probably is. Report scams to the South African Police Service (SAPS). 🛡️"
        };

        string[] privacyResponses =
        {
            "Check your social media privacy settings! Not everyone needs to see your phone number, address, or where you are every day. 🔒",
            "Only give apps the permissions they actually need. A calculator app does not need access to your camera or contacts! 📱",
            "Be careful what you share online. Scammers can use your personal details to trick you or steal your identity. 🌐"
        };

        string[] malwareResponses =
        {
            "Malware is harmful software that can steal your data or damage your device. Install a trusted antivirus program and keep it updated! 🦠",
            "Only download software from official websites. Free cracked or pirated software almost always contains hidden malware! 💀",
            "Warning signs of malware: your device runs slowly, strange pop-ups appear, or programs open by themselves. Run a scan immediately! ⚡"
        };

        string[] twoFAResponses =
        {
            "Two-Factor Authentication (2FA) means you need your password PLUS a code from your phone to log in. Even if hackers steal your password, they still cannot get in! 🛡️",
            "Use an authenticator app like Google Authenticator instead of SMS codes — it is much harder for hackers to intercept! 📲",
            "Enable 2FA on your email, banking, and social media. It only takes a minute to set up and it blocks the majority of attacks! 🔑"
        };

        string[] vpnResponses =
        {
            "A VPN hides your internet activity and protects you when using public Wi-Fi. Always use one at coffee shops, airports, or hotels. 🔒",
            "Free VPNs are often not safe — they may actually sell your data to advertisers. Choose a paid, trusted VPN with a no-logs policy. 💡",
            "A VPN is not a complete fix on its own, but it is a great layer of protection — especially on public internet connections. 🌐"
        };

        string[] backupResponses =
        {
            "Always keep backups of your important files! Follow the 3-2-1 rule: 3 copies of your data, on 2 different devices, with 1 stored in the cloud. 💾",
            "Test your backups regularly. A backup you have never tested might not actually work when you really need it! ✅",
            "If ransomware hits your device, having a backup means you do not have to pay criminals. Keep at least one backup stored offline! 🛡️"
        };

        // CONSTRUCTOR - this runs when the app starts
        public MainWindow()
        {
            InitializeComponent();

            // Play the voice greeting as soon as the app opens
            // This runs BEFORE the user enters their name
            PlayGreeting();

            // Focus the name box so the user can start typing right away
            NameTextBox.Focus();
        }

        private void PlayGreeting()
        {
            try {

                string resourceName = "Part_2.greeting.wav";
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream audioStream = assembly.GetManifestResourceStream(resourceName);

                if (audioStream != null)
                {
                    // SoundPlayer plays the WAV file
                    SoundPlayer player = new SoundPlayer(audioStream);
                    player.Play(); // Play() is async — it plays in the background
                    return;        // Exit the method if this worked
                }

                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");

            //if the embedded resource was not found, try to load from the file system 
            if (File.Exists(filePath))
                {
                    SoundPlayer fallbackPlayer = new SoundPlayer(filePath);
                    fallbackPlayer.Play();
                    return;
                }

                // If neither worked, silently skip — the app still runs fine
            }
            catch (Exception)
            {
                // If anything goes wrong with audio, just skip it silently
            }
        }


        // User presses Enter in the name box
        private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartChat();
            }
        }

        // User clicks the arrow button
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartChat();
        }

        // Starts the chat session
        private void StartChat()
        {
            // Make sure a name was entered
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please enter your name to begin! 🦇");
                return;
            }

            // Save name with capital first letter
            string rawName = NameTextBox.Text.Trim();
            userName = char.ToUpper(rawName[0]) + rawName.Substring(1).ToLower();

            // Hide the name entry panel
            NamePanel.Visibility = Visibility.Collapsed;

            // Enable the chat input and send button
            UserInput.IsEnabled = true;
            SendBtn.IsEnabled = true;

            // Show the user's name in the header badge
            UserBadgeText.Text = "👤 " + userName;
            UserBadge.Visibility = Visibility.Visible;

            // Show BIMO's welcome message
            AddBotMessage(
                "🦇 Welcome, " + userName + "! I am BIMO, your Cybersecurity Awareness Assistant.\n\n" +
                "I can help you with: passwords, phishing, scams, privacy, malware, 2FA, VPNs, and backups.\n\n" +
                "Tap a topic above or just type your question. Type 'help' to see all commands! 🩸"
            );

            // Move focus to the input box
            UserInput.Focus();
        }

        // User presses Enter in the input box
        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HandleUserInput();
            }
        }

        // User clicks the blood-drop send button
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            HandleUserInput();
        }

        // User clicks one of the quick topic chips
        private void TopicButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;

            // Strip emoji prefix to get the keyword
            // e.g. " password" → "password"
            string fullText = clickedButton.Content.ToString();
            string keyword = fullText.Substring(fullText.IndexOf(' ') + 1).Trim();

            // Show it as the user's message
            AddUserMessage(keyword);

            // Process it
            RespondToInput(keyword);
        }

        // this handles the users input
        private void HandleUserInput()
        {
            string input = UserInput.Text.Trim();

            // Clear the box ready for next message
            UserInput.Clear();

            // Handle empty input
            if (string.IsNullOrWhiteSpace(input))
            {
                emptyInputCount++;

                if (emptyInputCount >= 3)
                {
                    AddBotMessage("🦇 I notice you are not typing anything, " + userName + ". Tap a topic chip above or type 'help'!");
                    emptyInputCount = 0;
                }
                else
                {
                    AddBotMessage("Please type something, " + userName + "! I am here to help. 🩸");
                }
                return;
            }

            // Reset counter since user typed something
            emptyInputCount = 0;

            // Show what the user typed
            AddUserMessage(input);

            // Get a response
            RespondToInput(input);
        }

        // here is the main method that generates the bot's response based on the user's input.
        // It checks for keywords, patterns, and sentiment to provide a relevant and engaging reply.
        // It also handles special commands like "help" and "exit".
        private void RespondToInput(string input)
        {
            // Lowercase version for easy comparison
            string lower = input.ToLower();


            // if the message is too long
            if (input.Length > 300)
            {
                AddBotMessage("That message is very long, " + userName + "! Please keep it shorter. Try asking about one topic at a time. 🦇");
                return;
            }


            // EXIT 
            if (lower == "exit" || lower == "quit" || lower == "bye" || lower == "goodbye")
            {
                AddBotMessage("Farewell, " + userName + "! Stay safe in the digital night. 🦇🩸\n\nRemember: cybersecurity is everyone's responsibility!");
                UserInput.IsEnabled = false;
                SendBtn.IsEnabled = false;
                return;
            }


            // HELP 
            if (lower == "help" || lower == "commands")
            {
                AddBotMessage(
                    " WHAT I CAN HELP WITH:\n\n" +
                    "* password  — Password safety tips\n" +
                    "* phishing  — How to spot phishing\n" +
                    "* scam      — How to avoid scams\n" +
                    "* privacy   — Protecting your privacy\n" +
                    "* malware   — What malware is\n" +
                    "* 2fa       — Two-Factor Authentication\n" +
                    "* vpn       — What a VPN does\n" +
                    "* backup    — How to back up your data\n\n" +
                    "* You can also say:\n" +
                    "  • 'how are you'\n" +
                    "  • 'tell me more'\n" +
                    "  • 'I am interested in [topic]'\n" +
                    "  • 'what do you remember'\n\n" +
                    "🦇 Type 'bye' to exit"
                );
                return;
            }


            // HOW ARE YOU
            if (lower == "how are you" || lower == "how are you?" || lower == "how r u")
            {
                string[] replies =
                {
                    "I am doing great, " + userName + "! Ready to guard you in the digital darkness. 🦇",
                    "I am excellent, " + userName + "! All my cybersecurity fangs are sharp and ready! 🩸",
                    "I feel alive... well, sort of. Ready to help you stay safe online, " + userName + "! 🦇"
                };

                int pick = random.Next(replies.Length);
                AddBotMessage(replies[pick]);
                return;
            }


            //  WHAT IS YOUR PURPOSE / WHO ARE YOU
            if (lower.Contains("your purpose") || lower.Contains("what are you") || lower.Contains("who are you"))
            {
                AddBotMessage("I am BIMO 🦇 — your Cybersecurity Awareness Assistant! My purpose is to help people like you, " + userName + ", stay safe online. I cover passwords, phishing, scams, malware and much more. Think of me as your digital vampire bodyguard! 🩸");
                return;
            }


            //  WHO MADE YOU 
            if (lower.Contains("who made you") || lower.Contains("who created you"))
            {
                AddBotMessage("I was created by a cybersecurity student to help raise awareness about online safety! My mission is to protect people in the dark corners of the internet. 🦇");
                return;
            }


            // this if block checks if the user is greeting the bot, and if so,
            // it responds with a random greeting and does not process further input
            if (lower == "hello" || lower == "hi" || lower == "hey" || lower == "greetings")
            {
                string[] greetings =
                {
                    "Greetings, " + userName + "! Welcome to the dark side of cybersecurity. 🦇",
                    "Hello, " + userName + "! How can I protect you in the digital night? 🩸",
                    "Hey " + userName + "! The vampires of the internet won't get you on my watch! 🛡️"
                };

                int pick = random.Next(greetings.Length);
                AddBotMessage(greetings[pick]);
                return;
            }


            // this if block checks if the user is expressing gratitude, and if so,
            // it responds with a caring message and does not process further input
            if (lower.Contains("thank you") || lower.Contains("thanks") || lower == "thx")
            {
                AddBotMessage("You are very welcome, " + userName + "! Stay vigilant in the digital darkness. 🦇 Is there anything else I can help you with?");
                return;
            }


            // this if block checks if the user is expressing interest in a topic, and if so, it extracts the topic and
            // stores it in userInterest for later recall
            if (lower.Contains("i am interested in") || lower.Contains("i'm interested in"))
            {
                int index = lower.IndexOf("interested in");
                string topic = input.Substring(index + 13).Trim();
                topic = topic.TrimEnd('.', '!', '?');

                userInterest = topic;

                AddBotMessage("🩸 Noted! I will remember that you are interested in " + topic + ", " + userName + ". That is an important area of cybersecurity!\n\nFeel free to ask me anything about it anytime.");
                return;
            }


            // this if else block checks if the user is asking the bot what it remembers about them, and responds with the stored name and interest if available
            if (lower.Contains("what do you remember") || lower.Contains("do you remember me") || lower.Contains("my interest"))
            {
                if (userInterest != "")
                {
                    AddBotMessage(" I remember that you are interested in: " + userInterest + ", " + userName + "!\n\nWould you like more tips on that topic?");
                }
                else
                {
                    AddBotMessage("I know your name is " + userName + "! 🦇\n\nYou have not told me your interests yet. Try saying: 'I am interested in privacy'.");
                }
                return;
            }


            // this checks if the user is asking for more tips on the last topic discussed
            if (lower.Contains("tell me more") || lower.Contains("another tip") ||
                lower.Contains("more details") || lower.Contains("what else") ||
                lower == "more")
            {
                if (lastTopic == "")
                {
                    AddBotMessage("I am not sure what to continue! Ask about a topic first — like 'password' or 'phishing'. 🦇");
                }
                else
                {
                    AddBotMessage("* Here is another tip about " + lastTopic + ":\n\n" + GetRandomResponse(lastTopic));
                }
                return;
            }

            //this area of code checks for sentiment in the user's message and adds a caring prefix
            //to the bot's response if needed
            // Worried
            if (lower.Contains("worried") || lower.Contains("scared") ||
                lower.Contains("anxious") || lower.Contains("afraid") ||
                lower.Contains("unsafe") || lower.Contains("nervous"))
            {
                AddBotMessage("It is completely okay to feel worried, " + userName + ". 🦇\n\nCyber threats can feel scary — but knowing about them is the first step to staying safe!\n\nWhat worries you most? Try tapping 'phishing', 'scam', or 'password' above. 🩸");
                return;
            }

            // Frustrated
            if (lower.Contains("frustrated") || lower.Contains("confused") ||
                lower.Contains("don't understand") || lower.Contains("difficult") ||
                lower.Contains("complicated") || lower.Contains("overwhelmed"))
            {
                AddBotMessage("I hear you, " + userName + " — cybersecurity can feel overwhelming at first! 🦇\n\nLet's slow things down. Tap one of the topic chips above and I will explain it simply. 🩸");
                return;
            }


            // when the user input contains keywords related to a topic,
            // the bot responds with a random tip from that topic's array

            if (lower.Contains("password") || lower.Contains("passphrase"))
            {
                lastTopic = "password";
                string prefix = CheckSentiment(lower);
                string response = GetRandomResponse("password");
                AddBotMessage(prefix + response);
                AddBotMessage("💡 Type 'tell me more' for another password tip!");
                return;
            }

            if (lower.Contains("phishing") || lower.Contains("fake email") || lower.Contains("suspicious email"))
            {
                lastTopic = "phishing";
                string prefix = CheckSentiment(lower);
                string response = GetRandomResponse("phishing");
                AddBotMessage(prefix + response);
                AddBotMessage("💡 Type 'tell me more' for another phishing tip!");
                return;
            }

            if (lower.Contains("scam") || lower.Contains("fraud") || lower.Contains("con "))
            {
                lastTopic = "scam";
                string prefix = CheckSentiment(lower);
                string response = GetRandomResponse("scam");
                AddBotMessage(prefix + response);
                AddBotMessage("💡 Type 'tell me more' for another scam tip!");
                return;
            }

            if (lower.Contains("privacy") || lower.Contains("personal data") || lower.Contains("personal information"))
            {
                lastTopic = "privacy";
                string prefix = CheckSentiment(lower);
                string response = GetRandomResponse("privacy");
                AddBotMessage(prefix + response);
                AddBotMessage("💡 Type 'tell me more' for another privacy tip!");
                return;
            }

            if (lower.Contains("malware") || lower.Contains("virus") || lower.Contains("antivirus"))
            {
                lastTopic = "malware";
                string prefix = CheckSentiment(lower);
                string response = GetRandomResponse("malware");
                AddBotMessage(prefix + response);
                AddBotMessage("💡 Type 'tell me more' for another malware tip!");
                return;
            }

            if (lower.Contains("2fa") || lower.Contains("two factor") || lower.Contains("two-factor") ||
                lower.Contains("mfa") || lower.Contains("authentication"))
            {
                lastTopic = "2fa";
                string prefix = CheckSentiment(lower);
                string response = GetRandomResponse("2fa");
                AddBotMessage(prefix + response);
                AddBotMessage("💡 Type 'tell me more' for another 2FA tip!");
                return;
            }

            if (lower.Contains("vpn") || lower.Contains("public wifi") || lower.Contains("public wi-fi"))
            {
                lastTopic = "vpn";
                string prefix = CheckSentiment(lower);
                string response = GetRandomResponse("vpn");
                AddBotMessage(prefix + response);
                AddBotMessage("💡 Type 'tell me more' for another VPN tip!");
                return;
            }

            if (lower.Contains("backup") || lower.Contains("back up") || lower.Contains("ransomware"))
            {
                lastTopic = "backup";
                string prefix = CheckSentiment(lower);
                string response = GetRandomResponse("backup");
                AddBotMessage(prefix + response);
                AddBotMessage("💡 Type 'tell me more' for another backup tip!");
                return;
            }


            // this runs if no keywords or patterns were recognized
            // — it gives a random default reply to encourage the user to ask about a valid topic
            string[] defaultReplies =
            {
                "I did not quite catch that, " + userName + ". 🦇 I specialise in cybersecurity! Try typing 'password', 'phishing', or 'help'.",
                "Hmm, that is outside my expertise, " + userName + ". Ask me about passwords, scams, privacy, malware, 2FA, VPNs, or backups! 🩸",
                "I am not sure about that one, " + userName + ". Try tapping one of the topic chips above to get started! 🦇"
            };

            int defaultPick = random.Next(defaultReplies.Length);
            AddBotMessage(defaultReplies[defaultPick]);
        }

        // it returns a random response from the appropriate array based on the topic keyword
        private string GetRandomResponse(string topic)
        {
            if (topic == "password")
                return passwordResponses[random.Next(passwordResponses.Length)];

            else if (topic == "phishing")
                return phishingResponses[random.Next(phishingResponses.Length)];

            else if (topic == "scam")
                return scamResponses[random.Next(scamResponses.Length)];

            else if (topic == "privacy")
                return privacyResponses[random.Next(privacyResponses.Length)];

            else if (topic == "malware")
                return malwareResponses[random.Next(malwareResponses.Length)];

            else if (topic == "2fa")
                return twoFAResponses[random.Next(twoFAResponses.Length)];

            else if (topic == "vpn")
                return vpnResponses[random.Next(vpnResponses.Length)];

            else if (topic == "backup")
                return backupResponses[random.Next(backupResponses.Length)];

            return "I do not have tips on that topic yet!";
        }

        // HELPER: Check sentiment and return a caring prefix
        private string CheckSentiment(string lower)
        {
            // Worried
            if (lower.Contains("worried") || lower.Contains("scared") ||
                lower.Contains("afraid") || lower.Contains("nervous"))
            {
                return "It sounds like you are worried, " + userName + ". Here is what you need to know:\n\n";
            }

            // Curious
            if (lower.Contains("curious") || lower.Contains("interested") ||
                lower.Contains("want to know") || lower.Contains("wondering"))
            {
                return "Great question, " + userName + "! Here is what you should know:\n\n";
            }

            // Frustrated
            if (lower.Contains("frustrated") || lower.Contains("confused") ||
                lower.Contains("don't understand"))
            {
                return "Let me explain this simply for you, " + userName + ":\n\n";
            }

            // No strong sentiment — no prefix needed
            return "";
        }

        // Adds a user message to the chat panel, aligned to the right with the bubble 
        private void AddUserMessage(string text)
        {
            StackPanel row = new StackPanel();
            row.Orientation = Orientation.Horizontal;
            row.HorizontalAlignment = HorizontalAlignment.Right;
            row.Margin = new Thickness(50, 3, 8, 3);

            Border bubble = new Border();
            bubble.Background = new SolidColorBrush(Color.FromRgb(0x3d, 0x00, 0x15));
            bubble.BorderBrush = new SolidColorBrush(Color.FromRgb(0x8b, 0x00, 0x00));
            bubble.BorderThickness = new Thickness(1);
            bubble.CornerRadius = new CornerRadius(16, 16, 2, 16);
            bubble.Padding = new Thickness(12, 8, 12, 8);
            bubble.MaxWidth = 280;

            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xCC, 0xD5));
            textBlock.FontSize = 13;
            textBlock.FontFamily = new FontFamily("Segoe UI");
            textBlock.TextWrapping = TextWrapping.Wrap;

            bubble.Child = textBlock;
            row.Children.Add(bubble);

            MessagesPanel.Children.Add(row);
            ChatScroller.ScrollToBottom();
        }

        // Adds a bot message to the chat panel, aligned to the left with the bat avatar and "BIMO" label
        private void AddBotMessage(string text)
        {
            StackPanel row = new StackPanel();
            row.Orientation = Orientation.Horizontal;
            row.HorizontalAlignment = HorizontalAlignment.Left;
            row.Margin = new Thickness(8, 3, 50, 3);
            row.VerticalAlignment = VerticalAlignment.Top;

            // Bat avatar circle
            Border avatar = new Border();
            avatar.Width = 32;
            avatar.Height = 32;
            avatar.CornerRadius = new CornerRadius(16);
            avatar.Background = new SolidColorBrush(Color.FromRgb(0x5c, 0x00, 0x18));
            avatar.BorderBrush = new SolidColorBrush(Color.FromRgb(0x8b, 0x00, 0x00));
            avatar.BorderThickness = new Thickness(1);
            avatar.VerticalAlignment = VerticalAlignment.Bottom;
            avatar.Margin = new Thickness(0, 0, 6, 0);

            TextBlock batIcon = new TextBlock();
            batIcon.Text = "🦇";
            batIcon.FontSize = 16;
            batIcon.HorizontalAlignment = HorizontalAlignment.Center;
            batIcon.VerticalAlignment = VerticalAlignment.Center;
            avatar.Child = batIcon;

            // Message bubble
            Border bubble = new Border();
            bubble.Background = new SolidColorBrush(Color.FromRgb(0x22, 0x00, 0x08));
            bubble.BorderBrush = new SolidColorBrush(Color.FromRgb(0x6b, 0x00, 0x20));
            bubble.BorderThickness = new Thickness(1);
            bubble.CornerRadius = new CornerRadius(2, 16, 16, 16);
            bubble.Padding = new Thickness(12, 8, 12, 8);
            bubble.MaxWidth = 280;

            StackPanel inner = new StackPanel();

            // "BIMO" label at the top of the bubble
            TextBlock nameLabel = new TextBlock();
            nameLabel.Text = "🦇 BIMO";
            nameLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x44, 0x66));
            nameLabel.FontSize = 11;
            nameLabel.FontWeight = FontWeights.Bold;
            nameLabel.FontFamily = new FontFamily("Segoe UI");
            nameLabel.Margin = new Thickness(0, 0, 0, 4);

            // The message text
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xCC, 0xD5));
            textBlock.FontSize = 13;
            textBlock.FontFamily = new FontFamily("Segoe UI");
            textBlock.TextWrapping = TextWrapping.Wrap;

            inner.Children.Add(nameLabel);
            inner.Children.Add(textBlock);

            bubble.Child = inner;

            row.Children.Add(avatar);
            row.Children.Add(bubble);

            MessagesPanel.Children.Add(row);
            ChatScroller.ScrollToBottom();
        }


    } 
} 

