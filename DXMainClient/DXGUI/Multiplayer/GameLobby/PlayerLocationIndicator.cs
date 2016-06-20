﻿using Rampastring.XNAUI.DXControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rampastring.XNAUI;
using Microsoft.Xna.Framework.Graphics;
using DTAClient.domain.CnCNet;
using PlayerInfo = DTAClient.domain.CnCNet.PlayerInfo;
using Microsoft.Xna.Framework;

namespace DTAClient.DXGUI.Multiplayer.GameLobby
{
    /// <summary>
    /// A player location indicator for the map preview.
    /// </summary>
    public class PlayerLocationIndicator : XNAControl
    {
        const float TEXTURE_SCALE = 0.25f;

        public PlayerLocationIndicator(WindowManager windowManager, List<MultiplayerColor> mpColors,
            Color nameBackgroundColor, Color nameBorderColor, XNAContextMenu contextMenu) : base(windowManager)
        {
            this.mpColors = mpColors;
            this.nameBackgroundColor = nameBackgroundColor;
            this.nameBorderColor = nameBorderColor;
            this.contextMenu = contextMenu;
            HoverRemapColor = Color.White;
        }

        Texture2D baseTexture;
        Texture2D hoverTexture;
        Texture2D usedTexture;
        public Texture2D WaypointTexture { get; set; }
        public List<PlayerInfo> Players = new List<PlayerInfo>();
        List<MultiplayerColor> mpColors;

        public bool BackgroundShown { get; set; }

        public int FontIndex { get; set; }

        public double AngularVelocity = 0.015;
        public double ReservedAngularVelocity = -0.0075;

        public Color HoverRemapColor { get; set; }

        XNAContextMenu contextMenu { get; set; }

        Color nameBackgroundColor;
        Color nameBorderColor;

        string[] teamIds = new string[] { String.Empty, "[A]", "[B]", "[C]", "[D]" };

        bool isHoveredOn = false;

        double backgroundAlpha = 0.0;
        double backgroundAlphaRate = 0.1;

        double angle;


        int lineHeight;

        Vector2 textSize;

        public override void Initialize()
        {
            base.Initialize();

            baseTexture = AssetLoader.LoadTexture("slocindicator.png");
            hoverTexture = AssetLoader.LoadTexture("slocindicatorh.png");
            ClientRectangle = baseTexture.Bounds;
            lineHeight = (int)Renderer.GetTextDimensions("@", FontIndex).Y + 1;

            usedTexture = baseTexture;
        }

        public void SetPosition(Point p)
        {
            int width = (int)(baseTexture.Width * TEXTURE_SCALE);
            int height = (int)(baseTexture.Height * TEXTURE_SCALE);

            ClientRectangle = new Rectangle(p.X - width / 2,
                p.Y - height / 2,
                width, height);
        }

        public void Refresh()
        {
            textSize = Vector2.Zero;

            foreach (PlayerInfo pInfo in Players)
            {
                string text = (pInfo.Index + 1) + ". " + pInfo.Name;
                if (pInfo.TeamId > 0)
                    text = teamIds[pInfo.TeamId] + " " + (pInfo.Index + 1) + ". " + pInfo.Name;

                Vector2 pInfoSize = Renderer.GetTextDimensions(text, FontIndex);

                if (pInfoSize.X > textSize.X)
                    textSize = new Vector2(pInfoSize.X, Players.Count * (pInfoSize.Y + 1));
            }
        }

        protected override void OnVisibleChanged(object sender, EventArgs args)
        {
            base.OnVisibleChanged(sender, args);

            backgroundAlpha = 0.0;
        }

        public override void OnMouseEnter()
        {
            //usedTexture = hoverTexture;

            isHoveredOn = true;

            base.OnMouseEnter();
        }

        public override void OnMouseLeave()
        {
            //usedTexture = baseTexture;

            isHoveredOn = false;

            base.OnMouseLeave();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            angle += Players.Count > 0 ? ReservedAngularVelocity : AngularVelocity;

            if (Players.Count > 0)
            {
                usedTexture = hoverTexture;
            }
            else
                usedTexture = baseTexture;

            if (BackgroundShown)
                backgroundAlpha = Math.Min(backgroundAlpha + backgroundAlphaRate, 1.0);
            else
                backgroundAlpha = Math.Max(backgroundAlpha - backgroundAlphaRate, 0.0);
        }

        public override void Draw(GameTime gameTime)
        {
            Rectangle displayRectangle = WindowRectangle();

            Vector2 origin = new Vector2(usedTexture.Width / 2, usedTexture.Height / 2);

            Renderer.DrawTexture(usedTexture,
                new Vector2(displayRectangle.Center.X + 1.5f, displayRectangle.Center.Y + 1f),
                (float)angle,
                origin,
                new Vector2(TEXTURE_SCALE), Color.Black);

            if (isHoveredOn || 
                (contextMenu.Tag == this.Tag && contextMenu.Visible))
            {
                Renderer.DrawTexture(usedTexture,
                new Vector2(displayRectangle.Center.X + 0.5f, displayRectangle.Center.Y),
                (float)angle,
                origin,
                new Vector2(TEXTURE_SCALE + 0.1f), HoverRemapColor);
            }

            Renderer.DrawTexture(usedTexture, 
                new Vector2(displayRectangle.Center.X + 0.5f, displayRectangle.Center.Y),
                (float)angle, 
                origin, 
                new Vector2(TEXTURE_SCALE), Color.White);

            if (WaypointTexture != null)
            {
                Renderer.DrawTexture(WaypointTexture, displayRectangle, Color.White);
            }

            int y = displayRectangle.Y + ((int)(baseTexture.Height * TEXTURE_SCALE) - lineHeight) / 2;

            foreach (PlayerInfo pInfo in Players)
            {
                Color textColor = Color.White;
                if (pInfo.ColorId > 0)
                    textColor = mpColors[pInfo.ColorId - 1].XnaColor;

                string text = (pInfo.Index + 1) + ". " + pInfo.Name;

                int textXPosition = 3;

                if (ClientRectangle.Right + textXPosition + (int)textSize.X > Parent.ClientRectangle.Width)
                {
                    textXPosition = -(int)textSize.X - 3 - (int)(baseTexture.Width * TEXTURE_SCALE);
                    text = pInfo.TeamId > 0 ? text + " " + teamIds[pInfo.TeamId] : text;
                }
                else
                {
                    text = pInfo.TeamId > 0 ? teamIds[pInfo.TeamId] + " " + text : text; 
                }

                int rectangleCoordX = displayRectangle.Right + textXPosition - 2;
                int rectangleWidth = (int)textSize.X + 5;
                if (backgroundAlpha > 0.0)
                {
                    Renderer.FillRectangle(new Rectangle(rectangleCoordX, y, rectangleWidth, lineHeight),
                        new Color(nameBackgroundColor.R, nameBackgroundColor.G, nameBackgroundColor.B,
                        (int)(nameBackgroundColor.A * backgroundAlpha)));

                    Renderer.DrawRectangle(new Rectangle(rectangleCoordX, y, rectangleWidth, lineHeight),
                        new Color(nameBorderColor.R, nameBorderColor.G, nameBorderColor.B, (int)(nameBorderColor.A * backgroundAlpha)));
                }

                Renderer.DrawStringWithShadow(text, FontIndex,
                    new Vector2(displayRectangle.Right + textXPosition,
                    y), textColor);

                y += lineHeight;
            }

            base.Draw(gameTime);
        }
    }
}
