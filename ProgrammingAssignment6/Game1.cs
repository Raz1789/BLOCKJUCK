﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaCards;

namespace ProgrammingAssignment6
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int WindowWidth = 800;
        const int WindowHeight = 600;

        // max valid blockjuck score for a hand
        const int MaxHandValue = 21;

        // deck and hands
        Deck deck;
        List<Card> dealerHand = new List<Card>();
        List<Card> playerHand = new List<Card>();

        // hand placement
        const int TopCardOffset = 100;
        const int HorizontalCardOffset = 150;
        const int VerticalCardSpacing = 125;

        // messages
        SpriteFont messageFont;
        const string ScoreMessagePrefix = "Score: ";
        Message playerScoreMessage;
        Message dealerScoreMessage;
        Message winnerMessage;
		List<Message> messages = new List<Message>();

        // message placement
        const int ScoreMessageTopOffset = 25;
        const int HorizontalMessageOffset = HorizontalCardOffset;
        Vector2 winnerMessageLocation = new Vector2(WindowWidth / 2,
            WindowHeight / 2);

        // menu buttons
        Texture2D quitButtonSprite;
        List<MenuButton> menuButtons = new List<MenuButton>();

        // menu button placement
        const int TopMenuButtonOffset = TopCardOffset;
        const int QuitMenuButtonOffset = WindowHeight - TopCardOffset;
        const int HorizontalMenuButtonOffset = WindowWidth / 2;
        const int VerticalMenuButtonSpacing = 125;

        // use to detect hand over when player and dealer didn't hit
        bool playerHit = false;
        bool dealerHit = false;

        // game state tracking
        static GameState currentState = GameState.WaitingForPlayer;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // setting game screen resolution
            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;

            //setting mouse to visible
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // create and shuffle deck
            deck = new Deck(Content, WindowWidth / 2, WindowHeight / 2 + 100);
            deck.Shuffle();

            // first player card
            Card tempCard = deck.TakeTopCard();
            tempCard.FlipOver();
            tempCard.X = HorizontalCardOffset;
            tempCard.Y = TopCardOffset;
            playerHand.Add(tempCard);

            // first dealer card
            tempCard = deck.TakeTopCard();
            tempCard.X = WindowWidth - HorizontalCardOffset;
            tempCard.Y = TopCardOffset;
            dealerHand.Add(tempCard);

            // second player card
            tempCard = deck.TakeTopCard();
            tempCard.FlipOver();
            tempCard.X = HorizontalCardOffset;
            tempCard.Y = TopCardOffset + VerticalCardSpacing;
            playerHand.Add(tempCard);

            // second dealer card
            tempCard = deck.TakeTopCard();
            tempCard.FlipOver();
            tempCard.X = WindowWidth - HorizontalCardOffset;
            tempCard.Y = TopCardOffset+ VerticalCardSpacing;
            dealerHand.Add(tempCard);

            // load sprite font, create message for player score and add to list
            messageFont = Content.Load<SpriteFont>(@"fonts\Arial24");
            playerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(playerHand).ToString(),
                messageFont,
                new Vector2(HorizontalMessageOffset, ScoreMessageTopOffset));
            messages.Add(playerScoreMessage);
            

            // load quit button sprite for later use
            quitButtonSprite = Content.Load<Texture2D>(@"graphics\quitbutton");

            // create hit button and add to list
            menuButtons.Add(
                new MenuButton(Content.Load<Texture2D>(@"graphics\hitbutton"),
                new Vector2(HorizontalMenuButtonOffset, TopMenuButtonOffset ), GameState.PlayerHitting));

            // create stand button and add to list
            menuButtons.Add(
                new MenuButton(Content.Load<Texture2D>(@"graphics\standbutton"),
                new Vector2(HorizontalMenuButtonOffset, VerticalMenuButtonSpacing + TopMenuButtonOffset), GameState.WaitingForDealer));

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // temporary variables
            MouseState mouse = Mouse.GetState();
            Card tempCard;

            // update menu buttons as appropriate
            foreach (MenuButton menubutton in menuButtons)
            {
                menubutton.Update(mouse);
            }

            // game state-specific processing
            switch(currentState)
            {
                case GameState.WaitingForPlayer:
                    playerHit = false;
                    break;
                case GameState.PlayerHitting:
                    tempCard = deck.TakeTopCard();
                    tempCard.FlipOver();
                    tempCard.X = HorizontalCardOffset;
                    tempCard.Y = TopCardOffset + VerticalCardSpacing * (playerHand.Count);
                    playerHand.Add(tempCard);
                    playerScoreMessage.Text = ScoreMessagePrefix + GetBlockjuckScore(playerHand).ToString();
                    playerHit = true;
                    if (GetBlockjuckScore(playerHand) == MaxHandValue)
                        ChangeState(GameState.CheckingHandOver);
                    else
                        ChangeState(GameState.WaitingForDealer);
                    break;
                case GameState.WaitingForDealer:
                    if(GetBlockjuckScore(dealerHand) <= 16)
                    {
                        ChangeState(GameState.DealerHitting);
                    } else
                    {
                        dealerHit = false;
                        ChangeState(GameState.CheckingHandOver);
                    }
                    break;
                case GameState.DealerHitting:
                    tempCard = deck.TakeTopCard();
                    tempCard.FlipOver();
                    tempCard.X = WindowWidth - HorizontalCardOffset;
                    tempCard.Y = TopCardOffset + VerticalCardSpacing * (dealerHand.Count);
                    dealerHand.Add(tempCard);
                    dealerHit = true;
                    if (GetBlockjuckScore(dealerHand) == MaxHandValue)
                        ChangeState(GameState.CheckingHandOver);
                    else
                        ChangeState(GameState.WaitingForDealer);
                    break;
                case GameState.CheckingHandOver:
                    if((!playerHit && !dealerHit) || (GetBlockjuckScore(playerHand) >= MaxHandValue) || (GetBlockjuckScore(dealerHand) >= MaxHandValue))
                    {
                        dealerHand[0].FlipOver();
                        dealerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(dealerHand).ToString(),
                            messageFont,
                            new Vector2(WindowWidth - HorizontalMessageOffset, ScoreMessageTopOffset));
                        messages.Add(dealerScoreMessage);

                        // Score calculation
                        int playerScore = GetBlockjuckScore(playerHand);
                        int dealerScore = GetBlockjuckScore(dealerHand);

                        //Winning Condition checking
                        if (playerScore > dealerScore && playerScore <= MaxHandValue)
                        {
                            winnerMessage = new Message("PLAYER WINS!!!!!", messageFont, winnerMessageLocation);
                        } else if (dealerScore > playerScore && dealerScore <= MaxHandValue)
                        {
                            winnerMessage = new Message("PLAYER LOSES!!!", messageFont, winnerMessageLocation);
                        } else if ( dealerScore == playerScore && dealerScore <= MaxHandValue )
                        {
                            winnerMessage = new Message("IT'S A TIE!!!", messageFont, winnerMessageLocation);
                        } else if (playerScore > MaxHandValue)
                        {
                            winnerMessage = new Message("PLAYER LOSES!!!!!", messageFont, winnerMessageLocation);
                        } else if (dealerScore > MaxHandValue)
                        {
                            winnerMessage = new Message("PLAYER WINS!!!!!", messageFont, winnerMessageLocation);
                        }

                        messages.Add(winnerMessage);
                        
                        menuButtons.Clear();
                        menuButtons.Add(new MenuButton(quitButtonSprite,
                            new Vector2(HorizontalMenuButtonOffset, QuitMenuButtonOffset), GameState.Exiting));
                        ChangeState(GameState.DisplayingHandResults);
                    } else
                    {
                        ChangeState(GameState.WaitingForPlayer);
                    }
                    break;
                case GameState.DisplayingHandResults:
                    break;
                case GameState.Exiting:
                    Exit();
                    break;

            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Goldenrod);
						
            spriteBatch.Begin();

            // draw hands
            foreach(Card hand in playerHand)
            {
                hand.Draw(spriteBatch);
            }
            foreach (Card hand in dealerHand)
            {
                hand.Draw(spriteBatch);
            }

            // draw messages
            foreach(Message message in messages)
            {
                message.Draw(spriteBatch);
            }

            // draw menu buttons
            foreach (MenuButton menuButton in menuButtons)
            {
                menuButton.Draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Calculates the Blockjuck score for the given hand
        /// </summary>
        /// <param name="hand">the hand</param>
        /// <returns>the Blockjuck score for the hand</returns>
        private int GetBlockjuckScore(List<Card> hand)
        {
            // add up score excluding Aces
            int numAces = 0;
            int score = 0;
            foreach (Card card in hand)
            {
                if (card.Rank != Rank.Ace)
                {
                    score += GetBlockjuckCardValue(card);
                }
                else
                {
                    numAces++;
                }
            }

            // if more than one ace, only one should ever be counted as 11
            if (numAces > 1)
            {
                // make all but the first ace count as 1
                score += numAces - 1;
                numAces = 1;
            }

            // if there's an Ace, score it the best way possible
            if (numAces > 0)
            {
                if (score + 11 <= MaxHandValue)
                {
                    // counting Ace as 11 doesn't bust
                    score += 11;
                }
                else
                {
                    // count Ace as 1
                    score++;
                }
            }

            return score;
        }

        /// <summary>
        /// Gets the Blockjuck value for the given card
        /// </summary>
        /// <param name="card">the card</param>
        /// <returns>the Blockjuck value for the card</returns>
        private int GetBlockjuckCardValue(Card card)
        {
            switch (card.Rank)
            {
                case Rank.Ace:
                    return 11;
                case Rank.King:
                case Rank.Queen:
                case Rank.Jack:
                case Rank.Ten:
                    return 10;
                case Rank.Nine:
                    return 9;
                case Rank.Eight:
                    return 8;
                case Rank.Seven:
                    return 7;
                case Rank.Six:
                    return 6;
                case Rank.Five:
                    return 5;
                case Rank.Four:
                    return 4;
                case Rank.Three:
                    return 3;
                case Rank.Two:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Changes the state of the game
        /// </summary>
        /// <param name="newState">the new game state</param>
        public static void ChangeState(GameState newState)
        {
            currentState = newState;
        }
    }
}
