using Life;
using Life.CheckpointSystem;
using Life.DB;
using Life.Network;
using Life.UI;
using Mirror;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

namespace HueRecuperation
{
    public class HueRecuperation : HueHelper.AllHelper
    {
        public HueRecuperation(IGameAPI aPI) : base(aPI) { }



        public override void OnPluginInit()
        {
            base.OnPluginInit();

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            Console.WriteLine("[HueRecuperation V.2.0.0] initialized success");

            Console.ForegroundColor = ConsoleColor.White;

            CreateConfig();
        
        }

        public static Config config;

        public class Config
        {
            public float PosX;

            public float PosY;

            public float PosZ;

            public int Price;

            public string WebhookLogs;
        }

        public void CreateConfig()
        {
            string directoryPath = pluginsPath + "/HueRécuperation";

            string configFilePath = directoryPath + "/config.json";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(configFilePath))
            {
                var defaultConfig = new Config
                {
                    PosX = 0f,

                    PosY = 0f,

                    PosZ = 0f,

                    Price = 500,

                    WebhookLogs = "VotreWebhook"
                };
                string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(defaultConfig, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(configFilePath, jsonContent);
            }

            config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFilePath));
        }

        public override void OnPlayerSpawnCharacter(Player player, NetworkConnection conn, Characters character)
        {
            base.OnPlayerSpawnCharacter(player, conn, character);



            NCheckpoint point = new NCheckpoint(player.netId, new Vector3(config.PosX, config.PosY, config.PosZ), NAction =>
            {
                OnEnterPoints(player);

               
            });
            player.CreateCheckpoint(point);

            
        }

        public void OnEnterPoints(Player player)
        {

                UIPanel panel = new UIPanel($"<color=#bd2433>Hue Récuperation</color>", UIPanel.PanelType.Text);

                CloseButton(player, panel);

                panel.SetText($"Veux-tu commencer la récuperation d'un point de permis pour {config.Price.ToString()} € en banque ?");

                panel.AddButton("Suivant", ui =>
                {
                    if (player.character.Bank >= config.Price)
                    {

                        if (player.character.PermisB)
                        {
                            if (player.character.PermisPoints < 12)
                            {
                                FirstQuestion(player);

                            }
                            else
                            {
                                player.Notify("Avertissement ", " Tu as déja 12 points !", NotificationManager.Type.Warning);
                            }
                        }
                        else
                        {
                            player.Notify("Avertissement", "Tu n'as pas le Permis B !", NotificationManager.Type.Warning);
                        }
                    }
                    else
                    {
                        player.Notify("Avertissement", "Tu n'as pas assez d'argent en banque !", NotificationManager.Type.Warning);
                    }


                });

                ShowPanel(player, panel);
            
            
        }

        public void FirstQuestion(Player player)
        {

            UIPanel panel = new UIPanel($"<color=#bd2433>Question 1</color>", UIPanel.PanelType.Text);

            panel.SetText("Quel est la vitesse maximum en ville ?");

            panel.AddButton("70 km/h", ui =>
            {
                OnQuestionFalse(player);

                player.ClosePanel(ui);
            });

            panel.AddButton("50 km/h", ui =>
            {
                SecondQuestion(player);

            });

            panel.AddButton("65 km/h", ui =>
            {
                OnQuestionFalse(player);

                player.ClosePanel(ui);
            });

            ShowPanel(player, panel);
        }

        public void OnQuestionFalse(Player player)
        {
            player.Notify("Perdu", $"Tu as perdu {config.Price} €!" , NotificationManager.Type.Error);

            EmbedDiscord($"{config.WebhookLogs}", "HueRécuperation", $"Le joueur {player.GetFullName()} a raté la récupération de son point de permis !", $"#bd2433");

            player.character.Bank -= config.Price;
        }

        public void SecondQuestion(Player player)
        {
            UIPanel panel = new UIPanel($"<color=#bd2433>Question 2</color>", UIPanel.PanelType.Text);

            panel.SetText("Par quelle côté as tu le droit de doubler sur autoroute ?");

            panel.AddButton("Gauche", ui =>
            {
                ThirdQuestion(player);

            });


            panel.AddButton("Droite", ui =>
            {
                OnQuestionFalse(player);

                player.ClosePanel(ui);
            });

            ShowPanel(player, panel);
        }

        public void ThirdQuestion(Player player)
        {
            UIPanel panel = new UIPanel($"<color=#bd2433>Question 3</color>", UIPanel.PanelType.Text);

            panel.SetText("Quel gyrophare est prioritaire ?");

            panel.AddButton("Bleu", ui =>
            {
                SuccessToAnswerQuestion(player);

                player.ClosePanel(ui);

            });

            panel.AddButton("Orange", ui =>
            {
                OnQuestionFalse(player);

                player.ClosePanel(ui);
            });

            ShowPanel(player, panel);

        }

        public void SuccessToAnswerQuestion(Player player)
        {
            player.Notify("Succés", "Tu as récupéré ton point de permis !", NotificationManager.Type.Success);

            EmbedDiscord($"{config.WebhookLogs}", "HueRécuperation", $"Le joueur {player.GetFullName()} a réussi sa récupération de point de permis !", $"{Success}");

            player.character.PermisPoints++;

            player.character.Bank -= config.Price;

            

        }
    }

}
